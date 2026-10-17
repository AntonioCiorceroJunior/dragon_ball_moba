using DragonMoba.Domain;
using UnityEngine;
namespace DragonMoba.Runtime
{
    // Presentation only: position and all combat state remain owned by GameWorld.
    public sealed class CharacterPresentation : MonoBehaviour
    {
        public Animator animator;
        public ParticleSystem aura;
        public Color formColor = Color.cyan;
        private EntityState entity;
        private string state;
        private float actionUntil, deathTime;
        private Vector3 previous;
        private bool initialized, wasAlive = true;
        public void Sync(EntityState value)
        {
            entity = value;
            if (aura != null) { var emission = aura.emission; emission.enabled = value.Alive && value.Charging; }
            var delta = transform.position - previous;
            previous = transform.position;
            if (initialized && value.Alive && delta.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(delta, Vector3.up);
            initialized = true;
            if (wasAlive && !value.Alive) { deathTime = Time.time; SetState("Death"); }
            if (!wasAlive && value.Alive) { state = null; SetState("Idle"); }
            foreach (var c in GetComponents<Collider>()) c.enabled = value.Alive;
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = value.Alive || Time.time - deathTime < 3f;
            wasAlive = value.Alive;
            if (value.Alive && Time.time >= actionUntil)
                SetState(value.Charging ? "Charge" : delta.sqrMagnitude > 0.0001f ? "Run" : "Idle");
        }
        public void React(GameEvent e)
        {
            if (entity == null || !entity.Alive) return;
            if (e.TargetId == entity.Id && e.Kind == EventKind.Damage)
            { actionUntil = Time.time + .2f; SetState("Hit"); }
            else if (e.ActorId == entity.Id && (e.Kind == EventKind.Attack || e.Kind == EventKind.Ability || e.Kind == EventKind.Transform))
            { actionUntil = Time.time + .45f; SetState(e.Kind == EventKind.Transform ? "Transform" : e.Kind == EventKind.Ability ? "Cast" : "Attack"); }
        }
        private void SetState(string next)
        {
            if (animator == null || state == next) return;
            if (!animator.HasState(0, Animator.StringToHash(next))) next = next == "Charge" ? "Idle" : "Attack";
            state = next;
            animator.CrossFadeInFixedTime(next, .08f);
        }
    }
}
