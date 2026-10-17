# Pesquisa MOBA v2 — Especificação implementável dos sistemas de partida

**Data:** 16/10/2026 · **Status:** projeto de sistemas e critérios de teste, **não código implementado**.  
**Base científica/documental:** [fontes auditadas](01_FONTES_AUDITADAS_E_EVOLUCAO_MOBA.md).  
**Referência preexistente:** `docs/specifications/GAME_DESIGN.md` (preservar variantes, Ki, Oozaru, mecânica cooperativa).  
**Regra editorial:** "LoL oferece X" e "Nosso jogo fará Y" são afirmações diferentes. Todos os números abaixo para nosso jogo são **provisórios e sujeitos a playtest**, salvo dados explicitamente do código atual.

## 1. Arquitetura causal da partida: por que o jogo tem profundidade

```
Controles confiáveis → mobilidade/combate legíveis → last hit + zonas seguras/perigosas
→ ouro e XP desigualmente distribuídos → comprar contra o adversário
→ disputar prioridade de wave, selva, visão e objetivos
→ conquistar estruturas → ganhar mais espaço e visão → pressionar uma rota
→ alcançar e destruir núcleo → avaliação do desempenho e partida seguinte.
```

Mecânicas só tornam o jogo estratégico quando produzem **decisões concorrentes**. Se segurar C for sempre melhor que farm, o jogador terá incentivo a ficar parado. Se espadas forem melhores que resistências contra qualquer equipe, não existe counterbuild. Se comprar itens não consumir recursos corretamente, não há economia. Se o mapa tiver três linhas mas não tiver caminhos e visão, não há rotas de gank/roam.

### 1.1 Ciclo de vida completo

| Estado | Inputs permitidos | Saída válida |
|---|---|---|
| `Boot` | configurações/carregar | dados licenciados e catálogos válidos |
| `Home` | jogar, ajustar áudio/vídeo, sair | seleção solo contra bots |
| `ChampionSelect` | escolher herói, função, ver P/Q/W/E/R/formas, confirmar | `LoadoutSnapshot` imutável para a partida |
| `Loading` | cancelar limitado, progresso | cena e mundo inicializados sem múltiplos spawns |
| `PreGame` | comprar inicial, movimentar conforme regra | tempo de início explícito |
| `Playing` | mover, atacar, comprar na fonte, Ki, wards, skills, comunicação, câmera | vitória, derrota, surrender habilitado depois |
| `PostGame` | resultados, estatísticas, novo jogo | descarte de estado/objetos, início limpo |

**Código atual:** `PrototypeGame.Start` pode disparar `StartMatch("GKU_SUPER")` automaticamente se `organic.startDemo` for verdadeiro; `OrganicArena.startDemo=true` no componente por padrão. Há três botões de escolha simples, mas não seleção confirmável. A primeira correção deve impedir **start automático em fluxo normal**, preservando inicialização específica dos testes smoke por flag.

## 2. Atributos: stat budget e crescimento por nível

### 2.1 Tipos

| Família | Atributos |
|---|---|
| Vida | `MaxHP`, `HP`, `HPRegenPer5`, `Shield`, `HealPower`, `ShieldPower`, `GrievousWounds` |
| Resistência | `Armor`, `KiResist`, `DamageReduction`, `Tenacity` |
| Básicos | `PhysicalAttack`, `AttacksPerSecond`, `WindupPercent`, `Range`, `CritChance`, `CritMultiplier`, `LifeSteal` |
| Técnicas | `KiPower`, `AbilityHaste`, `PhysicalPenFlat/Percent`, `KiPenFlat/Percent`, `SpellVamp` |
| Movimento | `MoveSpeed`, `BodyRadius`, `DashRange`, `PathingFlags`, `TurnRate`, `VisionRadius` |
| Recurso | `MaxKi`, `Ki`, `KiRegenPer5`, `ChargeBonus`, `TrainingXpRate` |
| Estado | `Level`, `Experience`, `Zeni`, `AbilityRanks`, `StatusEffects`, `Inventory`, `FormId` |

**Pipeline obrigatório de agregação:** campeão base por nível → forma atual → atributos absolutos dos itens → bônus percentuais dos itens → runas → buffs/debuffs temporários → clamps → `DerivedStatSnapshot`. A soma deve ser recalculável desde estado-base e equipamentos, evitando **bônus fantasma** ao vender, morrer ou desfazer transformação.

```
baseStat(level) = base + growth × (level - 1)
HPRegenPerSecond = (HPRegenPer5 / 5) × (1 + regenPercent)
HPnext = clamp(HP + HPRegenPerSecond × dt, 0, MaxHP)
CD=BaseCooldown × 100 / (100+AbilityHaste)
APS=clamp(BaseAPS × (1+ASBonus), minimum, maximum)
AttackInterval=1/APS
```

**Proposta inicial (não números LoL):** Goku Criança com 6 HP/5s e 0,65 HP/5s de crescimento por nível; Goku Super menos resistente, mais móvel; Cell com sustento condicional. Nada disso deve ser implantado sem tabela e testes TTK. Hoje só há cura na fonte; a falta de regen base é real no código atual.

### 2.2 Crescimento por personagem/formas

Forma não é nível de habilidade. Variáveis separadas:
- `ChampionVariantId` identifica o **herói escolhido**; `FormId` identifica a transformação;
- `Level` avança por XP, enquanto `AbilityRanks[4]` avançam por pontos;
- `FormModifiers` podem mudar visual/estilo, mas a engine deve recalcular atributos somente uma vez por transição;
- `AbilityOverride` pode alterar nome, modo de alvo, alcance, dano, cooldown e som da habilidade segundo a forma, preservando pontos investidos.

**Goku Criança:** nível1 tamanho claramente menor que o adulto (visual ~0,7x, colisor independente); nível11 ganha Nuvem, mas voo é apresentação/regras de pathing explícitas; nível16 pode obter R Oozaru após desbloqueio do rank adequado. **Não transformar instantaneamente só por atingir nível16 sem condição se o GDD prevê ultimate temporária.** Oozaru ~2,6–3,2x a altura do adulto é proposta, com colisor maior (não mesma escala da mesh), movimento 20–30% mais lento, ataques em área, fraqueza contra poke/controle, rig próprio, bloqueio de transformação se corredor não comportar a hitbox. O custo é exposição e menor mobilidade, não apenas bônus gratuito.

### 2.3 Regeneração, escudo e cura

**Regen de HP** desde nível1 fora da fonte. **Cura** é evento causado por skill/item. **Escudo** absorve dano temporariamente com duração, não aumenta MaxHP; remover escudo não subtrai HP previamente curado. **Life steal** cura percentual do HP realmente removido pelo ataque básico elegível, nunca de dano bruto sem verificar resistências/escudos. **Spell vamp** usa tags elegíveis separadas, com fator de área/DoT. Anti-cura limita restauração após mitigação e antes de aplicar cura, não reduz tamanho do escudo salvo efeito explícito.

Exemplos de invariantes:
- 100 de dano bruto físico contra Armor 100 → 50 antes de escudo;
- HP 200/500, escudo 40, hit 50 → escudo 0, HP 190;
- ataque com life steal 10% removendo 100 HP sem escudo → cura 10, limitada por MaxHP;
- shield absorvendo todo dano → cura 0 desse hit;
- 12 HP/5s = 2,4 HP/s enquanto vivo; morto = 0 de regen.

## 3. Combate e input — microjogo que deve parecer profissional

### 3.1 Máquina de ações

`Idle → Move/Approach → Windup → Strike → Recovery → NextStrike`. Ataques básicos precisam de alvo validado, alcance, intervalo, prioridades e retorno legível. **Ataque não é repetição de clique:** input define ordem persistente; o mundo resolve execuções seguintes até cancelar. Depois do windup e emissão do golpe, jogador deve conseguir mover-se (kite/orbwalk) sem desferir golpe fantasma. **Não vincular frame de aplicação de dano a VFX visual.**

Controles configuráveis mínimos:

| Entrada | Ação e cancelamento |
|---|---|
| Direita no chão | rota para ponto alcançável; cancela perseguição anterior |
| Direita no inimigo | alvo persistente, perseguição até alcance, ataques espaçados por cooldown |
| A + clique (proposta) | `AttackMove`, adquirir alvo elegível no caminho |
| S | `Stop`, cancela move e ataque; sem autoaproximação |
| H (a remapear) | `HoldPosition`, ataca próximos sem sair do local |
| QWER | skills com rank, custo, alcance, windup e feedback |
| Ctrl+QWER | gastar ponto conforme gates de habilidade |
| C pressionado | carregar Ki, depois treinar XP; interrompido por dano/ação/movimento |
| Space | centralizar câmera; ao soltar pode manter ou voltar modo livre conforme escolha |
| B | retorno à base canalizado, interruptível |
| 1–6 | itens ativos, com cooldowns e restrições |
| Tab | placar, economia pública, itens observáveis |
| P/Esc/Enter | loja/config/chat/ajuda sem input atravessando a UI |

O protótipo já tem DBM-001 (habilidades e autoataque), mas **não** tem `AttackMove`, `Stop`, `HoldPosition`, projéteis lógicos, windup com cancelamento e sistema completo de CC.

### 3.2 Pipeline de golpe

```
ValidateRequest → AcquireTarget → CheckFog/Range/Collision
→ Windup → Fire[ProjectileId/Instant/Area] → HitCheck
→ ComposeDamage(base[rank]+ADratio×Attack+KiRatio×KiPower)
→ Crit (if tagged) → Reduction/Penetration → Shield
→ ApplyHP → Lifesteal/OnHit/CombatStatus
→ Threat/TowerAggro → OnDeath/AwardXP/Zeni → Events
```

**Tipos**: `Physical`, `Ki`, `True`; tags `BasicAttack`, `Ability`, `OnHit`, `Area`, `Dot`, `CanCrit`, `CanLifeSteal`, `Unblockable` (raríssima). Resistências físicas e de Ki independentes; dano verdadeiro pula resistências, mas pode sofrer bloqueios/imunidades explícitas por tag.

**Crítico:** usar RNG determinístico com seed por partida e eventos, não `UnityEngine.Random` sem seed no domínio; chance não precisa ter exatamente estatísticas de LoL de 2026 (patch 26.1 alterou multiplicadores-base). Registrar `WasCrit` no evento e testar distribuições ao longo de seeds.

**Projéteis:** spawn lógico, velocidade, homing ou skillshot, hitboxes e timeout; raio de feixe em data; efeitos visuais recebem eventos. Feixe reto não equivale a skillshot bom: prever warning rectangle/cone, iluminação, impacto, espessura e travel time claros. Resolução de colisão usa model geométrico, não só linha desenhada.

### 3.3 Habilidades e ranks (com identidade)

DBM-001 implementou gates gerais de QWE 1/3/5/7/9 e R 6/11/16, além de escalonamento genérico. **Próximo passo de design** é por campeão, forma e slot:

`AbilityDefinition{AbilityId,Slot,CastType,TargetType,Range[rank],Damage[rank],ADRatio[rank],KiRatio[rank],Cost[rank],Cooldown[rank],WindupTicks,TravelSpeed,CC,ReactionWindow,VisualCue,AudioCue}`.

Goku Criança: Q Bastão Mágico físico com alcance; W Salto; E Kamehameha inicial canalizado; R Sequência do Torneio e, no 16, Oozaru temporário. Goku Super: engage/reação/teleporte condicionado + R que evolui por forma. Cell: jogo de absorção defensável, com fraqueza explícita para anti-cura/kiting (reconciliar detalhes com GDD). Não usar o mesmo Q genérico colorido para todos.

**Counterplay tático**: feixe com sinal no chão, charge exposto, invocação interrompível, hitbox/sonic cue coerente. **Counterplay estratégico**: escudos e resistências de Ki contra burst, armor vs ataque básico, anti-cura vs Cell, visão vs assassin, wave control vs herói dependente de skills. Não transformar counter em vitória automática.

### 3.4 Controles de grupo e temporizadores

Tipos mínimos: `Stun`, `Slow`, `Root`, `Silence`, `Knockup`, `Knockback`, `Untargetable`, `Disarm`, `Shielded`, `GrievousWounds`. Uma `StatusEffect` tem fonte, duração em ticks, stacks, categoria, intensidade, regras de empilhamento/limite, prioridade e formas de limpeza. Definir matriz de ações permitidas sob cada CC; evitar que stun cancele só animação mas não ataque. Para teleporte, validar destino e colisor maior antes de aceitar.

## 4. Economia: criar o loop de itens e counters de verdade

### 4.1 Eventos de renda

`ZeniLedger` precisa de `Passive`, `LastHit`, `Assist`, `Kill`, `Tower`, `Camp`, `EpicObjective`, `Purchase`, `Combine`, `Sell`, `Undo`; toda transação tem `eventId`, `tick`, `ownerId`, `amount`, `balanceAfter`. XP de tropa pode ser partilhado dentro de range; Zeni por last hit pertence ao responsável. Estado da primeira torre, placas, kill streak e shutdown será projetado depois das primeiras waves funcionar corretamente.

**Observar:** o código atual cria uma wave de **3 minions por rota/time**, a cada **14s**; modelos e regras são genéricos. Evoluir para melee, ranged, siege e super minions somente com IA, range, custo e visão funcionais.

### 4.2 Inventário e receitas

`InventorySlots=6`, `ItemInstance` por slot (não string livre), item com `RecipeNode`, preço total, custo de combinação, categoria, tags, bônus/passivas únicas, limites, `IconAssetId`, requisito de loja, venda e undo. DAG da receita rejeita ciclo e ingrediente inexistente.

```
componentes já comprados: luva (350) + luva (350) + cristal (400)
preço final: 2800
pagar para combinar = 2800 - 1100 = 1700
consumir os 3 ingredientes + debitar 1700 + receber Luvas do Dragão
```

Sem repetir atributos dos componentes! **Conservação contábil**: `Zeni + gastos líquido` bate com entradas do ledger; transação com inventário cheio/sem saldo não muta nada; duas compras simultâneas idempotentes; desfazer compra restaura estado anterior se ainda válido.

### 4.3 Itens úteis por categoria

| Família | Componentes | Item avançado | Decisão/counter |
|---|---|---|---|
| Attack/espada | luva, cabo, lâmina, velocidade | Espada Capsule, Manoplas Saiyajins | matar rapidamente quem tem pouca Armor |
| KiPower | célula, foco, condutor | Cristal de Kaiô, Núcleo Blue | burst/poke energético; sofre contra KiResist |
| Tank físico | placa, cristal vida | Colete Freeza, Armadura Saiyajin | lidar com carry de ataque |
| Tank Ki | isolante, cristal vida | Barreira Namekuseijin | lidar com magia/Ki |
| Regen/sustain | semente regeneração, cristal | Núcleo de Regeneração | sobreviver poke, menos dano |
| Life steal | essência vampírica, luva | Espada Capsule vampírica | cura por hit real; sofre anti-cura |
| Anti-cura | cristal Ki, dispositivo | Pulso Interferência | counterar Cell/Buu/cura excessiva |
| Anti-tanque | cristais/penetração | Lentes de Energia / perfurante | responder a armadura/resist alta |
| Mobilidade | botas, item de defesa | Botas Vento, Armadura, Ki | kite/roaming, tradeoff de dano |
| Treinamento | botas específicas | bônus permanente após 300s | perda temporária de velocidade, retorno posterior |
| Visão | ward, radar, scanner | Radar Capsule | informação e segurança de objetivos |

Usar todos os itens e números de `GAME_DESIGN.md` §16 como **intenção aprovada**, adaptando nomes/atributos apenas com decisão registrada. Todo item terá ícone **próprio/licenciado**. Não copiar ícones de LoL. Recomendação de loja deve explicar "bom contra [tipo de ameaça]" e mostrar ganho líquido, não selecionar automaticamente.

### 4.4 Contraexemplos a impedir

- Comprar armadura aumenta dano de Ki (bug de tipo).
- Escudo concedido por compra dura para sempre por falta de expiração.
- Item com life steal cura com ataques em escudo.
- Vender item não revoga bônus.
- Combinar componentes concede bônus duplicados.
- Comprar espada com Zeni insuficiente debita até ficar negativo.
- Item perdido quando o inventário está cheio.
- "Build recomendada" não considera nenhum inimigo nem tipo de dano.

## 5. Ki e XP: a mecânica diferencial Dragon Ball

### 5.1 Diagnóstico do projeto observado

Em `GameWorld.TickHeroTimers` na `main`:
- `KiRegen=2` passivo padrão por segundo, acrescido a `hero.Ki`.
- Segurar C recupera Ki com bônus `15 + KiRegen` por segundo no ramo de charging, **além** de regen já aplicada antes no tick (comportamento a decidir se é intencional). Não confundir taxa de ramo com taxa total.
- Ki cheio e fora da fonte: `GrantExperience(1.8 * multiplier * dt)`. Multiplicador `0.55` após 30 s: taxa nominal cai a 0,99 XP/s.
- XP exigido: `ExperienceForNextLevel(level)=90+40*(level-1)`; para evoluir do 16 ao 17: 690 XP → se a pessoa dependesse só de 0,99 XP/s, aproximadamente **697 segundos**, ~11,6 minutos, ignorando todos os ganhos anteriores, interrupções e farm.
- `TrainingSeconds` decai gradualmente quando C não é pressionado; um antipadrão que evita reset instantâneo, mas ainda não garante que repetidas alternâncias não permitam contornar penalidade.

**Leitura:** a impressão do usuário é justificada pela taxa diminuta no nível alto e por feedback de UI pobre. A documentação anterior precisa deixar claro que o ganho de Ki do ramo não é o total exato por segundo.

### 5.2 Sistema recomendado

Estados `Idle`, `ChargingKi`, `TrainingXP`, `Interrupted`. Transição ao Ki cheio aciona informação diferente na UI: mudança de aura, som e texto `XP/s`; XP de treino é **recurso produzido por risco**, não bônus escondido.

Definir taxa como fração do `XP/min` obtido por farm competente no mesmo estágio; meta inicial de laboratório: **20–30% do farm competente**, com fadiga crescendo ao segurar parado, mas sem zerar por muitos segundos. Manter possibilidade de upar treinando até 18, ainda que menos eficiente. Não conceder treino na fonte. A cada tick mostrar estado derivado e projeção aproximada para próximo level; reset de fadiga gradual, nunca exploit de soltar/pressionar.

**Som/vocalização:** usar fonte original licenciada e `AudioMixer`, com envelopes `start/loop/stop`, ducking para golpes e limitador de repetição. Se sair do estado Charging/Training, interromper áudio e aura imediatamente. Goku Kid, Super, Cell têm timbres próprios. Volume de gritos independente e opção de silêncio.

### 5.3 Experimentos

- 2 minutos de C isolado em local seguro no nível1, 6, 12 e 17; XP real versus simulada, com e sem item.
- 2 minutos de farming competente em cenário fixo equivalente; razão `C_XP/Farm_XP`.
- C próximo a inimigos (risco e vulnerabilidade), dano interrompe; no brush e na fonte.
- Alternar 1s C / 1s parado ao longo de 5 min, comparar fadiga contra 5 min contínuos; não pode superar estratégia sem custo em excesso.
- Aura e XP/s mantêm visibilidade em resoluções diferentes; valor mostrado condiz com incremento de XP.

## 6. Mapa e macroestratégia: o desenho produz decisões

### 6.1 Grafo antes de decoração

A versão atual usa arena de coordenadas `x∈[-32,32]`, `y∈[-41,41]`, torres em `x=-17,0,17`, bases superior/inferior, rio cruzando linhas. Isso é **topologia de três colunas**, não mapa de top/mid/bot equivalente em navegação a Summoner's Rift. A nova arena deverá ser **original**, aproximadamente quadrada, com bases em cantos opostos, duas rotas contornando as bordas e mid diagonal, selvas separadas e rio diagonal. O map layout deve ser definido em `MapGraph` antes de novo cenário 3D.

**Contrato de mapa:** `Nodes` de base/spawn/turret/objective/brush/portal; `Edges` com `length`, `walkableWidth`, `agentRadiusMax`, `fowBlocker`; `Polygons` para rio, mata, selva, visão, arena de objetivos, colisão. `NavGraph` e minimapa derivam da **mesma** definição.

### 6.2 Estruturas e progressão

Etapa inicial: 3 torres por rota por equipe, gerador/inibidor protegido, 2 torres finais e núcleo; destruição de núcleo exige caminho desbloqueado. Valores de dano/range prototipados por tier e tempo (não copiar tabelas Riot sem contexto de patch). Tower aggro tem regras consistentes com alvo válido, prioridade, stickiness, hero punindo agressão a aliado defendido e redução de dano de herói solo contra estruturas.

**PR DBM-002:** ainda não compilado/testado; seu modelo provisório de torre prioriza minion próximo e troca seleção por tick, sem necessariamente manter **sticky targeting** típico. Regra definitiva de perseguição/dano exige testes com alvo retendo prioridade ao entrar uma segunda tropa. Não presumir que PR #2 já cobre escalonamento por tiro, tier, placas ou projéteis lógicos.

### 6.3 Waves/selva/objetivos

- `WaveSchedule`: melee/ranged/siege/super, timestamps versionados, spawn com múltiplos waypoints, aggro, prioridade, last hit, compressão/escalonamento de waves.
- `JungleCamp`: múltiplos minions, leash radius, reset HP, aggro, respawn, recompensa, anti-abuso ao puxar campo.
- `EpicObjective`: spawn/re-spawn, buffs condicionais, contestação, prioridades de ataque, mensagens no mapa; monstros originais dinossauro/dragão/guardião.
- `VisionGrid`: fontes da equipe, obstáculos, brush, wards, scanner, cooldown; visibilidade do HUD e bots baseada em `SightSnapshot`, nunca em `World.Entities` sem filtragem.
- `MatchPhases`: early (farm), mid (rotações/objetivos), late (cerco/fechamento); durações derivadas de renda/XP e tempo até torre, não só relógio.
- `CounterStrategy`: wave control, roam, gank, objective trade, split pressure, engage/peel e escolha de item.

### 6.4 Métricas de mapa

Comparar por lado: distância base–primeiro minion, tempo jungle–primeiro gank em top/bot, tempo até rio e objetivos, número de entradas seguras, alcance de visão de brush, pontos de ward, largura de gargalo contra Oozaru, tempo de rotação com velocidade de cada classe. **Simetria competitiva medida**, não apenas rotacionar textura.

## 7. Câmera, modelos, UI e áudio: legibilidade de MOBA

### 7.1 Câmera

Estado da câmera: `CameraMode{Follow,Free,Recenter,Held,Observer}`. Projeção ortográfica inclinada, pan nas bordas opcional, arrastar botão central, click-to-focus no minimapa, zoom e clamp pela extensão real do mapa na resolução usada. A câmera **não** deve recenter por ataque/dash nem produzir descompasso entre raycast/área clicada. Teste sobre alvos transformados de alturas diferentes; espaço de tela do mesmo personagem deve ser suficiente para identificar animação e range.

Em GPU/monitor diferentes: testar 1280×720, 1920×1080 e 21:9; HUD não deve cobrir comandos nem nameplates; FPS estável em teamfight.

### 7.2 Visual do herói

`CharacterViewDefinition`: `modelAssetId`, `rigProfile`, `scale`, `rootMotionPolicy`, `colliderProfile`, `offset`, `animationSet`, `auraProfile`, `voicePackId`. O deslocamento é determinado pelo domínio; animação sincroniza ao delta real e **não desloca novamente** o GameObject por root motion, evitando foot sliding/teleporte duplo. Ao trocar forma, preservar HP/Shield/ranks/velocidade de simulação e mudar apenas propriedades aprovadas.

`WorldSpaceNameplate`: nome, equipe, nível, HP, escudo, efeitos de CC, carga Ki, badge de transformação, altura calculada pelo collider atual. Não desenhar informações do adversário fora do campo de visão.

### 7.3 Qualidade de VFX

Aura de Ki: malha/partículas adequadas (não esferas primitivas), variação de intensidade e forma por personagem, emissive moderado, som loop controlado. Técnicas: faixa de ameaça antes do acerto, projectile/beam trail, impacto com áudio/partículas e número de dano **decorrentes do evento de domínio**. O jogador deve saber onde seria atingido se VFX estivesse oculto. Partículas com limite, pooling e qualidade reduzida.

### 7.4 Áudio

Grupos `Master`, `Music`, `Ambience`, `Combat`, `Voices`, `UI`, `Objectives`; normalização e prioridades, ducking na explosão importante, cooldown de vocalização. Todos os sons originais/licenciados; nunca importar extratos de anime, vozes ou trilha de terceiros sem direito de distribuição.

## 8. Resolução de problemas relatados: contrato de aceitação

| Reclamação do jogador | Aceite mensurável |
|---|---|
| Herói pouco forte com nível | Level 1→6→11 mostra crescimento efetivo e TTK muda de modo controlado |
| Falta regenerar HP | Fora da base, 10 segundos vivo curam pelo HPregen exibido |
| Itens “bugados” | Compra/venda/receita mudam stats e HUD imediatamente; nenhum bônus fantasma |
| Ki C depois de cheio não compensa | XP/s exibido e calibrado contra farm; level 17 não requer espera irrazoável |
| Mapa não parece MOBA | Bases diagonais, top/bot/mid reais, selva, brush, controle de visão e 3 rotas navegáveis |
| Câmera ruim | Modo livre/recenter, pan/zoom estáveis, raycast preciso e minimapa coerente |
| Só um personagem tem modelo legal | Três perfis distintos de rig/model, visual certo em cada forma |
| Goku Criança é grande | Criança menor que Super em side-by-side no jogo, colisor compatível |
| Oozaru não gigante/lento | Transformação original grande, slow real e conversão segura de colisor |
| Ícones/nomes ausentes | Loja com ícones, receitas e barras/nome/nível sobre unidade, sem clipping |
| Sons fracos e gritos ausentes | Ki tem vocalização/loop, VFX e áudio cessam ao cancelar |
| Habilidades são iguais | P/QWER por personagem e curvas próprias por rank/forma |
| Faltam detalhes LoL | Cada novo detalhe depende de contrato, telemetria, teste e comparação de counter |

## 9. Limites que continuam em aberto

Não há, até agora, leitura do código proprietário Riot. Não temos métricas medidas de TTK, renda/XP e pathing de uma partida longa do nosso jogo, nem 10 bots competitivos, multiplayer, assets finais licenciados dos 15 heróis, áudio original, testes com jogadores ou decisão jurídica sobre IP Dragon Ball. Não há base honesta para prometer paridade com LoL já nesta etapa. O objetivo é reduzir lacunas **com evidência por feature**, não afirmar conclusão porque existe especificação.
