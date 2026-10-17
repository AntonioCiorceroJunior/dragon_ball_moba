using UnityEngine;
namespace DragonMoba.Runtime
{
    [RequireComponent(typeof(Camera))]
    public sealed class ArenaCameraController : MonoBehaviour
    {
        [Range(30,60)] public float elevation = 43f;
        public float defaultZoom = 11.5f, minimumZoom = 8f, maximumZoom = 18f;
        public float panSpeed = 18f, smoothing = .12f;
        public Vector2 mapHalfSize = new Vector2(36,44);
        public bool Following { get; private set; } = true;
        public Vector3 Focus { get; private set; }
        private Camera view;
        private Vector3 target, velocity, previousMouse;
        private bool initialized;
        private void Awake() { view = GetComponent<Camera>(); view.orthographic = true; view.orthographicSize = defaultZoom; }
        public Vector3 ClampFocus(Vector3 value)
        {
            float limitX = Mathf.Max(0, mapHalfSize.x - view.orthographicSize * view.aspect);
            float limitZ = Mathf.Max(0, mapHalfSize.y - view.orthographicSize / Mathf.Sin(elevation * Mathf.Deg2Rad));
            return new Vector3(Mathf.Clamp(value.x,-limitX,limitX),0,Mathf.Clamp(value.z,-limitZ,limitZ));
        }
        public void Zoom(float steps)
        {
            view.orthographicSize = Mathf.Clamp(view.orthographicSize - steps, minimumZoom, maximumZoom);
            target = ClampFocus(target);
        }
        public void Pan(Vector2 delta)
        {
            Following = false;
            target = ClampFocus(target + new Vector3(delta.x,0,delta.y));
        }
        public void Recenter(Vector3 hero, bool snap = false)
        {
            Following = true; target = ClampFocus(hero);
            if(snap) { Focus = target; velocity = Vector3.zero; Place(); }
        }
        public void Tick(Vector3 hero, bool readInput = true)
        {
            if(!initialized) { initialized = true; Recenter(hero,true); previousMouse = Input.mousePosition; }
            if(readInput)
            {
                Zoom(Input.mouseScrollDelta.y);
                if(Input.GetKeyDown(KeyCode.Y)) Following = !Following;
                if(Input.GetKeyDown(KeyCode.Space)) Recenter(hero);
                Vector2 input = new Vector2((Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.LeftArrow)?1:0),
                    (Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.DownArrow)?1:0));
                if(input.sqrMagnitude>0) Pan(input.normalized * (panSpeed * Time.unscaledDeltaTime));
                if(Input.GetMouseButton(2))
                {
                    Vector3 delta = Input.mousePosition - previousMouse;
                    float unitPerPixel = 2 * view.orthographicSize / Mathf.Max(1,Screen.height);
                    Pan(new Vector2(-delta.x, -delta.y / Mathf.Sin(elevation * Mathf.Deg2Rad)) * unitPerPixel);
                }
                previousMouse = Input.mousePosition;
            }
            if(Following) target = ClampFocus(hero);
            Focus = ClampFocus(Vector3.SmoothDamp(Focus,target,ref velocity,smoothing,Mathf.Infinity,Time.unscaledDeltaTime));
            Place();
        }
        private void Place()
        {
            transform.rotation = Quaternion.Euler(elevation,0,0);
            transform.position = Focus - transform.forward * 36f;
        }
    }
}

