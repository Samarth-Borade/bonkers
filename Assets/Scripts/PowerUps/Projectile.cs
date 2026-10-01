using UnityEngine;

// A flying punch / ice ball. Hits the opponent, or pops on the first wall or floor.
public class Projectile : MonoBehaviour
{
    PlayerController owner;
    System.Action<PlayerController> onHit;
    float life = 2f;

    public static void Fire(PlayerController owner, Color color, float speed, bool round,
        System.Action<PlayerController> onHit)
    {
        Vector2 pos = (Vector2)owner.transform.position + new Vector2(owner.Facing * 0.6f, 0.05f);
        var sr = Shapes.Make("Projectile", round ? Shapes.Circle : Shapes.Square, pos,
            round ? new Vector2(0.5f, 0.5f) : new Vector2(0.55f, 0.4f), color, 20);
        var go = sr.gameObject;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = new Vector2(owner.Facing * speed, 0f);

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var p = go.AddComponent<Projectile>();
        p.owner = owner;
        p.onHit = onHit;
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p)
        {
            if (p == owner) return;
            onHit(p);
            Destroy(gameObject);
            return;
        }
        if (other.isTrigger) return; // fly through water, boxes, etc.
        Destroy(gameObject);
    }
}
