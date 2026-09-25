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

    // Output goes to /docs so GitHub Pages can serve it straight from the main branch.
    [MenuItem("Bonkers/Build Game (WebGL for GitHub Pages)")]
    public static void BuildWebGL()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 540;
        Build(BuildTarget.WebGL, "docs");
        File.WriteAllText("docs/.nojekyll", "");
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
}
