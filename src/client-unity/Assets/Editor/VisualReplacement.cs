#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using DragonMoba.Domain;
using DragonMoba.Runtime;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonMoba.Editor
{
    public static class VisualReplacement
    {
        public const string ScenePath="Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity";
        public const string Folder="Assets/_DragonMOBA/VisualTest/";
        static Material Mat(string name,string shader,Color color)
        {
            var path=Folder+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(mat,path);}
            mat.color=color;return mat;
        }
        public static void Prepare()
        {
            Directory.CreateDirectory(Folder);
            Directory.CreateDirectory("Assets/_DragonMOBA/Scenes/VisualTest");
            AssetDatabase.Refresh();
            var controller=Controller();
            var forms=Catalog.Hero("GKU_SUPER").Forms;
            var prefabs=new GameObject[forms.Count];
            for(int i=0;i<forms.Count;i++) prefabs[i]=Fighter(forms[i].Id,i,controller);
            if(!File.Exists(ScenePath)&&!AssetDatabase.CopyAsset(ArtSetup.ArtScenePath,ScenePath))throw new Exception("Scene copy failed");
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var game=UnityEngine.Object.FindAnyObjectByType<PrototypeGame>();
            if(game==null)throw new Exception("Missing original bootstrap");
            var arena=game.GetComponent<OrganicArena>()??game.gameObject.AddComponent<OrganicArena>();
            arena.ground=Mat("Ground","DragonMoba/Organic Ground",Color.white);
            arena.water=Mat("Water","DragonMoba/Flowing River",new Color(.025f,.24f,.29f));
            arena.water.SetColor("_Shallow",new Color(.15f,.5f,.47f));
            arena.stone=Mat("SanctuaryStone","Standard",new Color(.31f,.36f,.36f));
            arena.wood=Mat("BridgeTimber","Standard",new Color(.27f,.19f,.12f));
            arena.energy=Mat("SentinelEnergy","Standard",Color.white);
            arena.energy.EnableKeyword("_EMISSION");
            arena.ring=Mat("EnergyRing","Sprites/Default",Color.white);
            arena.tree=Forest("Tree");arena.bush=Forest("Bush");arena.rock=Forest("Rock");
            arena.formIds=forms.Select(f=>f.Id).ToArray();arena.characterForms=prefabs;arena.startDemo=true;
            EditorSceneManager.SaveScene(scene);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("VISUALPREPARE PASSED | "+ScenePath+" | six directly wired animated fighter prefabs");
        }
        static AnimatorController Controller()
        {
            string path=Folder+"VisualHero.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(path);
            var sm=controller.layers[0].stateMachine;foreach(var state in sm.states)sm.RemoveState(state.state);
            var library=AssetDatabase.LoadAllAssetsAtPath(ArtSetup.ArtRoot+"Animations/Humanoid.fbx").OfType<AnimationClip>();
            string[] names={"Idle","Walk","Run","Attack","Hit","Death","Charge","Cast","Transform"};
            string[] clips={"Idle_Loop","Walk_Loop","Jog_Fwd_Loop","Punch_Jab","Hit_Chest","Death01","Spell_Simple_Idle_Loop","Spell_Simple_Shoot","Spell_Simple_Enter"};
            for(int i=0;i<names.Length;i++){var state=sm.AddState(names[i]);state.motion=library.First(c=>c.name=="Rig|"+clips[i]);if(i==0)sm.defaultState=state;}
            return controller;
        }
        static GameObject Forest(string type)
        {
            var original=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_DragonMOBA/Prefabs/"+type+".prefab");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(original);
            try
            {
                foreach(var renderer in go.GetComponentsInChildren<Renderer>())
                {
                    var originals=renderer.sharedMaterials;var materials=new Material[originals.Length];
                    for(int i=0;i<materials.Length;i++)
                    {
                        string path=Folder+"Forest"+type+i+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                        if(material==null){material=new Material(originals[i]);AssetDatabase.CreateAsset(material,path);}
                        material.color=type=="Rock"?new Color(.55f,.62f,.6f):type=="Tree"&&i==0?new Color(.65f,.55f,.43f):new Color(.11f,.29f,.15f);
                        materials[i]=material;
                    }
                    renderer.sharedMaterials=materials;
                }
                return PrefabUtility.SaveAsPrefabAsset(go,Folder+"Forest"+type+".prefab");
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        static GameObject Fighter(string form,int index,AnimatorController controller)
        {
            var old=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/CharacterForms/GKU_SUPER/"+form+".prefab");
            var root=(GameObject)PrefabUtility.InstantiatePrefab(old);
            try
            {
                root.name=form;
                var presentation=root.GetComponent<CharacterPresentation>();var animator=presentation.animator;
                animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
                animator.Rebind();
                Color[] colors={new Color(.025f,.018f,.035f),new Color(.85f,.06f,.1f),new Color(.02f,.85f,1),new Color(.85f,.06f,.6f),new Color(.4f,.55f,.75f),new Color(.9f,.95f,1)};
                var tag=root.AddComponent<PlayableCharacterVisual>();tag.formId=form;tag.hairColor=colors[index];
                foreach(var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var material=Mat("Fighter-"+form+"-"+renderer.name,"DragonMoba/Fighter Toon",new Color(.93f,.29f,.045f));
                    var bounds=renderer.sharedMesh.bounds;
                    bool zUp=bounds.size.z>bounds.size.y;
                    material.SetFloat("_MinY",zUp?bounds.min.z:bounds.min.y);material.SetFloat("_SpanY",zUp?bounds.size.z:bounds.size.y);material.SetFloat("_Width",bounds.size.x);
                    material.SetVector("_HeightAxis",zUp?Vector3.forward:Vector3.up);
                    material.SetColor("_Skin",new Color(.93f,.64f,.4f));material.SetColor("_Trim",new Color(.025f,.09f,.22f));
                    bool body=renderer.name.Contains("Super");
                    material.SetFloat("_Outfit",body?1:0);
                    if(!body)material.color=renderer.name.Contains("Eyes")?Color.white:new Color(.03f,.025f,.03f);
                    renderer.sharedMaterials=Enumerable.Repeat(material,renderer.sharedMaterials.Length).ToArray();
                    if(body)Debug.Log("FIGHTER MESH | "+form+" | "+bounds);
                }
                var hairMaterial=Mat("Hair-"+form,"DragonMoba/Fighter Toon",colors[index]);hairMaterial.SetFloat("_Outfit",0);
                var head=animator.GetBoneTransform(HumanBodyBones.Head);
                if(head==null)throw new Exception("Humanoid head missing");
                var hair=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ArtSetup.ArtRoot+"Characters/Hair.fbx"));
                var renderers=hair.GetComponentsInChildren<Renderer>();
                var hairBounds=renderers[0].bounds;foreach(var renderer in renderers) {hairBounds.Encapsulate(renderer.bounds);renderer.sharedMaterial=hairMaterial;}
                float scale=.45f/Mathf.Max(.001f,hairBounds.size.x);
                hair.transform.localScale*=scale;hairBounds=renderers[0].bounds;
                hair.transform.position+=head.position+Vector3.up*.12f-hairBounds.center;
                hair.transform.SetParent(head,true);hair.name="Rig attached hair";
                var spikeMesh=SpikeMesh();
                for(int i=0;i<7;i++)
                {
                    var spike=new GameObject("Anime hair lock "+i);spike.transform.SetParent(head,false);
                    spike.transform.position=head.position+new Vector3((i%3-1)*.13f,.12f+(i/3)*.045f,-.045f);
                    Vector3 direction=new Vector3((i%3-1)*.65f,1,.25f+(i/3)*.35f).normalized;
                    spike.transform.rotation=Quaternion.FromToRotation(Vector3.up,direction);
                    // Scale in world units, then keep the animation bone parent.
                    spike.transform.SetParent(null,true);
                    spike.transform.localScale=new Vector3(.12f,index==0?.29f:.43f,.12f);
                    spike.transform.SetParent(head,true);
                    spike.AddComponent<MeshFilter>().sharedMesh=spikeMesh;spike.AddComponent<MeshRenderer>().sharedMaterial=hairMaterial;
                }
                var main=presentation.aura.main;main.startColor=index==0?new Color(1,.64f,.15f):colors[index];
                main.startSize=.1f;main.startLifetime=.7f;
                presentation.formColor=index==0?Color.yellow:colors[index];
                var auraMaterial=Mat("Aura-"+form,"Particles/Standard Unlit",presentation.formColor);
                auraMaterial.mainTexture=GlowTexture();
                auraMaterial.SetFloat("_Mode",4);auraMaterial.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                auraMaterial.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.One);auraMaterial.SetFloat("_ZWrite",0);
                auraMaterial.EnableKeyword("_ALPHABLEND_ON");auraMaterial.DisableKeyword("_ALPHATEST_ON");
                auraMaterial.SetOverrideTag("RenderType","Transparent");auraMaterial.renderQueue=3000;
                var psRenderer=presentation.aura.GetComponent<ParticleSystemRenderer>();psRenderer.sharedMaterial=auraMaterial;
                var emission=presentation.aura.emission;emission.enabled=false;
                SelectionRing(root.transform,index==0?new Color(.2f,.8f,1):colors[index]);
                var result=PrefabUtility.SaveAsPrefabAsset(root,Folder+(index==0?"PlayableCharacterVisual":form)+".prefab");
                return result;
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static Mesh SpikeMesh()
        {
            string path=Folder+"HairLock.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
            var vertices=new Vector3[6];vertices[0]=Vector3.up;vertices[5]=Vector3.zero;
            for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;vertices[i+1]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));}
            var triangles=new int[24];for(int i=0;i<4;i++){int j=i*6,n=1+(i+1)%4;triangles[j]=0;triangles[j+1]=i+1;triangles[j+2]=n;triangles[j+3]=5;triangles[j+4]=n;triangles[j+5]=i+1;}
            mesh=new Mesh{name="Original stylized hair lock"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static Texture2D GlowTexture()
        {
            string path=Folder+"AuraGlow.png";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture!=null)return texture;
            texture=new Texture2D(32,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/16;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2)));}
            texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static void SelectionRing(Transform root,Color color)
        {
            var go=new GameObject("Fighter selection ring");go.transform.SetParent(root,false);
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=48;
            line.sharedMaterial=Mat("SelectionRing","Sprites/Default",Color.white);line.startWidth=line.endWidth=.055f;line.startColor=line.endColor=color;
            for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;line.SetPosition(i,new Vector3(Mathf.Cos(a)*.63f,.06f,Mathf.Sin(a)*.63f));}
        }
        public static void Build()
        {
            DomainSelfTests.Run();VisualStageChecks.CellRespawn();
            Directory.CreateDirectory("Builds/VisualTest");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{ScenePath},locationPathName="Builds/VisualTest/DragonMobaVisualTest.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Visual build failed");
            var now=DateTime.UtcNow;
            File.SetLastWriteTimeUtc("Builds/VisualTest/DragonMobaVisualTest.exe",now);
            File.WriteAllText("Builds/VisualTest/build-info.json",JsonUtility.ToJson(new BuildInfo{scene=ScenePath,version=OrganicArena.Version,utc=now.ToString("O"),unity=Application.unityVersion,bytes=report.summary.totalSize},true));
            Debug.Log("VISUALBUILD PASSED | "+Path.GetFullPath("Builds/VisualTest/DragonMobaVisualTest.exe")+" | "+report.summary.totalSize+" bytes | "+now.ToString("O"));
        }
        [Serializable]class BuildInfo{public string scene,version,utc,unity;public ulong bytes;}
    }
}
#endif

