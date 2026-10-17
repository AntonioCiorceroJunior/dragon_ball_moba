#if UNITY_EDITOR
using System;
using DragonMoba.Domain;
using UnityEditor;
using UnityEngine;

namespace DragonMoba.Editor
{
    // Built-in smoke tests requiring no external test framework. Run from Editor menu.
    public static class DomainSelfTests
    {
        [MenuItem("Dragon MOBA/Executar testes de dominio")]
        public static void Run()
        {
            try
            {
                RunAll();
                Debug.Log("Dragon MOBA: PASSOU — verificacoes basicas de dominio.");
            }
            catch (Exception e)
            {
                Debug.LogError("Dragon MOBA: TESTE FALHOU\n" + e);
                throw;
            }
        }

        public static void RunAll()
        {
            var world = new GameWorld("GKU_SUPER");
            var hero = world.Player;
            Require(hero != null && hero.Level == 1 && hero.Alive, "Criacao do heroi");
            Require(world.Entities.Count >= 25, "Tres rotas com tropas, torres e bases");

            hero.Ki = 0;
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Charge, Pressed = true }), "Iniciar carregamento");
            for (var i = 0; i < 30; ++i) world.Tick(GameWorld.FixedStep);
            Require(hero.Ki > 10 && hero.Ki < hero.MaxKi, "Ki recupera sem superar maximo");

            hero.Position = new V2(8, -20);
            hero.Destination = hero.Position;
            hero.Ki = hero.MaxKi;
            var beforeXp = hero.Experience;
            for (var i = 0; i < 90; ++i) world.Tick(GameWorld.FixedStep);
            Require(hero.Experience > beforeXp, "Ki cheio gera XP fora da fonte");

            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Move, Position = new V2(8, -20) }), "Mover");
            Require(!hero.Charging, "Movimento cancela carregamento");

            world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Charge, Pressed = true });
            EntityState enemy = null;
            foreach (var e in world.Entities)
                if (e.Kind == EntityKind.Hero && e.Team == Team.Red) enemy = e;
            Require(enemy != null, "Bot inimigo disponivel");
            world.DealDamage(enemy, hero, 12, false);
            Require(!hero.Charging, "Dano interrompe carregamento");

            hero.Position = new V2(0, -35); // arrange test state directly, not gameplay UI
            hero.Destination = hero.Position;
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Buy, ItemId = "KI_CELL" }), "Compra na fonte");
            Require(hero.MaxKi > 135 && hero.Zeni == 900, "Compra debita ouro e aplica atributos");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Buy, ItemId = "TRAINING_BOOTS" }), "Recusa compra sem ouro");

            world.GrantExperience(hero, 9999);
            Require(hero.Level == 18 && hero.FormId == "super_ui", "Transicoes por nivel e teto 18");
            Require(Catalog.Mitigate(100, 100) == 50, "Mitigacao com resistencia 100");
            TestSkillProgression();
            TestPersistentAttack();
        }


        private static void TestSkillProgression()
        {
            var world = new GameWorld("GKU_SUPER");
            var hero = world.Player;
            Require(hero.UnspentSkillPoints == 1 && Array.TrueForAll(hero.SkillRanks, r => r == 0),
                "SK-01: Level 1 begins with one unspent point and all skills locked");

            var lockedKi = hero.Ki;
            Require(!world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Cast,
                Slot = AbilitySlot.Q, Position = hero.Position }), "SK-01: Locked Q rejected");
            Require(hero.Ki == lockedKi && hero.SkillCooldown[0] == 0 && hero.AttackTargetId == 0,
                "SK-01: Locked cast has no side effects");

            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.UpgradeAbility,
                Slot = AbilitySlot.Q }), "SK-02: Q rank 1 upgrade");
            Require(hero.SkillRanks[0] == 1 && hero.UnspentSkillPoints == 0, "SK-02: Point consumed exactly once");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.UpgradeAbility,
                Slot = AbilitySlot.Q }), "SK-02: No second point at level 1");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.UpgradeAbility,
                Slot = AbilitySlot.R }), "SK-03: R cannot be learned at level 1");

            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Cast,
                Slot = AbilitySlot.Q, Position = hero.Position }), "SK-02: Learned Q can be cast");
            Require(hero.Ki < lockedKi && hero.SkillCooldown[0] > 0, "SK-02: Learned Q costs Ki and has cooldown");

            world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level));
            Require(hero.Level == 2 && hero.UnspentSkillPoints == 1, "SK-04: One point per level");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.UpgradeAbility,
                Slot = AbilitySlot.Q }), "SK-03: Q rank 2 requires level 3");
            world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level));
            Require(hero.Level == 3 && hero.UnspentSkillPoints == 2, "SK-04: Points accumulated");

            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.UpgradeAbility,
                Slot = AbilitySlot.E }), "SK-04: Learn E");
            var fullKi = hero.MaxKi;
            hero.Ki = fullKi;
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Cast,
                Slot = AbilitySlot.E, Position = hero.Position }), "SK-04: E rank 1");
            var shieldOne = hero.Shield;
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.UpgradeAbility,
                Slot = AbilitySlot.E }), "SK-04: E rank 2");
            hero.Ki = hero.MaxKi;
            hero.Shield = 0f;
            hero.SkillCooldown[2] = 0f;
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Cast,
                Slot = AbilitySlot.E, Position = hero.Position }), "SK-04: E rank 2 cast");
            Require(hero.Shield > shieldOne + 20f, "SK-04: Higher rank strengthens the ability");

            while (hero.Level < 6) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience);
            Require(hero.Level == 6 && world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: First R at level 6");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Second R requires level 11");
            while (hero.Level < 11) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience);
            Require(world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Second R at level 11");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Third R requires level 16");
            while (hero.Level < 16) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience);
            Require(world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Third R at level 16");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: R capped at 3");

            while (hero.Level < 18) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience);
            var earned = hero.UnspentSkillPoints;
            world.GrantExperience(hero, 999999);
            Require(hero.Level == 18 && hero.UnspentSkillPoints == earned, "SK-04: No points after 18");
            var ranks = hero.SkillRanks[0] + hero.SkillRanks[1] + hero.SkillRanks[2] + hero.SkillRanks[3];
            Require(ranks + hero.UnspentSkillPoints == 18, "SK-04: Spent plus unspent equals level");

            var opponent = world.Find(world.Entities.Count > 0 ? hero.Id + 1 : 0);
            Require(opponent != null && opponent.Team != hero.Team && opponent.SkillRanks[0] >= 1,
                "SK-05: Bot starts with legal Q rank");
            var fresh = new GameWorld("GKU_SUPER").Player;
            Require(fresh.Level == 1 && fresh.UnspentSkillPoints == 1 && fresh.SkillRanks[0] == 0,
                "SK-04: New match resets skill points");
        }

        private static void TestPersistentAttack()
        {
            var world = new GameWorld("GKU_KID");
            var hero = world.Player;
            EntityState target = null;
            foreach (var e in world.Entities)
                if (e.Kind == EntityKind.Hero && e.Team == Team.Red) { target = e; break; }
            Require(target != null, "AA-01: Enemy hero exists");
            hero.Position = new V2(30, -36);
            hero.Destination = hero.Position;
            hero.MaxHp = hero.Hp = 5000;
            target.Position = new V2(30, -26);
            target.Destination = target.Position;
            target.MoveSpeed = 0;
            target.AttackRange = 0;
            target.PhysicalAttack = 0;
            target.MaxHp = target.Hp = 5000;
            Require(!world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Attack,
                TargetId = hero.Id }), "AA-03: Cannot attack ally");
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Attack,
                TargetId = target.Id }), "AA-01: One attack order accepted");

            var attacks = 0;
            var first = -1;
            var second = -1;
            for (var i = 0; i < 300; ++i)
            {
                world.Tick(GameWorld.FixedStep);
                foreach (var e in world.DrainEvents())
                {
                    if (e.Kind != EventKind.Attack || e.ActorId != hero.Id || e.TargetId != target.Id) continue;
                    ++attacks;
                    if (first < 0) first = world.TickIndex;
                    else if (second < 0) second = world.TickIndex;
                }
                if (attacks >= 2) break;
            }
            Require(attacks >= 2 && target.Hp < target.MaxHp, "AA-01: One order produces repeated damage");
            Require(V2.Distance(hero.Position, target.Position) <= hero.AttackRange + .05f,
                "AA-01: Hero chases target until attack range");
            Require((second - first) * GameWorld.FixedStep >= hero.AttackInterval - GameWorld.FixedStep * 1.1f,
                "AA-01: Repeat attacks obey cooldown");

            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Move,
                Position = new V2(30, -40) }), "AA-02: Move cancels attack");
            Require(hero.AttackTargetId == 0, "AA-02: Target cleared on movement");
            for (var i = 0; i < 60; ++i)
            {
                world.Tick(GameWorld.FixedStep);
                foreach (var e in world.DrainEvents())
                    Require(!(e.Kind == EventKind.Attack && e.ActorId == hero.Id && e.TargetId == target.Id),
                        "AA-02: No attack after cancellation");
            }
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Attack,
                TargetId = target.Id }), "AA-03: Target can be reacquired");
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Charge,
                Pressed = true }) && hero.AttackTargetId == 0, "AA-03: Charging cancels target");
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new Exception("FALHA: " + label);
        }
    }
}
#endif
