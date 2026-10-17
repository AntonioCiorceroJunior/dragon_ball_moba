#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using DragonMoba.Runtime;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonMoba.Editor
{
    public static partial class ArtSetup
    {
        public const string ArtScenePath = "Assets/_DragonMOBA/Scenes/ArtValidation/ArenaArt.unity";
        public static void CreateScene()
        {
            foreach(var file in new[]{"Tree","Bush","Rock"})
            {
                var path=ArtRoot+"Environment/"+file+".fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
                importer.SaveAndReimport();
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                try
                {
                    foreach(var r in instance.GetComponentsInChildren<Renderer>())
                    {
                        var materials=r.sharedMaterials;
                        for(int i=0;i<materials.Length;i++)
                        {
                            string label=materials[i]==null?r.name:materials[i].name;
                            Debug.Log("ENV MATERIAL | "+file+" | "+label);
                            bool bark=label.ToLowerInvariant().Contains("bark")||label.ToLowerInvariant().Contains("trunk");
                            var color=file=="Rock"?new Color(.48f,.53f,.59f):bark?new Color(.4f,.25f,.13f):new Color(.23f,.55f,.16f);
                            materials[i]=MaterialAsset("Env-"+file+"-"+i,color);
                            string textureName = label.Contains("Rocks") ? "Rocks_Diffuse" : label;
                            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(ArtRoot+"Environment/Textures/"+textureName+".png");
                            if(texture!=null) {
                                materials[i].mainTexture=texture; materials[i].color=label.Contains("Leaves") ? new Color(.28f,.58f,.16f) : Color.white;
                                if(label.Contains("Leaves")) {
                                    materials[i].SetFloat("_Mode",1); materials[i].SetFloat("_Cutoff",.45f);
                                    materials[i].SetOverrideTag("RenderType","TransparentCutout");
                                    materials[i].EnableKeyword("_ALPHATEST_ON"); materials[i].renderQueue=2450;
                                }
                            }
                        }
                        r.sharedMaterials=materials;
                    }
                    PrefabUtility.SaveAsPrefabAsset(instance,"Assets/_DragonMOBA/Prefabs/"+file+".prefab");
                }
                finally { UnityEngine.Object.DestroyImmediate(instance); }
            }
            foreach(var path in Directory.GetFiles(ArtRoot+"UI","*.png"))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.textureType=TextureImporterType.GUI; importer.mipmapEnabled=false; importer.SaveAndReimport();
            }
            foreach(var path in Directory.GetFiles(ArtRoot+"Audio","*.ogg"))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                var settings=importer.defaultSampleSettings;
                settings.loadType=AudioClipLoadType.DecompressOnLoad; importer.defaultSampleSettings=settings;
                importer.SaveAndReimport();
            }
            if(!File.Exists(ArtScenePath) && !AssetDatabase.CopyAsset(ProjectValidation.ScenePath,ArtScenePath)) throw new Exception("Scene copy failed");
            var scene=EditorSceneManager.OpenScene(ArtScenePath);
            var game=UnityEngine.Object.FindAnyObjectByType<PrototypeGame>();
            if(game==null) throw new Exception("Missing prototype bootstrap");
            var art=game.GetComponent<ArtPresentation>()??game.gameObject.AddComponent<ArtPresentation>();
            art.tree=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DragonMOBA/Prefabs/Tree.prefab");
            art.bush=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DragonMOBA/Prefabs/Bush.prefab");
            art.rock=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DragonMOBA/Prefabs/Rock.prefab");
            art.stoneMaterial=MaterialAsset("Stone",new Color(.36f,.43f,.5f));
            art.energyMaterial=MaterialAsset("Energy",Color.white);
            art.panel=AssetDatabase.LoadAssetAtPath<Texture2D>(ArtRoot+"UI/Panel.png");
            art.bar=AssetDatabase.LoadAssetAtPath<Texture2D>(ArtRoot+"UI/Bar.png");
            art.keys=new[]{"q","w","e","r"}.Select(k=>AssetDatabase.LoadAssetAtPath<Texture2D>(ArtRoot+"UI/Key-"+k+".png")).ToArray();
            art.laser=AssetDatabase.LoadAssetAtPath<AudioClip>(ArtRoot+"Audio/Laser.ogg");
            art.charge=AssetDatabase.LoadAssetAtPath<AudioClip>(ArtRoot+"Audio/Charge.ogg");
            art.impact=AssetDatabase.LoadAssetAtPath<AudioClip>(ArtRoot+"Audio/Impact.ogg");
            EditorSceneManager.SaveScene(scene);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ArtScenePath,true),new EditorBuildSettingsScene(ProjectValidation.ScenePath,true)};
            AssetDatabase.SaveAssets();
            Debug.Log("ARTSCENE PASSED");
        }
        public static Material MaterialAsset(string name,Color color,string shader="Standard")
        {
            var path=ArtRoot+"Environment/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null) { mat=new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(mat,path); }
            mat.color=color; if(mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness",.2f);
            return mat;
        }
        public static void Build()
        {
            DomainSelfTests.Run();
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ArtScenePath,true),new EditorBuildSettingsScene(ProjectValidation.ScenePath,true)};
            Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
            Directory.CreateDirectory("Builds/ArtWindows");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{ArtScenePath},locationPathName="Builds/ArtWindows/DragonMobaArt.exe",
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Art Windows build failed: "+report.summary.result);
            Debug.Log("ARTBUILD PASSED | "+report.summary.totalSize+" bytes");
        }
    }
}
#endif
