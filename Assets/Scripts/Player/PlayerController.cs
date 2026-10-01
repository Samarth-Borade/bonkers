using UnityEngine;

// One racer. Moves, jumps, pushes, holds one power-up, and reacts to getting BONKED.
public class PlayerController : MonoBehaviour
{
    [Header("Who")]
    public string displayName = "RED";
    public Color color = Color.red;
    public PlayerController opponent;
    public int spawnSide = -1; // -1 or +1, so the two players never respawn inside each other

    [Header("Keys")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode useKey = KeyCode.S;

    [Header("Feel")]
    public float moveSpeed = 8f;
    public float groundAccel = 80f;
    public float airAccel = 50f;
    public float jumpVelocity = 15f;
    public float gravity = 3f;
    public float maxFall = 22f;

    public PowerUpType Held { get; set; } = PowerUpType.None;
    public int Facing { get; private set; } = 1;
    public int Row { get; private set; }
    public bool ControlsEnabled { get; set; }

    // Effect timers, in seconds left. Power-ups just set these.
    [HideInInspector] public float slowT, stunT, freezeT, reverseT, heavyT, speedT, superJumpT, shieldT;
    float invulnT, safeT, coyoteT, bufferT, pushCooldown;
    float controlVx, pushVx, lastVy;
    float moveInput;
    bool grounded, jumpHeld, ghosting;
    int waterCount;

    Rigidbody2D rb;
    BoxCollider2D box;
    Transform visual;
    SpriteRenderer bodySr, shieldSr, iceSr, heldSr;

    public static readonly Vector2 Size = new Vector2(0.6f, 0.7f);
    public bool InWater => waterCount > 0;
    public Vector2 Velocity => rb.linearVelocity;

    public static PlayerController Create(string name, Color color, int side, Vector2 pos,
        KeyCode left, KeyCode right, KeyCode jump, KeyCode use, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var p = go.AddComponent<PlayerController>();
        p.displayName = name;
        p.color = color;
        p.spawnSide = side;
        p.leftKey = left; p.rightKey = right; p.jumpKey = jump; p.useKey = use;
        p.BuildBody();
        return p;
    }

    void BuildBody()
    {
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = gravity;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        box = gameObject.AddComponent<BoxCollider2D>();
        box.size = Size;
        // Zero friction so players slide along walls instead of sticking to them.
        box.sharedMaterial = new PhysicsMaterial2D("Slippery") { friction = 0f, bounciness = 0f };

        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);
        bodySr = Shapes.Box("Body", Vector2.zero, Size, color, 10, visual);

        shieldSr = Shapes.Make("Shield", Shapes.Circle, Vector2.zero, new Vector2(1.2f, 1.2f), Palette.Shield, 12, transform);
        iceSr = Shapes.Box("Ice", Vector2.zero, new Vector2(0.8f, 0.9f), Palette.Ice, 13, transform);
        heldSr = Shapes.Make("Held", Shapes.Circle, new Vector2(0, 0.6f), new Vector2(0.25f, 0.25f), Color.white, 14, transform);
    }

    // ---------------- input (every frame) ----------------

    void Update()
    {
        float dt = Time.deltaTime;
        Tick(ref slowT, dt); Tick(ref stunT, dt); Tick(ref freezeT, dt); Tick(ref reverseT, dt);
        Tick(ref heavyT, dt); Tick(ref speedT, dt); Tick(ref superJumpT, dt); Tick(ref shieldT, dt);
        Tick(ref invulnT, dt); Tick(ref safeT, dt); Tick(ref pushCooldown, dt);

        moveInput = 0;
        jumpHeld = false;
        if (ControlsEnabled && stunT <= 0 && freezeT <= 0)
        {
            if (KeyHeld(leftKey)) moveInput -= 1;
            if (KeyHeld(rightKey)) moveInput += 1;
            if (reverseT > 0) moveInput = -moveInput;
            if (moveInput != 0) Facing = moveInput > 0 ? 1 : -1;

            if (KeyDown(jumpKey)) bufferT = 0.12f;
            jumpHeld = KeyHeld(jumpKey);
            if (KeyDown(useKey)) PowerUps.Use(this);
        }

        UpdateVisuals();
    }

    static void Tick(ref float t, float dt) { if (t > 0) t -= dt; }

    // Test hook: an automated test (DevBot) can press keys instead of the keyboard.
    public System.Func<KeyCode, bool> testHeld, testDown;
    bool KeyHeld(KeyCode k) => testHeld != null ? testHeld(k) : Input.GetKey(k);
    bool KeyDown(KeyCode k) => testDown != null ? testDown(k) : Input.GetKeyDown(k);

    // ---------------- physics (fixed steps) ----------------

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        grounded = CheckGround(out var standingOn);
        if (grounded)
        {
            coyoteT = 0.1f;
            if (!standingOn) Row = LevelBuilder.RowFromY(transform.position.y);
        }
        else coyoteT -= dt;
        bufferT -= dt;

        float speedMul = 1f;
        if (slowT > 0) speedMul *= 0.45f;
        if (heavyT > 0) speedMul *= 0.7f;
        if (speedT > 0) speedMul *= 1.7f;
        if (InWater) speedMul *= 0.6f;

        controlVx = Mathf.MoveTowards(controlVx, moveInput * moveSpeed * speedMul, (grounded ? groundAccel : airAccel) * dt);
        pushVx = Mathf.MoveTowards(pushVx, 0f, 22f * dt);

        var v = rb.linearVelocity;
        v.x = controlVx + pushVx;

        if (bufferT > 0 && (coyoteT > 0 || InWater))
        {
            float jumpMul = 1f;
            if (superJumpT > 0) jumpMul *= 1.25f;
            if (heavyT > 0) jumpMul *= 0.6f;
            if (InWater) jumpMul *= 0.5f;
            v.y = jumpVelocity * jumpMul;
            bufferT = 0;
            coyoteT = 0;
            Sfx.Play(InWater ? Sfx.Swim : Sfx.Jump, 0.5f);
        }

        float g = gravity;
        if (heavyT > 0) g *= 2f;
        if (InWater) g *= 0.3f;
        else if (!jumpHeld && v.y > 0) g *= 1.8f; // let go of jump early = shorter hop
        rb.gravityScale = g;

        float fallCap = InWater ? 3.5f : maxFall;
        if (v.y < -fallCap) v.y = -fallCap;

        rb.linearVelocity = v;
        lastVy = v.y;
        UpdateGhost(v.y);
    }

    bool CheckGround(out PlayerController standingOn)
    {
        standingOn = null;
        bool found = false;
        Vector2 feet = (Vector2)transform.position + Vector2.down * (Size.y / 2f + 0.04f);
        foreach (var hit in Physics2D.OverlapBoxAll(feet, new Vector2(Size.x * 0.9f, 0.08f), 0f))
        {
            if (hit.isTrigger || hit.attachedRigidbody == rb) continue;
            if (ghosting && LevelBuilder.Floors.Contains(hit)) continue;
            var p = hit.GetComponent<PlayerController>();
            if (p) standingOn = p;
            found = true;
        }
        return found;
    }

    // Super Jump lets you fly up THROUGH floors. We switch floor collisions off while rising,
    // and back on once we're falling and clear of the floor.
    void UpdateGhost(float vy)
    {
        bool want = superJumpT > 0 && vy > 0.5f;
        if (want && !ghosting) SetGhost(true);
        else if (!want && ghosting && !OverlapsFloor()) SetGhost(false);
    }

    void SetGhost(bool on)
    {
        ghosting = on;
        foreach (var f in LevelBuilder.Floors) if (f) Physics2D.IgnoreCollision(box, f, on);
    }

    bool OverlapsFloor()
    {
        foreach (var hit in Physics2D.OverlapBoxAll(transform.position, Size * 0.98f, 0f))
            if (LevelBuilder.Floors.Contains(hit)) return true;
        return false;
    }

    // ---------------- pushing + stomping ----------------

    void OnCollisionEnter2D(Collision2D c)
    {
        var other = c.collider.GetComponent<PlayerController>();
        if (!other || other != opponent) return;

        // Land on their head = STOMP.
        if (transform.position.y - other.transform.position.y > 0.7f && lastVy < -3f)
        {
            other.Bonk(this, 1.5f, "STOMPED!");
            var v = rb.linearVelocity;
            v.y = 12f;
            rb.linearVelocity = v;
        }
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (pushCooldown > 0 || moveInput == 0) return;
        var other = c.collider.GetComponent<PlayerController>();
        if (!other || other != opponent) return;

        float dx = other.transform.position.x - transform.position.x;
        if (Mathf.Abs(other.transform.position.y - transform.position.y) > 0.5f) return;
        if (Mathf.Sign(dx) != Mathf.Sign(moveInput)) return;

        other.Shove(Mathf.Sign(dx) * 8f);
        pushCooldown = 0.45f;
        Sfx.Play(Sfx.Shove, 0.7f);
    }

    public void Shove(float vx)
    {
        pushVx = vx;
        if (grounded)
        {
            var v = rb.linearVelocity;
            v.y = Mathf.Max(v.y, 3f);
            rb.linearVelocity = v;
        }
    }

    // ---------------- getting hit ----------------

    // A shield eats one attack. Returns true if it did.
    public bool Blocks()
    {
        if (shieldT <= 0) return false;
        shieldT = 0;
        invulnT = 0.4f;
        FX.Pop("BLOCKED!", Head, Palette.Shield, 0.8f);
        Sfx.Play(Sfx.Block);
        return true;
    }

    // BONK! Knock back, slow down, big text. Returns false if the hit didn't land.
    public bool Bonk(PlayerController attacker, float slowFor, string word)
    {
        if (invulnT > 0 || Blocks()) return false;
        invulnT = 1.2f; // grace period so nobody gets stun-locked
        slowT = Mathf.Max(slowT, slowFor);

        float dir = attacker ? Mathf.Sign(transform.position.x - attacker.transform.position.x) : Facing;
        pushVx = dir * 9f;
        var v = rb.linearVelocity;
        v.y = Mathf.Max(v.y, 6f);
        rb.linearVelocity = v;

        FX.Bonk(Head, word);
        return true;
    }

    public void Slip()
    {
        if (Bonk(null, 1f, "SLIPPED!"))
        {
            stunT = 1f;
        }
    }

    // Lava / spikes / grinder: back to the start of the row you were on.
    public int Deaths { get; private set; }
    public string LastDeath { get; private set; }

    public void Die(string word)
    {
        if (safeT > 0) return;
        Deaths++;
        LastDeath = word + " at x=" + transform.position.x.ToString("0.0") + " row " + (Row + 1);
        FX.Pop(word, Head, Color.white, 0.8f);
        Sfx.Play(Sfx.Die);

        SetGhost(false);
        transform.position = LevelBuilder.RespawnPoint(Row, spawnSide);
        rb.linearVelocity = Vector2.zero;
        controlVx = pushVx = 0;
        safeT = 1.5f;
        stunT = Mathf.Max(stunT, 0.35f);
        freezeT = 0;
        waterCount = 0;
    }

    public void TeleportTo(Vector2 pos, int row)
    {
        SetGhost(false);
        transform.position = pos;
        rb.linearVelocity = Vector2.zero;
        controlVx = pushVx = 0;
        Row = row;
        waterCount = 0;
    }

    public void FaceTowards(float x) => Facing = x >= transform.position.x ? 1 : -1;

    public void EnterWater() { waterCount++; }
    public void ExitWater() { waterCount = Mathf.Max(0, waterCount - 1); }

    public Vector2 Head => (Vector2)transform.position + Vector2.up * 0.9f;

    // ---------------- looks ----------------

    // Plain colors only: the body color tells you what is happening to you.
    void UpdateVisuals()
    {
        Color c = color;
        if (heavyT > 0) c = Palette.Heavy;
        if (reverseT > 0) c = Palette.Reverse;
        if (slowT > 0) c = new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f);
        bodySr.color = c;

        shieldSr.enabled = shieldT > 0;
        iceSr.enabled = freezeT > 0;
        heldSr.enabled = Held != PowerUpType.None;
        if (heldSr.enabled) heldSr.color = PowerUps.ColorOf(Held);
    }
}
