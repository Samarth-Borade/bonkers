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
        col.radius = 0.5f;

        go.AddComponent<Hazard>().word = "GROUND UP!";

        var g = go.AddComponent<Grinder>();
        g.from = from;
        g.to = to;
        g.speed = speed;
        g.rb = rb;
        g.t = Random.value * 6f;

        Shapes.Make("Saw", Shapes.Circle, Vector2.zero, new Vector2(1f, 1f), Palette.Grinder, 20, go.transform);
    }

    void FixedUpdate()
    {
        if (from == to) return;
        t += Time.fixedDeltaTime * speed;
        rb.MovePosition(Vector2.Lerp(from, to, (Mathf.Sin(t) + 1f) / 2f));
    }
}
