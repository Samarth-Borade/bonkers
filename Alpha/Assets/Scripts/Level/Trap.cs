using UnityEngine;

// A trap that sleeps until its lever is pulled (or a Brick Wall power-up drops one).
//   Off     : a faint outline, so you can see where it will pop up
//   Warning : brighter outline for a moment (time to react)
//   Active  : Gate = a solid hurdle you must jump; Spikes = deadly strip
public class Trap : MonoBehaviour
{
    public enum Kind { Gate, Spikes }

    public const float WarnTime = 0.6f;
    public const float ActiveTime = 3.5f;

    public const float GateHeight = 1.2f;
    public const float GateWidth = 0.5f;   // lever gates; Brick Wall passes its own width

    Kind kind;
    Color color;
    SpriteRenderer[] parts;
    Collider2D solid;     // Gate
    Collider2D deadly;    // Spikes
    float warnLeft, activeLeft;
    bool destroyWhenDone;

    public bool Busy => warnLeft > 0 || activeLeft > 0;

    // groundPos = the point on the floor the trap stands on.
    public static Trap Create(Kind kind, Vector2 groundPos, float width, string tag, Transform parent)
    {
        var go = new GameObject("Trap " + tag + " (" + kind + ")");
        go.transform.SetParent(parent, false);
        go.transform.position = groundPos;

        var t = go.AddComponent<Trap>();
        t.kind = kind;
        t.color = Palette.Link(tag);

        if (kind == Kind.Gate)
        {
            var block = Shapes.Box("Gate", new Vector2(0, GateHeight / 2f), new Vector2(width, GateHeight), Color.white, 4, go.transform);
            t.parts = new[] { block };
            var col = go.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, GateHeight / 2f);
            col.size = new Vector2(width, GateHeight);
            t.solid = col;
        }
        else
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(width / 0.35f));
            float step = width / n;
            t.parts = new SpriteRenderer[n];
            for (int i = 0; i < n; i++)
                t.parts[i] = Shapes.Make("Spike", Shapes.Triangle, new Vector2(-width / 2f + step * (i + 0.5f), 0.15f),
                    new Vector2(step, 0.3f), Color.white, 4, go.transform);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.offset = new Vector2(0, 0.12f);
            col.size = new Vector2(width * 0.9f, 0.22f);
            go.AddComponent<Hazard>().word = "TRAPPED!";
            t.deadly = col;
        }

        if (!string.IsNullOrEmpty(tag))
            FX.Label(tag, new Vector2(0, kind == Kind.Gate ? GateHeight + 0.35f : 0.6f), t.color, 0.35f, go.transform, 5);

        t.Apply();
        return t;
    }

    // Used by the Brick Wall power-up: a one-off gate that removes itself afterwards.
    public static void PopUpOnce(Vector2 groundPos, float width, Transform parent = null)
    {
        var t = Create(Kind.Gate, groundPos, width, "", parent);
        t.destroyWhenDone = true;
        t.Fire();
    }

    public void Fire()
    {
        if (Busy) return;
        warnLeft = WarnTime;
        Apply();
    }

    void Update()
    {
        if (warnLeft > 0)
        {
            warnLeft -= Time.deltaTime;
            if (warnLeft <= 0) activeLeft = ActiveTime;
            Apply();
        }
        else if (activeLeft > 0)
        {
            activeLeft -= Time.deltaTime;
            if (activeLeft <= 0)
            {
                if (destroyWhenDone) { Destroy(gameObject); return; }
                Apply();
            }
        }
    }

    void Apply()
    {
        bool active = activeLeft > 0 && warnLeft <= 0;
        Color c = active ? (kind == Kind.Gate ? Color.Lerp(Palette.Solid, color, 0.5f) : Palette.Spikes)
                         : new Color(color.r, color.g, color.b, warnLeft > 0 ? 0.55f : 0.18f);
        foreach (var p in parts) p.color = c;
        if (solid) solid.enabled = active;
        if (deadly) deadly.enabled = active;
    }
}
