// Pure C# gameplay contracts. No UnityEngine dependency.
using System;
using System.Collections.Generic;

namespace DragonMoba.Domain
{
    public enum Team { Blue = 0, Red = 1, Neutral = 2 }
    public enum EntityKind { Hero, Minion, Tower, Core }
    public enum CommandKind { Move, Attack, Charge, Cast, Buy, UpgradeAbility, Sell, Stop, AttackMove }
    public enum AbilitySlot { Q = 0, W = 1, E = 2, R = 3 }
    public enum EventKind { Attack, Ability, Damage, Death, Respawn, LevelUp, Transform, Charge, Purchase, Victory, Warning, SkillUp }

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
        // Optional stat bonuses. Item ownership changes trigger a single derived-stat refresh.
        public float MaxHp;
        public float HpRegenPer5;
        public float KiResist;
        public float KiPower;
        public float AttackSpeedBonus;
        public float LifeSteal;
        public float SpellVamp;
        public float CritChance;
        public float AbilityHaste;
        public float ArmorPenPercent;
        public float KiPenPercent;
        public float MoveSpeed;
        public float AntiHealOnHit;
        public string[] Components = new string[0];
        public string Category = "Diversos";
        public string IconSymbol = "?";
        public string Description = "";
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
            kid.Forms.Add(new FormStage(16, "kid_oozaru", "Oozaru", 34, 12, -1.65f));
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

            Items.Add("KI_CELL", new ItemDefinition { Id = "KI_CELL", Name = "Célula de Ki", Price = 300, MaxKi = 45, KiRegen = 2.5f, Category = "Ki", IconSymbol = "✦", Description = "+45 Ki maximo, +2.5 Regeneracao Ki/s" });
            Items.Add("ARMOR", new ItemDefinition { Id = "ARMOR", Name = "Placa de Armadura", Price = 400, Armor = 30, Category = "Defesa", IconSymbol = "◆", Description = "+30 Armadura" });
            Items.Add("GLOVES", new ItemDefinition { Id = "GLOVES", Name = "Luvas Reforçadas", Price = 350, Attack = 15, Category = "Ataque", IconSymbol = "✦", Description = "+15 Ataque" });
            Items.Add("TRAINING_BOOTS", new ItemDefinition { Id = "TRAINING_BOOTS", Name = "Botas de Treinamento", Price = 1100, TrainingBoots = true, Category = "Movimento", IconSymbol = "»", Description = "−8% movimento por 300s; depois +8% permanente" });
            // Every recipe cost is the total price. The shop consumes owned ingredients,
            // charges only the difference, and stores only the completed item's modifiers.
            AddItem("HEALTH_GEM", "Cristal de Vida", 400, "Defesa", "♥", "+150 Vida", maxHp:150);
            AddItem("RESIST_CORE", "Nucleo Isolante", 400, "Defesa", "◆", "+25 Resistencia de Ki", kiResist:25);
            AddItem("REGEN_SEED", "Semente Revigorante", 400, "Defesa", "+", "+3 Vida/5s", hpRegen:3);
            AddItem("ENERGY_FOCUS", "Condutor de Ki", 500, "Ki", "✦", "+20 Poder de Ki", kiPower:20);
            AddItem("FAST_GEAR", "Engrenagem Agil", 350, "Ataque", "»", "+12% Velocidade de ataque", attackSpeed:.12f);
            AddItem("VAMP_ESSENCE", "Essencia Vampirica", 600, "Ataque", "♥", "+6% Roubo de vida", lifeSteal:.06f);
            AddItem("HASTE_SHARD", "Fragmento de Aceleracao", 500, "Ki", "◈", "+10 Aceleracao", haste:10);
            AddItem("BASIC_BOOTS", "Botas Comuns", 300, "Movimento", "»", "+0.35 Movimentacao", speed:.35f);
            AddItem("DRAGON_GLOVES", "Luvas do Dragao", 2800, "Ataque", "✦", "+45 Ataque, +200 Vida", attack:45, maxHp:200,
                components:new[]{"GLOVES","GLOVES","HEALTH_GEM"});
            AddItem("KAIO_CRYSTAL", "Cristal de Kaio", 3000, "Ki", "✦", "+70 Ki, +20 Aceleracao", kiPower:70, haste:20,
                components:new[]{"ENERGY_FOCUS","HASTE_SHARD","KI_CELL"});
            AddItem("PRECISION_GAUNTLET", "Bracadeira de Precisao", 2700, "Ataque", "✦", "+35 Ataque, +25% Critico", attack:35, critChance:.25f,
                components:new[]{"GLOVES","FAST_GEAR"});
            AddItem("ENERGY_LENSES", "Lentes de Energia", 2800, "Ki", "◆", "+60 Ki, +15% Penetracao Ki", kiPower:60, kiPen:.15f,
                components:new[]{"ENERGY_FOCUS","ENERGY_FOCUS"});
            AddItem("FRIEZA_VEST", "Colete do Exercito Freeza", 2800, "Defesa", "◆", "+500 Vida, +55 Armadura", maxHp:500, armor:55,
                components:new[]{"HEALTH_GEM","ARMOR"});
            AddItem("NAMEK_BARRIER", "Barreira Namekuseijin", 3000, "Defesa", "◆", "+350 Vida, +55 Resistencia Ki", maxHp:350, kiResist:55,
                components:new[]{"HEALTH_GEM","RESIST_CORE"});
            AddItem("REGEN_CORE", "Nucleo de Regeneracao", 3100, "Defesa", "♥", "+500 Vida, +8 Vida/5s", maxHp:500, hpRegen:8,
                components:new[]{"HEALTH_GEM","REGEN_SEED"});
            AddItem("CAPSULE_SWORD", "Espada da Corporacao Capsula", 3000, "Ataque", "⚔", "+50 Ataque, +10% Roubo de vida", attack:50, lifeSteal:.1f,
                components:new[]{"GLOVES","VAMP_ESSENCE"});
            AddItem("INTERFERENCE_PULSE", "Pulso de Interferencia", 2800, "Ki", "✦", "+40 Ki, +8% Vampirismo", kiPower:40, spellVamp:.08f,
                components:new[]{"ENERGY_FOCUS","ENERGY_FOCUS"});
            AddItem("SHIELD_AMPLIFIER", "Amplificador de Escudo", 2700, "Defesa", "◆", "+250 Vida, +20 Poder Ki", maxHp:250, kiPower:20,
                components:new[]{"ENERGY_FOCUS","HASTE_SHARD"});
            AddItem("ANTI_HEAL_BLADE", "Lamina de Supressao", 2600, "Ataque", "⚔", "+40 Ataque; aplica -35% de cura por 3s", attack:40, antiHeal:.35f,
                components:new[]{"GLOVES","ENERGY_FOCUS"});
            AddItem("ARMORED_BOOTS", "Botas de Armadura Saiyajin", 1200, "Movimento", "»", "+0.4 Movimento, +30 Armadura", armor:30, speed:.4f,
                components:new[]{"BASIC_BOOTS","ARMOR"});
            AddItem("KI_BOOTS", "Botas de Ki", 1200, "Movimento", "»", "+0.4 Movimento, +30 Resistencia Ki", kiResist:30, speed:.4f,
                components:new[]{"BASIC_BOOTS","RESIST_CORE"});
        }

        private static void AddItem(string id, string name, int price, string category, string icon,
            string description, float maxHp = 0, float hpRegen = 0, float kiResist = 0,
            float kiPower = 0, float attackSpeed = 0, float lifeSteal = 0, float spellVamp = 0,
            float critChance = 0, float haste = 0, float armorPen = 0, float kiPen = 0,
            float speed = 0, float attack = 0, float armor = 0, float antiHeal = 0,
            string[] components = null)
        {
            Items.Add(id, new ItemDefinition {
                Id = id, Name = name, Price = price, Category = category, IconSymbol = icon,
                Description = description, MaxHp = maxHp, HpRegenPer5 = hpRegen, KiResist = kiResist,
                KiPower = kiPower, AttackSpeedBonus = attackSpeed, LifeSteal = lifeSteal,
                SpellVamp = spellVamp, CritChance = critChance, AbilityHaste = haste,
                ArmorPenPercent = armorPen, KiPenPercent = kiPen, MoveSpeed = speed,
                Attack = attack, Armor = armor, AntiHealOnHit = antiHeal,
                Components = components ?? new string[0]
            });
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
