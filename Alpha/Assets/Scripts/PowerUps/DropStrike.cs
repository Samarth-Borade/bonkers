using UnityEngine;

// A bomb that drops on the opponent from the roof of their row. Always dodgeable:
//   1) Aim   - the warning beam follows them and leads their running (0.5s)
//   2) Lock  - the beam turns darker and stops moving (0.45s). Stop, turn back or jump away!
//   3) Drop  - the bomb falls fast. Still standing under it? BONK.
public class DropStrike : MonoBehaviour
{
    const float AimTime = 0.5f, LockTime = 0.45f, FallSpeed = 22f, Lead = 0.45f;

    PlayerController owner, target;
    System.Action<PlayerController> onHit;
    Color color;
    SpriteRenderer beam;
    Rigidbody2D rb;
    float age, roof, floor;
    bool falling;

    public static void Launch(PlayerController owner, PlayerController target, Color color,
        System.Action<PlayerController> onHit)
    {
        int row = target.Row;
        float roof = LevelBuilder.RoofY(row), floor = LevelBuilder.FloorTop(row);

        var bomb = Shapes.Make("Drop Bomb", Shapes.Circle, new Vector2(target.transform.position.x, roof - 0.4f),
            new Vector2(0.6f, 0.6f), color, 25);
        var go = bomb.gameObject;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var s = go.AddComponent<DropStrike>();
        s.owner = owner;
        s.target = target;
        s.onHit = onHit;
        s.color = color;
        s.rb = rb;
        s.roof = roof;
        s.floor = floor;
        var beamColor = color;
        beamColor.a = 0.18f;
        // The beam is a child of the (0.6 scaled) bomb, so divide by 0.6 to get world units.
        float beamCenter = (roof + floor) / 2f - (roof - 0.4f);
        s.beam = Shapes.Box("Warning", new Vector2(0, beamCenter / 0.6f), new Vector2(1f / 0.6f, (roof - floor) / 0.6f), beamColor, 2, go.transform);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (falling)
        {
            if (transform.position.y < floor - 0.5f || age > 3f) Destroy(gameObject);
            return;
        }

        var pos = transform.position;
        if (age < AimTime)
        {
            // Lead the target so running in a straight line isn't a free dodge.
            float x = target.transform.position.x + target.Velocity.x * Lead;
            pos.x = Mathf.Clamp(x, -LevelBuilder.HalfWidth + 0.5f, LevelBuilder.HalfWidth - 0.5f);
            transform.position = pos;
        }
        else if (age < AimTime + LockTime)
        {
            var c = beam.color;
            c.a = 0.45f; // locked: the beam goes darker
            beam.color = c;
        }
        else
        {
            falling = true;
            Destroy(beam.gameObject);
            rb.linearVelocity = Vector2.down * FallSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!falling) return;
        var p = other.GetComponent<PlayerController>();
        if (p)
        {
            if (p == owner) return;
            onHit(p);
            Destroy(gameObject);
            return;
        }
        if (other.isTrigger) return;
        Destroy(gameObject);
    }
}
