#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DragonMoba.Domain;
using UnityEngine;

namespace DragonMoba.Runtime
{
    // Opt-in native UI fixture. No effect in ordinary matches; absent in release builds.
    public sealed class GameplayValidationProbe : MonoBehaviour
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        static GameplayValidationProbe instance;
        PrototypeGame game;
        GameWorld world;
        float nextSnapshot;
        string directory;
        int attacks, lastAttackTick, minimumAttackTicks = int.MaxValue;
        string fixture = "ordinary match; no arranged state";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!Environment.GetCommandLineArgs().Contains("-dbm001-native-test")) return;
            instance = new GameObject("DBM-001 opt-in native validation").AddComponent<GameplayValidationProbe>();
            instance.directory = Path.Combine(Application.dataPath, "../Logs");
            Directory.CreateDirectory(instance.directory);
        }

        public static void Observe(GameEvent e, GameWorld source)
        {
            if (instance == null || e.ActorId != source.PlayerId) return;
            if (e.Kind == EventKind.Attack)
            {
                if (instance.lastAttackTick > 0)
                    instance.minimumAttackTicks = Math.Min(instance.minimumAttackTicks, source.TickIndex-instance.lastAttackTick);
                instance.lastAttackTick = source.TickIndex;
                ++instance.attacks;
            }
            if (e.Kind == EventKind.Attack || e.Kind == EventKind.SkillUp || e.Kind == EventKind.LevelUp ||
                e.Kind == EventKind.Death || e.Kind == EventKind.Respawn || e.Kind == EventKind.Transform)
                Debug.Log("DBM001 EVENT | " + e.Kind + " | tick=" + source.TickIndex + " target=" + e.TargetId + " " + e.Message);
        }

        void LateUpdate()
        {
            if (game == null) game = FindAnyObjectByType<PrototypeGame>();
            if (game == null) return;
            var current = (GameWorld)typeof(PrototypeGame).GetField("world", Flags).GetValue(game);
            if (current != world)
            {
                world = current; attacks = lastAttackTick = 0; minimumAttackTicks = int.MaxValue;
                fixture = "ordinary match; no arranged state";
            }
            if (world == null) return;
            string commandPath = Path.Combine(directory, "DBM001-command.txt");
            if (File.Exists(commandPath))
            {
                string command = File.ReadAllText(commandPath).Trim();
                File.Delete(commandPath); // Only this probe's one-shot command file.
                var hero = world.Player;
                if (command == "combat")
                {
                    var enemy = world.Entities.First(e => e.Kind == EntityKind.Hero && e.Team != hero.Team);
                    hero.Position = hero.Destination = new V2(30, -34);
                    enemy.Position = enemy.Destination = new V2(30, -26);
                    enemy.MoveSpeed = enemy.PhysicalAttack = enemy.Ki = enemy.KiRegen = 0;
                    enemy.Hp = enemy.MaxHp = 5000;
                    fixture = "controlled combat positions; stationary enemy with no attacks/skills";
                }
                else if (command.StartsWith("level:"))
                {
                    int level;
                    if (int.TryParse(command.Substring(6), out level) && level >= 2 && level <= 18)
                        while (hero.Level < level)
                            world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level)-hero.Experience+.01f);
                    fixture = "controlled XP via GrantExperience: " + level;
                }
                else if (command == "death")
                {
                    world.DealDamage(world.Entities.First(e => e.Kind == EntityKind.Hero && e.Team != hero.Team), hero, 100000, false);
                    fixture = "controlled lethal damage; normal respawn timer";
                }
                else if (command == "enemy-transform")
                {
                    var enemy = world.Entities.First(e => e.Kind == EntityKind.Hero && e.Team != hero.Team);
                    while (enemy.Level < 11)
                        world.GrantExperience(enemy, Catalog.ExperienceForNextLevel(enemy.Level)-enemy.Experience+.01f);
                    enemy.MoveSpeed = enemy.PhysicalAttack = enemy.Ki = enemy.KiRegen = 0;
                    fixture = "controlled enemy XP to level 11; normal form replacement";
                }
                Debug.Log("DBM001 FIXTURE | " + command + " | " + fixture);
            }
            if (Time.unscaledTime < nextSnapshot) return;
            nextSnapshot = Time.unscaledTime + .15f;
            var player = world.Player;
            var target = world.Entities.First(e => e.Kind == EntityKind.Hero && e.Team != player.Team);
            var view = GameObject.Find("Entity-" + target.Id + "-" + target.DisplayName);
            var mesh = view == null ? null : view.GetComponentInChildren<SkinnedMeshRenderer>();
            var body = mesh == null ? new Vector3(target.Position.X, 1, target.Position.Y) : mesh.bounds.center;
            var pixel = Camera.main.WorldToScreenPoint(body);
            var state = new Snapshot {
                tick=world.TickIndex, level=player.Level, points=player.UnspentSkillPoints, ranks=player.SkillRanks,
                ki=player.Ki, cooldowns=player.SkillCooldown, charging=player.Charging, hp=player.Hp,
                target=player.AttackTargetId, enemyId=target.Id, enemyHp=target.Hp, form=player.FormId, enemyForm=target.FormId,
                attacks=attacks, minimumAttackTicks=minimumAttackTicks, attackInterval=player.AttackInterval,
                enemyX=pixel.x, enemyY=Screen.height-pixel.y, fixture=fixture
            };
            File.WriteAllText(Path.Combine(directory, "DBM001-state.json"), JsonUtility.ToJson(state));
        }

        [Serializable] class Snapshot
        {
            public int tick, level, points, target, enemyId, attacks, minimumAttackTicks;
            public int[] ranks;
            public float ki, hp, enemyHp, attackInterval, enemyX, enemyY;
            public float[] cooldowns;
            public bool charging;
            public string form, enemyForm, fixture;
        }
    }
}
#endif
