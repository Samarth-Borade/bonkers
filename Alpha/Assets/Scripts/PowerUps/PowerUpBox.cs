using System.Collections.Generic;
using UnityEngine;

// A "?" box. Nearly invisible until you get close (they're hidden!).
// No dice rolls: every box has two FIXED prizes, and which one you get depends on your place.
//   chasing -> the attack / catch-up prize
//   leading -> the defensive prize
// Each player has their own cooldown on a box, so the leader can't grab it and leave the chaser nothing.
public class PowerUpBox : MonoBehaviour
{
    const float Cooldown = 7f;

    PowerUpType chaserPrize, leaderPrize;
    SpriteRenderer sr;
    TextMesh mark, label;
    Vector2 home;
    readonly Dictionary<PlayerController, float> readyAt = new Dictionary<PlayerController, float>();

    public static void Create(Vector2 pos, PowerUpType chaser, PowerUpType leader, Transform parent)
    {
        var go = new GameObject("PowerUp Box");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        var box = go.AddComponent<PowerUpBox>();
        box.home = pos;
        box.chaserPrize = chaser;
        box.leaderPrize = leader;
        box.sr = Shapes.Box("Box", Vector2.zero, new Vector2(0.7f, 0.7f), Color.white, 8, go.transform);
        box.mark = FX.Label("?", Vector2.zero, Color.black, 0.55f, go.transform, 9);
        box.label = FX.Label("", new Vector2(0, 0.75f), Color.white, 0.28f, go.transform, 9, true);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);
    }

    public PowerUpType PrizeFor(PlayerController p) =>
        GameManager.I && GameManager.I.IsLeading(p) ? leaderPrize : chaserPrize;

    bool ReadyFor(PlayerController p) => !readyAt.TryGetValue(p, out var t) || Time.time >= t;

    void Update()
    {
        var gm = GameManager.I;
        if (!gm) return;

        // Look at the box from the nearest player's point of view.
        float dRed = Vector2.Distance(home, gm.Red.transform.position);
        float dYellow = Vector2.Distance(home, gm.Yellow.transform.position);
        var viewer = dRed <= dYellow ? gm.Red : gm.Yellow;
        float near = Mathf.Min(dRed, dYellow);

        float alpha = Mathf.Lerp(1f, 0.2f, Mathf.InverseLerp(2.5f, 6f, near));
        if (!ReadyFor(viewer)) alpha *= 0.25f;

        var prize = PrizeFor(viewer);
        var c = Color.white;
        c.a = alpha;
        sr.color = c;
        mark.color = new Color(0, 0, 0, alpha);

        // Close enough to read it? Show what you'd get.
        bool show = near < 3.5f && ReadyFor(viewer);
        label.text = show ? PowerUps.NameOf(prize) : "";
        foreach (Transform child in label.transform) child.GetComponent<TextMesh>().text = label.text;
        label.color = new Color(1, 1, 1, alpha);
    }

    void OnTriggerEnter2D(Collider2D other) => TryGive(other);
    void OnTriggerStay2D(Collider2D other) => TryGive(other);

    void TryGive(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (!p || p.Held != PowerUpType.None || !ReadyFor(p)) return;

        p.Held = PrizeFor(p);
        readyAt[p] = Time.time + Cooldown;
        FX.Pop(PowerUps.NameOf(p.Held) + "!", p.Head, PowerUps.ColorOf(p.Held), 0.6f);
        Sfx.Play(Sfx.Pickup);
    }
}
