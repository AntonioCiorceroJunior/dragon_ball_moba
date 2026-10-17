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
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new Exception("FALHA: " + label);
        }
    }
}
#endif
