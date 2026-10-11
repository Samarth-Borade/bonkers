using UnityEngine;

// The leader's trick: a ? box that looks real, dropped behind them.
// The chaser who runs into it gets BONKED instead of a prize. The owner can never trigger it.
public class FakeBox : MonoBehaviour
{
    PlayerController owner;
    SpriteRenderer box;
    TextMesh mark;
    float age;

    public static void Drop(PlayerController owner)
    {
        Vector2 start = (Vector2)owner.transform.position + new Vector2(-owner.Facing * 1.2f, 0f);

        // Find the floor under the drop point.
        float y = start.y - PlayerController.Size.y / 2f;
        foreach (var hit in Physics2D.RaycastAll(start, Vector2.down, 4f))
        {
            if (hit.collider.isTrigger || hit.collider.GetComponent<PlayerController>()) continue;
            y = hit.point.y;
            break;
        }

        var go = new GameObject("Fake Box");
        go.transform.position = new Vector2(start.x, y + 0.75f);
        // Same look as a real box.
        var box = Shapes.Box("Box", Vector2.zero, new Vector2(0.7f, 0.7f), Color.white, 8, go.transform);
        var mark = FX.Label("?", Vector2.zero, Color.black, 0.55f, go.transform, 9);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);
        var fake = go.AddComponent<FakeBox>();
        fake.owner = owner;
        fake.box = box;
        fake.mark = mark;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age > 20f) Destroy(gameObject);

        // Fade in as a player gets near, exactly like a real box does.
        var gm = GameManager.I;
        if (!gm) return;
        float near = Mathf.Min(Vector2.Distance(transform.position, gm.Red.transform.position),
                               Vector2.Distance(transform.position, gm.Yellow.transform.position));
        float alpha = Mathf.Lerp(1f, 0.2f, Mathf.InverseLerp(2.5f, 6f, near));
        box.color = new Color(1, 1, 1, alpha);
        mark.color = new Color(0, 0, 0, alpha);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (!p || p == owner) return;
        if (p.Bonk(owner, 1.5f, "FAKE BOX!")) RaceAnalytics.Hit(owner, PowerUpType.FakeBox);
        Destroy(gameObject);
    }
}
