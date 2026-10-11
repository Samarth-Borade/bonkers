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
// The walls, floors and gaps are the same in every level. What goes on each row
// (hazards, boxes, levers) lives in LevelLayouts.cs, one method per level.
public static partial class LevelBuilder
{
    public const int Rows = 6;
    public const float Bottom = -8f;        // top of the ground
    public const float RowHeight = 3f;
    public const float HalfWidth = 15.5f;    // inner edge of the side walls
    public const float GapWidth = 3f;
    public const float FloorThickness = 0.4f;
    public const float CeilingY = 9.6f;     // same headroom on the top row as every other row

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

    // Which level to build next (0, 1, 2). The GameManager sets this between races.
    public static int Current;

    public static (PlayerController red, PlayerController yellow) Build(Transform root)
    {
        Floors.Clear();
        level = new GameObject("Level").transform;
        level.SetParent(root, false);

        BuildShell();
        Layouts[Mathf.Clamp(Current, 0, Layouts.Length - 1)]();

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
        int n = Mathf.Max(1, Mathf.RoundToInt(width / 0.35f));
        float step = width / n;
        for (int i = 0; i < n; i++)
            Shapes.Make("Spike", Shapes.Triangle, new Vector2(-width / 2f + step * (i + 0.5f), 0.15f),
                new Vector2(step, 0.3f), Palette.Spikes, 3, go.transform);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(0, 0.12f);
        col.size = new Vector2(width * 0.9f, 0.22f);
        go.AddComponent<Hazard>().word = "SPIKED!";
    }

    static void Lava(int row, float x, float width)
    {
        float top = FloorTop(row);
        var sr = Shapes.Box("Lava", new Vector2(x, top + 0.15f), new Vector2(width, 0.3f), Palette.Lava, 3, level);
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        sr.gameObject.AddComponent<Hazard>().word = "TOASTED!";
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

    // Every box has two fixed prizes: one for whoever is CHASING, one for whoever is LEADING.
    static void Box(int row, float x, PowerUpType chaser, PowerUpType leader) =>
        PowerUpBox.Create(new Vector2(x, FloorTop(row) + 0.75f), chaser, leader, level);
}
