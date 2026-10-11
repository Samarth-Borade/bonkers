using System.Collections;
using UnityEngine;

// Test helper, not part of the game. Launch the built game with
//   -bonkersShot /some/folder/shot_            stages key moments, saves screenshots, quits
//   -bonkersShot /some/folder/shot_ -bonkersLevelShot   just one picture of the level, then quits
public class DevScreenshots : MonoBehaviour
{
    string prefix;
    bool levelOnly;

    public static void AttachIfRequested(GameObject host)
    {
        var args = System.Environment.GetCommandLineArgs();
        string prefix = null;
        bool levelOnly = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-bonkersShot" && i + 1 < args.Length) prefix = args[i + 1];
            if (args[i] == "-bonkersLevelShot") levelOnly = true;
        }
        if (prefix == null) return;
        var d = host.AddComponent<DevScreenshots>();
        d.prefix = prefix;
        d.levelOnly = levelOnly;
    }

    IEnumerator Start()
    {
        var gm = GameManager.I;
        yield return new WaitForSeconds(1.5f);

        if (levelOnly)
        {
            gm.StartCountdown();
            yield return new WaitForSeconds(3.6f);
            Shot("level" + (GameManager.Level + 1));
            yield return new WaitForSeconds(0.3f);
            Application.Quit();
            yield break;
        }

        Shot("1_title");
        yield return null;
        gm.StartCountdown();
        yield return new WaitForSeconds(3.6f);

        var red = gm.Red;
        var yellow = gm.Yellow;
        float y0 = LevelBuilder.FloorTop(0) + 0.5f;

        // A punch lands.
        red.TeleportTo(new Vector2(-3f, y0), 0);
        yellow.TeleportTo(new Vector2(0.5f, y0), 0);
        red.FaceTowards(0.5f);
        red.Held = PowerUpType.Fist;
        yield return new WaitForSeconds(0.3f);
        PowerUps.Use(red);
        yield return new WaitForSeconds(0.35f);
        Shot("2_bonk");

        // Red pulls ahead to row 3; Yellow (chasing) jumps onto lever A on row 2.
        yield return new WaitForSeconds(1.5f);
        red.TeleportTo(new Vector2(2.5f, LevelBuilder.FloorTop(2) + 0.5f), 2);
        yellow.TeleportTo(new Vector2(9.5f, LevelBuilder.FloorTop(1) + 1.7f), 1);
        yield return new WaitForSeconds(1.0f);
        Shot("3_lever_trap");

        // Yellow fires a Brick Wall at Red and turns Ghost; Red drops a Fake Box.
        yield return new WaitForSeconds(0.5f);
        yellow.Held = PowerUpType.BrickWall;
        PowerUps.Use(yellow);
        red.Held = PowerUpType.FakeBox;
        PowerUps.Use(red);
        yield return new WaitForSeconds(0.9f);
        Shot("4_brick_wall");

        // Drop bomb warning.
        yield return new WaitForSeconds(2.5f);
        yellow.Held = PowerUpType.GravityBomb;
        PowerUps.Use(yellow);
        yield return new WaitForSeconds(0.7f);
        Shot("5_bomb_warning");

        // Yellow wins the race.
        yield return new WaitForSeconds(1.5f);
        yellow.TeleportTo(new Vector2(-13.5f, LevelBuilder.FloorTop(5) + 0.6f), 5);
        yield return new WaitForSeconds(1.2f);
        Shot("6_race_won");

        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    void Shot(string name) => ScreenCapture.CaptureScreenshot(prefix + name + ".png");
}
