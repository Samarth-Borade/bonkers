using UnityEngine;

// A peel on the floor. Anyone who steps on it spins out (you're safe for the first second).
public class Banana : MonoBehaviour
{
    PlayerController owner;
    float age;

    public static void Drop(PlayerController owner)
    {
        Vector2 start = (Vector2)owner.transform.position + new Vector2(-owner.Facing * 0.9f, 0f);

        // Find the floor under the drop point.
        float y = start.y - PlayerController.Size.y / 2f;
        foreach (var hit in Physics2D.RaycastAll(start, Vector2.down, 4f))
        {
            if (hit.collider.isTrigger || hit.collider.GetComponent<PlayerController>()) continue;
            y = hit.point.y;
            break;
        }

        var root = new GameObject("Banana");
        root.transform.position = new Vector2(start.x, y + 0.14f);
        Shapes.Make("Peel", Shapes.Circle, Vector2.zero, new Vector2(0.65f, 0.28f), PowerUps.ColorOf(PowerUpType.Banana), 15, root.transform);
        Shapes.Box("Tip", new Vector2(0.3f, 0.06f), new Vector2(0.1f, 0.1f), new Color(0.45f, 0.3f, 0.1f), 16, root.transform);

        var col = root.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.35f);
        root.AddComponent<Banana>().owner = owner;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age > 25f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (!p || (p == owner && age < 1f)) return;
        p.Slip();
        Destroy(gameObject);
    }
}
