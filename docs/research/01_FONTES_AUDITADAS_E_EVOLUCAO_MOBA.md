# Pesquisa MOBA v2 — Fontes auditadas, evolução do LoL e interpretação crítica

**Data de corte:** 16/10/2026. **Status:** pesquisa documental; não é implementação. **Escopo:** fontes oficiais Riot, engenharia Riot/Unity, papers de conferências/periódicos e repositórios GitHub. **Método:** conferir título, autoria, data, conteúdo acessível, escopo real do estudo e limitações. **Leitura acadêmica:** quando o acesso disponibilizou apenas resumo, isso está indicado; não afirmamos leitura integral de conteúdo fechado.

## 0. Critério de evidência: o que realmente sabemos?

- **FO (fonte oficial primária):** páginas da Riot que explicam a mecânica, filosofia ou mudança de patch; só afirmar que é atual quando se referir explicitamente à versão consultada.
- **ET (engenharia técnica primária):** textos escritos por engenheiros da Riot sobre funcionamento histórico de seus sistemas. São arquiteturas **parciais e datadas**, não um acesso ao código-fonte completo nem documentação da versão 2026.
- **AC (acadêmico):** estudos com objeto/método próprios; distinguir experimentos com LoL de Dota 2, MOBAs móveis, RTS ou jogos de luta.
- **DE (documentação de engine):** Unity sobre API/capacidade técnica. Não garante que um pacote seja compatível com o projeto sem teste.
- **OS (repositório aberto):** README/licença encontrados, sem prova automática de build, qualidade ou licença de todos os assets.
- **IP (interpretação/proposta):** decisões nossas motivadas pela pesquisa, mas que **não são regras oficiais do LoL**.
- **PO (projeto observado):** código/fonte versionado no repositório Dragon Ball MOBA, branch identificada.

**Ponto de rigor:** não existe prova de que tenhamos “tudo sobre o LoL”. A Riot não publica o código completo do servidor. Detalhes exatos de mecânicas podem mudar por patch, região, modo e campeão. Especificaremos contratos originais testáveis.

## 1. Leitura cronológica — o que a Riot aprendeu com o jogo

| Período | Documento auditado | Evidência direta | Aplicação e risco |
|---|---|---|---|
| 2017 | **[ET1] Determinism / Unified Clock** | A Riot descreve relógios concorrentes, gravação de inputs/reprodução e não uso de servidor fixo estrito naquela arquitetura; usa delta medido e limite de taxa. | Nosso tick fixo 30Hz é opção própria, não cópia do LoL. Registrar inputs/seeds e checar replay. |
| 2019 | **[FO2] Champion Balance Framework** | Quatro públicos: médio, avançado, elite e profissional, com métricas distintas; números apresentados no texto referem-se ao framework **de 2019**, não a limites oficiais eternos. | Dados pequenos de bots não equivalem à avaliação de jogadores experientes; planejar segmentação. |
| 2020 | **[FO3] Preseason Item Shop Update** | Recomendações ligadas ao que o herói já possui e ao estado da partida; ícones desenhados para silhueta/legibilidade em tamanho pequeno. | Loja com recomendações explicáveis, comparador, receitas e ícones legíveis. |
| 2020 (10.23) | **[FO4] Ability Haste** | Aceleração de habilidade substitui CDR com teto tradicional; 50 haste ≈ 50% mais casts no mesmo intervalo-base. | Fórmula proposta `CD=CDbase*100/(100+haste)`; não chamar item de haste equivalente a reduzir 50% do CD. |
| 2021 | **[FO5] Counterplay** | Respostas táticas (esquivar, cancelar, posicionar) e estratégicas (itens, visão, rota); fraquezas não equivalem a derrota automática. | Cada R/Oozaru/feixe precisa de preparação legível e resposta possível. |
| 2021 | **[FO6] Clarity in League** | Silhuetas, paletas e VFX devem indicar área de acerto e identificação, mesmo entre skins. | VFX de Ki e transformações jamais mascaram hitbox e time rival. |
| 2022 (12.10) | **[FO7] Durability Update** | A Riot elevou sobrevivência base de forma sistêmica e recalibrou heals/shields/sustain; esperava lutas mais longas e espaço para execução. | Alterar HP e regen sem ajustar itemização/TTK cria desequilíbrio; mexer em conjuntos de sistemas. |
| 2023→2024 | **[FO8] 2024 Item Changes** | A Riot considerou que míticos estavam absorvendo poder excessivo e contornando fantasia do herói; distinguiu itens que favorecem o **padrão de jogo** dos que automatizam seu resultado. | Dragon Ball depende da identidade de Goku, Cell e formas; espadas/armaduras ampliam estilo, não apagam diferenças. |
| 2023→2024 | **[FO9] 2024 Map Changes** | Ajustes procuram paridade de lados, proteção de rotas solos, ganks e disputas de objetivos com várias entradas. | Medir tempo de gank e rota para cada lado; desenhar mapa por grafo antes de aplicar cenário. |
| 2025 | **[FO10] 2025 Season** | Mudanças em incentivos, disputas e objetivos refletem efeitos de snowball e dinâmica macro. | Balancear recompensa e timing do objetivo, e não apenas aparência/HP do monstro. |
| 2025 | **[ET2] Tech Behind Swarm** | Riot descreve custo de A* por minion, flow fields para hordas, wave data-driven e carga de partículas. | Para 3 rotas com dezenas de minions, waypoint/A* pode bastar; flow field merece benchmark apenas se escala justificar. |
| 2026 (26.1) | **[FO11] Patch 26.1** | Missões de função, alterações estratégicas e recompensas especiais; conclusão de missão de top amplia teto daquele papel para 20. Itens/críticos também mudam no patch. | O teto 18 do nosso jogo continua válido; **não** presumir que o LoL inteiro segue nível 18 sem exceções em 2026. |
| 2026 (26.20) | **[FO12] Patch 26.20** | Continuidade da manutenção por patch e mudanças específicas de gameplay. | Versionar `BalanceProfile` e registrar motivo da atualização; conferir conteúdo exato ao propor equivalência. |

### 1.1 Um exemplo de erro metodológico que corrigimos

O documento anterior dizia “artigos científicos sobre como o LoL evoluiu” e citava trabalhos sobre **Dota 2**, **RTS** e **simuladores em grid** como se fossem comparáveis diretamente. São estudos úteis sobre métodos de avaliação, mas **não mostram causa e efeito da evolução do LoL**. A evolução histórica do LoL precisa ser ancorada em changelogs e artigos retrospectivos da Riot; papers ajudam a desenhar métricas, experimentos e cautelas estatísticas. O ciclo observado é `hipótese → mudança sistêmica → dados → resposta do jogador → patch/rollback`. Isso não é uma afirmação de que um artigo isolado provou a estratégia ideal.

### 1.2 Histórico crítico de itens

A Riot publicou em 2020 um redesenho de loja e recomendação; em 2021 já mediu diversidade de escolhas de míticos; em 2023 explicou por que decidiu retirar míticos para 2024. A lição **não** é replicar um sistema mítico que já foi abandonado: construir classes de item com alternativas e custos distintos. O indicador “um mesmo mítico em 75% de jogos” foi uma **meta de design contextual de 2021**, não um benchmark universal aplicável ao nosso projeto com três heróis e poucos playtests.

### 1.3 Histórico crítico de mapa

A Riot descreve que diferentes lados tinham vantagens de gank, posicionamento, brush e objetivos. Reposicionar um arbusto pode alterar o tempo de reação. Para nosso projeto:

1. Definir terreno navegável, paredes e brush como **dados da simulação**.
2. Medir distância percorrível base–rota, tempo de ataque a partir da selva e possíveis emboscadas por lado.
3. Testar pelo menos três arquétipos (curta distância, range, móvel).
4. Ajustar geometria e indicadores **antes** de vegetação/art final.
5. Evitar copiar o desenho geométrico exato, logos, skins ou assets do Summoner's Rift.

## 1.4 Postmortem oficial: o que aconteceu APÓS o lançamento de 2025

**[FO14] Riot — /dev: How the Season 1 Changes Landed (07/02/2025)**, [fonte oficial](https://www.leagueoflegends.com/en-us/news/dev/dev-how-the-season-1-changes-landed/). Este texto é particularmente relevante porque compara **intenção, resultados reais e correções**:

- **Feats of Strength:** o primeiro abate como meta gerou percepção de aleatoriedade e pressão individual. A Riot mudou para **3 eliminações somadas da equipe** no patch 25.S1.2. *Aplicação:* recompensas de missão não devem depender demais de uma única jogada irreversível; exigir contribuição coletiva e possibilidade de recuperação.
- **Snowball:** a Riot divulgou que a chance de vitória após primeira torre foi **70,4% (14.24) versus 70,3% (25.S1.2)**. Isso é **associação** entre primeiro objetivo e vitória, não impacto causal isolado do sistema, pois várias coisas mudaram simultaneamente. *Aplicação:* além de olhar quantidade de kills, comparar taxa de comeback e probabilidade de vitória condicionada à vantagem.
- **Atakhan:** intenção de nascer em duas formas com distribuição alvo de **50–70% de Ruinous**; resultado inicial foi **~85% Ruinous / 15% Voracious**, levando à correção do spawn. *Aplicação:* objetivos dinâmicos exigem telemetria de frequência, posição, tempo e impacto, com distribuição-alvo especificada.
- **Mapa/arte:** feedback de cenário excessivamente escuro e cansativo levou a ajustes planejados, apesar de ausência de grandes incidentes de clareza percebidos internamente. *Aplicação:* legibilidade visual exige testes com humanos, resolução e contraste — não apenas screenshots bonitas.
- **Adaptação:** a Riot ressalta que os dados de mudanças sistêmicas evoluem ao longo de dias/semanas enquanto jogadores aprendem. *Aplicação:* coletar dados em janela de adaptação e não reverter uma alteração pelo primeiro dia sem examinar riscos graves.

**Status:** leitura da página completa feita; é um relato de desenvolvimento da própria Riot, **não estudo experimental independente**. Data e números se referem ao começo de 2025.

## 1.5 Ritmo de waves, torres e composições — fontes oficiais que mostram efeitos cruzados

- **[FO15] Patch 13.10 (16/05/2023)** — [fonte oficial](https://www.leagueoflegends.com/en-us/news/game-updates/patch-13-10-notes/): a Riot ajustou a coincidência temporal de waves no mid e laterais para aumentar o custo do roam e reduziu a troca de alvo de minions durante tower dives. **Inferência de design:** variar timing de waves muda segurança de ganks, controle de lane e economia, mesmo sem tocar no dano dos personagens. Uma mudança na loja obrigou compensações nas estatísticas de campeões após observar efeitos indiretos.
- **[FO16] Patch 26.1 (jan/2026)** — [fonte oficial](https://www.leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/): além das missões de função, a Riot aumentou a frequência de waves no mid/late game (25s a partir de 14:00; 20s a partir de 30:00), redistribuiu ouro/XP e modificou resiliência/ataque a torres. **Inferência de design:** acelerar waves sem revisar recompensas por unidade provoca inflação de XP/ouro. Essas temporizações são de modo/patch específicos, **não valores que vamos copiar cegamente**.
- **[FO17] Patch 14.18 (2024)** — [fonte oficial](https://www.leagueoflegends.com/en-us/news/game-updates/patch-14-18-notes/): fortificação de torre contra estratégias de lane swap em competição. **Inferência de design:** proteção de estruturas nos minutos iniciais pode moderar snowball, mas depende de padrões reais de mapa e função.

**Consequência arquitetural:** `WaveSchedule`, `TowerPolicy` e `RewardTables` devem ser conjuntos data-driven **versionados conjuntamente**. Ao alterar intervalo de spawn, executar testes de ganho de ouro/XP por minuto, carga de simulação, push/time-to-tower e exposição ao gank.

## 2. Engenharia primária: o que sabemos e o que não sabemos

### [ET1] Determinismo / relógio

Fonte de engenharia Riot publicada em 2017. Relata gravação de entradas por frame, quantização, relógios múltiplos e decisão por delta medido limitado em vez de fixed-step no servidor de então. **Não prova** qual é a arquitetura exata de todos os modos/servidores em 2026. Implicação aplicável: fonte de tempo autoritativa e testes reprodutíveis. Nossa implementação pode optar por fixed-step **desde que** meça desempenho/jitter, mantenha input-ordering e não use `Time.time` dentro do domínio.

### [ET2] Minions / movimento / física de grupo

A Riot diz explicitamente que LoL clássico usa A* para pathfinding; para o modo Swarm o custo ficou ruim com centenas de minions e optou-se por flow fields/dijkstra grids e simplificação. **Não significa** que o nosso MOBA deva usar flow fields na primeira fase. A mesma publicação descreve: autoritarismo de servidor, waypoints interpolados por cliente e servidor, wave spawns data-driven e efeito sobre performance de VFX. Para nosso protótipo, iniciar com 1 caminho/patrulha por rota, colisão consistente e profiler. Medir custo por unidade antes de arquitetar uma solução de escala massiva.

### [ET3] Data Dragon

A Riot expõe dados estáticos por versão: nomes, classes, imagens, `gold.total`, `gold.sell`, componentes `from` e produtos `into`, estatísticas e descrições. **Não fornece o código real do servidor nem a totalidade de regras de passivas/hitboxes.** Ferramenta de referência para **desenho de schema**, não copiar assets ou executar código LoL; sujeito a políticas de desenvolvedor. Dados podem ter atraso de atualização em relação ao cliente.

### [DE1] Unity 6, dados e navegação

A documentação Unity fornece AI Navigation/NavMesh, obstáculos dinâmicos e links; ScriptableObject guarda authoring fora da Hierarchy; Animator oferece root motion ou posição controlada por script; Mixer controla grupos, ducking e snapshots. Nada disso substitui testes de integração com Unity 6000.6.6f1 e nosso domínio offline.

## 3. Papers: leitura crítica, objeto e transferência

| Estudo (publicador/DOI) | Objeto empírico e acesso | O que podemos transferir | O que **não** demonstra |
|---|---|---|---|
| **[AC1] Champion Recommendation System of LoL** (IEEE ICTC 2020, DOI 10.1109/ICTC49870.2020.9289546) | Resumo publicado; dados profissionais, sequências de picks e bans | Draft/assistente de composição **quando houver dados** | Que nosso campeão vence outro por um valor fixo ou que uma rede neural é necessária agora. |
| **[AC2] CUPID** (ACM PACM HCI 2024, DOI 10.1145/3686978) | Resumo + dados editoriais; matchmaking de MOBAs, incluindo implantação em jogo MOBA móvel, não necessariamente LoL | Matchmaking futuro equilibrando nível e preferência de função; validar por A/B | Matchmaking pronto ou comprovadamente ótimo para nosso jogo offline. |
| **[AC3] Metagame Autobalancing** (IEEE CoG 2020, DOI 10.1109/CoG47356.2020.9231762) | Resumo; simulador pedra-papel-tesoura e **jogo de luta assimétrico** | Representar counters como grafo de forças e alvos de balanceamento | Resultados de balanceamento empírico de LoL. |
| **[AC4] Simulation-Driven Balancing** (IEEE ToG 2024, DOI 10.1109/TG.2024.3399536) | Resumo; arenas **tile-based de dois jogadores** e PCGRL | Métodos para testar viés de lados e conectividade em mapas experimentais | Que o mesmo algoritmo é ideal para nosso mapa contínuo Unity ou para 5v5. |
| **[AC5] Role-Sensitive Analysis** (IEEE Access 2025, DOI 10.1109/ACCESS.2025.3607328) | Resumo; ~20.060 partidas profissionais de LoL em 2023/24, teste independente | Métricas por papel (ofensa, defesa, lane, ouro), segmentação e avaliação | Que previsões estatísticas causam vitórias ou valem automaticamente para players casuais. |
| **[AC6] What makes an ideal team?** (Entertainment Computing 2023, DOI 10.1016/j.entcom.2022.100523) | Resumo; análise abstrata de tamanho de equipe e competição | 5v5 muda responsabilidade e coordenação, logo introduzir bots e cooperação passo a passo | Tamanho “correto” comprovado para Dragon Ball MOBA. |
| **[AC7] A Gospel for MOBA Game** (IEEE ToG 2022, DOI 10.1109/TG.2021.3123583) | Resumo; **Dota 2**, previsão de alteração de heróis baseada em grafo temporal | Versionar atributos e comparar meta antes/depois de patches | Evidência de mudanças efetivas da Riot no LoL. |
| **[AC8] Toward Automated Game Balance** (IEEE CoG 2021, DOI 10.1109/CoG52621.2021.9619032) | Resumo; **microRTS**, bots, XAI e análise de incerteza | Infra de simulação para detectar parâmetros que causam desbalanço | Balanceamento real de LoL/índice de satisfação de MOBA. |
| **[AC9] Player Skill Decomposition** (arXiv 2017, 1702.06253) | Resumo preprint; dados LoL + Dota 2 | Distinguir habilidade do jogador, domínio de campeão e stats do avatar | Que nossos bots validam habilidade humana. |
| **[AC10] Motivational Profiling of LoL Players** (estudo 2020, PMC7387664) | Resumo aberto; 750 jogadores LoL, perfis de motivação | Monitorar frustração, clareza, diversão e retorno, além de KPIs de combate | Que aumentar tempo de partida ou recompensas gera satisfação para todos. |
| **[AC11] MDA: A Formal Approach** (AAAI 2004) | Texto/abstract publicado; game design geral | Mecânica → dinâmica de equipe → sensação (ex.: C parado gera risco estratégico, não só XP) | Design específico e validado de economia LoL. |

### 3.1 Plano de estudos empíricos próprios

Estudos externos oferecem **método**, não a calibração final do nosso título. Vamos produzir:
- **Mapa:** 100–1.000 simulações por lado e composição fixa, distâncias e first-gank/first-tower; intervalos e mapas de calor.
- **TTK:** 3 arquétipos × 3 fases × 5 builds × 2 posições → pelo menos 90 cenários determinísticos antes de testes humanos.
- **Economia:** distribuição de Zeni/XP por kill, last hit, objetivo, fonte passiva e C; concentração e snowball.
- **UI:** pessoas tentam achar item de anti-cura e construir receita sem instruções; medir tempo, acertos e fricção.
- **Diversão/clareza:** entrevista curta pós-partida (o que matou você, qual resposta teria ajudado), sem assumir que winrate implica satisfação.
- **Bots vs humanos:** separar testes de invariantes de avaliação de game feel.

## 3.1 Pesquisa longitudinal DIRETA sobre patches de League of Legends

### [AC12] He et al. — Heterogeneous Effects of Software Patches in a Multiplayer Online Battle Arena Game (FDG 2021)

- **Publicação:** Foundations of Digital Games 2021, DOI [10.1145/3472538.3472550](https://doi.org/10.1145/3472538.3472550). [Artigo completo em HTML](https://arxiv.org/html/2110.14632v1), [registro arXiv](https://arxiv.org/abs/2110.14632).
- **Situação de leitura:** consultadas as seções do **texto integral acessível**, não apenas resumo. Os números abaixo descrevem o **recorte histórico 2014–2016** do artigo, não a temporada 2026.
- **Dados:** 1,2 milhão de jogadores únicos, aproximadamente 437 mil partidas; 62 versões entre 4.6 e 6.22. O recorte experimental foca 5v5 ranqueado, dados antes/depois da alteração e efeitos heterogêneos estimados por árvores causais.
- **Resultado:** mudanças podem atingir **diferentemente** jogadores de habilidades diversas, picks e composições. Mudanças indiretas em campeões que não foram alterados também podem mudar desempenho por sinergias/counters. O estudo relata ampliação da diferença de desempenho entre jogadores mais fortes e mais fracos após certas atualizações, e efeitos associados ao intervalo entre partidas.
- **Limitações de interpretação:** inferência causal observacional requer hipóteses sobre comparabilidade de partidas antes/depois; um resultado sobre `4.12`, `4.20` ou `6.9` não autoriza inferir magnitudes de 2026. O artigo **não** ensina código C# de MOBA nem prova que "nerf = winrate cai" em qualquer composição.
- **Aplicação testável:** ao alterar stats/habilidades/receitas, comparar `patch A vs B` com mesmo seed/bot, posição, composição, itemização e nível; depois segmentar jogadores reais por domínio da mecânica, campeões e período de adaptação. Não ajustar o jogo olhando somente winrate global.

### [AC13] Kwak — When the Rule-Maker Runs the World Championship (preprint outubro/2026)

- **Publicação:** [arXiv:2610.07427](https://arxiv.org/abs/2610.07427), enviado em 05/10/2026; **preprint exploratório**, não presumir revisão por pares.
- **Leitura efetuada:** resumo detalhado disponível; **não** declarar auditoria do método e resultados integrais como se tivessem sido reproduzidos.
- **Dados informados pelo autor:** dez campeonatos mundiais entre 2016–2025, 1.097 partidas, 215 observações time-ano e 1.090 alterações de campeões em 40 patches.
- **Achado:** as mudanças próximas de torneios afetam desigualmente as preferências históricas dos times; efeitos sobre vencer campeonatos permanecem **incertos** — a própria pesquisa enfatiza associação, não efeito causal demonstrado.
- **Aplicação:** `BalanceProfile` versionado; registrar em changelog quem foi afetado e por quê, permitir períodos de estabilização antes de eventos, medir exposição diferencial de jogadores/equipes. Para nosso MVP offline, isso é uma futura regra de governança, **não** bloqueio técnico imediato.

**Por que estes dois estudos importam mais para a pergunta "como o LoL foi mudando":** ambos examinam **alterações reais em LoL ao longo do tempo**, enquanto vários papers da tabela anterior usam Dota2, RTS ou arenas simplificadas para propor métodos. Essa diferença de evidência deve guiar as prioridades da pesquisa.

## 4. Código aberto: resultados de leitura de README e licenças

| Repo | Evidência verificada | Licença observada | Decisão |
|---|---|---|---|
| https://github.com/yasgamesdev/MOBA_CSharp_Unity | README informa cliente Unity 2018.3.4f1, servidor C# independente, ENet, MessagePack, SharpNav, buffs/visão/bush/itens configuráveis | **MIT** no arquivo LICENSE (copyright 2018 sem titular preenchido no texto encontrado) | Melhor candidato a **estudo de algoritmo e contratos**, mas precisa auditoria de dependências e teste independente. Não importar projeto inteiro. |
| https://github.com/DevZhav/unity-moba | README informa lobby, serviços, matchmaking e três mapas de exemplo | **GPL-3.0** no LICENSE | Referência arquitetural de fluxo; não incorporar código em repositório com política incompatível sem análise de obrigações. |
| https://github.com/bensalilijames/Clash | README informa cliente em C# com uLink; checklists deixam morte, minions, itens e loja incompletos | Diz MIT no README; arquivo LICENSE raiz **não encontrado na consulta** | **Não declarar licença auditada** nem chamar MOBA pronto; estudar apenas conceitos. |
| https://github.com/InFaNsO/MOBA_Unity | README curtíssimo: protótipo Unity 2019 | **MIT** no LICENSE | Exemplo antigo; pouco ganho frente à base atual. |
| https://github.com/Unity-Technologies/EntityComponentSystemSamples | Amostras oficiais Unity ECS, não um MOBA turnkey | Precisa auditar licença e versões dos subprojetos em uso | Benchmark/estudo de otimização, nunca migrar automaticamente. |

**Status técnico das cinco integrações:** nenhuma foi baixada, compilada ou integrada nesta pesquisa; **somente README/licença de quatro repositórios foram conferidos**. A licença de cada **asset** incorporado é questão separada da licença da base C#.

## 5. Fontes primárias: registro de rastreabilidade (URL e classificação)

- **FO1** — LoL How to Play: https://www.leagueoflegends.com/en-us/how-to-play/
- **FO2** — Riot Champion Balance Framework (30/05/2019): https://www.leagueoflegends.com/en-us/news/dev/dev-champion-balance-framework/
- **FO3** — Preseason Item Shop Update (18/09/2020): https://www.leagueoflegends.com/en-us/news/dev/preseason-item-shop-update/
- **FO4** — Patch 10.23, Ability Haste (2020): https://www.leagueoflegends.com/en-us/news/game-updates/patch-10-23-notes/
- **FO5** — Champion Counterplay (14/05/2021): https://www.leagueoflegends.com/en-us/news/dev/quick-gameplay-thoughts-may-14/
- **FO6** — Clarity in League (12/03/2021): https://www.leagueoflegends.com/en-us/news/dev/clarity-in-league/
- **FO7** — Durability Patch 12.10 (24/05/2022): https://www.leagueoflegends.com/en-us/news/game-updates/patch-12-10-notes/
- **FO8** — 2024 Item Changes (29/11/2023): https://www.leagueoflegends.com/en-us/news/dev/dev-2024-item-changes/
- **FO9** — 2024 Map Changes (23/11/2023): https://www.leagueoflegends.com/en-ph/news/dev/dev-season-2024-map-changes/
- **FO10** — 2025 Season Preview: https://www.leagueoflegends.com/en-us/news/dev/dev-2025-season-one-gameplay-preview/
- **FO11** — Patch 26.1 Notes (07/01/2026): https://www.leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/
- **FO12** — Patch 26.20 Notes (out/2026): https://www.leagueoflegends.com/en-us/news/game-updates/league-of-legends-patch-26-20-notes/
- **FO13** — 2021 Item Choice Deep Dive (contextual): https://www.leagueoflegends.com/en-us/news/dev/quick-gameplay-thoughts-feb-26/
- **FO14** — Postmortem temporada 2025: https://www.leagueoflegends.com/en-us/news/dev/dev-how-the-season-1-changes-landed/
- **FO15** — Patch 13.10 (minions + itens): https://www.leagueoflegends.com/en-us/news/game-updates/patch-13-10-notes/
- **FO16** — Patch 26.1 (waves/economia): https://www.leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/
- **FO17** — Patch 14.18 (torres): https://www.leagueoflegends.com/en-us/news/game-updates/patch-14-18-notes/
- **ET1** — Determinism / Unified Clock (2017): https://technology.riotgames.com/node/73 e https://www.riotgames.com/en/news/determinism-league-legends-implementation
- **ET2** — The Tech Behind Swarm (2025): https://www.riotgames.com/en/news/the-tech-behind-swarm
- **ET3** — Riot Developer Portal, Data Dragon e política de IP: https://developer.riotgames.com/docs/lol
- **DE1** — Unity 6 AI Navigation: https://docs.unity.cn/6000.2/Documentation/Manual/com.unity.ai.navigation.html
- **DE2** — Unity 6 ScriptableObjects: https://unity.com/resources/create-modular-game-architecture-scriptableobjects-unity-6
- **DE3** — Unity 6.6 Audio Mixer: https://docs.unity.com/en-us/engine/6000.6/manual/audio/mixer
- **DE4** — Unity root motion: https://docs.unity.com/en-us/engine/6000.3/manual/animation-section/animation-mecanim/avatar-creationand-setup/root-motion
- **DE5** — Unity Profiler: https://docs.unity.com/en-us/engine/6000.3/manual/analysis/profiler
- **AC1** — https://ieeexplore.ieee.org/document/9289546/
- **AC2** — https://doi.org/10.1145/3686978
- **AC3** — https://ieeexplore.ieee.org/document/9231762/
- **AC4** — https://ieeexplore.ieee.org/document/10528885/
- **AC5** — https://ieeexplore.ieee.org/document/11153635/
- **AC6** — https://doi.org/10.1016/j.entcom.2022.100523
- **AC7** — https://scholarworks.brandeis.edu/esploro/outputs/journalArticle/A-Gospel-for-MOBA-Game-Ranking-Preserved/9924144022401921
- **AC8** — https://ieeexplore.ieee.org/document/9619032
- **AC9** — https://arxiv.org/abs/1702.06253
- **AC10** — https://pmc.ncbi.nlm.nih.gov/articles/PMC7387664/
- **AC11** — https://aaai.org/papers/ws04-04-001-mda-a-formal-approach-to-game-design-and-game-research/
- **AC12** — https://arxiv.org/html/2110.14632v1 e https://doi.org/10.1145/3472538.3472550
- **AC13** — https://arxiv.org/abs/2610.07427

## 6. IP e publicabilidade

O código/arte de Riot **não é open source por default**. O portal de desenvolvedores proíbe desenvolver jogos usando IP da Riot e restringe produtos que se assemelhem demasiadamente aos seus jogos, dentro das políticas aplicáveis aos desenvolvedores da plataforma. Isso **não impede estudar padrões de design gerais**, mas reforça criar mapa, ícones e modelos próprios e evitar redistribuir assets/engenharia reversa. Do mesmo modo, Dragon Ball é uma franquia de terceiros: **o repositório público e um protótipo privado não substituem autorização** para distribuir personagens/vozes/marcas. Planejar uma linha de arte original, licença apropriada ou consulta jurídica antes de eventual publicação comercial.

## 7. Revisão crítica do documento v1

Arquivo anterior: `docs/specifications/MOBA_RESEARCH_BLUEPRINT_2026.md`. Ele permanece como **hipóteses de design e roteiro preliminar**, não uma afirmação de que todos os papers foram lidos integralmente ou de que todos os links/compatibilidades de open source foram validados.

As correções editoriais deste v2 prevalecem especialmente em: (i) arquitetura do relógio LoL, (ii) papers não-LoL, (iii) status de licença Clash, (iv) dados Riot/Data Dragon, (v) papéis e teto de níveis de 2026, (vi) permissões de IP e (vii) o que é teste Unity versus proposta.
