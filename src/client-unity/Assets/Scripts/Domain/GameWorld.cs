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
        public const float TowerBodyRadius = 1.45f;
        public const float CoreBodyRadius = 1.65f;
        public const float HeroBodyRadius = 0.5f;
        public const float MinionBodyRadius = 0.4f;
        public const float TowerAggroDuration = 3f;
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
                if (e.Kind == EntityKind.Hero && e.Id == PlayerId && e.AttackMoveActive &&
                    e.AttackTargetId == 0 && !e.Charging)
                {
                    var acquired = NearestEnemy(e, e.AttackRange + 2.5f, false);
                    if (acquired != null) e.AttackTargetId = acquired.Id;
                }
                if (e.Kind == EntityKind.Minion) ThinkMinion(e);
                if (e.Kind == EntityKind.Tower) ThinkTower(e, dt);
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
                    var raw = e.PhysicalAttack;
                    if (e.Kind == EntityKind.Hero)
                    {
                        // Deterministic pseudo-random crit roll; no UnityEngine.Random
                        // or variable-frame-time dependency in the domain.
                        ++e.BasicAttacksIssued;
                        var roll = ((e.BasicAttacksIssued * 71 + e.Id * 37 + target.Id * 13) % 100) / 100f;
                        if (roll < e.CritChance) raw *= 1.75f;
                    }
                    DealDamage(e, target, raw, false, e.Kind == EntityKind.Hero);
                    events.Add(new GameEvent(EventKind.Attack, e.Id, target.Position, "Ataque", raw, target.Id));
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
                    hero.AttackMoveActive = false;
                    hero.Destination = ClampWalkable(hero, V2.Clamp(cmd.Position));
                    return true;
                case CommandKind.Attack:
                    var target = Find(cmd.TargetId);
                    if (target == null || !target.Alive || target.Team == hero.Team) return false;
                    hero.Charging = false;
                    hero.ChargeSeconds = 0;
                    hero.AttackTargetId = target.Id;
                    hero.AttackMoveActive = false;
                    // A target order replaces an old ground-move destination.
                    hero.Destination = hero.Position;
                    return true;
                case CommandKind.Charge:
                    hero.Charging = cmd.Pressed;
                    hero.AttackMoveActive = false;
                    hero.ChargeSeconds = 0;
                    hero.Destination = hero.Position;
                    hero.AttackTargetId = 0;
                    return true;
                case CommandKind.Cast:
                    return CastAbility(hero, cmd.Slot, cmd.Position);
                case CommandKind.UpgradeAbility:
                    return TryUpgradeAbility(hero, cmd.Slot);
                case CommandKind.Buy:
                    return Purchase(hero, cmd.ItemId);
                case CommandKind.Sell:
                    return Sell(hero, cmd.ItemId);
                case CommandKind.Stop:
                    hero.Charging = false;
                    hero.AttackMoveActive = false;
                    hero.AttackTargetId = 0;
                    hero.Destination = hero.Position;
                    return true;
                case CommandKind.AttackMove:
                    hero.Charging = false;
                    hero.ChargeSeconds = 0f;
                    hero.AttackTargetId = 0;
                    hero.AttackMoveActive = true;
                    hero.Destination = ClampWalkable(hero,V2.Clamp(cmd.Position));
                    return true;
                default:
                    return false;
            }
        }

        private EntityState SpawnHero(Team team, string champion, V2 pos)
        {
            var e = EntityState.CreateHero(nextId++, team, champion, pos);
            StatCalculator.Refresh(e);
            entities.Add(e);
            if (team != Team.Blue) AutoSpendSkillPoints(e);
            return e;
        }

        private EntityState SpawnStructure(EntityKind kind, Team team, int lane, V2 pos, float hp)
        {
            var e = new EntityState {
                Id = nextId++, Kind = kind, Team = team, Lane = lane, Position = pos,
                Destination = pos, DisplayName = kind == EntityKind.Core ? "Nucleo" : "Torre",
                MaxHp = hp, Hp = hp, PhysicalAttack = kind == EntityKind.Core ? 0 : 125,
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
                    hero.HealingReductionSeconds = 0f;
                    hero.HealingReductionRate = 0f;
                    hero.Position = BasePosition(hero.Team);
                    hero.Destination = hero.Position;
                    hero.AttackTargetId = 0;
                    hero.Charging = false;
                    events.Add(new GameEvent(EventKind.Respawn, hero.Id, hero.Position, "Ressurgiu"));
                }
                return;
            }
            // Gentle passive income begins after the opening phase. Keeps expensive
            // item builds viable without requiring every last hit, but active farming
            // remains more rewarding than waiting at the fountain.
            if (elapsed >= 30f)
            {
                hero.PassiveZeniRemainder += 3f * dt;
                var earned = (int)hero.PassiveZeniRemainder;
                if (earned > 0)
                {
                    hero.Zeni += earned;
                    hero.PassiveZeniRemainder -= earned;
                }
            }
            hero.Ki = Math.Min(hero.MaxKi, hero.Ki + hero.KiRegen * dt);
            if (hero.ShieldLifetime > 0f)
            {
                hero.ShieldLifetime = Math.Max(0f, hero.ShieldLifetime - dt);
                if (hero.ShieldLifetime == 0f) hero.Shield = 0f;
            }
            // Base regeneration applies anywhere; the fountain adds a separate burst.
            hero.HealingReductionSeconds = Math.Max(0f,hero.HealingReductionSeconds-dt);
            if(hero.HealingReductionSeconds <= 0f) hero.HealingReductionRate = 0f;
            var healingFactor = 1f - hero.HealingReductionRate;
            hero.Hp = Math.Min(hero.MaxHp, hero.Hp +
                (hero.HpRegenPer5 / 5f + (NearBase(hero) ? 80f : 0f)) * healingFactor * dt);
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
            hero.TrainingXpPerSecond = 0f;
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
                // Training remains weaker than active lane farming, but gets more
                // noticeable in later levels. Based on project design, not Riot values.
                var rate = (1.8f + Math.Max(0, hero.Level - 1) * 0.22f) * multiplier;
                hero.TrainingXpPerSecond = hero.Level >= 18 ? 0f : rate;
                GrantExperience(hero, hero.TrainingXpPerSecond * dt);
            }
        }

        // Pure domain rules shared by UI, player commands and bot skill allocation.
        public bool CanUpgradeAbility(EntityState hero, AbilitySlot slot)
        {
            if (hero == null || hero.Kind != EntityKind.Hero || hero.UnspentSkillPoints <= 0) return false;
            var i = (int)slot;
            if (i < 0 || i >= hero.SkillRanks.Length) return false;
            var rank = hero.SkillRanks[i];
            if (slot == AbilitySlot.R)
                return rank < 3 && hero.Level >= 6 + 5 * rank;
            return rank < 5 && hero.Level >= 2 * (rank + 1) - 1;
        }

        private bool TryUpgradeAbility(EntityState hero, AbilitySlot slot)
        {
            if (!CanUpgradeAbility(hero, slot)) return false;
            var i = (int)slot;
            ++hero.SkillRanks[i];
            --hero.UnspentSkillPoints;
            events.Add(new GameEvent(EventKind.SkillUp, hero.Id, hero.Position,
                slot + " nivel " + hero.SkillRanks[i]));
            return true;
        }

        private void AutoSpendSkillPoints(EntityState bot)
        {
            // Keep bot skills under the same legality rules as player skills.
            var priority = new[] { AbilitySlot.R, AbilitySlot.Q, AbilitySlot.E, AbilitySlot.W };
            while (bot.UnspentSkillPoints > 0)
            {
                var allocated = false;
                foreach (var slot in priority)
                    if (TryUpgradeAbility(bot, slot)) { allocated = true; break; }
                if (!allocated) break;
            }
        }

        public float BodyRadius(EntityState entity)
        {
            if (entity == null) return 0f;
            switch (entity.Kind)
            {
                case EntityKind.Tower: return TowerBodyRadius;
                case EntityKind.Core: return CoreBodyRadius;
                case EntityKind.Hero: return entity.FormId == "kid_oozaru" ? .95f :
                    entity.HeroId == "GKU_KID" ? .32f : HeroBodyRadius;
                case EntityKind.Minion: return MinionBodyRadius;
                default: return 0f;
            }
        }

        // Structures are solid even for allies. Destroyed structures stop blocking paths.
        public bool IsStructureCollision(EntityState mover, V2 point)
        {
            if (mover == null) return false;
            foreach (var obstacle in entities)
                if (BlocksMovement(mover, obstacle) &&
                    V2.Distance(point, obstacle.Position) < BodyRadius(mover) + BodyRadius(obstacle) - 0.001f)
                    return true;
            return false;
        }

        private static float Dot(V2 a, V2 b) { return a.X * b.X + a.Y * b.Y; }
        private static V2 Minus(V2 a, V2 b) { return new V2(a.X - b.X, a.Y - b.Y); }
        private static V2 Plus(V2 a, V2 b) { return new V2(a.X + b.X, a.Y + b.Y); }
        private static V2 Scale(V2 v, float factor) { return new V2(v.X * factor, v.Y * factor); }
        private static float Length(V2 v) { return (float)Math.Sqrt(Dot(v, v)); }

        private static bool BlocksMovement(EntityState mover, EntityState obstacle)
        {
            return obstacle != mover && obstacle.Alive &&
                (obstacle.Kind == EntityKind.Tower || obstacle.Kind == EntityKind.Core);
        }

        private V2 ClampWalkable(EntityState mover, V2 desired)
        {
            foreach (var obstacle in entities)
            {
                if (!BlocksMovement(mover, obstacle)) continue;
                var clearance = BodyRadius(mover) + BodyRadius(obstacle) + 0.05f;
                var away = Minus(desired, obstacle.Position);
                var length = Length(away);
                if (length >= clearance) continue;
                if (length < 0.0001f) { away = Minus(mover.Position, obstacle.Position); length = Length(away); }
                if (length < 0.0001f) { away = new V2(1f, 0f); length = 1f; }
                desired = V2.Clamp(Plus(obstacle.Position, Scale(away, clearance / length)));
            }
            return desired;
        }

        // Swept circle collision plus a tangent step prevents running through tower
        // centers at low frame rates. No Unity physics dependency in the domain.
        private V2 AdvanceWithoutStructureCollision(EntityState mover, V2 destination, float maxStep)
        {
            if (maxStep <= 0f) return mover.Position;
            var start = mover.Position;
            var proposed = V2.Clamp(V2.MoveTowards(start, destination, maxStep));
            EntityState blocker = null;
            var nearest = float.MaxValue;
            foreach (var obstacle in entities)
            {
                if (!BlocksMovement(mover, obstacle)) continue;
                var clearance = BodyRadius(mover) + BodyRadius(obstacle) + 0.02f;
                var segment = Minus(proposed, start);
                var lengthSquared = Dot(segment, segment);
                var projection = lengthSquared < 0.000001f ? 0f :
                    Math.Max(0f, Math.Min(1f, Dot(Minus(obstacle.Position, start), segment) / lengthSquared));
                var closest = Plus(start, Scale(segment, projection));
                if (V2.Distance(closest, obstacle.Position) >= clearance) continue;
                var distance = V2.Distance(start, obstacle.Position);
                if (distance >= nearest) continue;
                blocker = obstacle;
                nearest = distance;
            }
            if (blocker == null) return proposed;

            var radial = Minus(start, blocker.Position);
            var radialLength = Length(radial);
            if (radialLength < 0.0001f)
            {
                radial = new V2(1, 0);
                radialLength = 1f;
            }
            radial = Scale(radial, 1f / radialLength);
            var desired = Minus(destination, start);
            var cross = radial.X * desired.Y - radial.Y * desired.X;
            var side = Math.Abs(cross) < 0.0001f ? (mover.Id % 2 == 0 ? 1f : -1f)
                : (cross > 0f ? 1f : -1f);
            var tangent = new V2(-radial.Y * side, radial.X * side);
            var step = Math.Min(maxStep, Length(desired));
            var result = V2.Clamp(Plus(start, Scale(tangent, step)));
            // Project back onto the outside boundary if rounding or teleporting
            // brought the actor inside the structure's solid circle.
            var fromCenter = Minus(result, blocker.Position);
            var minimum = BodyRadius(mover) + BodyRadius(blocker) + 0.02f;
            var resultLength = Length(fromCenter);
            if (resultLength < minimum)
                result = V2.Clamp(Plus(blocker.Position, Scale(fromCenter, minimum / Math.Max(resultLength, 0.0001f))));
            return IsStructureCollision(mover, result) ? start : result;
        }

        // A dash travels in short collision-tested segments; a single long tangent
        // step would incorrectly teleport several units sideways through obstacles.
        private void DashWithoutStructureCollision(EntityState mover, V2 goal, float distance)
        {
            var remaining = Math.Min(distance, V2.Distance(mover.Position, goal));
            for (var i = 0; i < 80 && remaining > 0.001f; ++i)
            {
                var step = Math.Min(0.2f, remaining);
                var before = mover.Position;
                mover.Position = AdvanceWithoutStructureCollision(mover, goal, step);
                if (V2.Distance(before, mover.Position) < 0.0001f) break;
                remaining -= step;
            }
        }

        private void UpdateMovement(EntityState entity, float dt)
        {
            entity.AttackCooldown = Math.Max(0f, entity.AttackCooldown - (entity.Kind == EntityKind.Hero ? 0f : dt));
            if (entity.Charging) return;
            var victim = Find(entity.AttackTargetId);
            if (entity.AttackTargetId != 0 && (victim == null || !victim.Alive || victim.Team == entity.Team))
            {
                entity.AttackTargetId = 0;
                if (!entity.AttackMoveActive) entity.Destination = entity.Position;
                victim = null;
            }
            var destination = entity.Destination;
            if (victim != null && victim.Alive && victim.Team != entity.Team)
            {
                if (InRange(entity, victim, entity.AttackRange)) return;
                destination = victim.Position;
            }
            var speed = entity.MoveSpeed;
            if (entity.TrainingBootsRemaining > 0) speed *= 0.92f;
            if (entity.TrainingBootsCompleted) speed *= 1.08f;
            entity.Position = AdvanceWithoutStructureCollision(entity, destination, speed * dt);
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

        private void ThinkTower(EntityState turret, float dt)
        {
            if (!turret.Alive) return;
            turret.AttackCooldown = Math.Max(0f, turret.AttackCooldown - dt);
            turret.TowerAggroSeconds = Math.Max(0f, turret.TowerAggroSeconds - dt);

            var aggro = Find(turret.TowerAggroTargetId);
            if (turret.TowerAggroSeconds > 0f && aggro != null && aggro.Alive &&
                aggro.Kind == EntityKind.Hero && aggro.Team != turret.Team &&
                InRange(turret, aggro, turret.AttackRange))
            {
                turret.AttackTargetId = aggro.Id;
                return;
            }
            turret.TowerAggroTargetId = 0;
            turret.TowerAggroSeconds = 0f;

            // Keep firing at the same minion until it dies or leaves range, even
            // when another minion moves closer. Hero aggression above overrides.
            var retained = Find(turret.AttackTargetId);
            if (retained != null && retained.Kind == EntityKind.Minion &&
                retained.Alive && retained.Team != turret.Team &&
                InRange(turret,retained,turret.AttackRange))
                return;
            // Enemy minions always have priority unless an enemy hero provokes
            // the tower by damaging a defended ally. Never target outside range.
            EntityState nearestMinion = null;
            EntityState nearestHero = null;
            var minionDistance = float.MaxValue;
            var heroDistance = float.MaxValue;
            foreach (var candidate in entities)
            {
                if (!candidate.Alive || candidate.Team == turret.Team ||
                    candidate.Team == Team.Neutral || !InRange(turret, candidate, turret.AttackRange))
                    continue;
                var distance = V2.Distance(turret.Position, candidate.Position);
                if (candidate.Kind == EntityKind.Minion && distance < minionDistance)
                {
                    nearestMinion = candidate; minionDistance = distance;
                }
                else if (candidate.Kind == EntityKind.Hero && distance < heroDistance)
                {
                    nearestHero = candidate; heroDistance = distance;
                }
            }
            var selected = nearestMinion ?? nearestHero;
            turret.AttackTargetId = selected == null ? 0 : selected.Id;
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
            if (i < 0 || i > 3 || actor.SkillRanks[i] == 0 ||
                actor.SkillCooldown[i] > 0 || actor.Charging || !actor.Alive) return false;
            var cost = new[] { 20f, 25f, 30f, 50f }[i];
            var cooldown = new[] { 4f, 8f, 12f, 30f }[i];
            if (actor.Ki < cost) return false;
            if (slot != AbilitySlot.W && slot != AbilitySlot.E && V2.Distance(actor.Position, targetPoint) > 12f) return false;
            actor.Ki -= cost;
            actor.SkillCooldown[i] = cooldown * 100f / (100f + actor.AbilityHaste);
            // Casting a skill must not discard the basic-attack order.
            var names = actor.HeroId == "GKU_KID" ?
                new[] { "Bastao Magico", "Salto do Torneio", "Defesa do Kame", "Golpe do Grande Macaco" } :
                actor.HeroId == "CELL_Z" ?
                new[] { "Absorcao Celular", "Investida Biologica", "Carapaca", "Explosao Perfeita" } :
                new[] { "Rajada de Ki", "Transmissao Curta", "Barreira Divina", "Explosao Suprema" };
            var abilityName = names[i];
            switch (slot)
            {
                case AbilitySlot.Q:
                    var hit = NearestToPoint(actor.Team, targetPoint, 2.7f, actor.Position, 12f);
                    if (hit != null)
                    {
                        var bonus = 24f * (actor.SkillRanks[i]-1);
                        if (actor.HeroId == "GKU_KID")
                            DealDamage(actor,hit,45f+actor.PhysicalAttack*1.15f+bonus,false);
                        else if (actor.HeroId == "CELL_Z")
                        {
                            var hpBefore = hit.Hp;
                            if (DealDamage(actor,hit,50f+actor.KiPower*.65f+bonus,true))
                                actor.Hp=Math.Min(actor.MaxHp,actor.Hp+(hpBefore-hit.Hp)*.16f);
                        }
                        else
                            DealDamage(actor,hit,60f+actor.KiPower*.85f+bonus,true);
                    }
                    break;
                case AbilitySlot.W:
                    DashWithoutStructureCollision(actor, targetPoint,
                        (6f + actor.SkillRanks[i] - 1) *
                        (actor.FormId == "kid_oozaru" ? .65f :
                         actor.HeroId == "CELL_Z" ? .85f :
                         actor.HeroId == "GKU_KID" ? 1.15f : 1f));
                    actor.Destination = actor.Position;
                    break;
                case AbilitySlot.E:
                    actor.Shield = Math.Min(actor.MaxHp * 0.5f,
                        actor.Shield + (actor.HeroId == "CELL_Z" ? 105f :
                            actor.HeroId == "GKU_KID" ? 65f : 80f) +
                        actor.KiPower * (actor.HeroId == "CELL_Z" ? .3f : .5f) +
                        25f * (actor.SkillRanks[i] - 1));
                    actor.ShieldLifetime = 5f;
                    break;
                case AbilitySlot.R:
                    var targets = new List<EntityState>(entities);
                    foreach (var e in targets)
                        if (e.Alive && e.Team != actor.Team && e.Team != Team.Neutral &&
                            V2.Distance(targetPoint, e.Position) <= 4f && V2.Distance(actor.Position, e.Position) <= 12f)
                        {
                            var raw = actor.HeroId == "GKU_KID" ?
                                100f + actor.PhysicalAttack*1.2f + 40f*(actor.SkillRanks[i]-1) :
                                actor.HeroId == "CELL_Z" ?
                                115f + actor.KiPower*1.1f + 40f*(actor.SkillRanks[i]-1) :
                                105f + actor.KiPower*1.3f + 40f*(actor.SkillRanks[i]-1);
                            var hpBefore=e.Hp;
                            if (DealDamage(actor,e,raw,actor.HeroId!="GKU_KID") &&
                                actor.HeroId=="CELL_Z")
                                actor.Hp=Math.Min(actor.MaxHp,actor.Hp+(hpBefore-e.Hp)*.06f);
                        }
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

        public bool DealDamage(EntityState actor, EntityState victim, float raw, bool kiDamage,
            bool basicAttack = false)
        {
            if (actor == null || victim == null || !actor.Alive || !victim.Alive || actor.Team == victim.Team) return false;
            if (victim.Kind == EntityKind.Core && ProtectedCore(victim)) return false;
            var resistance = kiDamage ? victim.KiResist : victim.Armor;
            var penetration = actor.Kind == EntityKind.Hero ?
                (kiDamage ? actor.KiPenPercent : actor.ArmorPenPercent) : 0f;
            var amount = Catalog.Mitigate(raw, resistance * (1f - penetration));
            if (victim.Shield > 0)
            {
                var absorbed = Math.Min(victim.Shield, amount);
                victim.Shield -= absorbed;
                amount -= absorbed;
            }
            var lostHp = Math.Min(victim.Hp, amount);
            victim.Hp = Math.Max(0, victim.Hp - amount);
            // Sustain is based on HP actually removed, never absorbed shields.
            if (actor.Kind == EntityKind.Hero && lostHp > 0f)
            {
                var vamp = basicAttack ? actor.LifeSteal : kiDamage ? actor.SpellVamp : 0f;
                if (vamp > 0f)
                    actor.Hp = Math.Min(actor.MaxHp, actor.Hp +
                        lostHp * vamp * (1f - actor.HealingReductionRate));
            }
            if (actor.Kind == EntityKind.Hero && victim.Kind == EntityKind.Hero &&
                lostHp > 0f && actor.AntiHealOnHit > 0f)
            {
                victim.HealingReductionRate = Math.Max(victim.HealingReductionRate,actor.AntiHealOnHit);
                victim.HealingReductionSeconds = 3f;
            }
            if (actor.Kind == EntityKind.Hero && victim.Kind == EntityKind.Hero && amount > 0f)
            {
                foreach (var defender in entities)
                {
                    if (defender.Kind != EntityKind.Tower || !defender.Alive || defender.Team != victim.Team ||
                        !InRange(defender, actor, defender.AttackRange)) continue;
                    defender.TowerAggroTargetId = actor.Id;
                    defender.TowerAggroSeconds = TowerAggroDuration;
                }
            }
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
                ++hero.UnspentSkillPoints;
                ApplyForm(hero);
                StatCalculator.Refresh(hero);
                events.Add(new GameEvent(EventKind.LevelUp, hero.Id, hero.Position, "Nivel " + hero.Level));
                if (hero.Id != PlayerId) AutoSpendSkillPoints(hero);
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
                // Stats are rebuilt once by StatCalculator.Refresh after stage changes.
                hero.FormId = stage.Id;
                hero.FormName = stage.DisplayName;
                // A large transformation cannot materialize inside a solid tower.
                if (IsStructureCollision(hero, hero.Position))
                {
                    hero.Position = ClampWalkable(hero, hero.Position);
                    hero.Destination = hero.Position;
                    hero.AttackTargetId = 0;
                }
                events.Add(new GameEvent(EventKind.Transform, hero.Id, hero.Position, hero.FormName));
            }
        }

        // Return how much Zeni a player must still pay when combining owned parts.
        // An ingredient can only be consumed once even if a recipe lists duplicates.
        public int RemainingItemCost(EntityState hero, string itemId)
        {
            var item = Catalog.Item(itemId);
            if (hero == null || item == null) return -1;
            var remaining = new List<string>(hero.Inventory);
            var credit = 0;
            foreach (var component in item.Components)
            {
                if (!remaining.Remove(component)) continue;
                var part = Catalog.Item(component);
                if (part != null) credit += part.Price;
            }
            return Math.Max(0, item.Price - credit);
        }

        private bool Purchase(EntityState hero, string itemId)
        {
            var item = Catalog.Item(itemId);
            if (item == null || !NearBase(hero)) return false;
            if (item.TrainingBoots && hero.BoughtTrainingBoots) return false;
            var available = new List<string>(hero.Inventory);
            var consumed = new List<string>();
            var credit = 0;
            foreach (var component in item.Components)
            {
                if (!available.Remove(component)) continue;
                var definition = Catalog.Item(component);
                if (definition == null || definition.TrainingBoots) continue;
                consumed.Add(component);
                credit += definition.Price;
            }
            var cost = Math.Max(0, item.Price - credit);
            var slotCountAfter = hero.Inventory.Count - consumed.Count + 1;
            if (hero.Zeni < cost || slotCountAfter > 6) return false;

            // Atomic mutation only after all checks. The completed item replaces its
            // ingredients so their modifiers never survive in derived statistics.
            hero.Zeni -= cost;
            foreach (var part in consumed) hero.Inventory.Remove(part);
            hero.Inventory.Add(itemId);
            if (item.TrainingBoots)
            {
                hero.BoughtTrainingBoots = true;
                hero.TrainingBootsRemaining = 300f;
            }
            StatCalculator.Refresh(hero);
            events.Add(new GameEvent(EventKind.Purchase, hero.Id, hero.Position,
                "Comprou " + item.Name + " (" + cost + " Zeni)"));
            return true;
        }

        private bool Sell(EntityState hero, string itemId)
        {
            if (!NearBase(hero) || string.IsNullOrEmpty(itemId) || !hero.Inventory.Contains(itemId)) return false;
            var item = Catalog.Item(itemId);
            if (item == null || item.TrainingBoots) return false;
            hero.Inventory.Remove(itemId);
            hero.Zeni += (int)((long)item.Price * 70 / 100);
            StatCalculator.Refresh(hero);
            events.Add(new GameEvent(EventKind.Purchase, hero.Id, hero.Position, "Vendeu " + item.Name));
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
