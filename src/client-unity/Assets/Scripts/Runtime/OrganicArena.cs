using System;
using System.Collections.Generic;
using DragonMoba.Domain;
using UnityEngine;
using UnityEngine.Rendering;
namespace DragonMoba.Runtime
{
    // Optional replacement for the art scene. No movement, damage or progression rules here.
    public sealed class OrganicArena : MonoBehaviour
    {
        public Material ground, water, stone, energy, wood, ring;
        public GameObject tree, bush, rock;
        public string[] formIds;
        public GameObject[] characterForms;
        public bool startDemo = true;
        public const string Version = "VISUAL TEST 01 - Organic Arena";
        public int PlantCount { get; private set; }
        public int TerrainVertices { get; private set; }
        readonly List<Mesh> meshes = new List<Mesh>();
        Transform scenery;
        public GameObject Character(EntityState entity)
        {
            if(entity.HeroId!="GKU_SUPER" || formIds==null) return null;
            int index=Array.IndexOf(formIds,entity.FormId);
            return index>=0&&index<characterForms.Length ? characterForms[index] : null;
        }
        public static float RiverCenter(float x) { return .65f*Mathf.Sin(x*.14f); }
        public static float RiverWidth(float x) { return 2.5f+.35f*Mathf.Sin(x*.23f+1); }
        public static float RoadDistance(float x,float z)
        {
            float best=100;
            foreach(float lane in new[]{-17f,0f,17f})
            {
                float curve=.75f*Mathf.Sin(z*.15f+lane*.1f)*Mathf.Sin((z+44)/88*Mathf.PI);
                best=Mathf.Min(best,Mathf.Abs(x-lane-curve)-3.1f);
            }
            best=Mathf.Min(best,new Vector2(x,z+35).magnitude-7.1f);
            return Mathf.Min(best,new Vector2(x,z-35).magnitude-7.1f);
        }
        public void Build(List<GameObject> old)
        {
            foreach(var item in old) { var renderer=item.GetComponent<Renderer>(); if(renderer!=null) renderer.enabled=false; }
            if(scenery!=null) return;
            scenery=new GameObject("Organic battlefield - runtime replacement").transform;
            scenery.SetParent(transform,false);
            CreateTerrain();
            CreateRiver();
            foreach(float lane in new[]{-17f,0f,17f}) Bridge(lane);
            foreach(int side in new[]{-1,1}) Base(side);
            var random=new System.Random(621016);
            int count=0;
            for(int attempt=0;attempt<1800&&count<125;attempt++)
            {
                float x=(float)random.NextDouble()*63-31.5f,z=(float)random.NextDouble()*80-40;
                if(RoadDistance(x,z)<1.3f || Mathf.Abs(z-RiverCenter(x))<RiverWidth(x)+1.1f) continue;
                var prefab=count%5==0?rock:count%3==0?bush:tree;
                if(prefab==null) continue;
                var go=Instantiate(prefab,new Vector3(x,0,z),Quaternion.Euler(0,(float)random.NextDouble()*360,0),scenery);
                go.name="Jungle "+prefab.name;
                go.transform.localScale*=.52f+(float)random.NextDouble()*.4f;
                count++;
            }
            PlantCount=count;
            for(int i=0;i<38;i++)
            {
                float x=-34+i*1.84f;
                if(Mathf.Min(Mathf.Abs(x),Mathf.Min(Mathf.Abs(x-17),Mathf.Abs(x+17)))<4.7f) continue;
                foreach(float side in new[]{-1f,1f})
                    if(rock!=null)
                    {
                        var go=Instantiate(rock,new Vector3(x,-.05f,RiverCenter(x)+side*(RiverWidth(x)+.4f)),Quaternion.Euler(0,i*61,0),scenery);
                        go.name="River bank rock"; go.transform.localScale*=.55f+(i%3)*.16f;
                    }
            }
            foreach(var light in UnityEngine.Object.FindObjectsByType<Light>())
                if(light.type==LightType.Directional) { light.shadows=LightShadows.Soft; light.shadowStrength=.55f; light.intensity=1.05f; }
            RenderSettings.ambientLight=new Color(.42f,.48f,.52f);
            QualitySettings.shadowDistance=75;
            Debug.Log("ORGANIC MAP ACTIVE | vertices="+TerrainVertices+" | plants="+PlantCount+" | original colliders retained");
        }
        void CreateTerrain()
        {
            const int nx=145,nz=177;
            var vertices=new Vector3[nx*nz]; var colors=new Color[vertices.Length];
            var uv=new Vector2[vertices.Length]; var triangles=new int[(nx-1)*(nz-1)*6];
            for(int z=0;z<nz;z++) for(int x=0;x<nx;x++)
            {
                int index=z*nx+x; float wx=-36+x*.5f,wz=-44+z*.5f;
                float road=1-Mathf.SmoothStep(-.55f,.8f,RoadDistance(wx,wz)+(Mathf.PerlinNoise(wx*.6f+50,wz*.6f+50)-.5f)*.9f);
                float bank=Mathf.Abs(wz-RiverCenter(wx))-RiverWidth(wx);
                float height=-.07f;
                // Relief is outside the logical movement rectangle. Walking/hitboxes remain flat.
                float rim=Mathf.Max(Mathf.Abs(wx)-32,Mathf.Abs(wz)-41);
                if(rim>0) height+=Mathf.SmoothStep(0,3,rim)*(1.1f+Mathf.PerlinNoise(wx*.25f+20,wz*.25f+20)*1.8f);
                if(bank<0) height-=.45f;
                float noise=Mathf.PerlinNoise(wx*.12f+100,wz*.12f+100);
                Color grass=Color.Lerp(new Color(.11f,.23f,.16f),new Color(.23f,.34f,.18f),noise);
                Color path=new Color(.48f,.40f,.28f);
                Color color=Color.Lerp(grass,path,road);
                if(bank<1.05f) color=Color.Lerp(new Color(.27f,.33f,.28f),new Color(.43f,.43f,.31f),Mathf.Clamp01(bank));
                vertices[index]=new Vector3(wx,height,wz); colors[index]=color; uv[index]=new Vector2(wx,wz);
            }
            int t=0;
            for(int z=0;z<nz-1;z++) for(int x=0;x<nx-1;x++)
            {
                int a=z*nx+x,b=a+1,c=a+nx,d=c+1;
                triangles[t++]=a;triangles[t++]=c;triangles[t++]=b;
                triangles[t++]=b;triangles[t++]=c;triangles[t++]=d;
            }
            var mesh=NewMesh("Organic terrain",vertices,triangles,uv,colors);
            AddMesh("OrganicTerrain",mesh,ground,scenery);
            TerrainVertices=vertices.Length;
        }
        void CreateRiver()
        {
            const int count=145;
            var vertices=new Vector3[count*2];var uv=new Vector2[vertices.Length];var indices=new int[(count-1)*6];
            for(int i=0;i<count;i++)
            {
                float x=-36+i*.5f,z=RiverCenter(x),width=RiverWidth(x);
                vertices[i*2]=new Vector3(x,-.025f,z-width);vertices[i*2+1]=new Vector3(x,-.025f,z+width);
                uv[i*2]=new Vector2(i*.08f,0);uv[i*2+1]=new Vector2(i*.08f,1);
                if(i<count-1){int t=i*6,a=i*2;indices[t]=a;indices[t+1]=a+1;indices[t+2]=a+2;indices[t+3]=a+1;indices[t+4]=a+3;indices[t+5]=a+2;}
            }
            AddMesh("Flowing river with curved banks",NewMesh("River",vertices,indices,uv,null),water,scenery);
        }
        void Bridge(float x)
        {
            var root=new GameObject("Bridge lane "+x).transform;root.SetParent(scenery,false);
            root.localPosition=new Vector3(x,0,RiverCenter(x));
            for(int i=0;i<12;i++) Piece(root,"Timber deck",PrimitiveType.Cube,new Vector3(0,.035f,-3.4f+i*.62f),new Vector3(6.5f,.12f,.55f),wood);
            foreach(float side in new[]{-1f,1f})
            {
                Piece(root,"Stone bridge edge",PrimitiveType.Cube,new Vector3(side*3.4f,.3f,0),new Vector3(.22f,.65f,7.2f),stone);
                foreach(float z in new[]{-3.6f,3.6f})
                    Piece(root,"Bridge pillar",PrimitiveType.Cylinder,new Vector3(side*3.45f,.35f,z),new Vector3(.55f,.5f,.55f),stone);
            }
        }
        void Base(int side)
        {
            var root=new GameObject(side<0?"Blue sanctuary":"Red sanctuary").transform;root.SetParent(scenery,false);
            root.localPosition=new Vector3(0,-.1f,side*35);
            Piece(root,"Circular sanctuary paving",PrimitiveType.Cylinder,Vector3.zero,new Vector3(13,.1f,13),stone);
            Circle(root,"Team energy sigil",5.3f,.04f,side<0?new Color(.12f,.85f,1):new Color(1,.25f,.2f));
            Circle(root,"Inner sanctuary ring",4.65f,.05f,new Color(.4f,.65f,.62f));
        }
        public void DecorateEntity(GameObject entity,EntityState state)
        {
            if(state.Kind!=EntityKind.Tower&&state.Kind!=EntityKind.Core) return;
            entity.GetComponent<Renderer>().enabled=false;
            var root=new GameObject("Energy sentinel architecture").transform;root.SetParent(entity.transform,false);
            if(state.Kind==EntityKind.Core) root.localScale=Vector3.one*.55f;
            Piece(root,"Octagonal foundation",PrimitiveType.Cylinder,new Vector3(0,-.88f,0),new Vector3(1.5f,.12f,1.5f),stone);
            Piece(root,"Foot",PrimitiveType.Cylinder,new Vector3(0,-.62f,0),new Vector3(1.1f,.14f,1.1f),stone);
            Piece(root,"Pillar",PrimitiveType.Cylinder,new Vector3(0,-.1f,0),new Vector3(.65f,.5f,.65f),stone);
            Piece(root,"Capital",PrimitiveType.Cylinder,new Vector3(0,.45f,0),new Vector3(1.05f,.1f,1.05f),stone);
            Color team=state.Team==Team.Blue?new Color(.1f,.85f,1):new Color(1,.22f,.16f);
            var crystal=AddMesh("Energy prism",Prism(),energy,root);
            crystal.transform.localPosition=new Vector3(0,.88f,0);crystal.transform.localScale=new Vector3(.4f,.6f,.4f);
            var block=new MaterialPropertyBlock();block.SetColor("_Color",team);block.SetColor("_EmissionColor",team*.7f);
            crystal.GetComponent<Renderer>().SetPropertyBlock(block);
            for(int i=0;i<4;i++)
            {
                float angle=i*Mathf.PI*.5f;
                var fin=Piece(root,"Armored fin",PrimitiveType.Cube,new Vector3(Mathf.Cos(angle)*.46f,-.05f,Mathf.Sin(angle)*.46f),new Vector3(.16f,1.18f,.26f),stone);
                fin.transform.localRotation=Quaternion.Euler(0,-i*90,0);
            }
            Circle(root,"Team marker",.61f,.47f,team);
        }
        Mesh Prism()
        {
            return NewMesh("Prism",new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back},
                new[]{0,4,3,0,3,5,0,5,2,0,2,4,1,3,4,1,5,3,1,2,5,1,4,2},null,null);
        }
        void Circle(Transform parent,string name,float radius,float y,Color color)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            var line=go.AddComponent<LineRenderer>();line.sharedMaterial=ring;line.useWorldSpace=false;line.loop=true;
            line.positionCount=64;line.startWidth=line.endWidth=.055f;line.startColor=line.endColor=color;
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius));}
        }
        GameObject Piece(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            // Primitive collider exists only until the end of this frame, never used for navigation.
            var go=GameObject.CreatePrimitive(type);Destroy(go.GetComponent<Collider>());
            go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }
        GameObject AddMesh(string name,Mesh mesh,Material material,Transform parent)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;return go;
        }
        Mesh NewMesh(string name,Vector3[] vertices,int[] triangles,Vector2[] uv,Color[] colors)
        {
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;
            if(uv!=null)mesh.uv=uv;if(colors!=null)mesh.colors=colors;mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);return mesh;
        }
        void OnGUI()
        {
            GUI.color=Color.white;
            GUI.Box(new Rect(Screen.width-430,Screen.height-31,420,25),Version+" | "+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);}
    }
}

