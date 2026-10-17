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
            var world = new GameWorld("GKU_SUPER",ArenaLayout.Training);
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
            TestTowerRangeAndAggro();
            TestStructureCollision();
            TestDerivedStatsAndEquipment();
            TestAttackMoveAndStop();
            TestClassicLayout();
            Debug.Log("DBM-BATCH DOMAIN PASSED | regen, recipes, selling, lifesteal, haste, forms, attack-move");
            Debug.Log("DBM001 DOMAIN PASSED | locked skills, point conservation, rank gates, repeated attacks, cast resume and respawn persistence");
            Debug.Log("DBM002 DOMAIN PASSED | tower targeting, aggro, range, collision and detour");
        }


        private static void TestClassicLayout()
        {
            var world=new GameWorld("GKU_KID");var map=world.Layout;var hero=world.Player;
            Require(map==ArenaLayout.Classic&&map.HalfWidth==map.HalfHeight&&map.HalfWidth==72,"MAP-01: Square diagonal arena is the actual default");
            var towers=0;
            foreach(var e in world.Entities)if(e.Kind==EntityKind.Tower)towers++;
            Require(towers==18&&map.Core(Team.Blue).X<0&&map.Core(Team.Red).X>0,"MAP-02: Opposite corner bases and three tower tiers per lane");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Buy,ItemId="KI_CELL"}),"MAP-03: New spawn can use its own fountain shop");
            Require(map.At(0,.5f).X<0&&map.At(0,.5f).Y>0&&map.At(2,.5f).X>0&&map.At(2,.5f).Y<0,"MAP-04: Side lanes take separate corners");
            EntityState minion=null;foreach(var e in world.Entities)if(e.Kind==EntityKind.Minion&&e.Team==Team.Blue&&e.Lane==0){minion=e;break;}
            Require(minion!=null,"MAP-05: Top lane minion exists");
            var progress=map.Progress(0,minion.Position);
            for(var i=0;i<600;i++)world.Tick(GameWorld.FixedStep);
            Require(map.Progress(0,minion.Position)>progress+.15f&&map.RoadDistance(minion.Position)<0,"MAP-05: Waves advance along their curved lane");
            hero.Position=hero.Destination=new V2(5,-30);
            world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Move,Position=new V2(13.2f,-17.8f)});
            for(var i=0;i<30;i++)world.Tick(GameWorld.FixedStep);
            var dx=hero.Position.X-5;var dy=hero.Position.Y+30;
            Require(dx>1&&Math.Abs(dx/dy-8.2f/12.2f)<.002f,"MOVE-360: Arbitrary-angle movement is not quantized to eight directions");
            world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Move,Position=new V2(1000,-1000)});
            Require(hero.Destination.X==72&&hero.Destination.Y==-72,"MAP-06: Commands use the enlarged map bounds");
            Debug.Log("CLASSIC MAP PASSED | diagonal bases, 18 towers, lane routes, shop, bounds, continuous movement");
        }
        private static void TestSkillProgression()
        {
            var world = new GameWorld("GKU_SUPER",ArenaLayout.Training);
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

            while (hero.Level < 6) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience + 0.01f);
            Require(hero.Level == 6 && world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: First R at level 6");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Second R requires level 11");
            while (hero.Level < 11) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience + 0.01f);
            Require(world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Second R at level 11");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Third R requires level 16");
            while (hero.Level < 16) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience + 0.01f);
            Require(world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: Third R at level 16");
            Require(!world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.R }), "SK-03: R capped at 3");

            while (hero.Level < 18) world.GrantExperience(hero, Catalog.ExperienceForNextLevel(hero.Level) - hero.Experience + 0.01f);
            var earned = hero.UnspentSkillPoints;
            world.GrantExperience(hero, 999999);
            Require(hero.Level == 18 && hero.UnspentSkillPoints == earned, "SK-04: No points after 18");
            var ranks = hero.SkillRanks[0] + hero.SkillRanks[1] + hero.SkillRanks[2] + hero.SkillRanks[3];
            Require(ranks + hero.UnspentSkillPoints == 18, "SK-04: Spent plus unspent equals level");

            EntityState opponent = null;
            foreach (var candidate in world.Entities)
                if (candidate.Kind == EntityKind.Hero && candidate.Team != hero.Team) { opponent = candidate; break; }
            Require(opponent != null && opponent.SkillRanks[0] >= 1,
                "SK-05: Bot starts with legal Q rank");
            var fresh = new GameWorld("GKU_SUPER",ArenaLayout.Training).Player;
            Require(fresh.Level == 1 && fresh.UnspentSkillPoints == 1 && fresh.SkillRanks[0] == 0,
                "SK-04: New match resets skill points");
            var savedRanks = (int[])hero.SkillRanks.Clone();
            var savedPoints = hero.UnspentSkillPoints;
            world.DealDamage(opponent, hero, 100000, false);
            for (var i = 0; i < 1200 && !hero.Alive; ++i) world.Tick(GameWorld.FixedStep);
            Require(hero.Alive && hero.UnspentSkillPoints == savedPoints &&
                Array.TrueForAll(new[] {0,1,2,3}, i => hero.SkillRanks[i] == savedRanks[i]),
                "SK-06: Real respawn preserves ranks and points");
        }

        private static void TestPersistentAttack()
        {
            var world = new GameWorld("GKU_KID",ArenaLayout.Training);
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

            Require(world.Submit(new GameCommand { ActorId=hero.Id, Kind=CommandKind.UpgradeAbility, Slot=AbilitySlot.Q }),
                "AA-04: Learn Q before testing cast resumption");
            Require(world.Submit(new GameCommand { ActorId=hero.Id, Kind=CommandKind.Cast, Slot=AbilitySlot.Q,
                Position=target.Position }) && hero.AttackTargetId == target.Id, "AA-04: Cast keeps attack target");
            bool resumed = false;
            for (var i=0; i<90; ++i)
            {
                world.Tick(GameWorld.FixedStep);
                foreach (var e in world.DrainEvents())
                    if (e.Kind==EventKind.Attack && e.ActorId==hero.Id && e.TargetId==target.Id) resumed=true;
            }
            Require(resumed, "AA-04: Basic attacks resume without another order after cast");

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


        private static void TestTowerRangeAndAggro()
        {
            var world = new GameWorld("GKU_SUPER",ArenaLayout.Training);
            var hero = world.Player;
            EntityState tower = null, enemy = null, alliedMinion = null;
            foreach (var e in world.Entities)
            {
                if (e.Kind == EntityKind.Tower && e.Team == Team.Red && e.Lane == 1) tower = e;
                if (e.Kind == EntityKind.Hero && e.Team == Team.Red) enemy = e;
                if (e.Kind == EntityKind.Minion && e.Team == Team.Blue && e.Lane == 1) alliedMinion = e;
            }
            Require(tower != null && enemy != null && alliedMinion != null, "DBM-002: arena has combat fixtures");
            enemy.MoveSpeed = enemy.Ki = enemy.KiRegen = enemy.PhysicalAttack = 0;
            enemy.AttackRange = 0;

            hero.Position = hero.Destination = new V2(8, 19);
            var beforeHp = hero.Hp;
            world.Tick(GameWorld.FixedStep);
            Require(tower.AttackTargetId == hero.Id && hero.Hp < beforeHp,
                "TR-01: Tower hits opposing hero inside documented range");
            hero.Position = hero.Destination = new V2(8, 1);
            world.Tick(GameWorld.FixedStep);
            Require(tower.AttackTargetId == 0, "TR-02: Tower releases targets outside range");

            hero.Position = hero.Destination = new V2(4, 19);
            alliedMinion.Position = alliedMinion.Destination = new V2(0, 12);
            world.Tick(GameWorld.FixedStep);
            Require(tower.AttackTargetId == alliedMinion.Id,
                "TR-03: Tower prioritizes enemy minion even if hero is closer");

            Require(world.DealDamage(hero, enemy, 25, false),
                "TR-04: Hero damages enemy hero defended by tower");
            world.Tick(GameWorld.FixedStep);
            Require(tower.AttackTargetId == hero.Id && tower.TowerAggroSeconds > 0f,
                "TR-04: Tower prioritizes hero aggressor over minions");

            hero.Position = hero.Destination = new V2(0, -25);
            world.Tick(GameWorld.FixedStep);
            Require(tower.AttackTargetId == alliedMinion.Id && tower.TowerAggroTargetId == 0,
                "TR-05: Tower disengages aggressor who leaves range");
            EntityState secondMinion = null;
            foreach (var candidate in world.Entities)
                if (candidate.Kind == EntityKind.Minion && candidate.Team == Team.Blue &&
                    candidate.Id != alliedMinion.Id && candidate.Lane == 1)
                { secondMinion = candidate; break; }
            Require(secondMinion != null, "TR-06: Another minion exists");
            secondMinion.Position = secondMinion.Destination = new V2(0f,16f);
            world.Tick(GameWorld.FixedStep);
            Require(tower.AttackTargetId == alliedMinion.Id,
                "TR-06: Tower retains living minion despite a nearer minion arriving");
        }

        private static void TestStructureCollision()
        {
            var world = new GameWorld("GKU_KID",ArenaLayout.Training);
            var hero = world.Player;
            EntityState blueTower = null, redTower = null;
            foreach (var e in world.Entities)
            {
                if (e.Kind == EntityKind.Tower && e.Lane == 1)
                {
                    if (e.Team == Team.Blue) blueTower = e;
                    if (e.Team == Team.Red) redTower = e;
                }
            }
            Require(blueTower != null && redTower != null, "COL-01: Towers located");
            hero.Position = hero.Destination = new V2(0, -24);
            Require(!world.IsStructureCollision(hero, hero.Position), "COL-01: Starting point walkable");
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Move,
                Position = new V2(0, -13) }), "COL-01: Ground movement accepted");
            for (var i = 0; i < 150; i++)
            {
                world.Tick(GameWorld.FixedStep);
                Require(!world.IsStructureCollision(hero, hero.Position),
                    "COL-01: Swept character never enters tower footprint");
            }
            Require(hero.Position.Y > -18f && V2.Distance(hero.Position, new V2(0, -13)) < 1.5f,
                "COL-02: Character navigates around tower instead of getting permanently stuck");

            hero.Position = hero.Destination = new V2(0, -24);
            Require(world.Submit(new GameCommand { ActorId = hero.Id,
                Kind = CommandKind.UpgradeAbility, Slot = AbilitySlot.W }), "COL-03: W unlocked");
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Cast,
                Slot = AbilitySlot.W, Position = new V2(0, -13) }), "COL-03: W dash cast");
            Require(!world.IsStructureCollision(hero, hero.Position),
                "COL-03: Dash does not end inside a living tower");

            hero.Position = hero.Destination = new V2(0, -25);
            Require(world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Move,
                Position = blueTower.Position }), "COL-04: Click at structure center accepted");
            Require(!world.IsStructureCollision(hero, hero.Destination),
                "COL-04: Destination projected outside solid structure");
            redTower.Hp = 0f; // Controlled fixture only: destroy the blocker for walkability check.
            Require(!world.IsStructureCollision(hero, redTower.Position),
                "COL-05: Destroyed tower footprint is no longer solid");
        }

        private static void TestDerivedStatsAndEquipment()
        {
            var world = new GameWorld("GKU_KID",ArenaLayout.Training);
            var hero = world.Player;
            Require(hero.HpRegenPer5 >= 6f && Catalog.Item("CAPSULE_SWORD") != null,
                "STAT-01: Regeneration and expanded item catalog");
            hero.Position = hero.Destination = new V2(30,-36);
            hero.Hp = 250f;
            for (var t=0; t<30; ++t) world.Tick(GameWorld.FixedStep);
            Require(hero.Hp > 251f && hero.Hp < 252f,
                "STAT-02: 6 HP per five seconds regenerates outside fountain");

            hero.Position = hero.Destination = new V2(0,-34);
            hero.Zeni = 20000;
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Buy,
                ItemId="GLOVES"}), "ITEM-01: first glove");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Buy,
                ItemId="GLOVES"}), "ITEM-01: second glove");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Buy,
                ItemId="HEALTH_GEM"}), "ITEM-01: crystal");
            Require(hero.Inventory.Count == 3 && Math.Abs(hero.PhysicalAttack-85f)<.01f,
                "STAT-03: Both components add their attributes once");
            Require(world.RemainingItemCost(hero,"DRAGON_GLOVES")==1700,
                "ITEM-02: Missing combine cost is 1700");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Buy,
                ItemId="DRAGON_GLOVES"}), "ITEM-03: full recipe purchase");
            Require(hero.Inventory.Count==1 && hero.Inventory[0]=="DRAGON_GLOVES" &&
                hero.Zeni==17200 && Math.Abs(hero.PhysicalAttack-100f)<.01f &&
                hero.MaxHp==770f,
                "ITEM-03: Recipe consumes ingredients and no ghost bonuses remain");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Sell,
                ItemId="DRAGON_GLOVES"}), "ITEM-04: Sell finished item");
            Require(hero.Inventory.Count==0 && hero.PhysicalAttack==55f &&
                hero.MaxHp==570f && hero.Zeni==19160,
                "ITEM-04: Selling removes modifiers and credits 70 percent");

            hero.Zeni=20000;
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Buy,
                ItemId="VAMP_ESSENCE"}), "DMG-01: Equip lifesteal");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Buy,
                ItemId="HASTE_SHARD"}), "SK-07: Equip ability haste");
            EntityState enemy = null;
            foreach (var e in world.Entities)
                if (e.Kind==EntityKind.Hero && e.Team==Team.Red) { enemy=e; break; }
            Require(enemy!=null, "DMG-01: Enemy fixture exists");
            hero.Hp=100f;
            Require(world.DealDamage(hero,enemy,100,false,true) && hero.Hp>104f,
                "DMG-01: Basic attack lifesteal heals for actual HP damage");
            enemy.Shield=1000;
            hero.Hp=100f;
            Require(world.DealDamage(hero,enemy,100,false,true) && hero.Hp==100f,
                "DMG-02: Fully shielded damage never grants lifesteal");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.UpgradeAbility,
                Slot=AbilitySlot.Q}), "SK-07: Learn Q");
            hero.Ki=hero.MaxKi;
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Cast,
                Slot=AbilitySlot.Q,Position=hero.Position}) &&
                hero.SkillCooldown[0]>3.6f && hero.SkillCooldown[0]<3.7f,
                "SK-07: +10 ability haste reduces base 4s cooldown correctly");

            var antiWorld = new GameWorld("GKU_KID",ArenaLayout.Training);
            var buyer = antiWorld.Player;
            buyer.Zeni = 4000;
            Require(antiWorld.Submit(new GameCommand {ActorId=buyer.Id, Kind=CommandKind.Buy,
                ItemId="ANTI_HEAL_BLADE"}) && buyer.AntiHealOnHit > .34f,
                "COUNTER-01: Anti-heal item grants damage debuff");
            EntityState affected=null;
            foreach (var e in antiWorld.Entities)
                if(e.Kind==EntityKind.Hero && e.Team==Team.Red) { affected=e;break; }
            Require(affected!=null,"COUNTER-01: Enemy exists");
            affected.Position=affected.Destination=new V2(30,-36);
            affected.MoveSpeed=affected.PhysicalAttack=affected.AttackRange=0;
            affected.Hp=250f;
            Require(antiWorld.DealDamage(buyer,affected,40f,false,true) &&
                affected.HealingReductionRate>.34f && affected.HealingReductionSeconds==3f,
                "COUNTER-01: Damaging enemy hero reduces healing for three seconds");
            for (var j=0;j<115;++j) antiWorld.Tick(GameWorld.FixedStep);
            Require(affected.HealingReductionSeconds==0f && affected.HealingReductionRate==0f,
                "COUNTER-02: Anti-heal debuff expires without ghost modifiers");

            var apeWorld = new GameWorld("GKU_KID",ArenaLayout.Training);
            var kid = apeWorld.Player;
            kid.Position = kid.Destination = new V2(0,-17.2f); // fits Kid, not giant form.
            apeWorld.GrantExperience(kid,99999);
            Require(kid.Level==18 && kid.FormId=="kid_oozaru" &&
                apeWorld.BodyRadius(kid)>.9f && kid.MoveSpeed<6f &&
                !apeWorld.IsStructureCollision(kid,kid.Position),
                "FORM-01: Oozaru grows, slows and transforms outside tower collisions");
        }

        private static void TestAttackMoveAndStop()
        {
            var world = new GameWorld("GKU_KID",ArenaLayout.Training);
            var hero=world.Player;
            EntityState enemy=null;
            foreach (var e in world.Entities)
                if (e.Kind==EntityKind.Hero && e.Team==Team.Red) { enemy=e;break; }
            Require(enemy!=null,"MOVE-01: Enemy hero");
            hero.Position=hero.Destination=new V2(30,-36);
            enemy.Position=enemy.Destination=new V2(30,-26);
            enemy.MoveSpeed=enemy.PhysicalAttack=enemy.AttackRange=0f;
            enemy.MaxHp=enemy.Hp=5000f;
            hero.MaxHp=hero.Hp=5000f;
            Require(world.Submit(new GameCommand{ActorId=hero.Id,
                Kind=CommandKind.AttackMove,Position=new V2(30,-23)}),
                "MOVE-01: AttackMove command accepted");
            var hits=0;
            for(var i=0;i<260;i++)
            {
                world.Tick(GameWorld.FixedStep);
                foreach(var e in world.DrainEvents())
                    if(e.Kind==EventKind.Attack && e.ActorId==hero.Id &&
                        e.TargetId==enemy.Id) hits++;
                if(hits>=2)break;
            }
            Require(hits>=2 && hero.AttackMoveActive,
                "MOVE-02: AttackMove acquires and repeats attacks without clicking enemy");
            Require(world.Submit(new GameCommand{ActorId=hero.Id,Kind=CommandKind.Stop}) &&
                !hero.AttackMoveActive && hero.AttackTargetId==0 &&
                V2.Distance(hero.Destination,hero.Position)<.001f,
                "MOVE-03: Stop cancels attack-move and movement");
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new Exception("FALHA: " + label);
        }
    }
}
#endif
