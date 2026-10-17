#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using DragonMoba.Domain;
using DragonMoba.Runtime;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace DragonMoba.Editor
{
    public static partial class ArtSetup
    {
        private const string ControllerPath = "Assets/_DragonMOBA/Art/Animations/Hero.controller";
        public static void CreateBase()
        {
            Import();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var sm = controller.layers[0].stateMachine;
            foreach (var old in sm.states) sm.RemoveState(old.state);
            string[] names = { "Idle", "Walk", "Run", "Attack", "Hit", "Death" };
            string[] clips = { "Idle_Loop", "Walk_Loop", "Jog_Fwd_Loop", "Punch_Jab", "Hit_Chest", "Death01" };
            var library = AssetDatabase.LoadAllAssetsAtPath(ArtRoot + "Animations/Humanoid.fbx").OfType<AnimationClip>();
            for (int i = 0; i < names.Length; i++)
            {
                var state = sm.AddState(names[i]);
                state.motion = library.First(c => c.name == "Rig|" + clips[i]);
                if (i == 0) sm.defaultState = state;
            }
            MakeForm("GKU_SUPER", "super_base", new Color(.95f,.45f,.12f), 1.25f);
            AssetDatabase.SaveAssets();
            Debug.Log("ARTBASE PASSED");
        }
        public static void CreateForms()
        {
            int index = 0;
            foreach (var hero in Catalog.AllHeroes)
                foreach (var form in hero.Forms)
                {
                    Color color = form.Id.Contains("blue") ? (form.Id.EndsWith("kk") ? new Color(.95f,.12f,.6f) : Color.cyan) :
                        form.Id == "super_god" ? new Color(1,.12f,.18f) :
                        form.Id == "super_ui" ? Color.white : form.Id == "super_ui_sign" ? new Color(.45f,.6f,.85f) :
                        form.Id == "kid_nimbus" ? Color.yellow : form.Id == "kid_mastery" ? new Color(1,.24f,.08f) :
                        hero.Id == "CELL_Z" ? Color.HSVToRGB(.24f+(index%3)*.055f,.85f,.65f+(index%3)*.15f) : new Color(1,.45f,.08f);
                    index++;
                    MakeForm(hero.Id, form.Id, color, hero.Id == "GKU_KID" ? .95f : 1.25f);
                }
            AssetDatabase.SaveAssets();
            Debug.Log("ARTFORMS PASSED | " + index);
        }
        private static void MakeForm(string hero, string form, Color color, float scale)
        {
            var folder = "Assets/Resources/CharacterForms/" + hero;
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(ArtRoot + "Characters/Materials");
            AssetDatabase.Refresh();
            var materialPath = ArtRoot + "Characters/Materials/" + form + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, materialPath); }
            material.color = color;
            material.SetFloat("_Glossiness", .25f);
            var root = new GameObject(form);
            try
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ArtRoot + "Characters/Hero.fbx"));
                model.transform.SetParent(root.transform, false);
                model.transform.localScale *= scale;
                foreach (var r in model.GetComponentsInChildren<Renderer>()) r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();
                var animator = model.GetComponent<Animator>();
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var capsule = root.AddComponent<CapsuleCollider>();
                capsule.height = 1.95f * scale; capsule.center = Vector3.up * capsule.height / 2; capsule.radius = .4f * scale;
                var visual = root.AddComponent<CharacterPresentation>();
                visual.animator = animator; visual.formColor = color;
                var auraObject = new GameObject("Charging aura");
                auraObject.transform.SetParent(root.transform, false);
                var aura = auraObject.AddComponent<ParticleSystem>();
                aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = aura.main; main.startLifetime = .65f; main.startSpeed = 1.8f;
                main.startSize = .11f; main.maxParticles = 64; main.startColor = color;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                var emission = aura.emission; emission.rateOverTime = 35; emission.enabled = false;
                var shape = aura.shape; shape.shapeType = ParticleSystemShapeType.Cone;
                shape.radius = .65f; shape.angle = 8f; shape.rotation = new Vector3(-90,0,0);
                auraObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = MaterialAsset("Aura-" + form, color, "Particles/Standard Unlit");
                visual.aura = aura;
                aura.Play();
                PrefabUtility.SaveAsPrefabAsset(root, folder + "/" + form + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
#endif
