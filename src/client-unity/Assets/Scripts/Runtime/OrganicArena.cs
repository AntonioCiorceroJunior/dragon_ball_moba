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
        readonly List<Material> runtimeMaterials = new List<Material>();
        Texture2D terrainPaint;
        Material glowBlue, glowRed, groundPaint, riverPaint;
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
            CreateVisualMaterials();
            CreateTerrain();
            CreateRiver();
            foreach(float lane in new[]{-17f,0f,17f}) Bridge(lane);
            foreach(int side in new[]{-1,1}) Base(side);
            AddArchitecturalLandmarks();
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
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.40f,.49f,.54f);
            RenderSettings.fog=true;
            RenderSettings.fogColor=new Color(.12f,.20f,.23f);
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=50f;
            RenderSettings.fogEndDistance=115f;
            QualitySettings.shadowDistance=72f;
            Debug.Log("ARENA ART PASS 02 | painted terrain, river, landmarks | vertices="+TerrainVertices+
                " | plants="+PlantCount+" | no gameplay colliders added");
        }
        void CreateVisualMaterials()
        {
            // Runtime material instances are deliberately separate from imported assets.
            groundPaint = new Material(ground != null ? ground : new Material(Shader.Find("Standard")));
            groundPaint.name = "Original hand-painted arena terrain";
            groundPaint.color = Color.white;
            runtimeMaterials.Add(groundPaint);
            riverPaint = new Material(water != null ? water : new Material(Shader.Find("Standard")));
            riverPaint.name = "Translucent teal river";
            riverPaint.color = new Color(.20f,.66f,.73f,1f);
            if(riverPaint.HasProperty("_Metallic")) riverPaint.SetFloat("_Metallic",.42f);
            if(riverPaint.HasProperty("_Glossiness")) riverPaint.SetFloat("_Glossiness",.85f);
            runtimeMaterials.Add(riverPaint);
            glowBlue = GlowMaterial(new Color(.04f,.72f,1f), "Allied energy");
            glowRed = GlowMaterial(new Color(1f,.22f,.12f), "Enemy energy");
        }
        Material GlowMaterial(Color tint,string label)
        {
            var m=new Material(Shader.Find("Standard")) { name=label, color=tint };
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor",tint*2f);
            runtimeMaterials.Add(m);
            return m;
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
                var grass=Color.Lerp(new Color(.08f,.19f,.13f),new Color(.22f,.34f,.21f),noise);
                var sandstone=new Color(.51f,.43f,.29f);
                var stoneSpeckle=(Mathf.PerlinNoise(wx*1.6f+14,wz*1.6f+8)-.5f)*.10f;
                sandstone+=new Color(stoneSpeckle,stoneSpeckle,stoneSpeckle,0);
                var color=Color.Lerp(grass,sandstone,road);
                if(road>.72f && Mathf.PerlinNoise(wx*.7f+30,wz*.9f+11)>.75f)
                    color=Color.Lerp(color,new Color(.30f,.29f,.26f),.3f);
                if(bank<1.25f) color=Color.Lerp(new Color(.20f,.30f,.29f),
                    new Color(.45f,.44f,.33f),Mathf.Clamp01(bank/1.25f));
                vertices[index]=new Vector3(wx,height,wz);
                colors[index]=color;
                // UVs match the baked vertex color texture: the original material
                // did not necessarily render Mesh.colors, leaving flat-looking ground.
                uv[index]=new Vector2((float)x/(nx-1),(float)z/(nz-1));
            }
            int t=0;
            for(int z=0;z<nz-1;z++) for(int x=0;x<nx-1;x++)
            {
                int a=z*nx+x,b=a+1,c=a+nx,d=c+1;
                triangles[t++]=a;triangles[t++]=c;triangles[t++]=b;
                triangles[t++]=b;triangles[t++]=c;triangles[t++]=d;
            }
            var mesh=NewMesh("Organic terrain",vertices,triangles,uv,colors);
            // Paint at a higher frequency than the navigational mesh. The
            // original shader already supports vertex colors; the new optional
            // color map introduces stone seams, worn paths and grass variation
            // without changing collision or mesh density.
            const int texW=960,texH=1200;
            terrainPaint=new Texture2D(texW,texH,TextureFormat.RGB24,true);
            terrainPaint.name="Original detailed arena ground paint";
            terrainPaint.wrapMode=TextureWrapMode.Clamp;
            terrainPaint.filterMode=FilterMode.Trilinear;
            var pixels=new Color[texW*texH];
            for(var ty=0;ty<texH;ty++)
            for(var tx=0;tx<texW;tx++)
            {
                var wx=-36f+72f*tx/(texW-1f);
                var wz=-44f+88f*ty/(texH-1f);
                pixels[ty*texW+tx]=PaintSurface(wx,wz);
            }
            terrainPaint.SetPixels(pixels);
            terrainPaint.Apply(true,false);
            groundPaint.mainTexture=terrainPaint;
            if(groundPaint.HasProperty("_UseTexture"))groundPaint.SetFloat("_UseTexture",1f);
            AddMesh("OrganicTerrain",mesh,groundPaint,scenery);
            TerrainVertices=vertices.Length;
        }
        private static Color PaintSurface(float wx,float wz)
        {
            // 3 original curved lane strips with high-resolution stone wear.
            float nearest=100f;
            for(var index=-1;index<=1;index++)
            {
                float lane=index*17f;
                float curve=.75f*Mathf.Sin(wz*.15f+lane*.1f)*
                    Mathf.Sin((wz+44f)/88f*Mathf.PI);
                nearest=Mathf.Min(nearest,Mathf.Abs(wx-lane-curve)-3.1f);
            }
            nearest=Mathf.Min(nearest,new Vector2(wx,wz+35f).magnitude-7.1f);
            nearest=Mathf.Min(nearest,new Vector2(wx,wz-35f).magnitude-7.1f);
            var grassNoise=Mathf.PerlinNoise(wx*.13f+100,wz*.13f+100);
            var leafGrain=Mathf.PerlinNoise(wx*2.1f+25,wz*2.1f+18);
            var grass=Color.Lerp(new Color(.10f,.22f,.15f),
                new Color(.26f,.37f,.22f),grassNoise);
            grass*=.92f+leafGrain*.16f;
            var pathNoise=Mathf.PerlinNoise(wx*.95f+19,wz*.95f+20);
            var sandstone=Color.Lerp(new Color(.41f,.34f,.23f),
                new Color(.61f,.52f,.35f),pathNoise);
            var stoneX=wx*1.35f+Mathf.Floor(wz*1.15f)*.43f;
            var stoneZ=wz*1.15f;
            var seams=Mathf.Min(Mathf.Abs(Mathf.Repeat(stoneX,1f)-.5f),
                Mathf.Abs(Mathf.Repeat(stoneZ,1f)-.5f));
            if(seams<.045f)sandstone*=.74f;
            var edgeGrain=(Mathf.PerlinNoise(wx*.65f+37,wz*.65f+82)-.5f)*.55f;
            var road=1f-Mathf.SmoothStep(-.55f,.92f,nearest+edgeGrain);
            var color=Color.Lerp(grass,sandstone,road);
            var bank=Mathf.Abs(wz-RiverCenter(wx))-RiverWidth(wx);
            if(bank<1.5f)
            {
                var shore=Color.Lerp(new Color(.18f,.30f,.30f),
                    new Color(.46f,.45f,.33f),Mathf.Clamp01(bank/1.5f));
                color=Color.Lerp(color,shore,1f-Mathf.Clamp01(Mathf.Abs(bank)/1.5f));
            }
            return color;
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
            AddMesh("Flowing river with curved banks",NewMesh("River",vertices,indices,uv,null),riverPaint,scenery);
            // Thin, translucent-looking energy traces are purely visual river accents.
            for(var side=0;side<2;side++)
            {
                var lineRoot=new GameObject(side==0?"Northern river gleam":"Southern river gleam");
                lineRoot.transform.SetParent(scenery,false);
                var line=lineRoot.AddComponent<LineRenderer>();
                line.sharedMaterial=ring;
                line.positionCount=72;
                line.startWidth=line.endWidth=.10f;
                line.startColor=line.endColor=new Color(.27f,.84f,.85f,.58f);
                for(var i=0;i<72;i++)
                {
                    var x=-35f+i;
                    var z=RiverCenter(x)+(side==0?1f:-1f)*(RiverWidth(x)-.34f);
                    line.SetPosition(i,new Vector3(x,.045f,z));
                }
            }
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
        void AddArchitecturalLandmarks()
        {
            // A few distinctive landmarks make the silhouette of the battlefield
            // readable without imposing invisible walls on the domain.
            foreach(var lane in new[]{-17f,0f,17f})
            {
                foreach(var side in new[]{-1f,1f})
                {
                    var z=side*8f;
                    var x=lane+(lane==0?6.8f:(lane<0?-6.4f:6.4f));
                    var root=new GameObject("Ancient lookout at "+lane+" "+side).transform;
                    root.SetParent(scenery,false);
                    root.localPosition=new Vector3(x,0,z);
                    Piece(root,"Broken ring of sandstone",PrimitiveType.Cylinder,new Vector3(0,.04f,0),
                        new Vector3(1.75f,.12f,1.75f),stone);
                    for(int j=0;j<3;j++)
                    {
                        var theta=(j/3f)*Mathf.PI*2f;
                        var height=j==1?1.7f:2.4f;
                        Piece(root,"Weathered pillar",PrimitiveType.Cylinder,
                            new Vector3(Mathf.Cos(theta)*1.20f,height*.48f,Mathf.Sin(theta)*1.2f),
                            new Vector3(.33f,height*.5f,.33f),stone);
                    }
                    var orb=Piece(root,"Floating Ki lantern",PrimitiveType.Sphere,
                        new Vector3(0,2.85f,0),Vector3.one*.62f,side<0?glowBlue:glowRed);
                    orb.AddComponent<EnergyPulseVisual>();
                }
            }
            foreach(var side in new[]{-1f,1f})
            {
                var root=new GameObject(side<0?"Capsule-blue shrine":"Red army shrine").transform;
                root.SetParent(scenery,false);
                root.localPosition=new Vector3(0,0,side*36.5f);
                for(int i=0;i<6;i++)
                {
                    var theta=i*Mathf.PI/3f;
                    var radial=new Vector3(Mathf.Cos(theta)*6f,0,Mathf.Sin(theta)*4.8f);
                    Piece(root,"Ancient monolith",PrimitiveType.Cube,
                        radial+new Vector3(0,.82f,0),new Vector3(.75f,1.65f,.75f),stone);
                }
                var lamp=Piece(root,"Sanctuary heart",PrimitiveType.Sphere,
                    new Vector3(0,1.75f,0),Vector3.one*1.35f,side<0?glowBlue:glowRed);
                lamp.AddComponent<EnergyPulseVisual>();
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
        void OnDestroy()
        {
            foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);
            foreach(var material in runtimeMaterials)if(material!=null)Destroy(material);
            if(terrainPaint!=null)Destroy(terrainPaint);
            RenderSettings.fog=false;
        }
    }

    // Floating aura detail on cosmetic shrines. Does not alter gameplay.
    public sealed class EnergyPulseVisual : MonoBehaviour
    {
        private Vector3 originalPosition, originalScale;
        private float phase;
        private void Start()
        {
            originalPosition=transform.localPosition;
            originalScale=transform.localScale;
            phase=(transform.position.x+transform.position.z)*.17f;
        }
        private void Update()
        {
            var t=Time.time+phase;
            transform.localPosition=originalPosition+Vector3.up*Mathf.Sin(t*1.6f)*.1f;
            transform.localScale=originalScale*(1f+.065f*Mathf.Sin(t*2.25f));
            transform.Rotate(Vector3.up,Time.deltaTime*22f,Space.Self);
        }
    }
}
