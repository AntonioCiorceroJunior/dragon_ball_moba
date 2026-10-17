// Deterministic, Unity-independent derived-stat calculator.
// Rebuilds attributes from level, form and inventory. Never mutates definition data.
using System;

namespace DragonMoba.Domain
{
    public static class StatCalculator
    {
        public static void Refresh(EntityState hero)
        {
            if (hero == null || hero.Kind != EntityKind.Hero) return;
            var def = Catalog.Hero(hero.HeroId);
            if (def == null) return;
            var oldMaxHp = hero.MaxHp;
            var oldMaxKi = hero.MaxKi;
            var levels = Math.Max(0, hero.Level - 1);
            float maxHp = def.MaxHp + 24f * levels;
            float maxKi = def.MaxKi;
            float attack = def.BaseAttack + 2.8f * levels;
            float kiPower = def.KiPower + 3.1f * levels;
            float speed = def.BaseSpeed;
            float armor = 25f + .9f * levels;
            float kiResist = 25f + .9f * levels;
            float regen = 6f + .65f * levels;
            float kiRegen = 2f;
            float attackSpeed = .012f * levels;
            float lifeSteal = 0f, spellVamp = 0f, crit = 0f, haste = 0f;
            float armorPen = 0f, kiPen = 0f, antiHeal = 0f;
            // Historical form bonuses are incremental for every unlocked stage.
            foreach (var stage in def.Forms)
            {
                if (stage.Level <= 1 || stage.Level > hero.Level) continue;
                attack += stage.BonusAttack;
                kiPower += stage.BonusKiPower;
                speed += stage.BonusSpeed;
            }
            foreach (var itemId in hero.Inventory)
            {
                var item = Catalog.Item(itemId);
                if (item == null) continue;
                maxHp += item.MaxHp;
                maxKi += item.MaxKi;
                attack += item.Attack;
                kiPower += item.KiPower;
                armor += item.Armor;
                kiResist += item.KiResist;
                regen += item.HpRegenPer5;
                kiRegen += item.KiRegen;
                attackSpeed += item.AttackSpeedBonus;
                lifeSteal += item.LifeSteal;
                spellVamp += item.SpellVamp;
                crit += item.CritChance;
                haste += item.AbilityHaste;
                armorPen += item.ArmorPenPercent;
                kiPen += item.KiPenPercent;
                speed += item.MoveSpeed;
                antiHeal += item.AntiHealOnHit;
            }
            hero.MaxHp = Math.Max(1f, maxHp);
            hero.MaxKi = Math.Max(1f, maxKi);
            // Leveling and max-resource purchases grant their newly earned capacity.
            hero.Hp = Math.Min(hero.MaxHp, Math.Max(0f, hero.Hp + Math.Max(0f, hero.MaxHp - oldMaxHp)));
            hero.Ki = Math.Min(hero.MaxKi, Math.Max(0f, hero.Ki + Math.Max(0f, hero.MaxKi - oldMaxKi)));
            hero.PhysicalAttack = Math.Max(0f, attack);
            hero.KiPower = Math.Max(0f, kiPower);
            hero.Armor = armor;
            hero.KiResist = kiResist;
            hero.HpRegenPer5 = Math.Max(0f, regen);
            hero.KiRegen = Math.Max(0f, kiRegen);
            hero.MoveSpeed = Math.Max(.75f, speed);
            hero.AttackSpeedBonus = Math.Max(-.5f, attackSpeed);
            hero.AttackInterval = Math.Max(.4f, 1.1f / (1f + hero.AttackSpeedBonus));
            hero.LifeSteal = Math.Min(.65f, Math.Max(0f, lifeSteal));
            hero.SpellVamp = Math.Min(.4f, Math.Max(0f, spellVamp));
            hero.CritChance = Math.Min(1f, Math.Max(0f, crit));
            hero.AbilityHaste = Math.Max(0f, haste);
            hero.ArmorPenPercent = Math.Min(.6f, Math.Max(0f, armorPen));
            hero.KiPenPercent = Math.Min(.6f, Math.Max(0f, kiPen));
            hero.AntiHealOnHit = Math.Min(.6f,Math.Max(0f,antiHeal));
        }
    }
}
