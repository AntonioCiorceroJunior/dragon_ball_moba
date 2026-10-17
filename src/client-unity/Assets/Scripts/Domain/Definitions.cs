// Pure C# gameplay contracts. No UnityEngine dependency.
using System;
using System.Collections.Generic;

namespace DragonMoba.Domain
{
    public enum Team { Blue = 0, Red = 1, Neutral = 2 }
    public enum EntityKind { Hero, Minion, Tower, Core }
    public enum CommandKind { Move, Attack, Charge, Cast, Buy }
    public enum AbilitySlot { Q = 0, W = 1, E = 2, R = 3 }
    public enum EventKind { Attack, Ability, Damage, Death, Respawn, LevelUp, Transform, Charge, Purchase, Victory, Warning }

    public struct V2
    {
        public float X;
        public float Y;
        public V2(float x, float y) { X = x; Y = y; }
        public static float Distance(V2 a, V2 b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        public static V2 MoveTowards(V2 a, V2 b, float step)
        {
            var distance = Distance(a, b);
            if (distance <= step || distance <= 0.0001f) return b;
            var t = step / distance;
            return new V2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }
        public static V2 Clamp(V2 v)
        {
            return new V2(Math.Max(-32f, Math.Min(32f, v.X)), Math.Max(-41f, Math.Min(41f, v.Y)));
        }
    }

    public sealed class GameCommand
    {
        public int ActorId;
        public CommandKind Kind;
        public V2 Position;
        public int TargetId;
        public bool Pressed;
        public AbilitySlot Slot;
        public string ItemId;
    }

    public sealed class GameEvent
    {
        public EventKind Kind;
        public int ActorId;
        public int TargetId;
        public V2 Position;
        public string Message;
        public float Value;
        public GameEvent(EventKind kind, int actorId, V2 position, string message, float value = 0f, int targetId = 0)
        {
            Kind = kind; ActorId = actorId; Position = position; Message = message; Value = value; TargetId = targetId;
        }
    }

    public sealed class FormStage
    {
        public int Level;
        public string Id;
        public string DisplayName;
        public float BonusAttack;
        public float BonusKiPower;
        public float BonusSpeed;
        public FormStage(int level, string id, string name, float attack, float kiPower, float speed)
        {
            Level = level; Id = id; DisplayName = name;
            BonusAttack = attack; BonusKiPower = kiPower; BonusSpeed = speed;
        }
    }

    public sealed class HeroDefinition
    {
        public string Id;
        public string Name;
        public float MaxHp;
        public float MaxKi;
        public float BaseAttack;
        public float KiPower;
        public float BaseSpeed;
        public float AttackRange;
        public readonly List<FormStage> Forms = new List<FormStage>();
    }

    public sealed class ItemDefinition
    {
        public string Id;
        public string Name;
        public int Price;
        public float MaxKi;
        public float KiRegen;
        public float Armor;
        public float Attack;
        public bool TrainingBoots;
    }

    public static class Catalog
    {
        private static readonly Dictionary<string, HeroDefinition> Heroes = new Dictionary<string, HeroDefinition>();
        private static readonly Dictionary<string, ItemDefinition> Items = new Dictionary<string, ItemDefinition>();

        static Catalog()
        {
            var kid = new HeroDefinition { Id = "GKU_KID", Name = "Goku Criança", MaxHp = 570, MaxKi = 110, BaseAttack = 55, KiPower = 30, BaseSpeed = 6.2f, AttackRange = 2.4f };
            kid.Forms.Add(new FormStage(1, "kid_base", "Forma Base", 0, 0, 0));
            kid.Forms.Add(new FormStage(11, "kid_nimbus", "Nuvem Voadora", 8, 10, 0.75f));
            kid.Forms.Add(new FormStage(16, "kid_mastery", "Domínio de Combate", 18, 20, 0.15f));
            Heroes.Add(kid.Id, kid);

            var super = new HeroDefinition { Id = "GKU_SUPER", Name = "Goku Super", MaxHp = 520, MaxKi = 135, BaseAttack = 51, KiPower = 43, BaseSpeed = 6.5f, AttackRange = 5.8f };
            super.Forms.Add(new FormStage(1, "super_base", "Forma Base", 0, 0, 0));
            super.Forms.Add(new FormStage(6, "super_god", "Super Saiyajin God", 5, 12, 0.15f));
            super.Forms.Add(new FormStage(10, "super_blue", "Super Saiyajin Blue", 7, 16, 0.12f));
            super.Forms.Add(new FormStage(13, "super_blue_kk", "Blue + Kaioken", 9, 18, 0.12f));
            super.Forms.Add(new FormStage(16, "super_ui_sign", "Instinto Superior Incompleto", 10, 20, 0.2f));
            super.Forms.Add(new FormStage(18, "super_ui", "Instinto Superior Completo", 12, 22, 0.25f));
            Heroes.Add(super.Id, super);

            var cell = new HeroDefinition { Id = "CELL_Z", Name = "Cell Z", MaxHp = 640, MaxKi = 120, BaseAttack = 59, KiPower = 33, BaseSpeed = 5.8f, AttackRange = 3.0f };
            cell.Forms.Add(new FormStage(1, "cell_imperfect", "Imperfeito", 0, 0, 0));
            cell.Forms.Add(new FormStage(6, "cell_semi", "Semiperfeito", 10, 8, 0.08f));
            cell.Forms.Add(new FormStage(11, "cell_perfect", "Perfeito", 12, 14, 0.12f));
            Heroes.Add(cell.Id, cell);

            Items.Add("KI_CELL", new ItemDefinition { Id = "KI_CELL", Name = "Célula de Ki", Price = 300, MaxKi = 45, KiRegen = 2.5f });
            Items.Add("ARMOR", new ItemDefinition { Id = "ARMOR", Name = "Placa de Armadura", Price = 400, Armor = 30 });
            Items.Add("GLOVES", new ItemDefinition { Id = "GLOVES", Name = "Luvas Reforçadas", Price = 350, Attack = 15 });
            Items.Add("TRAINING_BOOTS", new ItemDefinition { Id = "TRAINING_BOOTS", Name = "Botas de Treinamento", Price = 1100, TrainingBoots = true });
        }

        public static IEnumerable<HeroDefinition> AllHeroes { get { return Heroes.Values; } }
        public static IEnumerable<ItemDefinition> AllItems { get { return Items.Values; } }
        public static HeroDefinition Hero(string id) { return Heroes.ContainsKey(id) ? Heroes[id] : null; }
        public static ItemDefinition Item(string id) { return Items.ContainsKey(id) ? Items[id] : null; }
        public static int ExperienceForNextLevel(int level) { return 90 + Math.Max(0, level - 1) * 40; }
        public static float Mitigate(float raw, float resistance)
        {
            if (raw <= 0) return 0;
            if (resistance >= 0) return raw * 100f / (100f + resistance);
            return raw * (2f - 100f / (100f - resistance));
        }
    }
}
