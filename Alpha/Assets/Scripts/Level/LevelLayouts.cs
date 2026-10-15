using UnityEngine;

// What sits on each row, for each level. One method per level.
// A match is best of 3, so the three races are played on these three levels in order.
//
// Rules of thumb for editing:
//   - Keep the ends of each row clear (|x| > 10): that's where players land and jump up.
//   - Hazards no wider than 2, so they can always be jumped.
//   - Box(row, x, chaserPrize, leaderPrize): chaser gets attacks/catch-up, leader gets defence.
//   - Levers sit on a small ledge (you have to jump up to pull them) and only work for the CHASER.
//     Never put a ledge above or right next to a hazard: you bump your head mid-jump and fall in.
//     A lever and its trap share a letter and a color.
//
// Coordinates:
//   - row is 0-5 in code, but "Row 1-6" in comments, the HUD and the GDD (row 0 = Row 1).
//   - x runs from -15.5 (left wall) to +15.5 (right wall); 0 is the middle.
//   - Even rows (Row 1, 3, 5) run right, odd rows (Row 2, 4, 6) run left.
// The full row-by-row list for every level is in Documentation/alpha/level_design.md.
public static partial class LevelBuilder
{
    public static readonly string[] Names = { "Classic Climb", "Lever Lab", "The Gauntlet" };

    const float FlagX = -14f;   // the flag stands at the far left end of Row 6 in every level

    static readonly System.Action[] Layouts = { ClassicClimb, LeverLab, Gauntlet };

    public static int LevelCount => Layouts.Length;

    // ---------------- Level 1: the paired-prototype layout, plus one lever ----------------
    static void ClassicClimb()
    {
        // Row 1 (run right)
        Spikes(0, -5f, 1f);
        Lava(0, 3f, 2f);
        Box(0, -1f, PowerUpType.Fist, PowerUpType.Shield);
        Box(0, 8f, PowerUpType.Rocket, PowerUpType.Banana);

        // Row 2 (run left)
        WaterPool(1, 2f, 6f);
        Saw(1, -7.5f, -3f, 0.4f, 1.1f);
        Box(1, 7.5f, PowerUpType.FreezeRay, PowerUpType.Banana);
        Box(1, -9.5f, PowerUpType.SuperJump, PowerUpType.Shield);
        LeverTo(1, 9.5f, "A", GateTrap(2, 4f, "A"));   // A: Row 2 ledge -> gate on Row 3

        // Row 3 (run right)
        Lava(2, -5f, 2f);
        Spikes(2, 1.5f, 0.8f);
        Spikes(2, 6.5f, 0.8f);
        Box(2, -1.5f, PowerUpType.GravityBomb, PowerUpType.Shield);
        Box(2, 9f, PowerUpType.Fist, PowerUpType.Banana);

        // Row 4 (run left)
        Saw(3, 1f, 7f, 0.4f, 0.9f);
        WaterPool(3, -5.5f, 5f);
        Box(3, 8.5f, PowerUpType.SuperJump, PowerUpType.Shield);
        Box(3, -1.5f, PowerUpType.Reverse, PowerUpType.Banana);

        // Row 5 (run right)
        Spikes(4, -5f, 1f);
        Lava(4, 1f, 1.8f);
        Saw(4, 5.5f, 8.5f, 0.4f, 1.0f);
        Box(4, -8.5f, PowerUpType.FreezeRay, PowerUpType.Shield);
        Box(4, 4f, PowerUpType.Rocket, PowerUpType.Banana);

        // Row 6 (run left) -> FLAG
        Lava(5, 4f, 2f);
        Spikes(5, -2f, 1f);
        Saw(5, -9f, -6f, 0.4f, 1.2f);
        Box(5, 8.5f, PowerUpType.Swap, PowerUpType.Shield);
        Flag.Create(new Vector2(FlagX, FloorTop(5)), level);
    }

    // ---------------- Level 2: fewer hazards, a lever on every row ----------------
    static void LeverLab()
    {
        // Row 1 (run right)
        Lava(0, 3f, 1.5f);
        Box(0, -2f, PowerUpType.Fist, PowerUpType.Shield);
        LeverTo(0, 7.5f, "A", SpikeTrap(1, 0f, 2f, "A"));   // A: Row 1 ledge -> spikes on Row 2

        // Row 2 (run left)
        Saw(1, 3f, 7f, 0.4f, 1.0f);
        Box(1, -5f, PowerUpType.Ghost, PowerUpType.FakeBox);
        LeverTo(1, -8.5f, "B", GateTrap(2, 2f, "B"));   // B: Row 2 ledge -> gate on Row 3

        // Row 3 (run right)
        Lava(2, -4f, 1.5f);
        Box(2, 6f, PowerUpType.BrickWall, PowerUpType.Shield);
        LeverTo(2, 8.5f, "C", SpikeTrap(3, -2f, 2f, "C"));   // C: Row 3 ledge -> spikes on Row 4

        // Row 4 (run left)
        WaterPool(3, 5f, 4f);
        Box(3, 8.5f, PowerUpType.FreezeRay, PowerUpType.Banana);
        Box(3, -5.5f, PowerUpType.Rocket, PowerUpType.Shield);
        LeverTo(3, -8.5f, "D", GateTrap(4, 0f, "D"));   // D: Row 4 ledge -> gate on Row 5

        // Row 5 (run right)
        Spikes(4, -5f, 1f);
        Saw(4, 3.5f, 6.5f, 0.4f, 1.0f);
        Box(4, -8f, PowerUpType.GravityBomb, PowerUpType.FakeBox);
        LeverTo(4, 9f, "E", SpikeTrap(5, 1f, 2f, "E"));   // E: Row 5 ledge -> spikes on Row 6

        // Row 6 (run left) -> FLAG
        Lava(5, 5f, 1.5f);
        Saw(5, -9f, -6f, 0.4f, 1.2f);
        Box(5, 8.5f, PowerUpType.Swap, PowerUpType.Shield);
        Flag.Create(new Vector2(FlagX, FloorTop(5)), level);
    }

    // ---------------- Level 3: everything at once ----------------
    static void Gauntlet()
    {
        // Row 1 (run right)
        Spikes(0, -6f, 1f);
        Lava(0, -1f, 1.5f);
        Saw(0, 3f, 7f, 0.4f, 1.0f);
        Box(0, 1.5f, PowerUpType.Fist, PowerUpType.Banana);
        Box(0, 9f, PowerUpType.Rocket, PowerUpType.Shield);
        LeverTo(0, -9.5f, "A", SpikeTrap(1, -3f, 2f, "A"));   // A: Row 1 ledge -> spikes on Row 2

        // Row 2 (run left)
        WaterPool(1, 4f, 4f);
        Lava(1, -4.5f, 1.5f);
        Box(1, 8f, PowerUpType.FreezeRay, PowerUpType.FakeBox);
        Box(1, 0f, PowerUpType.GravityBomb, PowerUpType.Shield);
        LeverTo(1, -9.5f, "B", GateTrap(2, 5f, "B"));   // B: Row 2 ledge -> gate on Row 3

        // Row 3 (run right)
        Spikes(2, -4f, 1f);
        Spikes(2, 1f, 1f);
        Saw(2, 7f, 9.5f, 0.4f, 1.0f);
        Box(2, -1.5f, PowerUpType.Ghost, PowerUpType.Banana);
        LeverTo(2, -9f, "C", GateTrap(3, -4f, "C"));   // C: Row 3 ledge -> gate on Row 4

        // Row 4 (run left)
        Lava(3, 3f, 1.5f);
        Saw(3, -9f, -6.5f, 0.4f, 1.0f);
        Box(3, 7f, PowerUpType.BrickWall, PowerUpType.Shield);
        Box(3, -1f, PowerUpType.Reverse, PowerUpType.FakeBox);
        LeverTo(3, 9.5f, "D", SpikeTrap(4, 3f, 2f, "D"));   // D: Row 4 ledge -> spikes on Row 5

        // Row 5 (run right)
        Lava(4, -4f, 1.5f);
        Spikes(4, 8f, 1f);
        Box(4, -8f, PowerUpType.SuperJump, PowerUpType.Shield);
        Box(4, 0f, PowerUpType.Fist, PowerUpType.Banana);
        LeverTo(4, -9.5f, "E", GateTrap(5, 0f, "E"));   // E: Row 5 ledge -> gate on Row 6

        // Row 6 (run left) -> FLAG
        Spikes(5, 4f, 1f);
        Lava(5, -4f, 1.5f);
        Saw(5, -10f, -7f, 0.4f, 1.2f);
        Box(5, 8.5f, PowerUpType.Swap, PowerUpType.Shield);
        Flag.Create(new Vector2(FlagX, FloorTop(5)), level);
    }

    // ---------------- lever + trap helpers ----------------

    static Trap GateTrap(int row, float x, string tag) =>
        Trap.Create(Trap.Kind.Gate, new Vector2(x, FloorTop(row)), 0.5f, tag, level);

    static Trap SpikeTrap(int row, float x, float width, string tag) =>
        Trap.Create(Trap.Kind.Spikes, new Vector2(x, FloorTop(row)), width, tag, level);

    static void LeverTo(int row, float x, string tag, params Trap[] traps) =>
        Lever.Create(new Vector2(x, FloorTop(row)), tag, traps, level);
}
