// Single-process authoritative simulation for the offline prototype.
// The Unity presentation sends commands and reads snapshots; it does not apply damage/XP.
using System;
using System.Collections.Generic;

namespace DragonMoba.Domain
{
    public sealed class GameWorld
    {
        public const float FixedStep = 1f / 30f;
        public const float BaseRadius = 9f;
        private readonly List<EntityState> entities = new List<EntityState>();
        private readonly List<GameEvent> events = new List<GameEvent>();
        private int nextId = 1;
        private float waveTimer;
        private float elapsed;
        private int currentTick;
        private readonly int[] laneX = { -17, 0, 17 };

        public IReadOnlyList<EntityState> Entities { get { return entities; } }
        public float Elapsed { get { return elapsed; } }
        public int TickIndex { get { return currentTick; } }
        public int PlayerId { get; private set; }
        public Team? Winner { get; private set; }

        public GameWorld(string playerHeroId)
        {
            if (Catalog.Hero(playerHeroId) == null) throw new ArgumentException("Unknown hero: " + playerHeroId);
            SpawnStructure(EntityKind.Core, Team.Blue, 0, new V2(0, -38), 1500);
            SpawnStructure(EntityKind.Core, Team.Red, 0, new V2(0, 38), 1500);
            for (var lane = 0; lane < laneX.Length; ++lane)
            {
                SpawnStructure(EntityKind.Tower, Team.Blue, lane, new V2(laneX[lane], -19), 850);
                SpawnStructure(EntityKind.Tower, Team.Red, lane, new V2(laneX[lane], 19), 850);
            }
            var player = SpawnHero(Team.Blue, playerHeroId, new V2(0, -29));
            PlayerId = player.Id;
            SpawnHero(Team.Red, playerHeroId == "CELL_Z" ? "GKU_SUPER" : "CELL_Z", new V2(0, 29));
            SpawnWave();
        }

        public EntityState Find(int id)
        {
            for (var i = 0; i < entities.Count; ++i) if (entities[i].Id == id) return entities[i];
            return null;
        }

        public EntityState Player { get { return Find(PlayerId); } }

        public List<GameEvent> DrainEvents()
        {
            var result = new List<GameEvent>(events);
            events.Clear();
            return result;
        }

        public void Tick(float dt)
        {
            if (Winner.HasValue || dt <= 0f) return;
            dt = Math.Min(dt, 0.1f);
            elapsed += dt;
            ++currentTick;
            waveTimer += dt;
            if (waveTimer >= 14f)
            {
                waveTimer -= 14f;
                SpawnWave();
            }
            // Timers and progression must be advanced before resolving attacks.
            foreach (var e in entities)
            {
                if (e.Kind != EntityKind.Hero) continue;
                TickHeroTimers(e, dt);
            }
            foreach (var e in entities)
            {
                if (!e.Alive || e.Kind == EntityKind.Core) continue;
                if (e.Kind == EntityKind.Hero && e.Id != PlayerId) ThinkBot(e);
                if (e.Kind == EntityKind.Minion) ThinkMinion(e);
                if (e.Kind == EntityKind.Tower) ThinkTower(e);
            }
            foreach (var e in entities)
            {
                if (!e.Alive || e.Kind == EntityKind.Tower || e.Kind == EntityKind.Core) continue;
                UpdateMovement(e, dt);
            }
            // Snapshot before damage: no iteration over entities after modifying membership.
            var snapshot = new List<EntityState>(entities);
            foreach (var e in snapshot)
            {
                if (!e.Alive) continue;
                var target = Find(e.AttackTargetId);
                if (target != null && target.Alive && target.Team != e.Team &&
                    InRange(e, target, e.AttackRange) && e.AttackCooldown <= 0f && !e.Charging)
                {
                    e.AttackCooldown = e.AttackInterval;
                    DealDamage(e, target, e.PhysicalAttack, false);
                    events.Add(new GameEvent(EventKind.Attack, e.Id, target.Position, "Ataque", e.PhysicalAttack, target.Id));
                }
            }
            entities.RemoveAll(e => e.Kind == EntityKind.Minion && !e.Alive);
        }

        public bool Submit(GameCommand cmd)
        {
            if (cmd == null || Winner.HasValue) return false;
            var hero = Find(cmd.ActorId);
            if (hero == null || hero.Kind != EntityKind.Hero || !hero.Alive) return false;
            // This offline prototype only accepts user input for the selected local hero.
            if (hero.Id != PlayerId) return false;
            switch (cmd.Kind)
            {
                case CommandKind.Move:
                    hero.Charging = false;
                    hero.ChargeSeconds = 0;
                    hero.AttackTargetId = 0;
                    hero.Destination = V2.Clamp(cmd.Position);
                    return true;
                case CommandKind.Attack:
                    var target = Find(cmd.TargetId);
                    if (target == null || !target.Alive || target.Team == hero.Team) return false;
                    hero.Charging = false;
                    hero.ChargeSeconds = 0;
                    hero.AttackTargetId = target.Id;
                    return true;
                case CommandKind.Charge:
                    hero.Charging = cmd.Pressed;
                    hero.ChargeSeconds = 0;
                    hero.Destination = hero.Position;
                    hero.AttackTargetId = 0;
                    return true;
                case CommandKind.Cast:
                    return CastAbility(hero, cmd.Slot, cmd.Position);
                case CommandKind.Buy:
                    return Purchase(hero, cmd.ItemId);
                default:
                    return false;
            }
        }

        private EntityState SpawnHero(Team team, string champion, V2 pos)
        {
            var e = EntityState.CreateHero(nextId++, team, champion, pos);
            entities.Add(e);
            return e;
        }

        private EntityState SpawnStructure(EntityKind kind, Team team, int lane, V2 pos, float hp)
        {
            var e = new EntityState {
                Id = nextId++, Kind = kind, Team = team, Lane = lane, Position = pos,
                Destination = pos, DisplayName = kind == EntityKind.Core ? "Nucleo" : "Torre",
                MaxHp = hp, Hp = hp, PhysicalAttack = kind == EntityKind.Core ? 0 : 68,
                AttackRange = kind == EntityKind.Tower ? 11.5f : 0,
                AttackInterval = kind == EntityKind.Tower ? 1.4f : 0,
                Armor = kind == EntityKind.Tower ? 45 : 25, KiResist = 25
            };
            entities.Add(e);
            return e;
        }

        private void SpawnWave()
        {
            for (var lane = 0; lane < laneX.Length; ++lane)
            {
                foreach (Team team in new[] { Team.Blue, Team.Red })
                {
                    var spawnY = team == Team.Blue ? -32f : 32f;
                    for (var index = 0; index < 3; ++index)
                    {
                        var pos = new V2(laneX[lane] + (index - 1) * 1.3f, spawnY);
                        entities.Add(new EntityState {
                            Id = nextId++, Team = team, Kind = EntityKind.Minion, Lane = lane,
                            DisplayName = "Robo de Treinamento", Position = pos, Destination = pos,
                            MaxHp = 190, Hp = 190, PhysicalAttack = 19, MoveSpeed = 3.25f,
                            AttackRange = 2.5f, AttackInterval = 1.5f, Armor = 10
                        });
                    }
                }
            }
        }

        private void TickHeroTimers(EntityState hero, float dt)
        {
            for (var i = 0; i < hero.SkillCooldown.Length; ++i)
                hero.SkillCooldown[i] = Math.Max(0f, hero.SkillCooldown[i] - dt);
            hero.AttackCooldown = Math.Max(0f, hero.AttackCooldown - dt);
            if (!hero.Alive)
            {
                hero.RespawnTime -= dt;
                if (hero.RespawnTime <= 0)
                {
                    hero.Hp = hero.MaxHp;
                    hero.Ki = hero.MaxKi;
                    hero.Shield = 0;
                    hero.Position = BasePosition(hero.Team);
                    hero.Destination = hero.Position;
                    hero.AttackTargetId = 0;
                    hero.Charging = false;
                    events.Add(new GameEvent(EventKind.Respawn, hero.Id, hero.Position, "Ressurgiu"));
                }
                return;
            }
            hero.Ki = Math.Min(hero.MaxKi, hero.Ki + hero.KiRegen * dt);
            if (hero.ShieldLifetime > 0f)
            {
                hero.ShieldLifetime = Math.Max(0f, hero.ShieldLifetime - dt);
                if (hero.ShieldLifetime == 0f) hero.Shield = 0f;
            }
            if (NearBase(hero)) hero.Hp = Math.Min(hero.MaxHp, hero.Hp + 80f * dt);
            hero.TrainingSeconds = Math.Max(0, hero.TrainingSeconds - (hero.Charging ? 0f : dt * 0.25f));
            if (hero.TrainingBootsRemaining > 0 && !NearBase(hero))
            {
                hero.TrainingBootsRemaining = Math.Max(0, hero.TrainingBootsRemaining - dt);
                if (hero.TrainingBootsRemaining <= 0 && !hero.TrainingBootsCompleted)
                {
                    hero.TrainingBootsCompleted = true;
                    hero.Inventory.Remove("TRAINING_BOOTS");
                    events.Add(new GameEvent(EventKind.Purchase, hero.Id, hero.Position, "Treinamento concluido: +8% de velocidade"));
                }
            }
            if (!hero.Charging) return;
            hero.Destination = hero.Position;
            hero.AttackTargetId = 0;
            hero.ChargeSeconds += dt;
            if (hero.ChargeSeconds < 0.15f) return;
            if (hero.Ki < hero.MaxKi - 0.00001f)
            {
                hero.Ki = Math.Min(hero.MaxKi, hero.Ki + (15f + hero.KiRegen) * dt);
            }
            else if (!NearBase(hero))
            {
                hero.TrainingSeconds += dt;
                var multiplier = hero.TrainingSeconds > 30f ? 0.55f : 1f;
                GrantExperience(hero, 1.8f * multiplier * dt);
            }
        }

        private void UpdateMovement(EntityState entity, float dt)
        {
            entity.AttackCooldown = Math.Max(0f, entity.AttackCooldown - (entity.Kind == EntityKind.Hero ? 0f : dt));
            if (entity.Charging) return;
            var victim = Find(entity.AttackTargetId);
            var destination = entity.Destination;
            if (victim != null && victim.Alive && victim.Team != entity.Team)
            {
                if (InRange(entity, victim, entity.AttackRange)) return;
                destination = victim.Position;
            }
            var speed = entity.MoveSpeed;
            if (entity.TrainingBootsRemaining > 0) speed *= 0.92f;
            if (entity.TrainingBootsCompleted) speed *= 1.08f;
            entity.Position = V2.Clamp(V2.MoveTowards(entity.Position, destination, speed * dt));
        }

        private void ThinkMinion(EntityState minion)
        {
            var enemy = NearestEnemy(minion, 7.5f, true);
            if (enemy == null)
            {
                enemy = TargetLaneStructure(minion.Team, minion.Lane);
                if (enemy == null) enemy = EnemyCore(minion.Team);
            }
            minion.AttackTargetId = enemy == null ? 0 : enemy.Id;
            if (enemy != null) minion.Destination = enemy.Position;
        }

        private EntityState TargetLaneStructure(Team attacker, int lane)
        {
            foreach (var e in entities)
                if (e.Kind == EntityKind.Tower && e.Team != attacker && e.Lane == lane && e.Alive) return e;
            return null;
        }

        private EntityState EnemyCore(Team attacker)
        {
            foreach (var e in entities)
                if (e.Kind == EntityKind.Core && e.Team != attacker && e.Alive) return e;
            return null;
        }

        private void ThinkTower(EntityState turret)
        {
            if (!turret.Alive) return;
            var nearestMinion = NearestEnemy(turret, turret.AttackRange, true);
            turret.AttackTargetId = nearestMinion == null ? 0 : nearestMinion.Id;
            turret.AttackCooldown = Math.Max(0, turret.AttackCooldown - FixedStep);
        }

        private void ThinkBot(EntityState bot)
        {
            if (!bot.Alive) return;
            var target = NearestEnemy(bot, 11f, true);
            if (target == null) target = TargetLaneStructure(bot.Team, 1);
            if (target == null) target = EnemyCore(bot.Team);
            if (target != null)
            {
                bot.AttackTargetId = target.Id;
                bot.Destination = target.Position;
                if (V2.Distance(bot.Position, target.Position) < 8f && bot.SkillCooldown[0] <= 0 && bot.Ki >= 20)
                    CastAbility(bot, AbilitySlot.Q, target.Position);
            }
        }

        private EntityState NearestEnemy(EntityState source, float range, bool minionsPreferred)
        {
            EntityState best = null;
            var bestScore = float.MaxValue;
            foreach (var e in entities)
            {
                if (!e.Alive || e.Team == source.Team || e.Team == Team.Neutral || e.Kind == EntityKind.Core) continue;
                var d = V2.Distance(source.Position, e.Position);
                if (d > range) continue;
                // Minion/turret waves generally protect heroes and structures.
                var score = d + (minionsPreferred && e.Kind == EntityKind.Hero ? 5f : 0f);
                if (score >= bestScore) continue;
                bestScore = score;
                best = e;
            }
            return best;
        }

        private bool CastAbility(EntityState actor, AbilitySlot slot, V2 targetPoint)
        {
            var i = (int)slot;
            if (i < 0 || i > 3 || actor.SkillCooldown[i] > 0 || actor.Charging || !actor.Alive) return false;
            var cost = new[] { 20f, 25f, 30f, 50f }[i];
            var cooldown = new[] { 4f, 8f, 12f, 30f }[i];
            if (actor.Ki < cost) return false;
            if (slot != AbilitySlot.W && slot != AbilitySlot.E && V2.Distance(actor.Position, targetPoint) > 12f) return false;
            actor.Ki -= cost;
            actor.SkillCooldown[i] = cooldown * 100f / 100f;
            actor.AttackTargetId = 0;
            var abilityName = new[] { "Rajada de Ki", "Impulso", "Barreira", "Explosao Suprema" }[i];
            switch (slot)
            {
                case AbilitySlot.Q:
                    var hit = NearestToPoint(actor.Team, targetPoint, 2.7f, actor.Position, 12f);
                    if (hit != null) DealDamage(actor, hit, 60f + actor.KiPower * 0.85f, true);
                    break;
                case AbilitySlot.W:
                    actor.Position = V2.Clamp(V2.MoveTowards(actor.Position, targetPoint, 6f));
                    actor.Destination = actor.Position;
                    break;
                case AbilitySlot.E:
                    actor.Shield = Math.Min(actor.MaxHp * 0.5f, actor.Shield + 80f + actor.KiPower * 0.5f);
                    actor.ShieldLifetime = 5f;
                    break;
                case AbilitySlot.R:
                    var targets = new List<EntityState>(entities);
                    foreach (var e in targets)
                        if (e.Alive && e.Team != actor.Team && e.Team != Team.Neutral &&
                            V2.Distance(targetPoint, e.Position) <= 4f && V2.Distance(actor.Position, e.Position) <= 12f)
                            DealDamage(actor, e, 105f + actor.KiPower * 1.3f, true);
                    break;
            }
            events.Add(new GameEvent(EventKind.Ability, actor.Id, targetPoint, abilityName, 0, actor.Id));
            return true;
        }

        private EntityState NearestToPoint(Team attacker, V2 center, float radius, V2 source, float maxRange)
        {
            EntityState chosen = null;
            var best = float.MaxValue;
            foreach (var e in entities)
            {
                if (!e.Alive || e.Team == attacker || e.Team == Team.Neutral) continue;
                if (e.Kind == EntityKind.Core && ProtectedCore(e)) continue;
                if (V2.Distance(source, e.Position) > maxRange) continue;
                var d = V2.Distance(center, e.Position);
                if (d < radius && d < best) { chosen = e; best = d; }
            }
            return chosen;
        }

        public bool DealDamage(EntityState actor, EntityState victim, float raw, bool kiDamage)
        {
            if (actor == null || victim == null || !actor.Alive || !victim.Alive || actor.Team == victim.Team) return false;
            if (victim.Kind == EntityKind.Core && ProtectedCore(victim)) return false;
            var amount = Catalog.Mitigate(raw, kiDamage ? victim.KiResist : victim.Armor);
            if (victim.Shield > 0)
            {
                var absorbed = Math.Min(victim.Shield, amount);
                victim.Shield -= absorbed;
                amount -= absorbed;
            }
            victim.Hp = Math.Max(0, victim.Hp - amount);
            if (victim.Kind == EntityKind.Hero)
            {
                victim.Charging = false;
                victim.ChargeSeconds = 0;
            }
            events.Add(new GameEvent(EventKind.Damage, actor.Id, victim.Position, "-" + (int)amount, amount, victim.Id));
            if (!victim.Alive) ProcessDeath(actor, victim);
            return true;
        }

        private bool ProtectedCore(EntityState core)
        {
            foreach (var e in entities)
                if (e.Team == core.Team && e.Kind == EntityKind.Tower && e.Alive) return true;
            return false;
        }

        private void ProcessDeath(EntityState killer, EntityState victim)
        {
            victim.AttackTargetId = 0;
            victim.Charging = false;
            events.Add(new GameEvent(EventKind.Death, killer.Id, victim.Position, victim.DisplayName + " derrotado", 0, victim.Id));
            if (victim.Kind == EntityKind.Core)
            {
                Winner = killer.Team;
                events.Add(new GameEvent(EventKind.Victory, killer.Id, victim.Position, "Nucleo destruido"));
                return;
            }
            if (victim.Kind == EntityKind.Hero) victim.RespawnTime = 8 + victim.Level * 1.5f;
            var reward = victim.Kind == EntityKind.Minion ? 24f : victim.Kind == EntityKind.Hero ? 180f : 110f;
            var zeni = victim.Kind == EntityKind.Minion ? 28 : victim.Kind == EntityKind.Hero ? 280 : 160;
            if (killer.Kind == EntityKind.Hero) killer.Zeni += zeni;
            foreach (var e in entities)
                if (e.Kind == EntityKind.Hero && e.Team == killer.Team && e.Alive &&
                    V2.Distance(e.Position, victim.Position) <= 13f)
                    GrantExperience(e, reward);
        }

        public void GrantExperience(EntityState hero, float amount)
        {
            if (hero == null || hero.Kind != EntityKind.Hero || amount <= 0f || hero.Level >= 18) return;
            hero.Experience += amount;
            while (hero.Level < 18 && hero.Experience >= Catalog.ExperienceForNextLevel(hero.Level))
            {
                hero.Experience -= Catalog.ExperienceForNextLevel(hero.Level);
                ++hero.Level;
                hero.MaxHp += 24;
                hero.Hp = Math.Min(hero.MaxHp, hero.Hp + 24);
                hero.PhysicalAttack += 2.8f;
                hero.KiPower += 3.1f;
                events.Add(new GameEvent(EventKind.LevelUp, hero.Id, hero.Position, "Nivel " + hero.Level));
                ApplyForm(hero);
            }
            if (hero.Level == 18) hero.Experience = 0;
        }

        private void ApplyForm(EntityState hero)
        {
            var def = Catalog.Hero(hero.HeroId);
            foreach (var stage in def.Forms)
            {
                if (stage.Level > hero.Level || stage.Id == hero.FormId) continue;
                var currentLevel = 0;
                foreach (var old in def.Forms)
                    if (old.Id == hero.FormId) currentLevel = old.Level;
                if (stage.Level <= currentLevel) continue;
                hero.PhysicalAttack += stage.BonusAttack;
                hero.KiPower += stage.BonusKiPower;
                hero.MoveSpeed += stage.BonusSpeed;
                hero.FormId = stage.Id;
                hero.FormName = stage.DisplayName;
                events.Add(new GameEvent(EventKind.Transform, hero.Id, hero.Position, hero.FormName));
            }
        }

        private bool Purchase(EntityState hero, string itemId)
        {
            var item = Catalog.Item(itemId);
            if (item == null || !NearBase(hero) || hero.Inventory.Count >= 6 || hero.Zeni < item.Price) return false;
            if (item.TrainingBoots && hero.BoughtTrainingBoots) return false;
            hero.Zeni -= item.Price;
            hero.MaxKi += item.MaxKi;
            hero.Ki += item.MaxKi;
            hero.KiRegen += item.KiRegen;
            hero.Armor += item.Armor;
            hero.PhysicalAttack += item.Attack;
            hero.Inventory.Add(item.Id);
            if (item.TrainingBoots)
            {
                hero.BoughtTrainingBoots = true;
                hero.TrainingBootsRemaining = 300f;
            }
            events.Add(new GameEvent(EventKind.Purchase, hero.Id, hero.Position, "Comprou " + item.Name));
            return true;
        }

        private static bool InRange(EntityState a, EntityState b, float range)
        {
            return V2.Distance(a.Position, b.Position) <= range;
        }
        private static V2 BasePosition(Team team) { return team == Team.Blue ? new V2(0, -34) : new V2(0, 34); }
        private static bool NearBase(EntityState hero) { return V2.Distance(hero.Position, BasePosition(hero.Team)) < BaseRadius; }
    }
}
