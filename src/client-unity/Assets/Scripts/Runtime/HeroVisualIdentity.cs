using System.Collections.Generic;
using UnityEngine;

namespace DragonMoba.Runtime
{
    // Original bone-attached designs; the licensed base rig, animations and collider stay intact.
    public sealed class HeroVisualIdentity : MonoBehaviour
    {
        public string heroId, formId;
        readonly List<Mesh> ownedMeshes=new List<Mesh>();
        readonly List<SkinnedMeshRenderer> bodies=new List<SkinnedMeshRenderer>();
        MaterialPropertyBlock properties;
        float bodyHeight;
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        static readonly Dictionary<PrimitiveType, Mesh> primitives = new Dictionary<PrimitiveType, Mesh>();
        static Mesh spike, wing;

        public static void Apply(GameObject root, string hero, string form)
        {
            if (root.GetComponent<HeroVisualIdentity>() != null) return;
            var identity = root.AddComponent<HeroVisualIdentity>(); identity.heroId = hero; identity.formId = form;
            var animator = root.GetComponentInChildren<Animator>();
            if (animator == null || !animator.isHuman) return;
            animator.Rebind();animator.Update(0);
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (head == null || chest == null || hips == null) return;
            var ape = form == "kid_oozaru";
            var cell = hero == "CELL_Z";
            var perfect = form == "cell_perfect";
            var semi = form == "cell_semi";
            var kid = hero == "GKU_KID";
            var skin = Mat(ape ? "ape-face" : cell ? "cell-face" : "kid-skin",
                ape ? new Color(.56f,.35f,.19f) : cell ? new Color(.75f,.76f,.57f) : new Color(1f,.72f,.48f));
            var dark = Mat("ink",new Color(.025f,.035f,.06f));
            var white = Mat("eye-white",new Color(.98f,.98f,.91f));
            var pupil = Mat(cell || ape ? "red-eye" : "kid-eye",cell || ape ? new Color(.8f,.08f,.07f) : new Color(.035f,.025f,.02f));
            var outfit = Mat(ape ? "oozaru-fur" : cell ? "cell-"+form : kid ? "kid-blue-gi" : "super-orange-gi",
                ape ? new Color(.30f,.14f,.065f) : cell ? new Color(.31f,.67f,.14f) : kid ? new Color(.035f,.24f,.60f) : new Color(.95f,.29f,.035f));
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var body = renderer.name.IndexOf("Eye",System.StringComparison.OrdinalIgnoreCase)<0;
                if (!body) { renderer.sharedMaterial = dark; continue; }
                var bodyMat = Mat("body-"+hero+"-"+form+"-"+renderer.name,outfit.color);
                var bounds = renderer.sharedMesh.bounds;
                var zUp = bounds.size.z > bounds.size.y;
                bodyMat.SetFloat("_MinY",zUp?bounds.min.z:bounds.min.y);
                bodyMat.SetFloat("_SpanY",zUp?bounds.size.z:bounds.size.y);
                bodyMat.SetFloat("_Width",bounds.size.x);
                bodyMat.SetVector("_HeightAxis",zUp?Vector3.forward:Vector3.up);
                bodyMat.SetFloat("_Style",ape?3:cell?2:form=="super_ui_sign"||form=="super_ui"?4:1);
                bodyMat.SetColor("_Skin",skin.color);
                bodyMat.SetColor("_Accent",cell?new Color(.10f,.12f,.17f):kid?new Color(.86f,.10f,.055f):new Color(.025f,.10f,.27f));
                var assigned = new Material[renderer.sharedMaterials.Length];
                for(var i=0;i<assigned.Length;i++)assigned[i]=bodyMat;
                renderer.sharedMaterials=assigned;
                identity.bodies.Add(renderer);
                identity.bodyHeight=Mathf.Max(identity.bodyHeight,renderer.bounds.size.y);
            }
            var unit = Mathf.Abs(root.transform.lossyScale.x);
            identity.UpdateProperties();
            var faceScale = ape ? new Vector3(.82f,.77f,.68f) : cell ? new Vector3(.51f,.66f,.48f) : kid ? new Vector3(.62f,.66f,.55f) : new Vector3(.55f,.60f,.50f);
            Piece(root,head,"Designed face",Primitive(PrimitiveType.Sphere),new Vector3(0,.09f,.015f),faceScale,skin,unit);
            var front=faceScale.z*.5f+.015f;
            for(var side=-1;side<=1;side+=2)
            {
                Piece(root,head,"Eye white",Primitive(PrimitiveType.Sphere),new Vector3(side*(ape?.19f:.135f),.13f,front+.012f),
                    new Vector3(ape?.21f:.16f,.105f,.038f),white,unit);
                Piece(root,head,"Eye pupil",Primitive(PrimitiveType.Sphere),new Vector3(side*(ape?.19f:.135f),.13f,front+.035f),
                    new Vector3(.055f,.073f,.018f),pupil,unit);
                Piece(root,head,"Eyebrow",Primitive(PrimitiveType.Cube),new Vector3(side*.135f,.205f,front+.015f),
                    new Vector3(.19f,.042f,.035f),dark,unit,Quaternion.Euler(0,0,side*-12));
                Piece(root,head,"Ear",Primitive(PrimitiveType.Sphere),new Vector3(side*(ape?.42f:.31f),.075f,0),
                    new Vector3(ape?.26f:.14f,ape?.29f:.21f,.13f),skin,unit);
            }
            Piece(root,head,"Nose",Primitive(PrimitiveType.Sphere),new Vector3(0,.025f,ape?.39f:front+.045f),
                ape?new Vector3(.37f,.2f,.22f):new Vector3(.08f,.09f,.08f),skin,unit);
            Piece(root,head,"Mouth",Primitive(PrimitiveType.Cube),new Vector3(0,-.075f,ape?.37f:front+.004f),
                new Vector3(ape?.35f:.13f,.025f,.025f),dark,unit);

            if (cell)
            {
                Piece(root,head,"Green bio-helmet",Primitive(PrimitiveType.Sphere),new Vector3(0,.33f,-.07f),new Vector3(.65f,.46f,.58f),outfit,unit);
                for(var side=-1;side<=1;side+=2)
                {
                    Piece(root,head,"Crown horn",Spike(),new Vector3(side*.25f,.36f,-.035f),
                        new Vector3(perfect?.19f:.13f,perfect?.68f:semi?.31f:.48f,.22f),outfit,unit,Quaternion.Euler(0,0,side*-16));
                    Piece(root,chest,"Black wing carapace",Wing(),new Vector3(side*.13f,.02f,-.23f),
                        new Vector3(side*(perfect?.88f:.70f),perfect?1.22f:1.04f,.20f),dark,unit);
                    Piece(root,animator.GetBoneTransform(side<0?HumanBodyBones.LeftUpperArm:HumanBodyBones.RightUpperArm),
                        "Chitin shoulder",Primitive(PrimitiveType.Sphere),Vector3.zero,new Vector3(.40f,.43f,.40f),outfit,unit);
                }
                var tailPoints=new[]{new Vector3(0,-.06f,-.13f),new Vector3(.12f,-.22f,-.51f),
                    new Vector3(.52f,-.15f,-.94f),new Vector3(.78f,.23f,-1.03f),new Vector3(.72f,.58f,-.96f)};
                Piece(root,hips,"Segmented bio-tail",Tube(tailPoints,perfect?.065f:.095f),Vector3.zero,Vector3.one,outfit,unit);
                Piece(root,hips,"Tail stinger",Spike(),tailPoints[4],new Vector3(.15f,.31f,.15f),skin,unit);
            }
            else if (ape)
            {
                Piece(root,head,"Great ape muzzle",Primitive(PrimitiveType.Sphere),new Vector3(0,-.015f,.30f),new Vector3(.64f,.39f,.39f),skin,unit);
                foreach(var side in new[]{-1,1})
                {
                    Piece(root,head,"Fang",Spike(),new Vector3(side*.20f,-.11f,.47f),new Vector3(.07f,.20f,.07f),white,unit,Quaternion.Euler(180,0,0));
                    Piece(root,animator.GetBoneTransform(side<0?HumanBodyBones.LeftUpperArm:HumanBodyBones.RightUpperArm),
                        "Powerful fur shoulder",Primitive(PrimitiveType.Sphere),Vector3.zero,new Vector3(.76f,.78f,.68f),outfit,unit);
                }
                Piece(root,hips,"Great ape tail",Tube(new[]{new Vector3(0,-.1f,-.1f),new Vector3(.3f,-.2f,-.6f),new Vector3(.8f,.1f,-1),new Vector3(.9f,.5f,-.9f)},.13f),Vector3.zero,Vector3.one,outfit,unit);
            }
            else if(kid)
            {
                Piece(root,head,"Black hair cap",Primitive(PrimitiveType.Sphere),new Vector3(0,.31f,-.04f),new Vector3(.65f,.30f,.57f),dark,unit);
                for(var i=0;i<7;i++)
                {
                    var side=i%3-1;
                    Piece(root,head,"Kid hair silhouette",Spike(),new Vector3(side*.18f,.29f+(i/3)*.05f,-.06f),
                        new Vector3(.15f,.39f+(i%2)*.08f,.16f),dark,unit,Quaternion.Euler(-12+i*5,0,-side*36));
                }
                var red=Mat("kid-red-staff",new Color(.77f,.055f,.035f));
                Piece(root,chest,"Red power pole",Primitive(PrimitiveType.Cylinder),new Vector3(-.22f,-.1f,-.25f),new Vector3(.065f,.75f,.065f),red,unit,Quaternion.Euler(0,0,-28));
                var brown=Mat("kid-tail",new Color(.34f,.17f,.065f));
                Piece(root,hips,"Saiyan tail",Tube(new[]{new Vector3(0,-.06f,-.1f),new Vector3(.25f,-.15f,-.45f),new Vector3(.55f,.02f,-.55f),new Vector3(.52f,.3f,-.4f)},.055f),Vector3.zero,Vector3.one,brown,unit);
                if(form=="kid_nimbus")
                {
                    var gold=Mat("nimbus-gold",new Color(1f,.72f,.11f));
                    for(var i=0;i<5;i++)
                        Piece(root,root.transform,"Nimbus cloud",Primitive(PrimitiveType.Sphere),new Vector3((i-2)*.28f,.08f,(i%2)*.17f),new Vector3(.72f,.28f,.72f),gold,unit);
                }
            }
        }
        void LateUpdate(){UpdateProperties();}
        void UpdateProperties()
        {
            if(properties==null)properties=new MaterialPropertyBlock();
            properties.SetVector("_RootWorld",transform.position);
            properties.SetVector("_RightWorld",transform.right);
            properties.SetFloat("_BodyHeight",Mathf.Max(.1f,bodyHeight));
            foreach(var renderer in bodies)if(renderer!=null)renderer.SetPropertyBlock(properties);
        }
        static Material Mat(string key, Color color)
        {
            if(materials.TryGetValue(key,out var value))return value;
            var shader=Resources.Load<Shader>("HeroIdentity");
            value=new Material(shader!=null?shader:Shader.Find("Standard")){name="Original identity / "+key,color=color};
            materials[key]=value;return value;
        }
        static Mesh Primitive(PrimitiveType type)
        {
            if(primitives.TryGetValue(type,out var mesh))return mesh;
            var temporary=GameObject.CreatePrimitive(type);mesh=temporary.GetComponent<MeshFilter>().sharedMesh;
            primitives[type]=mesh;Destroy(temporary);return mesh;
        }
        static void Piece(GameObject root, Transform bone,string name,Mesh mesh,Vector3 offset,Vector3 size,Material material,float unit,Quaternion? rotation=null)
        {
            if(bone==null)return;
            var go=new GameObject(name);
            go.transform.position=bone.position+root.transform.TransformDirection(offset)*unit;
            go.transform.rotation=root.transform.rotation*(rotation??Quaternion.identity);
            go.transform.localScale=size*unit;
            go.transform.SetParent(bone,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
            if(mesh.name=="Original articulated character tail")root.GetComponent<HeroVisualIdentity>().ownedMeshes.Add(mesh);
        }
        void OnDestroy(){foreach(var mesh in ownedMeshes)if(mesh!=null)Destroy(mesh);}
        static Mesh Spike()
        {
            if(spike!=null)return spike;
            spike=new Mesh{name="Original tapered character silhouette"};
            spike.vertices=new[]{new Vector3(0,1,.10f),new Vector3(-1,0,-1),new Vector3(1,0,-1),new Vector3(1,0,1),new Vector3(-1,0,1)};
            spike.triangles=new[]{0,2,1,0,3,2,0,4,3,0,1,4,1,2,3,1,3,4};spike.RecalculateNormals();return spike;
        }
        static Mesh Wing()
        {
            if(wing!=null)return wing;
            wing=new Mesh{name="Original Cell carapace silhouette"};
            wing.vertices=new[]{new Vector3(0,.55f,0),new Vector3(.65f,.35f,-.25f),new Vector3(.72f,-.8f,-.35f),new Vector3(.15f,-.55f,.05f),new Vector3(.28f,0,.18f)};
            wing.triangles=new[]{0,4,1,1,4,2,2,4,3,3,4,0,0,1,2,0,2,3};wing.RecalculateNormals();return wing;
        }
        static Mesh Tube(Vector3[] points,float radius)
        {
            const int sides=7;var vertices=new Vector3[points.Length*sides];var triangles=new int[(points.Length-1)*sides*6];
            for(var ring=0;ring<points.Length;ring++)
            {
                var direction=points[Mathf.Min(ring+1,points.Length-1)]-points[Mathf.Max(0,ring-1)];
                var axis=Quaternion.FromToRotation(Vector3.up,direction.normalized);
                for(var side=0;side<sides;side++){var angle=side*Mathf.PI*2/sides;vertices[ring*sides+side]=points[ring]+axis*new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);}
            }
            var index=0;
            for(var ring=0;ring<points.Length-1;ring++)for(var side=0;side<sides;side++)
            {var a=ring*sides+side;var b=ring*sides+(side+1)%sides;var c=a+sides;var d=b+sides;triangles[index++]=a;triangles[index++]=c;triangles[index++]=b;triangles[index++]=b;triangles[index++]=c;triangles[index++]=d;}
            var mesh=new Mesh{name="Original articulated character tail"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();return mesh;
        }
    }
}
