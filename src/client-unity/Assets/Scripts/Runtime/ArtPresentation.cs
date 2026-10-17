using System.Collections.Generic;
using DragonMoba.Domain;
using UnityEngine;
namespace DragonMoba.Runtime
{
    // Optional scene component. Owns decoration, bounded effects and sound only.
    public sealed class ArtPresentation : MonoBehaviour
    {
        public GameObject tree, bush, rock;
        public Material stoneMaterial, energyMaterial;
        public Texture2D panel, bar;
        public Texture2D[] keys;
        public AudioClip laser, impact, charge;
        public const int EffectLimit = 24;
        private readonly List<LineRenderer> effects = new List<LineRenderer>();
        private readonly List<float> expiry = new List<float>();
        private Material effectMaterial;
        private AudioSource source;
        private float nextSound;
        private int nextEffect;
        private GUISkin skin;
        private readonly Dictionary<AudioClip,float> gain = new Dictionary<AudioClip,float>();
        public int EffectCount { get { return effects.Count; } }
        private void Awake()
        {
            effectMaterial = new Material(Shader.Find("Sprites/Default"));
            source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend=0;
            foreach(var clip in new[]{laser,impact,charge})
            {
                if(clip==null) continue;
                var samples=new float[clip.samples*clip.channels];
                float peak=0;
                if(clip.GetData(samples,0)) foreach(float value in samples) peak=Mathf.Max(peak,Mathf.Abs(value));
                gain[clip]=Mathf.Min(.3f,.12f/Mathf.Max(.01f,peak));
            }
            RenderSettings.ambientLight = new Color(.52f,.57f,.64f);
        }
        public GUISkin Skin(GUISkin original)
        {
            if(skin!=null) return skin;
            skin=Instantiate(original);
            skin.box.normal.background=panel;
            skin.box.border=new RectOffset(12,12,12,12);
            skin.box.normal.textColor=Color.white;
            return skin;
        }
        public void DrawKeys()
        {
            if(keys==null) return;
            for(int i=0;i<keys.Length;i++) if(keys[i]!=null)
                GUI.DrawTexture(new Rect(Screen.width*.5f-206+i*105,Screen.height-120,24,24),keys[i],ScaleMode.ScaleToFit);
        }
        public void BuildEnvironment(List<GameObject> existing)
        {
            var organic = GetComponent<OrganicArena>();
            if (organic != null) { organic.Build(existing); return; }
            foreach(var item in existing)
                if(item.name=="Floresta")
                {
                    item.GetComponent<Renderer>().enabled=false;
                    Plant(tree,item.transform.position-new Vector3(0,1.1f,0),.8f);
                    Plant(bush,item.transform.position+new Vector3(1.6f,-1.1f,2),1.4f);
                    Plant(rock,item.transform.position+new Vector3(-1.4f,-1.1f,-2),1.1f);
                }
            foreach(float x in new[]{-29f,29f})
                foreach(float z in new[]{-28f,-15f,12f,27f}) Plant(tree,new Vector3(x,0,z),1.1f);
        }
        void Plant(GameObject asset,Vector3 position,float scale)
        {
            if(asset==null) return;
            var go=Instantiate(asset,position,Quaternion.Euler(0,position.z*7f,0),transform);
            go.transform.localScale=Vector3.one*scale;
        }
        public void DecorateEntity(GameObject root,EntityState entity)
        {
            if(entity.Kind!=EntityKind.Tower && entity.Kind!=EntityKind.Core) return;
            var renderer=root.GetComponent<Renderer>(); if(renderer!=null) renderer.enabled=false;
            Piece(root,"Pedestal",PrimitiveType.Cylinder,new Vector3(0,-.75f,0),new Vector3(1.5f,.25f,1.5f),stoneMaterial);
            Piece(root,"Column",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.65f,.65f,.65f),stoneMaterial);
            var crystal=Piece(root,"Energy crystal",PrimitiveType.Cube,new Vector3(0,.8f,0),Vector3.one*.65f,energyMaterial);
            crystal.transform.localRotation=Quaternion.Euler(0,45,35);
            var block=new MaterialPropertyBlock(); block.SetColor("_Color",entity.Team==Team.Blue?Color.cyan:new Color(1,.2f,.3f));
            crystal.GetComponent<Renderer>().SetPropertyBlock(block);
        }
        static GameObject Piece(GameObject root,string label,PrimitiveType type,Vector3 pos,Vector3 size,Material material)
        {
            var go=GameObject.CreatePrimitive(type); go.name=label; go.transform.SetParent(root.transform,false);
            go.transform.localPosition=pos; go.transform.localScale=size;
            Destroy(go.GetComponent<Collider>()); go.GetComponent<Renderer>().sharedMaterial=material; return go;
        }
        public void Confirm()
        {
            if(charge!=null && Time.time>=nextSound)
            { source.PlayOneShot(charge,gain[charge]); nextSound=Time.time+.16f; }
        }
        public void Emit(GameEvent e,GameWorld world)
        {
            if(e.Kind==EventKind.Purchase) { Confirm(); return; }
            if(e.Kind!=EventKind.Attack && e.Kind!=EventKind.Ability && e.Kind!=EventKind.Damage &&
               e.Kind!=EventKind.Transform && e.Kind!=EventKind.Charge) return;
            var actor=world.Find(e.ActorId);
            bool beam=e.Kind==EventKind.Attack||e.Kind==EventKind.Ability;
            var center=new Vector3(e.Position.X,1,e.Position.Y);
            LineRenderer line;
            int slot;
            if(effects.Count<EffectLimit)
            {
                slot=effects.Count; var go=new GameObject("Pooled energy effect"); go.transform.SetParent(transform,false);
                line=go.AddComponent<LineRenderer>(); line.sharedMaterial=effectMaterial;
                line.useWorldSpace=true; line.numCapVertices=2; effects.Add(line); expiry.Add(0);
            }
            else { slot=nextEffect++%EffectLimit; line=effects[slot]; }
            Color color=e.Kind==EventKind.Transform?Color.yellow:e.Kind==EventKind.Damage?new Color(1,.55f,.2f):Color.cyan;
            line.startColor=line.endColor=color; line.startWidth=line.endWidth=beam?.13f:.09f;
            line.loop=!beam; line.positionCount=beam?2:24;
            if(beam)
            {
                line.SetPosition(0,actor==null?center:new Vector3(actor.Position.X,1,actor.Position.Y));
                var target=world.Find(e.TargetId);
                line.SetPosition(1,e.Kind==EventKind.Attack&&target!=null?new Vector3(target.Position.X,1,target.Position.Y):center);
            }
            else for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI*2/24, radius=e.Kind==EventKind.Transform?2f:.55f;
                line.SetPosition(i,center+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));
            }
            expiry[slot]=Time.time+(e.Kind==EventKind.Transform?.8f:.22f); line.enabled=true;
            if(Time.time>=nextSound && (e.ActorId==world.PlayerId||e.TargetId==world.PlayerId))
            {
                var clip=e.Kind==EventKind.Damage?impact:e.Kind==EventKind.Charge?charge:laser;
                if(clip!=null) { source.PlayOneShot(clip,gain[clip]); nextSound=Time.time+.16f; }
            }
        }
        void Update() { for(int i=0;i<effects.Count;i++) if(Time.time>expiry[i]) effects[i].enabled=false; }
        void OnDestroy() { if(effectMaterial!=null) Destroy(effectMaterial); if(skin!=null) Destroy(skin); }
    }
}
