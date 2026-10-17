# Pesquisa MOBA v2 — Arquitetura executável, ordem de implantação e testes de aceitação

**Data:** 16/10/2026 · **Estado:** decisão recomendada, ainda sem alterar código.  
**Fonte da verdade atual:** branch `main` com DBM-001 validada e mesclada. **PR #2 DBM-002** em draft, **sem validação Unity**. Não modificar código desse PR durante pesquisa.  
**Relacionados:** [auditoria das fontes](01_FONTES_AUDITADAS_E_EVOLUCAO_MOBA.md), [especificação de sistemas](02_SISTEMAS_MOBA_ESPECIFICACAO_IMPLEMENTAVEL.md), `docs/specifications/GAME_DESIGN.md`.

## 1. Arquitetura escolhida — vertical slices sem "big bang"

**Escolha:** preservar Unity 6000.6.6f1/Built-in e o núcleo autoritativo offline C#, modificando gradualmente as fronteiras de responsabilidade. **Não migrar para URP, DOTS, Netcode ou engine nova antes de benchmark.** O esforço principal é estabilizar uma **partida offline completa** de 3 heróis com bots; network 5v5 só depois.

```
                      GameSessionFlow
 Boot → Select → LobbySolo → Spawn → Playing → PostGame
                                |
 Mouse/keyboard/UI → InputCommandMapper
                                ↓
                      GameCommandQueue
                                ↓
        ┌───────────── GameSimulation (pure C#) ─────────────┐
        │ TickClock, Commands, Status, Movement, Collision    │
        │ Stats, Abilities, Damage, Healing, Economy          │
        │ KiTraining, XP, Forms, Minions, Towers, Jungle      │
        │ Fog/Vision, Objectives, WinCondition, EventLedger   │
        └───────────────────┬─────────────────────────────────┘
                    immutable WorldSnapshot
                         + DomainEvents
                                ↓
      EntityViewFactory / Animator / VFX / Audio / HUD
                                |
                  Unity-only camera/UI/nav bake
```

**Domínio:** nunca `UnityEngine`, `Time.time` ou dependência de `Animator`; o servidor futuro deve poder executar a simulação sem render. **Views:** observam snapshots/eventos e nunca aplicam dano diretamente.

### 1.1 Estado atual conferido na main

- `Definitions.cs`: `HeroDefinition`, `ItemDefinition`, `GameCommand`, `GameEvent` e `Catalog`; quatro itens reais (`KI_CELL`, `ARMOR`, `GLOVES`, `TRAINING_BOOTS`). Catálogo de 15 heróis está **apenas no documento mestre**; implementados no runtime três IDs.
- `EntityState.cs`: HP/Ki/Armor/KiResist/PhysicalAttack/KiPower/MoveSpeed/AttackInterval; **não** `HPRegenPer5`, `LifeSteal`, `AbilityHaste`, `ItemInstance`, `HitboxProfile` ou `GoldLedger`.
- `GameWorld.cs`: simulação 30Hz; `GrantExperience` soma +24 MaxHP, +2,8 ataque, +3,1 KiPower por nível; não há aumento de regen base; XP no C: 1,8→0,99/s. Spawns fixos e mapa plano; dano/cooldown e itens são diretos.
- `PrototypeGame.cs`: tela inicial com 3 botões, mas start automático quando `OrganicArena.startDemo=true`, HUD em IMGUI, disparo de comando, views e input no mesmo componente.
- `ArenaCameraController.cs`: ortográfica, elevação 43°, zoom 8–18, pan por setas e botão central, `Space` recentraliza. Ainda precisa de avaliação real da UX/edge-scroll.
- `OrganicArena.cs`: estrada visual em 3 colunas (`-17/0/17`), vegetação procedural e rio; `startDemo=true` por padrão; caminho visual não configura malha de colisão autoritativa.
- **DBM-002 PR #2** propõe IA de torre, hitbox circular e aviso de alcance, mas não substitui grafo/A*; não tratá-lo como aprovado.

**Nota de cuidado:** abrir a `main` local na Unity após merge de DBM-001 continua exigindo sincronização e eventual build; os binários Windows ficam locais e não são fonte da verdade remota.

## 2. Organização incremental sugerida (pastas, namespaces, projetos)

Não mover código antigo em massa. Cada novo sistema pode começar em `Assets/Scripts/Domain/<submodulo>` para preservar `.meta` e testes, migrando `GameWorld` por adapter:

```
Assets/Scripts/
  Domain/
    Core/            TickId, UnitId, GameCommand, DomainEvent, WorldSnapshot
    Catalog/         ChampionData, FormData, AbilityData, ItemData, MapData, BalanceProfile
    Stats/           StatId, GrowthCurve, ModifierStack, DerivedStatCalculator
    Combat/          DamageRequest, AttackTimeline, DamageResolver, HealResolver, Shields
    Status/          EffectInstance, EffectRules, CCImmunity, Dispels, Timers
    Abilities/       CastRequest, CastValidator, CooldownTable, ProjectileState
    Economy/         ItemCatalog, RecipeGraph, ItemInstance, Inventory, GoldLedger
    Progression/     ExperienceSystem, SkillPointSystem, FormTransition, KiTraining
    Movement/        WalkabilityGrid, PathQuery, MoveIntent, MovementIntegrator, Collision
    World/           LaneGraph, TowerTargeting, MinionWaveSystem, JungleSystem, MatchPhases
    Vision/          SightSources, VisibilityGrid, BrushArea, WardEntities
    AI/              HeroBotPlanner, LaneMinionBrain, JungleBrain, BehaviorConfig
    Telemetry/       EventRecorder, ReplayInputLog, Metrics, StateChecks
  Runtime/
    Session/         GameSessionFlow, ChampionSelectView, MatchBootstrap
    Presentation/    ChampionView, NameplateView, AbilityVFX, ItemIconProvider
    Input/           GameInput, Keybinds, CameraInput, RaycastQuery
    UI/              HUD, Shop, Minimap, Scoreboard, PostGame
    Audio/           CueRegistry, AudioMixerController, KiChargeVoice
    Navigation/      MapBakeImporter, DebugGizmos, AuthoringAdapters
  Editor/            Test runners, content validators, fixture builders
```

**Dados de autoria:** Unity `ScriptableObjects` para leitura no editor; gerar runtime DTOs C# imutáveis/JSON versionado antes de entregar ao domínio. Dependência `Domain → Unity` é proibida.

## 3. Contratos e invariantes por fronteira

### 3.1 `DerivedStats`

Entradas: `ChampionGrowth`, nível, forma, multiset de `ItemInstance`, buffs e runas; resultado de leitura sem mutação:

```
StatSnapshot {
 MaxHP, HPRegenPer5, MaxKi, KiRegenPer5,
 PhysicalAttack, KiPower, Armor, KiResist,
 AttackSpeed, AbilityHaste, LifeSteal, SpellVamp,
 ArmorPenFlat, ArmorPenPct, KiPenFlat, KiPenPct,
 CritChance, CritMultiplier, MoveSpeed, BodyRadius, VisionRange
}
```

A transação de compra pode alterar **MaxHP**, mas deve definir como `CurrentHP` se comporta (manter quantidade, preservar percentual ou conceder diferença; escolher e testar). Quanto vender HP? Não matar instantaneamente, exceto efeito explicitamente letal. Efeitos temporários acumulam conforme prioridade e grupo único; nunca duplicar após save/respawn.

### 3.2 `DamageRequest → DamageResult`

```
DamageRequest {
 EventId, Tick, SourceId, TargetId, AbilityId?,
 RawPhysical, RawKi, RawTrue, Flags[CanCrit,OnHit,LifeStealEligible,Area,Dot],
 Context[Position,FormId,Rank,Direction]
}
DamageResult {
 Accepted, RejectionReason?, ActualHpLoss, ShieldAbsorbed,
 MitigatedPhysical, MitigatedKi, TrueDamage, WasCrit,
 LifeStealHeal, SpellVampHeal, TargetKilled
}
```

Dois `EventId` iguais nunca aplicam dano duas vezes em rede. Um ataque lançado num alvo morto/invisível/fora do alcance deve recusar antes de aplicar efeito conforme regra. Em caso de ataque com projétil em voo, distinguir validação no lançamento e elegibilidade no impacto. Não fazer exceção ad hoc para bots aplicarem habilidade proibida.

### 3.3 `ItemCatalog / RecipeResolver / Economy`

`ItemDefinition` tem `id`, `localizedName`, `iconAssetId`, `totalCost`, `components[]`, `combineCost`, `stats`, `uniqueEffectGroup`, `activeEffect`, `category`, `maxCopies`, `requirements`, `dataVersion`. Grafo DAG validado. Compras executadas como **transação atômica**, com `PurchaseResult` explicável. Venda/undo recompõem `DerivedStats` em vez de aplicar buffs incrementais assimétricos.

**Observação:** `Inventory.Count>=6` do código atual limita espaço, mas não implementa slots, ícones, receitas, equipamento ativo, reconstrução de stats após venda ou tipos de consumíveis. Não aceitar `List<string>` como inventário final.

### 3.4 `MapGraph / PathService`

`MapDefinition` tem `Version`, `Bounds`, `SpawnPoints`, `Lanes[3]`, `TowerAnchors`, `WalkablePolygons`, `Walls`, `Brush`, `River`, `CampSites`, `Objectives`. `PathService.FindPath(start,end,radius,flags)` retorna waypoints e status `Complete/Partial/Blocked`. Torre destruída altera colisão somente se gameplay decidir assim. Pathing tem **agente de raio diferente** para Goku pequeno e Oozaru; não basta scale visual.

Simetria: medir rotas por tempo, não somente distância euclidiana. `MinionBrain` percorre lane waypoints e escolhe targets baseados em visão e aggro; bots nunca leem posições inimigas invisíveis.

### 3.5 `GameSessionFlow / CharacterData`

`CharacterSelectRequest` escolhe `HeroId`, role e loadout; valida IDs/manifesto/licença/modelo, cria um `MatchConfig` congelado, e só então instancia `GameWorld`. Formas listadas, mas não selecionar instantaneamente UI como substituto da progressão. Fallback de asset faltante pode existir no Editor, mas **falhar build de lançamento** se um campeão elegível não tiver mesh/rig/ícones definidos.

## 4. Ordem de desenvolvimento em tickets (desdobramento dos documentos existentes)

**Regra:** DBM-002 mantém prioridade até ser testada. Não iniciar refatoração de dano enquanto PR #2 ainda não foi aprovado; proteger histórico de `DOMAIN_BASELINE.csv`.

| Seq. | ID sugerido | Escopo **mínimo** | Aceite verificável | Dependência |
|---:|---|---|---|---|
| 0 | DBM-002 VALIDATE | Unidades colidem com torres/núcleos, torres atacam no range, aggro | Unity Prepare/Play/Build, clicando no chão e testando QWER não atravessa torres; 0 regressão DBM-001 | PR #2 |
| 1 | DBM-003A | StatsDerived, growth, HPregen/5s, separação base/form | Regen HP no jogo e testes 1/6/11/18; sem bônus fantasma | 0 |
| 2 | DBM-003B | Pipeline de dano físico/Ki/true, escudo, life steal e healing | Casos com valores conhecidos, crítico deterministic RNG, anti-cura e resistências | 1 |
| 3 | DBM-003C | `ItemCatalog`, receitas, 6 slots, passivas únicas, ZeniLedger | Compra direta e por ingredientes; undo/venda; número de Zeni perfeito | 2 |
| 4 | DBM-003D | Ícones originais, HUD de stats/inventário, UI loja/recomendados e tooltips | Jogador constrói espada/escudo real clicando na loja e entende contravalor | 3 |
| 5 | DBM-004A | `KiTrainingSystem` por estado, XP/s por fase, fadiga/anti-AFK | Sem treino na fonte; UX + XP real e percentual farm | 1–4 |
| 6 | DBM-004B | Aura original, carga/treino visual, vocalizações e som | áudio/partículas respondem imediatamente a state events, cancelam sob dano | 5 |
| 7 | DBM-005A | `GameSessionFlow`, seleção 3 heróis, role, confirm/cancel, prévia kit | start automático removido na partida normal; 3 opções legíveis 720p/1080p | 0 e contratos |
| 8 | DBM-005B | `AbilityDefinition` e 3 kits P/QWER reais, escalando por rank/forma | cada herói executa mecânicas diferentes, sem cast genérico repetido | 1–2,7 |
| 9 | DBM-005C | Rigs/modelos consistentes, Goku pequeno/Nuvem, Cell formas, Oozaru | escalas corretas, Oozaru lento/hitbox robusta, no foot sliding | 8 |
| 10 | DBM-007A | Câmera follow/free, pan bordas, Space, zoom, clamps, nameplate | câmera não recenter ao cast, clique preciso em todas as resoluções | 7–9 |
| 11 | MAP-001 | Dados de mapa quadrado, bases diagonais, lanes top/mid/bot, jungle nodes | MapValidator garante simetria/rotas/spawns; blockout navegável | 0–1 |
| 12 | MAP-002 | A*/nav graph, paredes/brush, waypoints bots e Oozaru | path completo, sem stuck/jitter e sem atravessar paredes | 11 |
| 13 | MAP-003 | 3 tiers/rota, inibidores e núcleo, IA de wave, win condition | partida da compra inicial à vitória contra bots | 12 |
| 14 | ECON-001 | minions tipos, last hit, XP/renda por fonte, fases de partida | telemetria Zeni/XP correta, CS e órfãos de aggro ausentes | 13 + 3 |
| 15 | VISION-001 | fog, ward, brush, minimap/pings baseados em visibility snapshot | bots não conhecem alvos não visíveis; hud sem leak | 12–14 |
| 16 | OBJ-001 | buffs jungle, camps, objetivos maiores, resets, mortes | times podem contestar e ganhar vantagens sem exploit | 15 |
| 17 | DBM-006 | VFX/áudio/anim polish 3 heróis, projéteis e feedback | áudio/VFX sincronizados a eventos e hitboxes, FPS alvo medido | 8–16 |
| 18 | DBM-010 | jogo inteiro 3 lanes 1v1+bots + instrumentation | 10 partidas bot-bot sem crash, 3 sessões humanas com observações | anteriores |
| 19 | ROSTER-01…15 | variantes do GDD, kits e formas próprios por PR | 1 personagem por PR com testes/perf/licença | vertical slice aprovado |
| 20 | NET-001… | servidor headless 2 clientes → 5v5, sync, replay, anti-cheat | simulador verificável, sem divergência crítica, latência controlada | 18 |
| 21 | FUSION-001 | dois donos de input e uma entidade fusionada | fusão/defusão, morte, recomposição e desconexões corretas | 20 |

**Divergência proposital de ordem:** a câmera deve ganhar um **patch pequeno e antecipado** se continuar impedindo jogar/testar. A reforma completa da câmera faz parte de DBM-007A. O mapa detalhado pode ser trabalhado por design antes de ser codificado, mas a geometria nova não deve chegar sem navegação e validação de domínio. É possível executar documentação/ícones de itens em paralelo ao código de stats sem integrá-los prematuramente.

### 4.1 Contrato mínimo das primeiras entregas

#### DBM-003A — base do crescimento

Arquivos novos: `StatId.cs`, `StatModifier.cs`, `DerivedStatCalculator.cs`, `ChampionStatGrowth.cs`. Atualizar `GameWorld.TickHeroTimers` para HP regen fora da fonte; preservar cura da fonte e respawn; reutilizar HP/attack atuais por compatibilidade. É proibido adicionar stats direto a cada item sem centralizar agregação. Testes `STAT-01..15`: regen 10s, limite MaxHP, nível1→6→18, form sem duplo bônus, cooldown, compra sem duplicar, morte/respawn, clamp.

#### DBM-003B — danos e sustento

Arquivos novos: `DamageRequest.cs`, `DamageResult.cs`, `DamageResolver.cs`, `HealingResolver.cs`, `EffectTags.cs`. Implementar resolve damage e substituir a lógica em `GameWorld.DealDamage` de forma incremental. Gates `DMG-01..20`: armor100, KiResist100, pen flat/pct, shield vs HP, lifesteal, spellvamp área, imune, ataque negado, crítico seeded, dano verdadeiro, evento idempotente. Validar torres de DBM-002 e QWER.

#### DBM-003C — catálogo e itens

`ItemDefinition` evolui para `ItemData`; `ItemInstance` por slot; `RecipeGraph`; `EconomyService` com estado `PurchaseResult`. Exigir pelo menos 20 itens ativos com efeitos, 8 receitas DAG, 6 slots, ícones separados para próxima tarefa. Gates `ITEM-01..24`: saldo, componente 2×, slot, stack, sell, undo, unique passives, dupla compra, buff e mitigação corretos; testes de regressão de botas.

#### DBM-005A — seleção

Migrar `OrganicArena.startDemo` para flag explícita usada apenas smoke/dev, e `MatchBootstrap` respeita seleção. Cartões 3 heróis com papel, Kit P/QWER, dados `CharacterViewManifest`; UI de confirmação antes de `GameWorld`. Gates `SELECT-01..12`: abrir → trocar herói → confirmar → iniciar → morrer → ESC → sair → iniciar outro sem estado residual; sem auto-start na build real; smoke de teste consegue iniciar por parâmetro.

## 5. Validação integrada por camadas

| Gate | O que roda | Critério de falha |
|---|---|---|
| G0 fontes | hashes/licenças, `.meta`, `verify_publication.ps1` | tokens, arquivos gerados, licença duvidosa ou recurso faltante |
| G1 domínio | `DomainSelfTests` e nova suíte por subsystem | inviolabilidade de estado, regras numéricas, replay inconsistente |
| G2 Unity import | `VisualPrepare` | script C# não compila, ligação/prefab inexistente |
| G3 Play Mode | `VisualPlay` | rigs, camera, controles, forms, progressão quebrados |
| G4 Build | `VisualBuild`, `Build-identity` | erro de build, baseline divergente inexplicado |
| G5 nativo | mouse/teclado Windows, fixtures apenas opt-in dev | só domínio chamou comando, sem verificar UI/colisor/jogador |
| G6 gameplay | partida manual + bot-bot até um resultado final | habilidade invisível, tower diverge de anel, HUD enganosa |
| G7 performance | 10–15 minutos de partida com ondas, perf em build | CPU/tick e render P95/P99 fora de orçamento sem justificativa |

### 5.1 Valores-alvo de teste (não garantias)

- Tick de domínio com 30Hz = orçamento **33,3 ms por tick**, mas **meta própria** de P95 <= 4ms / P99 <= 8ms no PC de referência para preservar margem; medir com profiler, não assumir.
- Alvo render 60FPS = 16,67ms por frame; distinguir CPU, GPU, spikes GC, carga de shader e animações. Áudio sem loops duplicados.
- Hero click: raycast 3D converte corretamente para EntityId após trocar mesh/forma; eventos de dano provam hit.
- Caminhos: 0 unidade em parede/torre/núcleo durante 10 simulações de corrida/dash; Oozaru não ativa em local impossível; AI evita jitter.
- Zeni: 0 saldo negativo; combinação atômica; restauração após undo e venda; todos os 20 itens geram stats/ativo corretos.
- Ki: XP exibido difere da XP aplicada por no máximo tolerância arredondamento definida; farm e treino comparados por fase.

## 6. Estratégia de balanceamento experimental

### 6.1 Fases de poder e dados necessários

Criar `BalanceLab` para comparar 1v1 com tudo igual exceto a hipótese: nível1/6/11/16/18, com builds early/mid/full; 3 pares de heróis, ambos os lados, 5 perfis de input bot, 10 seeds. Começar com 3×3×5 = 45 combinações e duplicar por lado, ajustando por relevância; **não tomar taxa de vitória bot como substituto da pessoa**.

Registrar:
- `ttk`, `damageDoneByType`, `healing`, `shieldAbsorbed`, `criticalEvents`, `dps`, `timeOutOfCombat`;
- XP/Zeni por last hit, kill, wave, C, camp, objective, auxílio e fonte;
- tempo até primeira torre, ouro do primeiro item, taxa de comeback após déficit, duração da partida;
- `championId+formId+buildId+side+version+botPolicy+seed`.

### 6.2 Sem números arbitrários congelados

O nome `LoL` não justifica copiar valores. Números do nosso GDD: preço Luvas do Dragão 2800; BootsTreino 1100 e 300s; skills R níveis6/11/16; HP inicial do Goku Criança 570 no **código atual** — esses números são decisões do protótipo, com espaço para calibragem. **Crítico-base do LoL mudou em 2026**: cuidado com documentos que assumem 175% permanente por força de um patch de 2020. Escolher curva própria de `CritMultiplier`, com changelog.

### 6.3 Relatório de cada ciclo

`Report{hypothesis,baseline,experiment,seed,gameVersion,observedMetrics,uncertainty,screenshots,regressions,rolloutDecision}`. Documento vivo; uma alteração só é "concluída" com código, evidência, diferença no executável e teste. Gamification de indicadores bonitos sem jogo funcional não resolve satisfação.

## 7. Experimentos de interface e sensação

Fazer protocolos de três usuários, sem ajudá-los:
- selecionar Goku Criança e iniciar partida;
- encontrar item que aumenta Armor e combinar um escudo maior;
- identificar que Feixe de Ki é dano energético e que KiResist é o counter;
- usar um clique para atacar inimigo repetidamente, depois cancelar com movimento e voltar a atacar;
- carregar C até Ki cheio e **perceber** que XP está aumentando;
- navegar pela selva, identificar brush, guardar uma ward e contestar um objetivo;
- transformar em Oozaru e entender limitação de tamanho/velocidade.

Métricas de UX: tempo para conclusão, número de cliques equivocados, falsos ataques provocados por UI, leitura de HUD, percepção de quem causou dano/por quê. Sempre ter opção de corrigir UI e onboarding antes de aumentar complexidade.

## 8. Backlog de pesquisas ainda necessárias (não declarar “tudo pesquisado”)

| Área ainda aberta | Evidência necessária | Bloqueio |
|---|---|---|
| TTK e duração ideal | sessões humanas e bot-bot com farm/recursos reais | dados inexistentes na versão atual |
| C versus farm | XP/min aferido em waves/bots/jungle próprios | selva/last hit incompletos |
| Navegação grande/Oozaru | pathfinding por raio e perfil performance no mapa final | mapa novo não implementado |
| Multiplayer | protótipo 2 clientes, transport benchmark e replay | engine headless não extraída |
| Economia avançada | 20 itens/8 receitas reais e teste de escolha | catálogo atual só quatro itens |
| IP comercial | licenças escritas de modelos, vozes e universo | franquias de terceiros |
| Modelo 3D definitivo | rigs e assets adequados a cada forma | falta catálogo licenciado completo |
| Counterplay real | partidas humanas com diferentes arquétipos | apenas 3 heróis protótipo |
| FOG e bots | visão/oclusão navegável e UX | ausência de sistema autoritativo |
| Atualização LoL futura | novas notas de patch datadas | regras mudam por versão |

## 9. Protocolo ChatGPT ↔ Codex, sem gastar franquia desnecessária

1. ChatGPT documenta hipótese e altera **somente** o domínio/recurso necessário em branch `chatgpt/DBM-...-... `, gerando PR draft e testes novos.
2. Codex **não reimplementa**: sincroniza branch, roda Unity, reproduz bug, corrige no mesmo PR e salva resultados no `FROM_CODEX.md`/relatório.
3. ChatGPT revisa PR, resultados e regressões. Só então merge na `main`. Próxima tarefa publicada na fila.
4. Não iniciar DBM-003A enquanto o PR #2 aguarda teste e revisão, salvo pesquisa/dados fora do código. O `TO_CODEX.md` atual continua apontando ao PR #2.
5. Documentos de design não devem consumir tokens do Codex; só enviar prompt curto apontando arquivo e testes específicos.

## 10. Decisão final de arquitetura

**Implementar em incrementos, não reescrever tudo.** O maior risco no curto prazo não é falta de tecnologia avançada: é misturar regras, UI, arte, assets e economia no monolito `GameWorld+PrototypeGame` sem testes. A próxima etapa após DBM-002 é `StatCalculator+HPRegen` porque todas as espadas, escudos, itens, transformações, escalas por nível e counters dependem dela. Roteiro de produção deve preservar o objetivo: um MOBA de identidade Dragon Ball e estratégia rica, com assets licenciados, viabilidade técnica e aprendizado contínuo por dados.
