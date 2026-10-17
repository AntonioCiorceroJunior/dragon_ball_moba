#if UNITY_EDITOR
using System;
using System.Linq;
using DragonMoba.Domain;
using DragonMoba.Runtime;
using UnityEngine;
namespace DragonMoba.Editor
{
    public static class VisualStageChecks
    {
        public static void SelectionGallery()
        {
            var camera=Camera.main;
            Check(camera!=null,"Selection camera exists");
            var planes=GeometryUtility.CalculateFrustumPlanes(camera);
            for(var i=0;i<3;i++)
            {
                var anchor=GameObject.Find("Hero showcase "+i);
                Check(anchor!=null&&anchor.GetComponentsInChildren<SkinnedMeshRenderer>().Any(
                    r=>r.enabled&&GeometryUtility.TestPlanesAABB(planes,r.bounds)),
                    "Selection hero remains visible after returning from a match: "+i);
                if(i==1)Check(anchor.GetComponentInChildren<PlayableCharacterVisual>()!=null,
                    "Gallery uses the actual Super arena model");
            }
            Debug.Log("SELECTION GALLERY PASSED | three visible heroes and arena Super prefab");
        }
        public static void CellRespawn()
        {
            var world = new GameWorld("CELL_Z",ArenaLayout.Training);
            var hero = world.Player;
            while(hero.Level<5) world.GrantExperience(hero,Catalog.ExperienceForNextLevel(hero.Level)-hero.Experience+.01f);
            Check(hero.MaxHp==736,"Cell level five max HP");
            hero.Inventory.Add("KI_CELL");
            var inventory = hero.Inventory.ToArray();
            var enemy = world.Entities.First(e=>e.Kind==EntityKind.Hero&&e.Team==Team.Red);
            Check(world.DealDamage(enemy,hero,100000,false),"Lethal damage");
            Check(hero.Hp==0&&!hero.Alive,"Cell dead");
            float duration = hero.RespawnTime;
            Check(Math.Abs(duration-15.5f)<.001f,"Unmodified respawn duration");
            Check(!world.Submit(new GameCommand {ActorId=hero.Id,Kind=CommandKind.Move,Position=new V2(10,0)}),"Dead actor cannot move");
            world.DrainEvents();
            int count = (int)Math.Ceiling(duration/GameWorld.FixedStep);
            for(int i=0;i<count-2;i++) world.Tick(GameWorld.FixedStep);
            Check(!hero.Alive&&hero.RespawnTime>0,"Still dead before real timer elapses");
            for(int i=0;i<5;i++) world.Tick(GameWorld.FixedStep);
            Check(hero.Alive&&hero.Hp==hero.MaxHp&&hero.Ki==hero.MaxKi,"Actual timer restores HP/Ki");
            Check(world.Player==hero&&hero.Inventory.SequenceEqual(inventory),"Identity and inventory preserved");
            Check(V2.Distance(hero.Position,new V2(0,-34))<.01f,"Respawn at base");
            Check(world.DrainEvents().Any(e=>e.Kind==EventKind.Respawn&&e.ActorId==hero.Id),"Respawn event");
            Debug.Log("CELL REAL RESPAWN PASSED | 0/736 HP, 15.5 simulated seconds, identity, inventory, base and HP/Ki restored");
        }
        public static void CameraControls(ArenaCameraController controller,Vector3 hero)
        {
            if(controller==null) return; // The original scene keeps its baseline camera.
            var camera = controller.GetComponent<Camera>();
            float zoom = camera.orthographicSize;
            controller.Zoom(1000); Check(camera.orthographicSize==controller.minimumZoom,"Minimum zoom");
            controller.Zoom(-1000); Check(camera.orthographicSize==controller.maximumZoom,"Maximum zoom");
            controller.Pan(new Vector2(1000,-1000));
            controller.Tick(hero,false);
            Check(!controller.Following,"Free camera");
            Check(controller.Focus==controller.ClampFocus(controller.Focus),"Viewport stays inside map");
            controller.Zoom(camera.orthographicSize-zoom);
            controller.Recenter(hero,true);
            Check(controller.Following&&Vector3.Distance(controller.Focus,controller.ClampFocus(hero))<.001f,"Recenter and follow");
            var before=controller.Focus; controller.Pan(new Vector2(3,3)); controller.Tick(hero,false);
            Check(controller.Focus!=before,"Pan moves camera");
            controller.Recenter(hero,true);
            var minimapFocus=new Vector3(-9f,0f,12f);
            controller.FocusAt(minimapFocus,true);
            Check(!controller.Following &&
                Vector3.Distance(controller.Focus,controller.ClampFocus(minimapFocus))<.001f,
                "Minimap click sets exact free-camera focus");
            controller.Recenter(hero,true);
            Check(Math.Abs(camera.transform.eulerAngles.x-55.5f)<.1f,"55.5 degree strategic camera");
            Check(!camera.orthographic&&Math.Abs(camera.fieldOfView-40)<.1f,"Strategic perspective projection");
            Debug.Log("ARENA CAMERA PASSED | perspective 55.5 degrees, zoom bounds, pan, map bounds, recenter and follow");
        }
        static void Check(bool ok,string message) { if(!ok) throw new Exception(message); }
    }
}
#endif

