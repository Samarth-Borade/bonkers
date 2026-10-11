using UnityEngine;
using UnityEngine.SceneManagement;

// Runs the match: Title -> 3,2,1 -> Race -> someone touches the flag -> next race.
// A match is best of 3: first to win 2 races wins. Each race is on the next level.
// The whole level is built from code when the scene starts (see LevelBuilder).
public class GameManager : MonoBehaviour
{
    public enum State { Title, Countdown, Race, Won }

    public static GameManager I { get; private set; }

    public State Phase { get; private set; } = State.Title;
    public PlayerController Red { get; private set; }
    public PlayerController Yellow { get; private set; }
    public PlayerController Winner { get; private set; }
    public float CountdownLeft { get; private set; }
    public float RaceTime { get; private set; }

    public const int WinsNeeded = 2;

    // Static so the match survives the scene reload between races.
    public static int RedWins { get; private set; }
    public static int YellowWins { get; private set; }
    public static int Round { get; private set; }          // races played so far in this match
    public static int Level => Round % LevelBuilder.LevelCount;
    public bool MatchOver => RedWins >= WinsNeeded || YellowWins >= WinsNeeded;

    static bool skipTitle, argsRead;

    const float CountdownLength = 3f;
    int lastBeep;
    float wonAt;

    void Awake()
    {
        I = this;
        Application.targetFrameRate = 120;
        ReadLevelArg();
        EnsureCamera();

        LevelBuilder.Current = Level;
        var players = LevelBuilder.Build(transform);
        Red = players.red;
        Yellow = players.yellow;

        if (!GetComponent<Hud>()) gameObject.AddComponent<Hud>();
        DevScreenshots.AttachIfRequested(gameObject);
        DevBot.AttachIfRequested(gameObject);
    }

    void Start()
    {
        if (skipTitle) StartCountdown();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { ResetMatch(); skipTitle = false; Reload(); return; }

        switch (Phase)
        {
            case State.Title:
                if (StartPressed()) StartCountdown();
                break;

            case State.Countdown:
                CountdownLeft -= Time.deltaTime;
                int n = Mathf.CeilToInt(CountdownLeft);
                if (n != lastBeep && n > 0) { lastBeep = n; Sfx.Play(Sfx.Beep); }
                if (CountdownLeft <= 0) BeginRace();
                break;

            case State.Race:
                RaceTime += Time.deltaTime;
                RaceAnalytics.TrackLeader(IsLeading(Red) ? 1 : IsLeading(Yellow) ? 2 : 0);
                if (Input.GetKeyDown(KeyCode.R)) { skipTitle = true; Reload(); } // replay this race
                break;

            case State.Won:
                if (Time.time - wonAt > 1.2f && StartPressed())
                {
                    if (MatchOver) ResetMatch();
                    else Round++;
                    skipTitle = true;
                    Reload();
                }
                break;
        }
    }

    // Ahead by more than a little bit = leading. Neck and neck counts as both chasing.
    const float LeadMargin = 0.1f;

    public bool IsLeading(PlayerController p) =>
        LevelBuilder.Progress(p) - LevelBuilder.Progress(p.opponent) > LeadMargin;

    public void StartCountdown()
    {
        Phase = State.Countdown;
        CountdownLeft = CountdownLength;
        lastBeep = 0;
        SetControls(false);
    }

    void BeginRace()
    {
        Phase = State.Race;
        RaceTime = 0;
        RaceAnalytics.BeginRace();
        SetControls(true);
        FX.Pop("GO!", new Vector2(0, 1), Palette.Go, 3f, 1f);
        Sfx.Play(Sfx.Go);
    }

    public void Win(PlayerController p)
    {
        if (Phase != State.Race) return;
        Phase = State.Won;
        Winner = p;
        wonAt = Time.time;
        if (p == Red) RedWins++; else YellowWins++;
        SetControls(false);
        Sfx.Play(Sfx.Win);

        float gap = LevelBuilder.Rows - LevelBuilder.Progress(p.opponent);
        RaceAnalytics.EndRace(this, Level, Round, p.displayName, RaceTime, gap);
    }

    static void ResetMatch()
    {
        RedWins = YellowWins = 0;
        Round = 0;
    }

    void SetControls(bool on)
    {
        Red.ControlsEnabled = on;
        Yellow.ControlsEnabled = on;
    }

    static bool StartPressed() =>
        Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

    static void Reload() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    // Testing: "-bonkersLevel 3" starts on level 3.
    static void ReadLevelArg()
    {
        if (argsRead) return;
        argsRead = true;
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-bonkersLevel" && int.TryParse(args[i + 1], out int lv))
                Round = Mathf.Clamp(lv - 1, 0, LevelBuilder.LevelCount - 1);
    }

    static void EnsureCamera()
    {
        var cam = Camera.main;
        if (!cam)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        if (!cam.GetComponent<CameraRig>()) cam.gameObject.AddComponent<CameraRig>();
    }
}
