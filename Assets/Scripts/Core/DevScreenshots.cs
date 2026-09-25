using System.Collections;
using UnityEngine;

// Test helper, not part of the game. Launch the built game with
//   -bonkersShot /some/folder/shot_
// and it stages a few moments (title, countdown, a BONK, a win), saves screenshots and quits.
public class DevScreenshots : MonoBehaviour
{
    string prefix;

    public static void AttachIfRequested(GameObject host)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-bonkersShot")
                host.AddComponent<DevScreenshots>().prefix = args[i + 1];
    }

    IEnumerator Start()
    {
        var gm = GameManager.I;
        yield return new WaitForSeconds(1.5f);
        Shot("1_title");
        yield return null;

        gm.StartCountdown();
        yield return new WaitForSeconds(1.3f);
        Shot("2_countdown");

        yield return new WaitForSeconds(2.2f); // race has started
        float y = LevelBuilder.FloorTop(0) + 0.5f;
        gm.Red.TeleportTo(new Vector2(-3f, y), 0);
        gm.Yellow.TeleportTo(new Vector2(0.5f, y), 0);
        gm.Red.FaceTowards(0.5f);
        gm.Red.Held = PowerUpType.Fist;
        gm.Yellow.Held = PowerUpType.Banana;
        yield return new WaitForSeconds(0.3f);
        PowerUps.Use(gm.Red);
        yield return new WaitForSeconds(0.35f);
        Shot("3_bonk");

        yield return new WaitForSeconds(1.5f);
        // Yellow drops a Gravity Bomb on Red: grab the warning beam.
        gm.Red.TeleportTo(new Vector2(6.5f, y), 0);
        gm.Yellow.Held = PowerUpType.GravityBomb;
        PowerUps.Use(gm.Yellow);
        yield return new WaitForSeconds(0.7f);
        Shot("4_bomb_warning");

        yield return new WaitForSeconds(1.2f);
        // Red walks up to a box: it shows what Red would get.
        gm.Red.TeleportTo(new Vector2(5.8f, LevelBuilder.FloorTop(0) + 0.5f), 0);
        gm.Yellow.shieldT = 5f;
        yield return new WaitForSeconds(0.4f);
        Shot("5_box_and_rank");

        gm.Yellow.TeleportTo(new Vector2(-13.5f, LevelBuilder.FloorTop(5) + 0.6f), 5);
        yield return new WaitForSeconds(1.2f);
        Shot("6_win");

        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    void Shot(string name) => ScreenCapture.CaptureScreenshot(prefix + name + ".png");
}
