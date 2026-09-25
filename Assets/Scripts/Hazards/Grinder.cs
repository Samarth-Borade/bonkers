using UnityEngine;

// A spinning saw blade that slides between two points. Deadly (it has a Hazard on it too).
public class Grinder : MonoBehaviour
{
    public Vector2 from, to;
    public float speed = 1f;

    Rigidbody2D rb;
    Transform blade;
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

        g.blade = new GameObject("Blade").transform;
        g.blade.SetParent(go.transform, false);
        for (int i = 0; i < 4; i++)
        {
            var tooth = Shapes.Box("Tooth", Vector2.zero, new Vector2(1.3f, 0.22f), Palette.Grinder, 20, g.blade);
            tooth.transform.localRotation = Quaternion.Euler(0, 0, i * 45f);
        }
        Shapes.Make("Disc", Shapes.Circle, Vector2.zero, new Vector2(0.95f, 0.95f), Palette.Grinder * 0.8f + Color.black * 0.2f, 21, g.blade);
        Shapes.Make("Hub", Shapes.Circle, Vector2.zero, new Vector2(0.3f, 0.3f), new Color(0.25f, 0.25f, 0.3f), 22, g.blade);
    }

    void FixedUpdate()
    {
        if (from == to) return;
        t += Time.fixedDeltaTime * speed;
        rb.MovePosition(Vector2.Lerp(from, to, (Mathf.Sin(t) + 1f) / 2f));
    }

    void Update()
    {
        blade.Rotate(0, 0, -600f * Time.deltaTime);
    }
}
