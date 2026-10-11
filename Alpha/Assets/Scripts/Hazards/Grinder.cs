using UnityEngine;

// A saw (grey circle) that slides between two points. Deadly (it has a Hazard on it too).
public class Grinder : MonoBehaviour
{
    public Vector2 from, to;
    public float speed = 1f;

    Rigidbody2D rb;
    float t;

    public static void Create(Vector2 from, Vector2 to, float speed, Transform parent)
    {
        var go = new GameObject("Grinder");
        go.transform.SetParent(parent, false);
        go.transform.position = from;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f; // a bit smaller than it looks, so near misses don't count

        go.AddComponent<Hazard>().word = "GROUND UP!";

        var g = go.AddComponent<Grinder>();
        g.from = from;
        g.to = to;
        g.speed = speed;
        g.rb = rb;
        g.t = Random.value * 6f;

        Shapes.Make("Saw", Shapes.Circle, Vector2.zero, new Vector2(0.7f, 0.7f), Palette.Grinder, 20, go.transform);
        // little triangle teeth around the edge
        for (int i = 0; i < 6; i++)
        {
            float a = i * 60f;
            Vector2 dir = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
            var tooth = Shapes.Make("Tooth", Shapes.Triangle, dir * 0.4f, new Vector2(0.18f, 0.18f), Palette.Grinder, 20, go.transform);
            tooth.transform.localRotation = Quaternion.Euler(0, 0, a - 90f);
        }
    }

    void FixedUpdate()
    {
        if (from == to) return;
        t += Time.fixedDeltaTime * speed;
        rb.MovePosition(Vector2.Lerp(from, to, (Mathf.Sin(t) + 1f) / 2f));
    }
}
