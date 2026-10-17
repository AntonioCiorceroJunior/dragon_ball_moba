#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DragonMoba.Editor
{
    public static partial class ArtSetup
    {
        public const string ArtRoot = "Assets/_DragonMOBA/Art/";
        public static void Import()
        {
            foreach (var path in new[] { ArtRoot + "Characters/Hero.fbx", ArtRoot + "Animations/Humanoid.fbx" })
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                if (path.Contains("Animations"))
                {
                    var clips = importer.defaultClipAnimations;
                    foreach (var clip in clips)
                    {
                        clip.loopTime = clip.name.Contains("Jog") || clip.name.Contains("Sprint") || clip.name.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0 || clip.name.IndexOf("walk", StringComparison.OrdinalIgnoreCase) >= 0 || clip.name.IndexOf("run", StringComparison.OrdinalIgnoreCase) >= 0;
                        clip.lockRootRotation = true;
                        clip.lockRootHeightY = true;
                        clip.lockRootPositionXZ = true;
                        clip.keepOriginalPositionY = true;
                    }
                    importer.clipAnimations = clips;
                }
                importer.SaveAndReimport();
                var avatar = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
                if (avatar == null || !avatar.isValid || !avatar.isHuman) throw new Exception("Invalid humanoid avatar: " + path);
                Debug.Log("HUMANOID VALID | " + path);
                foreach (var renderer in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<Renderer>())
                    Debug.Log("RENDERER | " + renderer.name + " | " + renderer.bounds.size);
                var animations = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview"));
                Debug.Log("CLIPS | " + string.Join(", ", animations.Select(c => c.name)));
            }
            Debug.Log("ARTIMPORT PASSED");
        }
    }
}
#endif
