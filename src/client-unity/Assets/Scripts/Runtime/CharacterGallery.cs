// Visual-only 3D hero gallery shown before match start.
// Uses already imported, licensed prototype prefabs; creates no gameplay actors.
using System.Collections.Generic;
using UnityEngine;

namespace DragonMoba.Runtime
{
    public sealed class CharacterGallery : MonoBehaviour
    {
        private readonly List<Material> ownedMaterials=new List<Material>();
        private readonly List<Transform> heroes=new List<Transform>();
        private GameObject root;
        private int viewportWidth,viewportHeight;
        private static float Spacing()
        {
            return (Mathf.Min(920f,Screen.width-34f)+10f)/3f*15f/Mathf.Max(1,Screen.height);
        }
        public void Show()
        {
            FrameCamera();
            if(root!=null) {root.SetActive(true);return;}
            root=new GameObject("Capsule Corp — character selection gallery");
            root.transform.SetParent(transform,false);
            var ids=new[]{
                "CharacterForms/GKU_KID/kid_base",
                "CharacterForms/GKU_SUPER/super_base",
                "CharacterForms/CELL_Z/cell_imperfect"
            };
            var colors=new[]{
                new Color(.96f,.66f,.28f),
                new Color(.17f,.78f,1f),
                new Color(.58f,.93f,.39f)
            };
            for(int i=0;i<3;i++)
            {
                var x=(i-1)*Spacing();
                var anchor=new GameObject("Hero showcase "+i).transform;
                anchor.SetParent(root.transform,false);
                anchor.position=new Vector3(x,100.1f,-4f);
                var material=new Material(Shader.Find("Standard"));
                material.color=new Color(.11f,.18f,.22f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor",colors[i]*.1f);
                ownedMaterials.Add(material);
                var pad=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pad.name="Pedestal with energy accent";
                pad.transform.SetParent(anchor,false);
                pad.transform.localPosition=new Vector3(0,-.07f,0);
                pad.transform.localScale=new Vector3(2.55f,.24f,2.55f);
                pad.GetComponent<Renderer>().sharedMaterial=material;
                Destroy(pad.GetComponent<Collider>());
                var prefab=Resources.Load<GameObject>(ids[i]);
                // Match the Super prefab actually wired into the active arena.
                var arena=GetComponent<OrganicArena>();
                var formIndex=arena==null||arena.formIds==null?-1:System.Array.IndexOf(arena.formIds,"super_base");
                if(i==1 && formIndex>=0 && formIndex<arena.characterForms.Length && arena.characterForms[formIndex]!=null)
                    prefab=arena.characterForms[formIndex];
                if(prefab!=null)
                {
                    var model=Instantiate(prefab,anchor);
                    model.name="Existing animated character";
                    model.transform.localPosition=new Vector3(0,.18f,0);
                    model.transform.localRotation=Quaternion.Euler(0,160,0);
                    model.transform.localScale=Vector3.one*(i==0?2.15f:1.80f);
                    HeroVisualIdentity.Apply(model,i==0?"GKU_KID":i==1?"GKU_SUPER":"CELL_Z",
                        i==0?"kid_base":i==1?"super_base":"cell_imperfect");
                    foreach(var collider in model.GetComponentsInChildren<Collider>())
                        collider.enabled=false;
                    foreach(var rigid in model.GetComponentsInChildren<Rigidbody>())
                        rigid.isKinematic=true;
                    heroes.Add(model.transform);
                }
                else Debug.LogWarning("Missing showroom prefab: "+ids[i]);
                var lightRoot=new GameObject("Character accent spotlight");
                lightRoot.transform.SetParent(anchor,false);
                lightRoot.transform.localPosition=new Vector3(0,3.1f,1.7f);
                var light=lightRoot.AddComponent<Light>();
                light.type=LightType.Point;
                light.color=colors[i];
                light.range=7f;
                light.intensity=.56f;
                light.shadows=LightShadows.None;
            }
        }
        public void Hide()
        {
            if(root!=null)root.SetActive(false);
        }
        private static void FrameCamera()
        {
            var camera=Camera.main;
            if(camera==null)return;
            camera.orthographic=true;
            // Isolate the existing models from battlefield rocks and bridge geometry.
            camera.orthographicSize=7.5f;
            camera.transform.rotation=Quaternion.Euler(12f,0,0);
            camera.transform.position=new Vector3(0,100.9f,-4f)-camera.transform.forward*30f;
        }
        private void Update()
        {
            if(root==null||!root.activeSelf)return;
            if(viewportWidth!=Screen.width||viewportHeight!=Screen.height)
            {
                for(var i=0;i<heroes.Count;i++)if(heroes[i]!=null)
                    heroes[i].parent.position=new Vector3((i-1)*Spacing(),100.1f,-4f);
                viewportWidth=Screen.width;viewportHeight=Screen.height;
            }
            for(var i=0;i<heroes.Count;i++)
            {
                if(heroes[i]!=null)
                    heroes[i].localRotation=Quaternion.Euler(0,160+Mathf.Sin(Time.time*.42f+i)*12f,0);
            }
        }
        private void OnDestroy()
        {
            foreach(var material in ownedMaterials)if(material!=null)Destroy(material);
            if(root!=null)Destroy(root);
        }
    }
}
