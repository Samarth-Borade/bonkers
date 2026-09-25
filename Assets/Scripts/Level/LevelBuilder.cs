using System.Collections.Generic;
using UnityEngine;

// Builds the whole level from code. 6 rows stacked bottom -> top, zig-zag:
//
//   row 5  <- <- <- FLAG           (entered on the right)
//   row 4  -> -> -> -> ->  gap ^   (entered on the left)
//   row 3  gap ^ <- <- <- <-       ...
//   row 2  -> -> -> -> ->  gap ^
//   row 1  gap ^ <- <- <- <-
//   row 0  START -> -> ->  gap ^
//
// Each floor has a gap at one end. Jump up through it to reach the next row.
// Want a new level? Edit the "ROW CONTENT" section at the bottom.
public static class LevelBuilder
{
    public const int Rows = 6;
    public const float Bottom = -8f;        // top of the ground
    public const float RowHeight = 3f;
    public const float HalfWidth = 15.5f;    // inner edge of the side walls
    public const float GapWidth = 3f;
    public const float FloorThickness = 0.4f;
    public const float CeilingY = 9f;

    // Floors between rows. Super Jump uses this to fly through them.
    public static readonly HashSet<Collider2D> Floors = new HashSet<Collider2D>();

    public static float FloorTop(int row) => Bottom + row * RowHeight;

    // Underside of whatever is above this row (next floor, or the ceiling).
    public static float RoofY(int row) => row == Rows - 1 ? CeilingY : FloorTop(row + 1) - FloorThickness;

    // How far along the race someone is, in rows (0 = start, 6 = flag).
    // Even rows run left -> right, odd rows right -> left.
    public static float Progress(PlayerController p)
    {
        float x = p.transform.position.x;
        float along = p.Row % 2 == 0 ? (x + HalfWidth) / (HalfWidth * 2) : (HalfWidth - x) / (HalfWidth * 2);
        return p.Row + Mathf.Clamp01(along);
    }

    public static int RowFromY(float y) =>
        Mathf.Clamp(Mathf.FloorToInt((y - Bottom + 0.5f) / RowHeight), 0, Rows - 1);

    // Row 0 starts on the left. Odd rows are entered on the right, even rows on the left.
    public static int EntrySide(int row) => row % 2 == 1 ? 1 : -1;

    public static Vector2 RespawnPoint(int row, int side)
    {
        float x = row == 0 ? -13.2f : EntrySide(row) * 10.5f;
        return new Vector2(x + side * 0.6f, FloorTop(row) + 0.6f);
    }

    static Transform level;

    public static (PlayerController red, PlayerController yellow) Build(Transform root)
    {
        Floors.Clear();
        level = new GameObject("Level").transform;
        level.SetParent(root, false);

        BuildShell();
        BuildRowContent();

        var red = PlayerController.Create("RED", Palette.Red, -1, RespawnPoint(0, -1),
            KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, root);
        var yellow = PlayerController.Create("YELLOW", Palette.Yellow, 1, RespawnPoint(0, 1),
            KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, root);
        red.opponent = yellow;
        yellow.opponent = red;
        return (red, yellow);
    }

    // ---------------- walls, floors, background ----------------

    static void BuildShell()
    {
        for (int k = 0; k < Rows; k++)
        {
            float top = FloorTop(k);
            float roof = RoofY(k);
            Shapes.Box("Row " + k + " bg", new Vector2(0, (top + roof) / 2f), new Vector2(HalfWidth * 2, roof - top),
                k % 2 == 0 ? Palette.RowA : Palette.RowB, -10, level);

            // Faint arrows showing which way to run.
            string arrows = (k % 2 == 0) ? ">   >   >   >   >" : "<   <   <   <   <";
            var label = FX.Label(arrows, new Vector2(0, top + 1.5f), new Color(1, 1, 1, 0.05f), 1.2f, level, -9);
            label.name = "Arrows";
        }

        Solid("Wall L", new Vector2(-HalfWidth - 0.5f, 0.5f), new Vector2(1, 21));
        Solid("Wall R", new Vector2(HalfWidth + 0.5f, 0.5f), new Vector2(1, 21));
        Solid("Ceiling", new Vector2(0, CeilingY + 0.5f), new Vector2(HalfWidth * 2 + 2, 1));
        Solid("Ground", new Vector2(0, Bottom - 0.5f), new Vector2(HalfWidth * 2 + 2, 1));

        for (int k = 1; k < Rows; k++)
        {
            bool gapOnRight = EntrySide(k) == 1;
            float x0 = gapOnRight ? -HalfWidth : -HalfWidth + GapWidth;
            float x1 = gapOnRight ? HalfWidth - GapWidth : HalfWidth;
            float top = FloorTop(k);
            Floors.Add(Solid("Floor " + k, new Vector2((x0 + x1) / 2f, top - FloorThickness / 2f), new Vector2(x1 - x0, FloorThickness)));
        }
    }

    static Collider2D Solid(string name, Vector2 pos, Vector2 size)
    {
        var sr = Shapes.Box(name, pos, size, Palette.Solid, 0, level);
        // light top edge so floors read clearly
        Shapes.Box("Edge", new Vector2(0, 0.5f - 0.04f / size.y), new Vector2(1, 0.08f / size.y), Palette.SolidEdge, 1, sr.transform);
        return sr.gameObject.AddComponent<BoxCollider2D>();
    }

    // ---------------- hazard + item helpers ----------------

    static void Spikes(int row, float x, float width)
    {
        float top = FloorTop(row);
        var go = new GameObject("Spikes");
        go.transform.SetParent(level, false);
        go.transform.position = new Vector2(x, top);
        int n = Mathf.Max(1, Mathf.RoundToInt(width / 0.5f));
        float step = width / n;
        for (int i = 0; i < n; i++)
            Shapes.Make("Spike", Shapes.Triangle, new Vector2(-width / 2f + step * (i + 0.5f), 0.27f),
                new Vector2(step, 0.55f), Palette.Spikes, 3, go.transform);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(0, 0.2f);
        col.size = new Vector2(width * 0.9f, 0.35f);
        go.AddComponent<Hazard>().word = "SPIKED!";
    }

    static void Lava(int row, float x, float width)
    {
        float top = FloorTop(row);
        var sr = Shapes.Box("Lava", new Vector2(x, top + 0.15f), new Vector2(width, 0.3f), Palette.Lava, 3, level);
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        sr.gameObject.AddComponent<Hazard>().word = "TOASTED!";
        sr.gameObject.AddComponent<LavaGlow>();
    }

    static void WaterPool(int row, float x, float width)
    {
        float top = FloorTop(row);
        float roof = RoofY(row);
        var sr = Shapes.Box("Water", new Vector2(x, (top + roof) / 2f), new Vector2(width, roof - top), Palette.Water, 30, level);
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        sr.gameObject.AddComponent<Water>();
    }

    static void Saw(int row, float x0, float x1, float height, float speed) =>
        Grinder.Create(new Vector2(x0, FloorTop(row) + height), new Vector2(x1, FloorTop(row) + height), speed, level);

    static void SawVertical(int row, float x, float y0, float y1, float speed) =>
        Grinder.Create(new Vector2(x, FloorTop(row) + y0), new Vector2(x, FloorTop(row) + y1), speed, level);

    // Every box has two fixed prizes: one for whoever is CHASING, one for whoever is LEADING.
    static void Box(int row, float x, PowerUpType chaser, PowerUpType leader) =>
        PowerUpBox.Create(new Vector2(x, FloorTop(row) + 0.75f), chaser, leader, level);

    // ---------------- ROW CONTENT ----------------
    // Keep the ends of each row clear: that's where players land and jump up.
    // Safe zone for hazards is roughly x = -9 .. 9.

    static void BuildRowContent()
    {
        // Row 0 (run right)
        Spikes(0, -5f, 1.5f);
        Lava(0, 3f, 2f);
        Box(0, -1f, PowerUpType.Fist, PowerUpType.Shield);
        Box(0, 8f, PowerUpType.Rocket, PowerUpType.Banana);

        // Row 1 (run left)
        WaterPool(1, 2f, 6f);
        Saw(1, -7.5f, -3f, 0.8f, 1.6f);
        Box(1, 7.5f, PowerUpType.FreezeRay, PowerUpType.Banana);
        Box(1, -9.5f, PowerUpType.SuperJump, PowerUpType.Shield);

        // Row 2 (run right)
        Lava(2, -5f, 2.5f);
        Spikes(2, 2f, 1.2f);
        Spikes(2, 6f, 1.2f);
        Box(2, -1.5f, PowerUpType.GravityBomb, PowerUpType.Shield);
        Box(2, 9f, PowerUpType.Fist, PowerUpType.Banana);

        // Row 3 (run left)
        Saw(3, 1f, 7f, 0.8f, 1.3f);
        WaterPool(3, -5.5f, 5f);
        Box(3, 8.5f, PowerUpType.SuperJump, PowerUpType.Shield);
        Box(3, -1.5f, PowerUpType.Reverse, PowerUpType.Banana);

        // Row 4 (run right)
        Spikes(4, -5f, 1.5f);
        Lava(4, 1f, 3f);
        SawVertical(4, 6.5f, 0.6f, 2.1f, 2.2f);
        Box(4, -8.5f, PowerUpType.FreezeRay, PowerUpType.Shield);
        Box(4, 4f, PowerUpType.Rocket, PowerUpType.Banana);

        // Row 5 (run left) -> FLAG
        Lava(5, 4f, 2f);
        Spikes(5, -2f, 1.5f);
        Saw(5, -9f, -6f, 0.8f, 1.8f);
        Box(5, 8.5f, PowerUpType.Swap, PowerUpType.Shield);
        Flag.Create(new Vector2(-14f, FloorTop(5)), level);
    }
}

// Makes lava shimmer.
public class LavaGlow : MonoBehaviour
{
    SpriteRenderer sr;
    float seed;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        seed = Random.value * 10f;
    }

    void Update()
    {
        sr.color = Color.Lerp(Palette.Lava, Palette.LavaHot, 0.5f + 0.5f * Mathf.Sin(Time.time * 4f + seed));
        if (Random.value < 0.03f)
            FX.Burst((Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f) * transform.localScale.x, 0.1f),
                Palette.LavaHot, 1, 0.1f, 2f);
    }
}
