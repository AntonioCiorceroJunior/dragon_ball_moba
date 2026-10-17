#if UNITY_EDITOR
using System;
using System.Reflection;
using DragonMoba.Domain;
using DragonMoba.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DragonMoba.Editor
{
    // Exercises the existing runtime in actual Play Mode without changing gameplay APIs.
    [InitializeOnLoad]
    public static class PlayModeSmoke
    {
        private const string RunningKey = "DragonMoba.PlayModeSmoke.Running";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly string[] Champions = { "GKU_KID", "GKU_SUPER", "CELL_Z" };
        private static int championIndex;
        private static int firstFrame;
        private static double deadline;
        private static GameWorld world;
        private static V2 startPosition;
        private static PrototypeGame previousGame;

        static PlayModeSmoke()
        {
            if (SessionState.GetBool(RunningKey, false)) Subscribe();
        }

        public static void Run()
        {
            ProjectValidation.Prepare();
            SessionState.SetBool(RunningKey, true);
            Subscribe();
            // Finish editor startup (including Search indexing) before a domain reload.
            EditorApplication.delayCall += () => EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }

        private static void Subscribe()
        {
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
        }

        private static void OnLog(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Finish(false, message + "\n" + trace);
        }

        private static void Update()
        {
            if (EditorApplication.timeSinceStartup > deadline) { Finish(false, "Timeout entering/running Play Mode."); return; }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            try
            {
                if (world == null)
                {
                    // Let Awake run before starting a match through the same method as the menu.
                    var game = UnityEngine.Object.FindAnyObjectByType<PrototypeGame>();
                    if (game == null || Camera.main == null || game == previousGame) return;
                    previousGame = game;
                    Require(typeof(PrototypeGame).GetField("world", PrivateInstance).GetValue(game) == null, "Selection menu starts without a match");
                    typeof(PrototypeGame).GetMethod("StartMatch", PrivateInstance).Invoke(game, new object[] { Champions[championIndex] });
                    world = (GameWorld)typeof(PrototypeGame).GetField("world", PrivateInstance).GetValue(game);
                    Require(world.Player.HeroId == Champions[championIndex], "Selected champion");
                    Require(GameObject.Find("Solo") != null && Camera.main.orthographic, "Arena and isometric camera");
                    Require(UnityEngine.Object.FindObjectsByType<EntityHandle>().Length == world.Entities.Count, "Views for all initial entities");
                    foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>())
                        Require(renderer.sharedMaterial != null && renderer.sharedMaterial.shader.isSupported, "Supported placeholder material");
                    startPosition = world.Player.Position;
                    Require(world.Submit(new GameCommand { ActorId = world.PlayerId, Kind = CommandKind.Move, Position = new V2(8, -20) }), "Movement accepted");
                    firstFrame = Time.frameCount;
                    return;
                }
                if (Time.frameCount - firstFrame < 90 || world.TickIndex < 20) return;
                Require(V2.Distance(startPosition, world.Player.Position) > 0.5f, "Player moves during FixedUpdate");
                Debug.Log("PLAY MODE CHAMPION PASSED | " + Champions[championIndex] + " | ticks=" + world.TickIndex);
                if (++championIndex == Champions.Length) { Finish(true, "Three champions, scene, camera, materials, views, input polling and FixedUpdate movement."); return; }
                world = null;
                EditorSceneManager.LoadSceneInPlayMode(ProjectValidation.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            }
            catch (Exception e) { Finish(false, e.ToString()); }
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException(label);
        }

        private static void Finish(bool success, string detail)
        {
            SessionState.EraseBool(RunningKey);
            EditorApplication.update -= Update;
            Application.logMessageReceived -= OnLog;
            if (success) Debug.Log("PLAY MODE SMOKE PASSED | " + detail);
            else Debug.LogError("PLAY MODE SMOKE FAILED | " + detail);
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
#endif
