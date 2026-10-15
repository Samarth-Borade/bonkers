using UnityEngine;

// A lever on a small ledge. Jump up and touch it to spring its trap(s) further up the course.
// Only the player who is CHASING can use it - it's another way for whoever is behind to catch up.
// Pulling it costs the chaser a jump and a little time, so it's a choice, not a free hit.
public class Lever : MonoBehaviour
{
    const float Cooldown = 8f;
    const float LedgeHeight = 1.0f;   // underside of the ledge: you can walk under it
    const float LedgeThickness = 0.2f;
    const float LedgeWidth = 1.4f;
    const float NagInterval = 1.5f;   // how often a leader touching the lever sees "CHASERS ONLY"
    const float RechargingAlpha = 0.25f;   // handle opacity while on cooldown

    Trap[] traps;
    string tag_;
    Color color;
    SpriteRenderer handle;
    float readyAt, lastNagAt;

    public static void Create(Vector2 groundPos, string tag, Trap[] traps, Transform parent)
    {
        var go = new GameObject("Lever " + tag);
        go.transform.SetParent(parent, false);
        go.transform.position = groundPos;

        var color = Palette.Link(tag);
        float top = LedgeHeight + LedgeThickness;

        // the ledge you stand on
        var ledge = Shapes.Box("Ledge", new Vector2(0, LedgeHeight + LedgeThickness / 2f), new Vector2(LedgeWidth, LedgeThickness), Palette.Solid, 0, go.transform);
        ledge.gameObject.AddComponent<BoxCollider2D>();

        // the lever itself
        Shapes.Box("Post", new Vector2(0, top + 0.25f), new Vector2(0.08f, 0.5f), Color.white, 5, go.transform);
        var handle = Shapes.Box("Handle", new Vector2(0, top + 0.5f), new Vector2(0.3f, 0.18f), color, 6, go.transform);
        FX.Label(tag, new Vector2(0, top + 0.95f), color, 0.35f, go.transform, 6);

        var trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.offset = new Vector2(0, top + 0.35f);
        trigger.size = new Vector2(LedgeWidth, 0.7f);

        var lever = go.AddComponent<Lever>();
        lever.traps = traps;
        lever.tag_ = tag;
        lever.color = color;
        lever.handle = handle;
    }

    void Update()
    {
        // Dim the handle while it's recharging.
        handle.color = Time.time >= readyAt ? color : new Color(color.r, color.g, color.b, RechargingAlpha);
    }

    void OnTriggerEnter2D(Collider2D other) => TryPull(other);
    void OnTriggerStay2D(Collider2D other) => TryPull(other);

    void TryPull(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        var gm = GameManager.I;
        if (!p || !gm || gm.Phase != GameManager.State.Race || Time.time < readyAt) return;

        if (gm.IsLeading(p))
        {
            if (Time.time - lastNagAt > NagInterval)
            {
                lastNagAt = Time.time;
                FX.Pop("CHASERS ONLY", p.Head, Color.white, 0.45f);
            }
            return;
        }

        readyAt = Time.time + Cooldown;
        foreach (var t in traps) if (t) t.Fire();
        FX.Pop("TRAP " + tag_ + "!", p.Head, color, 0.6f);
        Sfx.Play(Sfx.Use);
        RaceAnalytics.LeverPulled(p);
    }
}
