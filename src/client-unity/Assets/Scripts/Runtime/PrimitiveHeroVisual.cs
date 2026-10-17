// Original procedural hero stand-ins; replace with licensed rigged models later.
// Only presentation. GameWorld owns collisions, attributes and transformation rules.
using DragonMoba.Domain;
using UnityEngine;

namespace DragonMoba.Runtime
{
    public static class PrimitiveHeroVisual
    {
        private static GameObject Part(Transform parent, string name, PrimitiveType type,
            Vector3 position, Vector3 scale, Color tint, Quaternion rotation)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.transform.localRotation = rotation;
            var collider = part.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            var renderer = part.GetComponent<Renderer>();
            renderer.material.color = tint;
            return part;
        }

        private static void Shape(Transform root, string name, PrimitiveType type,
            float x, float y, float z, float sx, float sy, float sz, Color color)
        {
            Part(root,name,type,new Vector3(x,y,z),new Vector3(sx,sy,sz),
                color,Quaternion.identity);
        }

        public static GameObject Create(EntityState hero)
        {
            var kid = hero.HeroId == "GKU_KID";
            var cell = hero.HeroId == "CELL_Z";
            var ape = hero.FormId == "kid_oozaru";
            var root = new GameObject("Prototype model " + hero.HeroId + " / " + hero.FormId);
            var model = new GameObject("Procedural model — not final art").transform;
            model.SetParent(root.transform, false);
            var scale = ape ? 2.15f : kid ? .7f : 1.05f;
            model.localScale = Vector3.one * scale;
            var body = cell ? new Color(.25f,.62f,.31f) : ape ?
                new Color(.42f,.25f,.15f) : new Color(.96f,.43f,.12f);
            var skin = cell ? new Color(.75f,.86f,.54f) : ape ?
                new Color(.50f,.35f,.24f) : new Color(1f,.78f,.58f);
            var accent = cell ? new Color(.08f,.18f,.12f) : ape ?
                new Color(.25f,.15f,.1f) : new Color(.12f,.15f,.38f);
            Shape(model,"Torso",PrimitiveType.Capsule,0,1.38f,0,.78f,1.1f,.43f,body);
            Shape(model,"Cabeça",PrimitiveType.Sphere,0,2.40f,0,.78f,.76f,.7f,skin);
            foreach (var side in new[]{-1f,1f})
            {
                Shape(model,"Braco",PrimitiveType.Capsule,.52f*side,1.48f,0,.32f,.9f,.34f,body);
                Shape(model,"Antebraco",PrimitiveType.Sphere,.62f*side,1.05f,.06f,.35f,.35f,.32f,skin);
                Shape(model,"Perna",PrimitiveType.Capsule,.24f*side,.59f,0,.38f,.99f,.4f,body);
                Shape(model,"Bota",PrimitiveType.Cube,.25f*side,.16f,.16f,.37f,.34f,.68f,accent);
                Shape(model,"Olho",PrimitiveType.Sphere,.18f*side,2.42f,.34f,.12f,.10f,.08f,Color.black);
            }
            if (!ape && !cell)
            {
                for(var i=0;i<5;i++)
                {
                    var x=(i-2)*.25f;
                    Part(model,"Cabelo",PrimitiveType.Cylinder,
                        new Vector3(x,2.87f+(i%2)*.08f,0),new Vector3(.23f,.34f,.25f),
                        new Color(.05f,.07f,.08f),Quaternion.Euler(0,0,(i-2)*-12f));
                }
                Shape(model,"Faixa",PrimitiveType.Cube,0,1.0f,.23f,.78f,.22f,.19f,accent);
            }
            if (cell)
            {
                Shape(model,"Carapaca",PrimitiveType.Capsule,0,1.55f,-.32f,.87f,1.15f,.35f,accent);
                foreach (var side in new[]{-1f,1f})
                {
                    Part(model,"Chifre",PrimitiveType.Cylinder,
                        new Vector3(.31f*side,2.88f,0),new Vector3(.18f,.48f,.2f),
                        accent,Quaternion.Euler(0,0,-side*18f));
                    Shape(model,"Placa de asa",PrimitiveType.Cube,side*.58f,1.68f,-.43f,.4f,1.0f,.14f,
                        new Color(.11f,.27f,.17f));
                }
            }
            if (ape)
            {
                Shape(model,"Focinho",PrimitiveType.Cube,0,2.24f,.37f,.66f,.37f,.48f,skin);
                Shape(model,"Peitoral",PrimitiveType.Sphere,0,1.52f,.36f,.86f,.83f,.34f,skin);
                Part(model,"Cauda",PrimitiveType.Capsule,new Vector3(0,.9f,-.76f),
                    new Vector3(.30f,1.0f,.34f),body,Quaternion.Euler(63f,0,0));
            }
            var collider = root.AddComponent<CapsuleCollider>();
            collider.radius = ape ? .95f : kid ? .43f : .52f;
            collider.height = 2.9f * scale;
            collider.center = new Vector3(0,collider.height*.5f,0);
            root.AddComponent<CharacterPresentation>();
            return root;
        }
    }
}
