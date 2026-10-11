using UnityEngine;

public enum PowerUpType
{
    None,
    Fist,        // punch flies forward: BONK + slow (jump over it to dodge)
    FreezeRay,   // shoots ice forward: opponent frozen solid
    Banana,      // drops a peel behind you: whoever steps on it spins out
    Swap,        // swap places with your opponent (great when you're losing!)
    GravityBomb, // bomb drops on opponent after a warning: heavy, tiny jumps
    Reverse,     // bomb drops on opponent after a warning: left/right swapped
    SuperJump,   // fly up THROUGH the floors
    Rocket,      // run super fast
    Shield,      // blocks the next attack
    // --- added in Alpha ---
    BrickWall,   // a wall pops up in front of the opponent (after a warning): jump it or lose time
    Ghost,       // for 3s lava, spikes and saws can't hurt you
    FakeBox,     // leader drops a fake ? box behind them: the chaser who grabs it gets BONKED
}

// What every power-up does. To add one: add it to the enum, give it a name/color below,
// add a case in Use(), and put it in a box in LevelBuilder.
public static class PowerUps
{
    public static string NameOf(PowerUpType t) => t switch
    {
        PowerUpType.Fist => "BONK FIST",
        PowerUpType.FreezeRay => "FREEZE RAY",
        PowerUpType.Banana => "BANANA PEEL",
        PowerUpType.Swap => "SWAPAROO",
        PowerUpType.GravityBomb => "GRAVITY BOMB",
        PowerUpType.Reverse => "BRAIN SCRAMBLE",
        PowerUpType.SuperJump => "SUPER JUMP",
        PowerUpType.Rocket => "ROCKET SHOES",
        PowerUpType.Shield => "BUBBLE SHIELD",
        PowerUpType.BrickWall => "BRICK WALL",
        PowerUpType.Ghost => "GHOST",
        PowerUpType.FakeBox => "FAKE BOX",
        _ => "-",
    };

    public static string HintOf(PowerUpType t) => t switch
    {
        PowerUpType.Fist => "punch forward",
        PowerUpType.FreezeRay => "freeze them",
        PowerUpType.Banana => "drop behind you",
        PowerUpType.Swap => "swap places",
        PowerUpType.GravityBomb => "drop on them: heavy",
        PowerUpType.Reverse => "drop on them: confused",
        PowerUpType.SuperJump => "jump up to their row",
        PowerUpType.Rocket => "go fast",
        PowerUpType.Shield => "block one hit",
        PowerUpType.BrickWall => "wall in front of them",
        PowerUpType.Ghost => "hazards can't hurt you",
        PowerUpType.FakeBox => "drop a trap box",
        _ => "",
    };

    public static Color ColorOf(PowerUpType t) => t switch
    {
        PowerUpType.Fist => new Color(1f, 0.55f, 0.2f),
        PowerUpType.FreezeRay => new Color(0.55f, 0.9f, 1f),
        PowerUpType.Banana => new Color(1f, 0.93f, 0.35f),
        PowerUpType.Swap => new Color(0.9f, 0.4f, 1f),
        PowerUpType.GravityBomb => Palette.Heavy,
        PowerUpType.Reverse => Palette.Reverse,
        PowerUpType.SuperJump => new Color(1f, 0.45f, 0.75f),
        PowerUpType.Rocket => new Color(1f, 0.3f, 0.3f),
        PowerUpType.Shield => new Color(0.4f, 0.95f, 1f),
        PowerUpType.BrickWall => new Color(0.85f, 0.45f, 0.3f),
        PowerUpType.Ghost => new Color(0.85f, 0.85f, 1f),
        PowerUpType.FakeBox => new Color(0.95f, 0.95f, 0.95f),
        _ => Color.white,
    };

    public static void Use(PlayerController p)
    {
        var type = p.Held;
        if (type == PowerUpType.None) return;
        p.Held = PowerUpType.None;
        var foe = p.opponent;
        RaceAnalytics.PowerUpUsed(p, type);
        Sfx.Play(Sfx.Use, 0.6f);

        switch (type)
        {
            case PowerUpType.Fist:
                Projectile.Fire(p, ColorOf(type), 15f, false, target => { if (target.Bonk(p, 2.5f, "PUNCHED!")) RaceAnalytics.Hit(p, type); });
                break;

            case PowerUpType.FreezeRay:
                Projectile.Fire(p, ColorOf(type), 12f, true, target =>
                {
                    if (target.Bonk(p, 0f, "FROZEN!")) { target.freezeT = 1.8f; RaceAnalytics.Hit(p, type); }
                });
                break;

            case PowerUpType.Banana:
                Banana.Drop(p);
                break;

            case PowerUpType.Swap:
                if (foe.Blocks()) break;
                Vector2 a = p.transform.position, b = foe.transform.position;
                int rowA = p.Row, rowB = foe.Row;
                p.TeleportTo(b, rowB);
                foe.TeleportTo(a, rowA);
                RaceAnalytics.Hit(p, type);
                FX.Pop("SWAP!", p.Head, ColorOf(type), 1f);
                FX.Pop("SWAP!", foe.Head, ColorOf(type), 1f);
                Sfx.Play(Sfx.Teleport);
                break;

            case PowerUpType.GravityBomb:
                DropStrike.Launch(p, foe, ColorOf(type), target =>
                {
                    if (target.Bonk(p, 0.5f, "HEAVY!")) { target.heavyT = 4f; RaceAnalytics.Hit(p, type); }
                });
                break;

            case PowerUpType.Reverse:
                DropStrike.Launch(p, foe, ColorOf(type), target =>
                {
                    if (target.Bonk(p, 0.5f, "CONFUSED!")) { target.reverseT = 4f; RaceAnalytics.Hit(p, type); }
                });
                break;

            case PowerUpType.SuperJump:
                p.superJumpT = 6f;
                FX.Pop("BOING!", p.Head, ColorOf(type), 0.9f);
                break;

            case PowerUpType.Rocket:
                p.speedT = 4f;
                FX.Pop("ZOOM!", p.Head, ColorOf(type), 0.9f);
                break;

            case PowerUpType.Shield:
                p.shieldT = 10f;
                FX.Pop("SHIELD!", p.Head, ColorOf(type), 0.9f);
                break;

            case PowerUpType.BrickWall:
            {
                if (foe.Blocks()) break;
                // 3.5 units ahead of the opponent, in the direction their row runs.
                int dir = foe.Row % 2 == 0 ? 1 : -1;
                float x = Mathf.Clamp(foe.transform.position.x + dir * 3.5f, -10f, 10f);
                Trap.PopUpOnce(new Vector2(x, LevelBuilder.FloorTop(foe.Row)), 0.5f);
                FX.Pop("WALL!", foe.Head, ColorOf(type), 0.8f);
                RaceAnalytics.Hit(p, type);
                break;
            }

            case PowerUpType.Ghost:
                p.ghostT = 3f;
                FX.Pop("GHOST!", p.Head, ColorOf(type), 0.9f);
                break;

            case PowerUpType.FakeBox:
                FakeBox.Drop(p);
                break;
        }
    }
}
