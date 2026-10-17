using System;
using System.Collections.Generic;

namespace DragonMoba.Domain
{
    public sealed class EntityState
    {
        public int Id;
        public EntityKind Kind;
        public Team Team;
        public string HeroId;
        public string DisplayName;
        public string FormId;
        public string FormName;
        public int Level = 1;
        public float Experience;
        public int Zeni;
        public float PassiveZeniRemainder;
        public V2 Position;
        public V2 Destination;
        public int AttackTargetId;
        public bool AttackMoveActive;
        // Defensive structures temporarily prioritize enemy heroes attacking allies.
        public int TowerAggroTargetId;
        public float TowerAggroSeconds;
        public int Lane;
        public float MaxHp;
        public float Hp;
        public float MaxKi;
        public float Ki;
        public float KiRegen = 2f;
        // Derived from level, current form and owned items (StatCalculator).
        public float HpRegenPer5;
        public float AttackSpeedBonus;
        public float LifeSteal;
        public float SpellVamp;
        public float CritChance;
        public float AbilityHaste;
        public float ArmorPenPercent;
        public float KiPenPercent;
        public float AntiHealOnHit;
        public float HealingReductionSeconds;
        public float HealingReductionRate;
        public int BasicAttacksIssued;
        // Read-only combat feedback, updated during charging/training.
        public float TrainingXpPerSecond;

        public float Armor = 25f;
        public float KiResist = 25f;
        public float PhysicalAttack;
        public float KiPower;
        public float MoveSpeed;
        public float AttackRange;
        public float AttackCooldown;
        public float AttackInterval = 1.1f;
        public float RespawnTime;
        public float Shield;
        public float ShieldLifetime;
        public bool Charging;
        public float ChargeSeconds;
        public float TrainingSeconds;
        public float TrainingBootsRemaining;
        public bool TrainingBootsCompleted;
        public bool BoughtTrainingBoots;
        // An unspent point is granted at level 1 and once for every level gained.
        public int UnspentSkillPoints = 1;
        public readonly int[] SkillRanks = new int[4];
        public readonly float[] SkillCooldown = new float[4];
        public readonly List<string> Inventory = new List<string>();
        public bool Alive { get { return Hp > 0f; } }
        public bool IsMoving { get { return V2.Distance(Position, Destination) > 0.1f; } }

        public static EntityState CreateHero(int id, Team team, string heroId, V2 position)
        {
            var def = Catalog.Hero(heroId);
            if (def == null) throw new ArgumentException("Heroi nao cadastrado: " + heroId);
            return new EntityState {
                Id = id, Kind = EntityKind.Hero, Team = team, HeroId = heroId,
                DisplayName = def.Name, FormId = def.Forms[0].Id, FormName = def.Forms[0].DisplayName,
                MaxHp = def.MaxHp, Hp = def.MaxHp, MaxKi = def.MaxKi, Ki = def.MaxKi,
                PhysicalAttack = def.BaseAttack, KiPower = def.KiPower,
                MoveSpeed = def.BaseSpeed, AttackRange = def.AttackRange,
                Position = position, Destination = position, Zeni = 1200
            };
        }
    }
}
