#if UNITY_EDITOR
using DragonMoba.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DragonMoba.Editor
{
    public static class CreateArenaScene
    {
        [MenuItem("Dragon MOBA/Criar arena de prototipo")]
        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("Dragon MOBA Prototype");
            bootstrap.AddComponent<PrototypeGame>();
            var path = "Assets/Scenes/ArenaPrototype.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            Debug.Log("Arena criada. Pressione Play e selecione Goku ou Cell.");
        }
    }
}
#endif
