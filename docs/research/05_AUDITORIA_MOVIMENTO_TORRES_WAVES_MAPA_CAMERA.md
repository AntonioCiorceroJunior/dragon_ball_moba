# Dossiê 05 — Navegação, torres, ondas, mapa e câmera

**Data:** 2026-10-16. **Status:** auditoria estática de C# e pesquisa documental; não executado no Unity nesta rodada.
**Bases:** main (DBM-001 integrada), branch chatgpt/DBM-002-towers-collisions (PR #2, NÃO mesclado), GAME_DESIGN §8–10.

## 1. Evidências primárias verificadas

| Documento | Resultado documentado | Aplicação |
|---|---|---|
| [Riot /dev Map Changes 2024](https://www.leagueoflegends.com/en-us/news/dev/dev-season-2024-map-changes/) | Objetivos: paridade entre lados, menos exposição de solo lanes a ganks, acessos diversos a objetivos; protótipos de paredes/arbustos descartados após playtest. | Desenhar mapa original por grafo e medir tempo de acesso, gank, visão e equilíbrio, não só imagem. |
| [Patch 13.10 — Minions](https://www.leagueoflegends.com/en-us/news/game-updates/patch-13-10-notes/) | Timings de waves foram aproximados entre mid/laterais; minions que atacam torres não mudam de alvo por pedido de ajuda. | Ritmo de lanes e persistência de alvo são regras estratégicas. |
| [Patch 26.1 — Minions](https://www.leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/) | Waves após minuto14 a cada25s, após minuto30 a cada20s; ajustes de recompensas compensam maior frequência. | Versionar conjuntamente wave schedule, composição e ouro/XP; números exclusivos de 26.1. |
| [Riot Tech — The Tech Behind Swarm](https://www.riotgames.com/en/news/the-tech-behind-swarm) | LoL tradicional usa pathfinding A*; para centenas de minions em Swarm foram usados flow fields baseados em Dijkstra. Cliente e servidor interpolam waypoints; otimizações de crowd e VFX foram medidas. | Escolher algoritmo por benchmark, não copiar flow fields para poucas unidades. |
| [Riot Clarity in League](https://www.leagueoflegends.com/en-us/news/dev/clarity-in-league/) | Identificação e silhueta, hitboxes e VFX precisam ser interpretáveis em combate. | O anel da torre, o feixe de Ki e o Oozaru devem corresponder ao alcance/hitbox real. |

**Limite:** fontes da Riot não fornecem código integral das torres, path solver ou câmera atual. Regras detalhadas abaixo são propostas próprias.

## 2. Mapa e bots no código atual (main)

- Definitions.cs: V2.Clamp restringe X a -32..32, Y a -41..41.
- OrganicArena.RoadDistance: rotas quase verticais em X=-17, 0, 17 e um rio quase horizontal. São principalmente marcas visuais, não topologia de bases diagonais/top/bot/mid.
- GameWorld.SpawnWave: 3 minions do mesmo tipo por rota/time na abertura e a cada 14s, portanto 18 entidades por wave completa. Falta melee/ranged/cannon/super.
- GameWorld.ThinkMinion: alvo por NearestEnemy; segue torre/core diretamente. Não existe NavGraph, fog, wall blockers, path parcial ou brush.
- GameWorld.ThinkTower (main): usa score distância+penalização de herói, podendo escolher hero em situações que não refletem prioridade absoluta de minion. Não existe target stickiness explícita.
- GameWorld.UpdateMovement (main): V2.MoveTowards sem colisão real com torres, pedras ou obstáculos da selva.
- ProtectedCore (main): core invulnerável enquanto qualquer torre amiga sobreviver; não há cadeia de tiers/inibidores.
- ArenaCameraController: ortográfica, elevação43°, zoom8–18, pan com setas/meio, tecla Space recentraliza e Y alterna follow. Edge-pan e minimapa para câmera não foram implementados.

## 3. O que foi proposto em DBM-002, ainda sem validação de Unity

- Raio lógico: herói0.5, minion0.4, torre1.45 e núcleo1.65. Colisão apenas contra torres/núcleos vivos.
- AdvanceWithoutStructureCollision verifica segmento contra círculos e propõe passo tangente. Dash W usa subpassos de 0.2. Isso evita atravessar estrutura no caso simples, mas **não substitui** grafo/A*.
- ThinkTower separa minion e hero, dá prioridade a minion e abre exceção quando herói agressor causa dano efetivo a um campeão sob torre durante janela de 3s. Proposta de dano bruto125 e alcance centro-centro11.5.
- TowerRangeVisual oferece aviso próximo da torre inimiga, mas é apenas apresentação.

### Falhas potenciais específicas que exigem testes

1. **Retarget antes de concluir combate:** ThinkTower escolhe a tropa viva mais próxima a cada tick; minion B entrando mais perto que minion A pode provocar troca precoce sem regra de retenção.
2. **Agressão mitigada por escudo:** a troca de alvo só ocorre se o dano após escudo for maior que zero. Regra implícita precisa ser decidida e testada.
3. **Caminho travado:** o contorno por tangente pode oscilar no corredor entre dois círculos. Precisa retornar caminho, parcial ou bloqueio, nunca loop permanente.
4. **Oozaru:** raio do herói permanece fixo; expansão de escala visual pode deixar gigante atravessando parede. Transformação exige agente/collider próprio.
5. **Torre destruída:** bloquear deve cessar no estado lógico e no collider visual; validar raycast/click.
6. **Target ring:** alcance desenhado centro a centro deve indicar, com tolerância clara, o alcance de hitbox e não prometer segurança incorreta.
7. **W regressão:** mudança no deslocamento deve revalidar DBM-001 QWER/ataque automático no build.
8. **Sem bloqueio de unidade:** heróis/minions podem se sobrepor enquanto a colisão só considera estruturas; definir prioridade e desempenho antes de mudar.

## 4. Arquitetura incremental de pathfinding e movimento

Sequência: InputCommand → ValidateGoal → PathPlanner → LocalSteering → MovementIntegrator → DomainEvents → Presentation.

PathPlanner(start, goal, radius, flags, mapVersion) devolve Complete, Partial, Blocked ou Invalid + waypoints. Raio do agente vem da forma e não da escala renderizada; uma transformação pode invalidar path em execução.

Proposta:
- Fase 1: preservar caminhos simples por lane e colisões de torre testadas.
- Fase 2: WalkabilityGrid autoritativo C# com terreno, paredes, brush, obstáculos e inflação por raio do agente; A* com g+h, desempate determinístico por CellId, bloqueio de diagonais que atravessam cantos, waypoint smoothing com swept-circle.
- Fase 3: local steering entre minions/heróis e repath limitado de alvos móveis; métricas de tempo de chegada e CPU.
- Flowfield/Dijkstra somente se centenas de agentes com alvo comum tornarem A* caro em benchmark, seguindo as condições do Swarm.

Movimento não modifica stats, dano nem XP. Animação interpola snapshot do domínio, sem root motion duplicando deslocamento.

### Attack-move e orbwalking

- AttackTarget persistente após um clique, path até range levando em conta radii, Acquire → Approach → Windup → Strike → Recovery.
- Orbwalking: permitir deslocamento depois de disparar golpe, mas sem acelerar intervalo ou criar golpe fantasma.
- A+clique: AttackMove; S: Stop; H: HoldPosition (hoje H abre ajuda, requer remap); Shift/quickcast opcionais.
- QWER devem validar range/shape, recarga, custo, raycast e visibilidade antes de aplicar efeitos; VFX responde a evento do domínio.
- Dash varre colisão; blink atravessa obstáculo somente com tag de habilidade e destino seguro.

## 5. Mapa novo original

Conceito: bases nos cantos sudoeste/nordeste, mid diagonal, top e bot curvas de borda, rio cruzando mid, duas selvas com rotas de gank e brush, arenas de objetivo com múltiplos acessos, três tiers de torres e dependências de geradores/inibidores/núcleo. Sem coordenadas, malha, textura ou layout exato do Summoner’s Rift.

MapDefinition: Bounds, LaneGraph, Node, Edge(width,length,cost), Obstacles, BrushPolygons, VisionBlockers, TowerAnchors, CampSpawns, ObjectiveSites, SpawnPoints, MinimapProjection. Renderização, pathfinding e minimapa partem desses dados.

**Validações do mapa:** todo spawn navegável; três lanes ligam as bases; tempo base→primeiro encontro de wave similar por lado; tempo de jungle→gank e entrada de objetivo mensurado; pelo menos duas entradas para cada objetivo; percurso de Oozaru só onde couber; terreno visual respeita colisores; brush e visão impedem leitura ilegítima de posições.

## 6. Torre e wave como sistemas de estados

Tower: Idle → Acquire → Windup → Fire → ProjectileTravel/InstantImpact → Hit → Recovery → RetainTarget ou Reacquire. TowerTargetPolicy versionada por tier: attack range, cadence, projectile speed, priority, sticky target, hero aggro, range exit, damage scaling. TowerBody e indicador de alcance têm configuração derivada da mesma definição.

Wave: WaveDefinition {spawnTick, laneId, team, unitCounts, spawnOffsets, waypoints, travelSpeed, goldXPRewards, matchPhase}. O comportamento deve permitir slow push, fast push, freeze, crash e bounce emergirem de controle de tropa, dano, last hit e IA; não “simular” esses conceitos por cutscene.

**Exemplo matemático:** reduzir intervalo de spawn de 14s para 12s sem alterar a recompensa nominal aumenta potencial de ondas por minuto em 14/12−1 ≈ 16.67%; deve ser detectado em cenários econômicos.

## 7. Câmera como ferramenta de informação e comando

Do protótipo: Following/Free, ortográfica43°, pan setas/meio, Space e Y, zoom8–18. Não afirmar que equivale ao LoL. Proposta de patch de UX: edge scroll configurável, pan e zoom com clamp do viewport na topologia real, minimapa controlando foco, click-raycast sobre modelos gigantes e cooldown de centralização; câmera não deve recentralizar a cada ataque ou transformação.

Testar em 1280×720, 1920×1080 e 21:9. Skillshot aponta mesmo ponto lógico sob zoom; mouse sobre HUD não emite ataque; nameplates posicionados com Bounds do collider/rig, não offset fixo.

## 8. Matriz de testes para Unity e domínio

| Caso | Exigência |
|---|---|
| MOV-01 | clique no centro da torre não penetra seu círculo |
| MOV-02 | duas torres próximas não provocam jitter infinito |
| MOV-03 | W longo não atravessa torre/núcleo |
| MOV-04 | Oozaru não ativa em corredor estreito sem destino válido |
| MOV-05 | mesmo mapa/seed/comandos → resultados determinísticos |
| TWR-01 | minion A alvo, minion B mais próximo chega: regra de retenção respeitada |
| TWR-02 | aggro herói atacante dentro do alcance e desengaja ao sair |
| TWR-03 | ataque em escudo total: comportamento documentado e reproduzido |
| TWR-04 | torre morta é não sólida em domínio e view |
| WAVE-01 | encontro das três waves em tempos definidos por lane |
| WAVE-02 | alteração de spawn gera relatório de Zeni/XP/min |
| CAM-01 | clique em alvo animado após zoom/pan escolhe EntityId certo |
| VSN-01 | inimigo fora da visão não é revelado por bot/HUD |
| PERF-01 | 18, 72 e 180 minions: CPU por tick P95/P99 e GC medidos |

**Nenhum teste foi executado nesta pesquisa**. O PR #2 continua sendo a primeira validação técnica, antes de integrarmos nova arquitetura.

## 9. Próximo trabalho efetivo

- Validar PR #2 e corrigir target stickiness, escudo-agressão e navegação circular conforme bugs reproduzidos.
- Implementar StatCalculator/regen/dano/itens em etapas, enquanto MAP-001 desenha apenas grafo do mapa e bloqueios.
- Após jogabilidade estabilizada, mudar mapa com navegação e câmera juntos, medir rotas e só então aplicar assets, brush/visão e objetivos.
- Preservar originalidade e licenças: pesquisar LoL não concede direito de copiar assets/mapa/código/ícones.
