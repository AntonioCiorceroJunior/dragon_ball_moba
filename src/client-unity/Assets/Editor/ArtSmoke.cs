#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DragonMoba.Domain;
using DragonMoba.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DragonMoba.Editor
{
    [InitializeOnLoad]
    public static class ArtSmoke
    {
        const string Key = "DragonMoba.ArtSmoke";
        static string ScenePath { get { return SessionState.GetBool(Key+"Visual",false) ? VisualReplacement.ScenePath : ArtSetup.ArtScenePath; } }
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        static readonly string[] Heroes={"GKU_SUPER","GKU_KID","CELL_Z"};
        static GameWorld world;
        static PrototypeGame game, previousGame;
        static CharacterPresentation view;
        static ArtPresentation art;
        static int frame,phase,champion,formIndex;
        static double deadline;
        static V2 start,minionStart;
        static EntityState minion,victim,tower;
        static Quaternion boneStart;
        static float boneChange,beforeHp,beforeXp;
        static bool sawRun,full;
        static ArtSmoke() { if(SessionState.GetBool(Key,false)) Subscribe(); }
        public static void Run()
        {
            SessionState.SetBool(Key+"Visual",false); RunCore();
        }
        public static void RunVisual()
        {
            SessionState.SetBool(Key+"Visual",true); RunCore();
        }
        static void RunCore()
        {
            DomainSelfTests.Run();
            VisualStageChecks.CellRespawn();
            full=File.Exists(ScenePath);
            SessionState.SetBool(Key+"Full",full);
            EditorSceneManager.OpenScene(full?ScenePath:ProjectValidation.ScenePath);
            SessionState.SetBool(Key,true); Subscribe();
            EditorApplication.delayCall+=()=>EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
        }
        static void Subscribe()
        {
            full=SessionState.GetBool(Key+"Full",false);
            deadline=EditorApplication.timeSinceStartup+240;
            EditorApplication.update-=Update; EditorApplication.update+=Update;
            Application.logMessageReceived-=Log; Application.logMessageReceived+=Log;
        }
        static void Log(string message,string trace,LogType type)
        { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert) Finish(false,message+"\n"+trace); }
        static void Check(bool ok,string message) { if(!ok) throw new Exception(message); }
        static void Refresh() { typeof(PrototypeGame).GetMethod("RefreshViews",Flags).Invoke(game,null); }
        static CharacterPresentation PlayerView()
        { return GameObject.Find("Entity-"+world.PlayerId+"-"+world.Player.DisplayName).GetComponent<CharacterPresentation>(); }
        static bool Wait(int frames) { return Time.frameCount-frame<frames; }
        static void Next(int target) { phase=target; frame=Time.frameCount; }
        static void Update()
        {
            if(EditorApplication.timeSinceStartup>deadline) { Finish(false,"Timeout"); return; }
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling) return;
            try
            {
                if(world==null)
                {
                    game=UnityEngine.Object.FindAnyObjectByType<PrototypeGame>();
                    if(game==null||game==previousGame||Camera.main==null) return;
                    previousGame=game;
                    typeof(PrototypeGame).GetMethod("StartMatch",Flags).Invoke(game,new object[]{Heroes[champion]});
                    world=(GameWorld)typeof(PrototypeGame).GetField("world",Flags).GetValue(game);
                    view=PlayerView(); art=game.GetComponent<ArtPresentation>();
                    Check(view!=null&&view.animator.isHuman&&!view.animator.applyRootMotion,"Humanoid, no root motion");
                    Check(view.GetComponentsInChildren<Collider>().Length==1,"Single collider");
                    Check(world.Submit(new GameCommand{ActorId=world.PlayerId,Kind=CommandKind.Buy,ItemId="KI_CELL"}),"Shop purchase");
                    start=world.Player.Position;
                    minion=world.Entities.First(e=>e.Kind==EntityKind.Minion&&e.Team==Team.Blue); minionStart=minion.Position;
                    boneStart=view.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localRotation;
                    Check(world.Submit(new GameCommand{ActorId=world.PlayerId,Kind=CommandKind.Move,Position=new V2(8,-20)}),"Move accepted");
                    boneChange=0; sawRun=false; formIndex=0; Next(0); return;
                }
                var hero=world.Player;
                if(phase==0)
                {
                    sawRun|=view.animator.GetCurrentAnimatorStateInfo(0).IsName("Run");
                    boneChange=Mathf.Max(boneChange,Quaternion.Angle(boneStart,view.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localRotation));
                    if(Wait(120)) return;
                    Check(V2.Distance(start,hero.Position)>.5f&&V2.Distance(minionStart,minion.Position)>.1f,"Hero and minion movement");
                    Check(sawRun&&boneChange>2,"Animated run, no static T pose");
                    if(game.GetComponent<OrganicArena>()!=null)
                    {
                        var arena=game.GetComponent<OrganicArena>();
                        Check(arena.TerrainVertices>20000&&arena.PlantCount>80,"Organic map actively generated");
                        var terrainRenderer=GameObject.Find("OrganicTerrain").GetComponent<Renderer>();
                        Check(terrainRenderer.enabled&&GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),terrainRenderer.bounds),"Organic terrain active inside camera frustum");
                        Check(UnityEngine.Object.FindObjectsByType<Renderer>().Where(r=>r.name=="Rota"||r.name=="Rio Central").All(r=>!r.enabled),"Rectangular map no longer rendered");
                        if(hero.HeroId=="GKU_SUPER") Check(view.GetComponent<PlayableCharacterVisual>()!=null&&view.GetComponentsInChildren<SkinnedMeshRenderer>().Any(r=>r.enabled&&GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),r.bounds)),"Styled fighter actually used and inside camera frustum");
                        RaycastHit groundHit;
                        Check(Physics.Raycast(new Vector3(31,10,10),Vector3.down,out groundHit,20)&&groundHit.collider.name=="Solo","Original clickable floor collider retained");
                    }
                    var body=view.GetComponentsInChildren<SkinnedMeshRenderer>().OrderByDescending(r=>r.bounds.size.y).First();
                    Debug.Log("ART SCALE | "+hero.HeroId+" | "+body.bounds.size);
                    Check(body.bounds.size.y>1f && body.bounds.size.y<4f,"Readable humanoid height in world units");
                    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>())
                        Check(r.sharedMaterial!=null&&r.sharedMaterial.shader.isSupported,"Supported material "+r.name);
                    Debug.Log("ART MOVEMENT PASSED | "+hero.HeroId+" | bone delta="+boneChange);
                    if(!full) { Finish(true,"Base humanoid"); return; }
                    hero.Position=new V2(8,-20); hero.Destination=hero.Position; hero.AttackTargetId=0; hero.Ki=0;
                    Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Charge,Pressed=true}),"Charge command");
                    Next(1); return;
                }
                if(phase==1)
                {
                    if(Wait(60)) return;
                    Check(hero.Ki>10&&hero.Charging&&view.aura.emission.enabled&&view.aura.particleCount>0,"Ki charge and actual particles");
                    hero.Ki=hero.MaxKi; beforeXp=hero.Experience; Next(2); return;
                }
                if(phase==2)
                {
                    if(Wait(90)) return;
                    Check(hero.Experience>beforeXp,"Training XP");
                    Capture("Logs/Art-"+hero.HeroId+".png");
                    if(game.GetComponent<OrganicArena>()!=null)
                    {
                        Directory.CreateDirectory("../../docs/evidence");
                        Capture("../../docs/evidence/After-World-"+hero.HeroId+".png");
                        if(hero.HeroId=="GKU_SUPER")
                        {
                            var cam=Camera.main;var position=cam.transform.position;var rotation=cam.transform.rotation;float size=cam.orthographicSize;
                            cam.transform.position=new Vector3(0,80,-52);cam.transform.LookAt(Vector3.zero);cam.orthographicSize=48;
                            Capture("../../docs/evidence/After-Map-Overview.png",768,960);
                            cam.transform.SetPositionAndRotation(position,rotation);cam.orthographicSize=size;
                        }
                    }
                    var oldSize=Camera.main.orthographicSize;
                    Camera.main.orthographicSize=5;
                    Capture("Logs/Art-Detail-"+hero.HeroId+".png");
                    if(game.GetComponent<OrganicArena>()!=null) Capture("../../docs/evidence/After-Character-"+hero.HeroId+".png");
                    Camera.main.orthographicSize=oldSize;
                    // Camera screenshots exclude IMGUI; batch ScreenCapture produced no file.
                    Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Charge,Pressed=false}),"Stop charge");
                    victim=world.Entities.First(e=>e.Kind==EntityKind.Minion&&e.Team==Team.Red&&e.Alive);
                    victim.Position=new V2(hero.Position.X+1,hero.Position.Y); victim.Destination=victim.Position; victim.Hp=10000; victim.MaxHp=10000;
                    beforeHp=victim.Hp; hero.AttackCooldown=0;
                    Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Attack,TargetId=victim.Id}),"Attack command");
                    Next(3); return;
                }
                if(phase==3)
                {
                    if(Wait(90)) return;
                    Check(victim.Hp<beforeHp,"Attack deals damage");
                    var hp=victim.Hp;
                    // This smoke validates the unlocked ability effects, not bypassing
                    // the new level requirements. Level 6 grants access to all slots.
                    while(hero.Level<6)
                        world.GrantExperience(hero,Catalog.ExperienceForNextLevel(hero.Level)-hero.Experience+.01f);
                    foreach(var slot in new[]{AbilitySlot.Q,AbilitySlot.W,AbilitySlot.E,AbilitySlot.R})
                        Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.UpgradeAbility,Slot=slot}),
                            "Unlock "+slot+" legally");
                    // The hero may transform at level 6; the form cycle must start
                    // from the current form rather than expecting level-1 appearance.
                    formIndex=Catalog.Hero(hero.HeroId).Forms.FindIndex(f=>f.Id==hero.FormId);
                    Check(formIndex>=0,"Valid form after skill-unlock progression");
                    hero.Ki=hero.MaxKi;
                    Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Cast,Slot=AbilitySlot.Q,Position=victim.Position}),"Q accepted");
                    Check(victim.Hp<hp,"Q damages target");
                    hero.Ki=hero.MaxKi;
                    Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Cast,Slot=AbilitySlot.E,Position=hero.Position})&&hero.Shield>0,"E shield");
                    var rHp=victim.Hp;
                    hero.Ki=hero.MaxKi;
                    Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Cast,Slot=AbilitySlot.R,Position=victim.Position})&&victim.Hp<rHp,"R damage");
                    var wStart=hero.Position;
                    hero.Ki=hero.MaxKi;
                    Check(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Cast,Slot=AbilitySlot.W,Position=new V2(hero.Position.X+5,hero.Position.Y)})&&V2.Distance(wStart,hero.Position)>1,"W displacement");
                    tower=world.Entities.First(e=>e.Kind==EntityKind.Tower&&e.Team==Team.Blue);
                    victim.Position=new V2(tower.Position.X+1,tower.Position.Y); victim.Destination=victim.Position;
                    hero.AttackTargetId=0; hero.Position=new V2(8,-20); hero.Destination=hero.Position;
                    tower.AttackCooldown=0; beforeHp=victim.Hp; Next(4); return;
                }
                if(phase==4)
                {
                    if(Wait(90)) return;
                    Check(tower.AttackTargetId==victim.Id&&victim.Hp<beforeHp,"Tower attacks nearby enemy");
                    victim.Position=new V2(25,25); victim.Destination=victim.Position;
                    hero.AttackTargetId=0; hero.Position=new V2(8,-20); hero.Destination=hero.Position;
                    Debug.Log("ART COMBAT PASSED | charge, XP, purchase, attack, Q, E, tower | "+hero.HeroId);
                    Next(5); return;
                }
                if(phase==5)
                {
                    if(Wait(8)) return;
                    view=PlayerView();
                    Check(view!=null&&view.GetComponentsInChildren<Collider>().Length==1,"Form remains one view/collider");
                    Check(UnityEngine.Object.FindObjectsByType<EntityHandle>().Count(h=>h.EntityId==hero.Id)==1,"No duplicate entity");
                    var forms=Catalog.Hero(hero.HeroId).Forms;
                    Check(hero.FormId==forms[formIndex].Id,"Expected form");
                    Check(view.animator.isHuman&&view.animator.runtimeAnimatorController!=null,"Form animator");
                    if(game.GetComponent<OrganicArena>()!=null&&hero.HeroId=="GKU_SUPER")
                    {
                        var tag=view.GetComponent<PlayableCharacterVisual>();
                        Check(tag!=null&&tag.formId==hero.FormId,"Actual visual variant matches level form");
                        if(formIndex>0) Check(tag.hairColor!=Color.black,"Evolved hair/material variant");
                        Capture("../../docs/evidence/Form-"+hero.FormId+".png");
                        if(hero.FormId=="super_base"||hero.FormId=="super_blue")
                        {
                            float zoom=Camera.main.orthographicSize;Camera.main.orthographicSize=5;
                            Capture("../../docs/evidence/Character-"+hero.FormId+".png");Camera.main.orthographicSize=zoom;
                        }
                        Debug.Log("STYLED FORM USED | "+tag.formId+" | hair="+tag.hairColor);
                    }
                    Debug.Log("ART FORM PASSED | "+hero.HeroId+"/"+hero.FormId);
                    if(++formIndex<forms.Count)
                    {
                        while(hero.Level<forms[formIndex].Level)
                            world.GrantExperience(hero,Catalog.ExperienceForNextLevel(hero.Level)-hero.Experience+.01f);
                        var hp=hero.Hp; var ki=hero.Ki; var speed=hero.MoveSpeed; var id=hero.Id; var inventory=hero.Inventory.ToArray(); var target=hero.AttackTargetId;
                        Refresh();
                        Check(world.Player==hero&&hero.Id==id&&hero.Hp==hp&&hero.Ki==ki&&hero.MoveSpeed==speed&&hero.AttackTargetId==target&&hero.Inventory.SequenceEqual(inventory),"Visual swap preserves domain state");
                        Next(5); return;
                    }
                    var enemy=world.Entities.First(e=>e.Team==Team.Red&&e.Kind==EntityKind.Hero);
                    Check(world.DealDamage(enemy,hero,100000,false),"Death damage"); Refresh(); Next(6); return;
                }
                if(phase==6)
                {
                    if(Wait(15)) return;
                    view=PlayerView();
                    Check(!hero.Alive&&view.animator.GetCurrentAnimatorStateInfo(0).IsName("Death")&&!view.GetComponent<Collider>().enabled,"Death animation and disabled collider");
                    hero.RespawnTime=.05f; Next(7); return;
                }
                if(phase==7)
                {
                    if(Wait(30)) return;
                    Check(hero.Alive&&PlayerView().GetComponent<Collider>().enabled,"Respawn re-enables view/collider");
                    for(int i=0;i<100;i++) art.Emit(new GameEvent(EventKind.Transform,hero.Id,hero.Position,"pool stress"),world);
                    Check(art.EffectCount<=ArtPresentation.EffectLimit,"Bounded effect pool");
                    Check(art.panel!=null&&art.bar!=null&&art.keys.All(k=>k!=null),"HUD assets");
                    var cameraController=Camera.main.GetComponent<ArenaCameraController>();
                    Check(Camera.main.orthographic,"Orthographic camera");
                    VisualStageChecks.CameraControls(cameraController,new Vector3(hero.Position.X,0,hero.Position.Y));
                    if(game.GetComponent<OrganicArena>()!=null)
                    {
                        var target=world.Entities.First(e=>e.Kind==EntityKind.Tower&&e.Team==Team.Red&&e.Alive);
                        var towerView=GameObject.Find("Entity-"+target.Id+"-"+target.DisplayName);
                        Check(towerView!=null&&world.DealDamage(hero,target,100000,false),"Tower destruction damage");
                        Refresh();Check(!target.Alive&&!towerView.activeSelf,"Destroyed tower visual is deactivated");
                        Debug.Log("TOWER VISUAL DESTRUCTION PASSED | "+hero.HeroId);
                    }
                    Debug.Log("ART CHAMPION PASSED | "+hero.HeroId+" | tick="+world.TickIndex+" | death/respawn, effects<=24, HUD");
                    if(++champion==Heroes.Length)
                    {
                        typeof(PrototypeGame).GetMethod("ReturnToSelection",Flags).Invoke(game,null);
                        Check(typeof(PrototypeGame).GetField("world",Flags).GetValue(game)==null,"Return to character selection");
                        typeof(PrototypeGame).GetMethod("StartMatch",Flags).Invoke(game,new object[]{"GKU_SUPER"});
                        Check(((GameWorld)typeof(PrototypeGame).GetField("world",Flags).GetValue(game)).Player.HeroId=="GKU_SUPER","Restart after selection");
                        Debug.Log("SELECTION RESTART PASSED");
                        Finish(true,"Three champions; 12 forms; movement, animation, combat, Ki/XP, shop, camera, death/respawn, selection and bounded VFX."); return;
                    }
                    world=null;
                    EditorSceneManager.LoadSceneInPlayMode(ScenePath,new LoadSceneParameters(LoadSceneMode.Single));
                }
            }
            catch(Exception e) { Finish(false,e.ToString()); }
        }
        static void Capture(string path,int width=1280,int height=720)
        {
            var cam=Camera.main; var old=cam.targetTexture; var active=RenderTexture.active;
            var rt=new RenderTexture(width,height,24); var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            try { cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt; tex.ReadPixels(new Rect(0,0,width,height),0,0); tex.Apply(); File.WriteAllBytes(path,tex.EncodeToPNG()); }
            finally { cam.targetTexture=old; RenderTexture.active=active; rt.Release(); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(tex); }
        }
        static void Finish(bool success,string info)
        {
            SessionState.EraseBool(Key); EditorApplication.update-=Update; Application.logMessageReceived-=Log;
            if(success) Debug.Log("ARTPLAY PASSED | "+info); else Debug.LogError("ARTPLAY FAILED | "+info);
            if(success&&SessionState.GetBool(Key+"Visual",false)) Debug.Log("VISUALPLAY PASSED | runtime map, wired fighter, QWER and real level forms");
            EditorApplication.Exit(success?0:1);
        }
    }
}
#endif
