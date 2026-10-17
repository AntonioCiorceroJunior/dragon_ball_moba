#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DragonMoba.Editor
{
    public static class ProjectValidation
    {
        public const string ScenePath = "Assets/Scenes/ArenaPrototype.unity";

        [MenuItem("Dragon MOBA/Preparar e validar projeto")]
        public static void Prepare()
        {
            PlayerSettings.productName = "Dragon MOBA Prototype";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            // Keep the existing UnityEngine.Input controls without adding Input System.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input == null) throw new InvalidOperationException("Active Input Handling setting unavailable.");
            input.intValue = 0;
            settings.ApplyModifiedPropertiesWithoutUndo();
            Directory.CreateDirectory("Assets/Scenes");
            if (!File.Exists(ScenePath)) CreateArenaScene.Create();
            else EditorSceneManager.OpenScene(ScenePath);
            var others = EditorBuildSettings.scenes.Where(s => s.path != ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }.Concat(others).ToArray();
            AssetDatabase.SaveAssets();
            Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
            DomainSelfTests.Run();
            Debug.Log("PROJECT VALIDATION PASSED | Unity " + Application.unityVersion + " | Input Manager | " + ScenePath);
        }

        public static void BuildWindows()
        {
            Prepare();
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/DragonMoba.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("WINDOWS BUILD PASSED | " + report.summary.totalSize + " bytes");
        }
    }
}
#endif
