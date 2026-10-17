# Plano mestre vigente — Dragon Ball MOBA

**Revisão:** 17/10/2026 · **Tipo:** consolidação técnico-editorial. **Não equivale a novo balanceamento aprovado, código implementado ou testes.**
**Visão do produto:** [GAME_DESIGN.md](specifications/GAME_DESIGN.md). **Pesquisa detalhada:** [research/README.md](research/README.md). **Tarefa executável atual:** [TO_CODEX.md](coordination/TO_CODEX.md).
**Princípio:** transformar o protótipo Unity numa partida MOBA profunda sem recomeçar do zero, sem misturar responsabilidades do domínio com Unity e sem copiar arte/código proprietário de LoL.

## 1. Objetivo e recorte de produto

O projeto quer reproduzir a **profundidade sistêmica** de um MOBA competitivo, com universo e mecânicas inspirados em Dragon Ball:
- partida com rotas estratégicas, selva, objetivos, torres, núcleos, itens, XP, vantagem territorial, counterplay e vitória;
- Ki como recurso que pode ser carregado com `C` e, quando cheio, produzir treinamento de XP sob custo tático; efeitos visuais/sonoros claros;
- cada versão de personagem representa um estilo e tem `P/Q/W/E/R`, atributos, transformações, animações e tamanho próprio;
- Goku Criança verdadeiramente menor; Oozaru gigante, forte, lento e com hitbox/caminhos compatíveis;
- seleção real antes da partida, câmera livre/follow, controles de MOBA, nome/barras dos personagens e loja visual;
- itens básicos e completos, receitas com componentes, counters por dano físico/Ki, defesa, regen, crítico, velocidade, roubo de vida, vampirismo e anti-cura;
- expansão posterior para variantes definidas no GDD, partidas online, cooperação, fusão e sistemas sociais.

**Escopo do primeiro jogo que vale testar:** três heróis originais do protótipo (Goku Criança, Goku Super, Cell), 1 jogador + bots, **partida offline completa e reproduzível**. A profundidade de um sistema integrado vem antes de 15 personagens parcialmente implementados.

O uso de IP Dragon Ball e o estudo de League of Legends **não concedem licença** para comercializar personagens, vozes, logos, mapas, assets ou códigos protegidos. O repositório público deve conter apenas recursos redistribuíveis, e qualquer possível publicação requer revisão de direitos.

## 2. Separação que organiza as decisões

| Dimensão | Onde está a verdade | Exemplo de distinção |
|---|---|---|
| Comportamento atual | `main` e evidência correspondente | Goku Kid tem 570 HP e +24 por nível no protótipo |
| Comportamento futuro desejado | GDD mestre e decisões aprovadas | GDD sugere Kid com 660 HP iniciais; balanceamento definitivo em aberto |
| Hipótese técnica | dossiês de pesquisa | `StatCalculator`, `HPRegenPer5`, `DamageResolver`, receitas de itens |
| Verificação | testes de domínio e Unity + relatório versionado | DBM-001 validada; DBM-002 ainda depende de testes |
| Tarefa que Codex executa | `coordination/TO_CODEX.md` | DBM-002 exclusivamente, sem merge por conta própria |

**Não copiar fórmulas numéricas de LoL sem patch/versão.** Nossos coeficientes de HP, regen, Ki, crítico, itemização e tamanho são parâmetros de balanceamento, não regras já fixadas.

## 3. Arquitetura alvo — evoluir o código por incrementos

```text
GameSessionFlow (menu → seleção → loading → partida → resultados)
          ↓                     ↑ eventos + snapshots
InputCommandMapper → GameCommandQueue
          ↓
GameWorld (fachada compatível; regras ainda existentes)
  ├─ Stats: ChampionGrowth + Forms + Equipment + Buffs → DerivedStats
  ├─ Combat: Orders + AttackTimeline + Abilities + DamageResolver + Shields/Healing
  ├─ Progression: Level/XP + SkillRanks + KiCharging/Training + Transformations
  ├─ Economy: ZeniLedger + ItemCatalog + RecipeResolver + ItemInstance/Inventory
  ├─ World: MapGraph + Nav/Collision + Minions + Towers + Jungle + Vision
  ├─ AI: bot, targeting, goals, objectives
  └─ Telemetry: DomainEvents, tick/seed, deterministic replay
          ↓
Unity Adapters (visual, rig/animator, câmera, HUD/loja, áudio/VFX, build)
```

**Invariantes:**
1. O domínio em C# puro continua autoritativo para HP, XP, Ki, ouro, posição e colisão.
2. Render, animação e áudio reagem aos eventos; não aplicam dano ou dinheiro por conta própria.
3. Stats finais são derivados das fontes, não modificados por adições irreversíveis a cada compra/transformação.
4. Um comando inválido não consome Ki, Zeni, habilidade ou vida. Eventos têm ID/tick para permitir teste e eventual rede.
5. Uma versão de conteúdo por partida evita mudanças silenciosas de skills, itens, rota e recompensas.
6. Nenhuma migração de Unity/URP/DOTS/rede é requisito antes de um benchmark específico.
7. A interface consome o mesmo `ItemCatalog` e `AbilityDefinition` da simulação; ícones e tooltips não definem efeitos.
8. O multiplayer posterior usa servidor autoritativo, sem confiar em dano, visão ou compra calculados pelo cliente.

**Migração sem reescrita geral:** introduzir `DerivedStats` ao lado de `GameWorld`, trocar só o ponto necessário, garantir testes, depois extrair módulos de combate/loja/mapa. Os arquivos atuais `Definitions.cs`, `EntityState.cs`, `GameWorld.cs`, `PrototypeGame.cs`, `OrganicArena.cs`, `ArenaCameraController.cs` permanecem fonte concreta para cada tarefa.

## 4. Regras-alvo por subsistema e critério de aceitação

| Sistema | Contrato que precisa existir | Como comprovar |
|---|---|---|
| Stats por nível | Dados por herói de HP, ataque, KiPower, regen/5s, Armor/KiResist, ataque/s, speed | Níveis 1, 6, 11, 16 e 18; derivados iguais às tabelas; regen fora da fonte e cap no MaxHP |
| Dano e sustain | Físico, Ki, verdadeiro; resists e penetração; escudo; life steal/spell vamp sobre dano efetivo; anti-cura | Fixtures numéricas 100 vs armor100; cura sobre HP real; 0 lifesteal em escudo total |
| Habilidades | P/Q/W/E/R distintos por campeão; ranks e custos/cooldowns/ranges/scaling por habilidade e forma | Rank 2 altera valor; QWER bloqueadas quando não aprendidas; counterplay/avisos legíveis |
| Itens | 6 slots; catálogo com preços, itens básicos e avançados, receitas DAG, venda, undo, passivas únicas, itens ativos | Componente já comprado reduz custo; ao combinar remove componentes sem bônus fantasma; UI/ícones refletem inventário |
| Ki C | Estado ChargingKi → TrainingXP com fadiga, risco/interrupção e XP/s visível | Após Ki cheio fora da base XP sobe; na fonte 0 XP; press/release não explora fadiga |
| Movimentação | Ordem persistente, attack-move, stop, orbwalk, raio por forma, swept collision e path válido | Clique contorna obstáculos, W não cruza parede proibida, Oozaru não entra em corredor estreito |
| Torre | Range/cadência/targeting/hero aggro/retention/projétil por tier, anel visual preciso | Ataca dentro, não adquire fora, minion prioritário, troca aggro apenas conforme política |
| Waves/economia | Minions melee/ranged/siege/super, last hit, XP compartilhada, Zeni ledger, push/freeze emergentes | XP/Zeni/min por fase e mudança no spawn não criam inflação inesperada |
| Mapa | Base em cantos opostos, top/bot/mid reais, selvas, rio, walls/brush/vision, torres em tier, inibidores | Validador de conectividade, tempos de acesso semelhantes entre lados, bot pathing confiável |
| Jungle/visão/objetivos | Camps, respawns/buffs, fog, wards, neutral objectives, bosses Dragon Ball | Sem vazamento de inimigos invisíveis; vitória/derrota após disputa de objetivos |
| Personagens | Seleção obrigatória, três modelos/rigs coerentes, tamanhos/anim/skin/form e nomeplates | Começa apenas após confirmar; Goku Kid menor; Oozaru gigante e lento sem foot sliding |
| Câmera/HUD | Follow/free, Space, pan/edge scroll opcional, zoom/clamp, minimapa, informações reais e UI click protection | testes manuais e automatizados 720p/1080p/21:9, click-raycast correto |
| VFX/SFX | Auras, voz original/licenciada de C, feixes, aviso, impacto, mixagem e pooling | áudio cessa ao soltar C; VFX condiz com dano e hitbox e não cria duplo hit |
| Telemetria/balance | métricas por herói, versão, build, modo, lado, nível e fase | análise antes/depois por seed; não declarar winrate humana só por bot |

## 5. Ordem real de construção, respeitando a fila existente

**A fila formal continua em [coordination/TASK_QUEUE.md](coordination/TASK_QUEUE.md)**. IDs adicionais abaixo são recortes sugeridos, **não tickets ativados**; só ChatGPT atualiza fila e TO_CODEX após revisar evidência. Não há ação do Codex automática.

| Prioridade | Vínculo com fila | Slice técnico recomendado | Dependência/gate |
|---:|---|---|---|
| 0 | **DBM-002** | Testar PR #2: torres, aggro, alcance, W/colisões; corrigir erros específicos | Unity/Windows, relatório e revisão; merge só após aceite |
| 1 | **DBM-003A** (proposta de recorte de DBM-003) | `DerivedStats`, atributos básicos e `HPRegenPer5` por nível/herói | Stats não duplicados; HP realmente regenera |
| 2 | **DBM-003B** | Pipeline `DamageResolver`: físico/Ki/true, escudo, penetração, life steal, vampirismo, haste/crítico/AS | Testes determinísticos, efeito visível no jogo |
| 3 | **DBM-003C** | Catálogo data-driven, 6 slots, Zeni ledger, itens básicos/avançados, componentes, receitas DAG e transações | Comprar/vender/compor altera stats corretos |
| 4 | **DBM-003D** | Loja real com ícones próprios, receitas, busca, categorias, tooltips e comparação de counterbuild | Compra nativa por mouse funciona |
| 5 | **DBM-004** | Treino Ki C, cálculo de XP, fadiga, feedback e interrupção; depois efeitos visuais | XP em Ki cheio mensurável, sem exploração |
| 6 | **DBM-005** | Três rigs/modelos/forma escalados e animados; Oozaru só após collider/kit prontos | Goku pequeno, Cell distinto, transformações verificáveis |
| 7 | **DBM-007** | Seleção pré-partida obrigatória, HUD, nomeplates, inventário/minimapa e câmera MOBA | Partida começa após escolha e input/câmera testados |
| 8 | **Novo ticket proposto: SKILL-001** | Kits P/QWER individuais do GDD e overrides de forma, combinados a stats/ranks de DBM-001 | 3 heróis com 3 padrões de gameplay e counterplay distintos |
| 9 | **Novo ticket proposto: MAP-001/002** | Grafo original e blockout de mapa; A*/navegação/brush/colisão; depois arte | simetria de tempo e path válidos por raio |
| 10 | **Novos tickets: WAVE/TOWER/VISION/OBJ** | Tropas tipadas, hierarquia de torres/geradores, visão, selva e bosses | partida completa do começo à vitória, sem trapaça dos bots |
| 11 | **DBM-006** | Polimento de VFX/SFX sincronizados aos sistemas efetivos | legibilidade, performance e cancelamento |
| 12 | **DBM-008** | Auditoria adicional de bases abertas, licenças, dependências e benchmarks | não importar projeto antigo inteiro sem prova |
| 13 | **DBM-010** | Testes e telemetria end-to-end; regressão e playtests | relato com métricas, bugs e limites |
| 14 | **DBM-009 / NET** | Servidor autoritativo, snapshots, replay, lobby/5v5 depois do MVP | prova headless, segurança e latência aceitável |
| 15 | **Roster/fusão** | variantes futuras, fusão cooperativa e social/ranked | só após partida funcional/online pertinente |

**Ressalva de IDs:** pesquisas anteriores sugeriam `DBM-005A` para seleção, mas a fila atual atribui DBM-005 a rigs/animações e DBM-007 a seleção. **Não reinterpretar ID sem decisão.** Este plano respeita a fila; novos recortes precisam ser adicionados formalmente quando chegar a vez.

**Ajuste de ordem possível:** o usuário considera a câmera ruim. Um pequeno patch de câmera pode ser priorizado antes do MAP-001 como tarefa isolada e com testes, sem transformar DBM-002 em revisão de UI.

## 6. Critérios de qualidade e evidência (Definition of Done)

Para cada slice, produzir:
1. **Contrato explícito**: mudança, arquivo(s) alterados, origem da regra (GDD, pesquisa, proposta, bug), casos limites.
2. **Teste automatizado do domínio**: regras matemáticas, comandos inválidos, cooldown, economia, colisão, regressão, seed.
3. **Validação Unity**: importação/compilação, Play Mode e build Windows no projeto ativo, não no protótipo antigo.
4. **Comprovação da experiência**: clicar/comprar/mover/ver dano pelo executável, com captura/log; benchmark quando sistema impactar FPS.
5. **Documentos de handoff**: relatório por execução em `coordination/reports/`, estado e bloqueios.
6. **Revisão do PR**: ChatGPT revisa resultados antes do merge. Falta de prova não é sucesso.

Use níveis de evidência E1–E5 definidos em [research/STATUS_INVESTIGACAO.md](research/STATUS_INVESTIGACAO.md). Teste de bot não é teste de jogador humano. Bibliografia não é teste de código.

## 7. Decisões técnicas já orientadas por pesquisa — sem congelar números

- **Itens:** aprender com a retirada dos míticos em 2024; reforçar kits/identidade em vez de dar poderes universais obrigatórios [Riot /dev 2024](https://www.leagueoflegends.com/en-us/news/dev/dev-2024-item-changes/).
- **Balanceamento:** aumentos de HP alteram sustain, torres e lutas; revisar dados de combate como conjunto [Patch 12.10](https://www.leagueoflegends.com/en-us/news/game-updates/patch-12-10-notes/).
- **Mapa:** geometria e timing de waves mudam gank/snowball; validar simetria de tempo [Map Changes 2024](https://www.leagueoflegends.com/en-us/news/dev/dev-season-2024-map-changes/) e [13.10](https://www.leagueoflegends.com/en-us/news/game-updates/patch-13-10-notes/).
- **Ataques:** a [auditoria de C# open source](research/06_AUDITORIA_CODIGO_ABERTO_MOBA_CSHARP_UNITY.md) estudou estados Move→BeforeAttack→AfterAttack, stats por componentes e torre com projétil, sem importar nem compilar código externo.
- **Ki:** o código atual concede 1,8 XP/s com C após Ki cheio e 0,99 XP/s após 30 s contínuos; ganho por minuto precisa ser confrontado com XP/min de farm real, não reajustado por intuição.
- **Arquitetura:** manter simulação C# independente e adaptar Unity. Outras estratégias (DOTS, ECS, NavMesh, flow fields) dependem de benchmark, licença e integração comprovada.

## 8. Pendências de investigação antes de declarar especificação definitiva

1. Visão/fog/wards e quais informações bots realmente podem consultar.
2. Livro de fórmulas de progressão por campeão, incluindo HP, regen, Ki, attack speed, resistências e tempo de partida.
3. Dano, CC/tenacidade, shield, life steal, vamp, on-hit, crítica/haste/penetração por **patch e exceção**; calibragem própria.
4. Pathfinding completo e comportamento de obstáculos, multidão, dash, teleporte e hitboxes de Oozaru.
5. Economia de tropa/objetivo, fluxo de last hit, late game, comeback e duração desejada; validar por simulação/usuários.
6. Arquitetura detalhada de servidor, snapshots, replay, reconciliação e latência para um 5v5 futuro.
7. Catálogo/qualidade/licença dos assets de cada variante, rig, VFX e sons.
8. Mais papers integrais e revisão crítica de causalidade, amostragem e limites; fontes abertas podem não representar Riot 2026.
9. Testes humanos de UX de câmera, seleção, loja e entendimento de dano.
10. Avaliação de direitos de uso de franquias no eventual lançamento.

**Progresso de pesquisa ≠ implementação de gameplay.** Ver [STATUS_ATUAL.md](STATUS_ATUAL.md) para versão real e [REVISAO_DOCUMENTAL_2026-10-17.md](REVISAO_DOCUMENTAL_2026-10-17.md) para conflitos identificados.
