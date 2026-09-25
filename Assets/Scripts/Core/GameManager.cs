using UnityEngine;
using UnityEngine.SceneManagement;

// Runs the match: Title -> 3,2,1 Countdown -> Race -> Someone wins -> play again.
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

    // Static so the score survives the scene reload between rounds.
    public static int RedWins { get; private set; }
    public static int YellowWins { get; private set; }
    static bool skipTitle;

    const float CountdownLength = 3f;
    int lastBeep;
    float wonAt;

    void Awake()
    {
        I = this;
        Application.targetFrameRate = 120;
        EnsureCamera();

        var players = LevelBuilder.Build(transform);
        Red = players.red;
        Yellow = players.yellow;

        if (!GetComponent<Hud>()) gameObject.AddComponent<Hud>();
        DevScreenshots.AttachIfRequested(gameObject);
    }

    void Start()
    {
        if (skipTitle) StartCountdown();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { skipTitle = false; Reload(); return; }

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
                if (Input.GetKeyDown(KeyCode.R)) { skipTitle = true; Reload(); }
                break;

            case State.Won:
                if (Time.time - wonAt > 1.2f && (StartPressed() || Input.GetKeyDown(KeyCode.R)))
                {
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
        FX.Confetti(p.transform.position);
        CameraRig.Shake(0.3f, 0.4f);
        Sfx.Play(Sfx.Win);
    }

    void SetControls(bool on)
    {
        Red.ControlsEnabled = on;
        Yellow.ControlsEnabled = on;
    }

    static bool StartPressed() =>
        Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

    static void Reload() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

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
