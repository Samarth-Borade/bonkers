using System.Collections.Generic;
using UnityEngine;

// Test helper, not part of the game. Launch the built game with  -bonkersBot
// and a simple bot plays Red through the whole level: run, jump over every hazard,
// jump up through each gap. It logs how many times it died and whether it reached the flag.
// If a dumb bot can finish, a person can too.
public class DevBot : MonoBehaviour
{
    PlayerController p;
    bool pressJump, holdJump;
    float holdUntil, started;
    readonly List<Collider2D> hazards = new List<Collider2D>();
    int lastRow = -1, lastDeaths;

    public static void AttachIfRequested(GameObject host)
    {
        foreach (var a in System.Environment.GetCommandLineArgs())
            if (a == "-bonkersBot") host.AddComponent<DevBot>();
    }

    void Start()
    {
        var gm = GameManager.I;
        p = gm.Red;
        foreach (var h in FindObjectsByType<Hazard>(FindObjectsSortMode.None)) hazards.Add(h.GetComponent<Collider2D>());

        p.testHeld = k => (k == p.leftKey && move < 0) || (k == p.rightKey && move > 0) || (k == p.jumpKey && holdJump);
        p.testDown = k => { if (k == p.jumpKey && pressJump) { pressJump = false; return true; } return false; };
        gm.Yellow.testHeld = k => false;
        gm.Yellow.testDown = k => false;

        gm.StartCountdown();
        started = Time.time;
    }

    float move;

    void Update()
    {
        var gm = GameManager.I;
        if (gm.Phase == GameManager.State.Won)
        {
            Debug.Log($"BOT RESULT: FINISHED in {gm.RaceTime:0.0}s with {p.Deaths} deaths");
            Application.Quit();
            enabled = false;
            return;
        }
        if (Time.time - started > 120f)
        {
            Debug.Log($"BOT RESULT: STUCK on row {p.Row + 1} at x={p.transform.position.x:0.0}, deaths {p.Deaths}");
            Application.Quit();
            enabled = false;
            return;
        }
        if (gm.Phase != GameManager.State.Race) return;

        if (p.Deaths != lastDeaths) { lastDeaths = p.Deaths; Debug.Log("BOT: died: " + p.LastDeath); }
        if (p.Row != lastRow) { Debug.Log($"BOT: reached row {p.Row + 1} at {gm.RaceTime:0.0}s, deaths so far {p.Deaths}"); lastRow = p.Row; }

        int row = p.Row;
        int dir = row % 2 == 0 ? 1 : -1;
        Vector2 pos = p.transform.position;
        bool grounded = Mathf.Abs(p.Velocity.y) < 0.05f;

        if (Time.time > holdUntil) holdJump = false;

        // End of the row: climb up through the gap above.
        if (row < LevelBuilder.Rows - 1 && dir * pos.x > 12.7f)
        {
            float gapX = dir * 14f;
            bool aboveNextFloor = pos.y - PlayerController.Size.y / 2f > LevelBuilder.FloorTop(row + 1) + 0.05f;
            if (aboveNextFloor) move = -dir; // drift onto the next floor
            else
            {
                move = Mathf.Abs(gapX - pos.x) > 0.3f ? Mathf.Sign(gapX - pos.x) : 0f;
                if (grounded) Jump(0.5f);
            }
            return;
        }

        move = dir;

        // Something deadly just ahead on this row? Jump.
        if (!grounded) return;
        float front = pos.x + dir * PlayerController.Size.x / 2f;
        foreach (var h in hazards)
        {
            var b = h.bounds;
            if (LevelBuilder.RowFromY(b.min.y + 0.05f) != row) continue;
            float gap = dir > 0 ? b.min.x - front : front - b.max.x;
            if (gap > 0f && gap < 0.7f) { Jump(0.35f); break; }
        }
    }

    void Jump(float hold)
    {
        pressJump = true;
        holdJump = true;
        holdUntil = Time.time + hold;
    }
}
