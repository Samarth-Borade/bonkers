using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor helpers under the "Bonkers" menu.
// The Main scene only holds a camera and the GameManager; everything else is built by code at Play.
public static class BonkersSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("Bonkers/Rebuild Main Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 10.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Background;
        camGo.transform.position = new Vector3(0, 1f, -10);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CameraRig>();

        new GameObject("GameManager").AddComponent<GameManager>();

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.productName = "Bonkers";
        PlayerSettings.companyName = "Team Bonkers";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        Debug.Log("Bonkers: Main scene built at " + ScenePath);
    }

    [MenuItem("Bonkers/Build Game (Mac)")]
    public static void BuildMac() => Build(BuildTarget.StandaloneOSX, "Builds/Mac/Bonkers.app");

    [MenuItem("Bonkers/Build Game (Windows)")]
    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Bonkers.exe");

    // ---------------- WebGL build for this milestone ----------------
    // This folder is the ALPHA project. Every milestone is its own Unity project
    // (Alpha/, Beta/, Gold/) and builds into the shared docs/ folder at the repo root,
    // which GitHub Pages serves:
    //   docs/              the paired prototype, as submitted. Never rebuild it.
    //   docs/alpha-check/  https://samarth-borade.github.io/bonkers/alpha-check/
    //   docs/alpha/        .../alpha/
    //   docs/beta/         .../beta/
    //   docs/gold/         .../gold/
    // The course wants a separate link per submission, so a build is never overwritten
    // without asking.

    [MenuItem("Bonkers/Build WebGL/Alpha Progress Check")]
    public static void BuildAlphaCheck() => BuildWebGL("alpha-check");

    [MenuItem("Bonkers/Build WebGL/Alpha")]
    public static void BuildAlpha() => BuildWebGL("alpha");

    // From the command line: -executeMethod BonkersSetup.BuildWebGLFromArgs -milestone <name>
    public static void BuildWebGLFromArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-milestone") { BuildWebGL(args[i + 1]); return; }
        Debug.LogError("Pass -milestone <name>");
        EditorApplication.Exit(1);
    }

    static void BuildWebGL(string milestone)
    {
        string path = "../docs/" + milestone;
        if (Directory.Exists(path) && !Application.isBatchMode &&
            !EditorUtility.DisplayDialog("Overwrite build?",
                $"{path} already has a build. The course wants each submission to keep its own link.\n\nOverwrite it?",
                "Overwrite", "Cancel"))
            return;

        PlayerSettings.bundleVersion = milestone;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 540;
        Build(BuildTarget.WebGL, path);
        File.WriteAllText("../docs/.nojekyll", "");
    }

    static void Build(BuildTarget target, string path)
    {
        if (!File.Exists(ScenePath)) BuildScene();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = path,
            target = target,
        });
        Debug.Log("Bonkers build: " + report.summary.result + " -> " + path);
        if (Application.isBatchMode && report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // When someone opens the project, jump straight to the Main scene.
    [InitializeOnLoadMethod]
    static void OpenMainOnFirstLoad()
    {
        if (Application.isBatchMode || SessionState.GetBool("Bonkers.Opened", false)) return;
        SessionState.SetBool("Bonkers.Opened", true);
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath)) BuildScene();
            else if (EditorSceneManager.GetActiveScene().path != ScenePath) EditorSceneManager.OpenScene(ScenePath);
        };
    }

    // The level is built by code at Play, so the Scene view is empty while editing.
    // This builds a look-only copy so you can see the layout without pressing Play.
    // It is never saved, and it is removed automatically when you press Play.
    const string PreviewName = "Level Preview (not saved)";

    [MenuItem("Bonkers/Show Level in Scene View")]
    public static void ShowLevelPreview()
    {
        HideLevelPreview();
        var root = new GameObject(PreviewName);
        LevelBuilder.Build(root.transform);
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.hideFlags = HideFlags.DontSave;

        var view = SceneView.lastActiveSceneView;
        if (view)
        {
            view.in2DMode = true;
            view.LookAt(new Vector3(0, 1, 0), Quaternion.identity, 11f);
        }
    }

    [MenuItem("Bonkers/Hide Level Preview")]
    public static void HideLevelPreview()
    {
        var old = GameObject.Find(PreviewName);
        if (old) Object.DestroyImmediate(old);
    }

    [InitializeOnLoadMethod]
    static void HidePreviewOnPlay()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode) HideLevelPreview();
        };
    }
}
