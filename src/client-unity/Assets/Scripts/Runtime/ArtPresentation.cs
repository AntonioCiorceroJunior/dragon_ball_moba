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
        private readonly List<LineRenderer> effectCores = new List<LineRenderer>();
        private readonly List<float> expiry = new List<float>();
        private Material effectMaterial;
        private AudioSource source;
        private BattleSoundscape originalSound;
        private Texture2D panelDark, buttonDark, buttonHover;
        private float nextSound;
        private int nextEffect;
        private GUISkin skin;
        private readonly Dictionary<AudioClip,float> gain = new Dictionary<AudioClip,float>();
        public int EffectCount { get { return effects.Count; } }
        private void Awake()
        {
            effectMaterial = new Material(Shader.Find("Sprites/Default"));
            originalSound = GetComponent<BattleSoundscape>();
            if(originalSound==null) originalSound = gameObject.AddComponent<BattleSoundscape>();
            originalSound.SetExistingClips(laser,impact,charge);
            source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend=0;
            foreach(var clip in new[]{laser,impact,charge})
            {
                if(clip==null) continue;
                var samples=new float[clip.samples*clip.channels];
                float peak=0;
                if(clip.GetData(samples,0)) foreach(float value in samples) peak=Mathf.Max(peak,Mathf.Abs(value));
                gain[clip]=Mathf.Min(.3f,.12f/Mathf.Max(.01f,peak));
            }
            RenderSettings.ambientLight = new Color(.43f,.52f,.57f);
        }
        // A consistent, readable dark Capsule Corp-inspired UI replaces generic
        // flat boxes. Textures are procedural and original; no external graphics.
        private static Texture2D PanelTexture(int size, Color upper, Color lower,
            Color frame, int edge)
        {
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
            tex.name="Original dark UI panel";
            tex.filterMode=FilterMode.Bilinear;
            tex.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<size;y++)
            for(int x=0;x<size;x++)
            {
                var t=1f-y/(float)(size-1);
                var fill=Color.Lerp(upper,lower,t);
                var border=x<edge||y<edge||x>=size-edge||y>=size-edge;
                var corner=(x<6&&y<6)||(x<6&&y>=size-6)||
                    (x>=size-6&&y<6)||(x>=size-6&&y>=size-6);
                tex.SetPixel(x,y,corner?new Color(0,0,0,0):
                    border?frame:fill);
            }
            tex.Apply(false,true);
            return tex;
        }
        public GUISkin Skin(GUISkin original)
        {
            if(skin!=null)return skin;
            skin=Instantiate(original);
            panelDark=PanelTexture(48,new Color(.055f,.105f,.14f,.95f),
                new Color(.08f,.15f,.18f,.95f),new Color(.33f,.46f,.43f,.96f),2);
            buttonDark=PanelTexture(40,new Color(.14f,.23f,.27f,1f),
                new Color(.09f,.17f,.21f,1f),new Color(.44f,.61f,.59f,1f),2);
            buttonHover=PanelTexture(40,new Color(.20f,.37f,.42f,1f),
                new Color(.12f,.23f,.28f,1f),new Color(.89f,.73f,.41f,1f),2);
            skin.box.normal.background=panelDark;
            skin.box.border=new RectOffset(7,7,7,7);
            skin.box.normal.textColor=new Color(.91f,.95f,.94f);
            skin.box.fontStyle=FontStyle.Bold;
            skin.box.fontSize=12;
            skin.box.padding=new RectOffset(9,9,6,6);
            skin.button.normal.background=buttonDark;
            skin.button.hover.background=buttonHover;
            skin.button.active.background=buttonHover;
            skin.button.onNormal.background=buttonDark;
            skin.button.normal.textColor=Color.white;
            skin.button.hover.textColor=new Color(1f,.91f,.64f);
            skin.button.active.textColor=Color.white;
            skin.button.alignment=TextAnchor.MiddleCenter;
            skin.button.fontStyle=FontStyle.Bold;
            skin.button.fontSize=12;
            skin.button.border=new RectOffset(7,7,7,7);
            skin.label.normal.textColor=new Color(.92f,.96f,.96f);
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
            if(originalSound!=null) { originalSound.Confirm(); return; }
            if(charge!=null && Time.time>=nextSound)
            { source.PlayOneShot(charge,gain[charge]); nextSound=Time.time+.16f; }
        }
        public void Emit(GameEvent e,GameWorld world)
        {
            if(originalSound!=null) originalSound.React(e,world);
            if(e.Kind==EventKind.Purchase)
            {
                if(originalSound==null) Confirm();
                return;
            }
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
                line.useWorldSpace=true; line.numCapVertices=6; effects.Add(line); expiry.Add(0);
                var coreRoot=new GameObject("Inner Ki glow");
                coreRoot.transform.SetParent(go.transform,false);
                var core=coreRoot.AddComponent<LineRenderer>();
                core.sharedMaterial=effectMaterial;
                core.useWorldSpace=true;
                core.numCapVertices=6;
                effectCores.Add(core);
            }
            else { slot=nextEffect++%EffectLimit; line=effects[slot]; }
            var coreLine=effectCores[slot];
            var color=e.Kind==EventKind.Transform?new Color(1f,.71f,.25f,.92f):
                e.Kind==EventKind.Damage?new Color(1f,.27f,.12f,.89f):
                e.Kind==EventKind.Charge?new Color(.22f,.77f,1f,.90f):
                new Color(.12f,.64f,1f,.89f);
            line.startColor=line.endColor=color;
            line.startWidth=line.endWidth=beam?.28f:.115f;
            line.loop=!beam;
            line.positionCount=beam?2:40;
            coreLine.startColor=coreLine.endColor=new Color(1f,.96f,.81f,.9f);
            coreLine.startWidth=coreLine.endWidth=beam?.07f:.035f;
            coreLine.loop=!beam;
            coreLine.positionCount=beam?2:40;
            if(beam)
            {
                var source=actor==null?center:new Vector3(actor.Position.X,1.15f,actor.Position.Y);
                var target=world.Find(e.TargetId);
                var destination=e.Kind==EventKind.Attack&&target!=null?
                    new Vector3(target.Position.X,1.05f,target.Position.Y):center;
                line.SetPosition(0,source);line.SetPosition(1,destination);
                coreLine.SetPosition(0,source);coreLine.SetPosition(1,destination);
            }
            else for(int i=0;i<40;i++)
            {
                float a=i*Mathf.PI*2/40f, radius=e.Kind==EventKind.Transform?2.4f:
                    e.Kind==EventKind.Damage?.75f:1.05f;
                var at=center+new Vector3(Mathf.Cos(a)*radius,.11f,Mathf.Sin(a)*radius);
                line.SetPosition(i,at);
                coreLine.SetPosition(i,at+Vector3.up*.025f);
            }
            expiry[slot]=Time.time+(e.Kind==EventKind.Transform?.8f:.25f);
            line.enabled=true;
            coreLine.enabled=true;
            if(originalSound==null && Time.time>=nextSound &&
               (e.ActorId==world.PlayerId||e.TargetId==world.PlayerId))
            {
                var clip=e.Kind==EventKind.Damage?impact:e.Kind==EventKind.Charge?charge:laser;
                if(clip!=null) { source.PlayOneShot(clip,gain[clip]); nextSound=Time.time+.16f; }
            }
        }
        void Update()
        {
            for(int i=0;i<effects.Count;i++)
                if(Time.time>expiry[i])
                {
                    effects[i].enabled=false;
                    effectCores[i].enabled=false;
                }
        }
        void OnDestroy()
        {
            if(effectMaterial!=null) Destroy(effectMaterial);
            if(skin!=null) Destroy(skin);
            if(panelDark!=null) Destroy(panelDark);
            if(buttonDark!=null) Destroy(buttonDark);
            if(buttonHover!=null) Destroy(buttonHover);
        }
    }
}
