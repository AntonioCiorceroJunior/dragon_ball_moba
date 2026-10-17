// Low-cost original sentry-drone silhouettes instead of undifferentiated spheres.
// Decorative geometry has no colliders; root collider is for raycast targeting.
using DragonMoba.Domain;
using UnityEngine;

namespace DragonMoba.Runtime
{
    public static class MinionVisual
    {
        private static Material blueMetal, redMetal, blueGlow, redGlow;
        private static Material Make(Color color, bool luminous)
        {
            var m=new Material(Shader.Find("Standard")) { color=color };
            m.name=luminous?"Sentry Ki light":"Sentry metal";
            if(luminous)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor",color*1.4f);
            }
            else if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",.35f);
            return m;
        }
        private static GameObject Piece(Transform parent,string label,PrimitiveType type,
            Vector3 position,Vector3 scale,Material shared)
        {
            var go=GameObject.CreatePrimitive(type);
            go.name=label;
            go.transform.SetParent(parent,false);
            go.transform.localPosition=position;
            go.transform.localScale=scale;
            var c=go.GetComponent<Collider>();
            if(c!=null)Object.Destroy(c);
            go.GetComponent<Renderer>().sharedMaterial=shared;
            return go;
        }
        public static GameObject Create(EntityState entity)
        {
            var blue=entity.Team==Team.Blue;
            if(blueMetal==null)
            {
                blueMetal=Make(new Color(.15f,.31f,.43f),false);
                redMetal=Make(new Color(.45f,.19f,.19f),false);
                blueGlow=Make(new Color(.13f,.77f,1f),true);
                redGlow=Make(new Color(1f,.39f,.20f),true);
            }
            var metal=blue?blueMetal:redMetal;
            var energy=blue?blueGlow:redGlow;
            var root=new GameObject("Ki Sentry drone");
            var core=new GameObject("Animated sentry model").transform;
            core.SetParent(root.transform,false);
            Piece(core,"Hull",PrimitiveType.Capsule,new Vector3(0,.67f,0),
                new Vector3(.78f,.65f,.68f),metal);
            Piece(core,"Ki reactor",PrimitiveType.Sphere,new Vector3(0,1.03f,.31f),
                new Vector3(.42f,.45f,.22f),energy);
            Piece(core,"Helmet",PrimitiveType.Cube,new Vector3(0,1.37f,0),
                new Vector3(.64f,.27f,.66f),metal);
            for(var side=-1;side<=1;side+=2)
            {
                var wing=Piece(core,"Armored wing",PrimitiveType.Cube,
                    new Vector3(side*.52f,.65f,-.12f),
                    new Vector3(.18f,.50f,.92f),metal);
                wing.transform.localRotation=Quaternion.Euler(0,0,side*19f);
            }
            var collider=root.AddComponent<CapsuleCollider>();
            collider.center=new Vector3(0,.75f,0);
            collider.height=1.6f;
            collider.radius=.55f;
            root.AddComponent<MinionFloatAnimation>().model=core;
            return root;
        }
    }

    public sealed class MinionFloatAnimation : MonoBehaviour
    {
        public Transform model;
        private Vector3 previous;
        private float phase;
        private void Start()
        {
            previous=transform.position;
            phase=(transform.position.x+transform.position.z)*.16f;
        }
        private void LateUpdate()
        {
            if(model==null)return;
            var velocity=transform.position-previous;
            previous=transform.position;
            model.localPosition=new Vector3(0,Mathf.Sin(Time.time*3.2f+phase)*.075f,0);
            if(velocity.sqrMagnitude>.0001f)
            {
                var facing=Quaternion.LookRotation(new Vector3(velocity.x,0,velocity.z));
                model.rotation=Quaternion.Slerp(model.rotation,facing,Time.deltaTime*7f);
            }
        }
    }
}
