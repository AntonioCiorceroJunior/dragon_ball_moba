# Dossiê 07 — Estudos longitudinais e protocolo de balanceamento experimental

**Data:** 2026-10-16 · **Método:** texto integral de um estudo específico de LoL (He et al., 2021) e relatório postmortem original da Riot (2025). **Não é revisão sistemática exaustiva de todos os papers de MOBAs**, nem teste estatístico real do nosso jogo.

## 1. Paper lido diretamente, além do abstract

**Yuzi He, Christopher Tran, Julie Jiang, Keith Burghardt, Emilio Ferrara, Elena Zheleva e Kristina Lerman (2021)**, [Heterogeneous Effects of Software Patches in a Multiplayer Online Battle Arena Game](https://arxiv.org/html/2110.14632v1), arXiv:2110.14632. O documento HTML permite acessar introdução, dados, métodos, resultados e discussão. Esta leitura se restringe ao paper publicado e aos resultados narrados pelos autores; **não reproduz suas árvores causais com o dataset**.

### 1.1 Pergunta e desenho

Pergunta: atualizações de software e balanceamento do LoL afetam igualmente jogadores, campeões e estratégias, ou o efeito depende do contexto?

Dataset descrito no artigo: meados de 2014 até fim de 2016, aproximadamente 1,2 milhão de jogadores únicos e 437 mil partidas, cobrindo patches 4.x a 6.x. Dados incluem resultado da partida, campeão, papel/rota, kills/deaths/assists, ouro e data/versão. **Números se aplicam ao dataset histórico, nunca à base completa do LoL 2026.**

Método: tratamento = antes versus depois do patch, resultado = win/loss ou métricas de performance. Para estudar heterogeneidade, os autores usam **causal trees** para separar subgrupos com efeitos distintos. O raciocínio exige comparabilidade do antes/depois e cuidado com fatores que mudam junto com o patch.

### 1.2 O que a análise mostra (e o que não mostra)

- Exemplos do artigo incluem efeitos condicionais nas versões 4.12, 6.4, 4.20 e 6.9.
- O paper descreve interação entre alterações em Lucian, outros campeões e composição (por exemplo, Nami com Lucian). Ou seja, buffar um campeão pode aumentar ou diminuir o resultado de outro sem que o segundo tenha recebido patch.
- No estudo, o efeito de ter Lucian e Nami juntos pode diferir muito do efeito agregado de Nami sozinha. Alguns subconjuntos carecem de evidência estatística forte, como associação com Rengar no exemplo apresentado (p=0.11).
- O paper também relaciona desempenho após patches ao nível de habilidade e ao intervalo entre partidas; mudanças não impactam todos os segmentos igualmente.
- **Não demonstra** que todos os patches modernos agem de modo semelhante, nem que algum algoritmo do artigo determina os melhores valores de um item ou skill.

**Lição de engenharia para nosso MOBA:** não usar somente winrate global nem DPS isolado para declarar balanceamento. O que importa é a distribuição por matchup, build, fase, posição, lado e domínio do personagem, e a evolução após o patch. Um resultado em 30 simulações de IA não é evidência de preferência humana.

### 1.3 Transferência à matriz de counters do Dragon Ball MOBA

Ao desenhar Goku Criança, Goku Super e Cell:
- Goku Super pode vencer Cell em burst curto mas perder luta longa por sustain; item anti-cura muda o matchup.
- Goku Criança com Oozaru grande pode vencer fight em área, mas sofrer kite de carry ranged; corredor estreito pode impedir transformação.
- Item de resistência de Ki pode conter Freeza, porém talvez enfraqueça versus atacante físico; não combinar ambas resistências no mesmo item com custo baixo demais.
- A presença de suporte de escudo muda a eficácia de ataque com life steal (no nosso sistema proposto, sustain só ocorre por HP realmente perdido), portanto um matchup não é binário e muda por composição.

São **hipóteses de design**, não resultados do paper sobre nosso jogo.

## 2. Postmortem Riot de 2025 — intenção versus dados observados

Fonte oficial: [/dev: How the Season 1 Changes Landed](https://www.leagueoflegends.com/en-us/news/dev/dev-how-the-season-1-changes-landed/), 7/2/2025, editado com dados coletados até fim de janeiro. É documentação retrospectiva primária Riot, **não experimento independente randomizado**.

### 2.1 Sistema de recompensas

A Riot introduziu Feats of Strength visando dar estrutura à fase inicial e adiar parte do poder do first blood/first turret. O First Blood foi considerado aleatório/pressionador; a Riot alterou o critério para **três kills coletivas** no patch 25.S1.2. Também corrigiu itens de botas considerados fortes demais por ajustes de stats e custo.

**Aplicação:** missões/recompensas devem favorecer atividades de equipe e admitir ajustes sem refazer código, com lever separado de custo e poder. Uma missão de Dragon Ball que dependa do **primeiro golpe do jogo** geraria frustração análoga; preferir recompensa por contribuição distribuída/objetivos alcançáveis.

### 2.2 Snowball e correlação

Winrate condicional a first turret, nos dados do relato: patch14.24 **70,4%**, patch25.S1.2 **70,3%**. Condicional a first blood: **57,6%** versus **55,4%**. A proximidade não prova que Feats não tenham impacto causal: outras alterações simultâneas (XP range, homeguards, recompensa etc.) afetam o resultado. Como projeto técnico, guardaremos **registros por versão** e evitaremos atribuir causalidade sem controle experimental.

### 2.3 Eventos e spawn

Atakhan teve distribuição-alvo de aproximadamente **50–70%** Ruinous. A Riot observou aproximadamente **85%/15%** Ruinous/Voracious em partidas normais e corrigiu taxas; distribuição top/bot ficava mais próxima da intenção, ~44%/56%. Isso demonstra problema clássico: hipóteses sobre condições de spawn não garantem distribuição real após jogadores adaptarem comportamento.

**Aplicação original:** se nossos objetivos dinâmicos surgirem segundo número de mortes, torres ou dano, medir frequência por fase e posição e manter limites/fallbacks. Não supor que regras matemáticas de spawn produzirão distribuição desejada em partidas humanas.

## 3. Por que mudar um stat nunca é ação isolada

Exemplo de nossas futuras mudanças combinadas:

**Hipótese:** Goku Super tem dano explosivo alto e elimina Cell rápido demais. Aumentar HP de Cell em +15% pode melhorar TTK, mas:
- Eleva o valor relativo de HP regen fora do combate;
- Melhora passivas de cura com coeficientes de vida máxima;
- Prolonga tempo de torre;
- Aumenta tempo necessário para obter farm/XP no matchup;
- Pode reforçar item defensivo de forma desproporcional;
- Muda valor do burst de Ki e da penetração;
- Altera recompensa do item anti-HP máximo.

Por isso, o update sistêmico de durabilidade 12.10 da Riot também enfraqueceu curas/escudos/sustain e monitorou retornos por campeões: [Patch 12.10](https://www.leagueoflegends.com/en-us/news/game-updates/patch-12-10-notes/). O ajuste por nível deve ter **orçamento de poder** e testes de TTK e sustain.

## 4. Especificação de um BalanceLab independente da Unity

Entradas:
- BalanceVersion e MapVersion congeladas;
- ChampionIds, FormIds, skills/ranks, level, inventários, AI policy;
- Side, seed, posição inicial, táticas declaradas;
- Tempo máximo e cenário (1v1, 2v2 com suporte, torre, objetivo).

Saídas por match:
- vencedor, duração, TTK por tipo de adversário, dano físico/Ki/verdadeiro;
- dano mitigado por armadura/KiResist, escudos, cura, lifesteal, críticos, TTK;
- XP, ouro, ações C, tempo parado, itens, fontes de vantagem, torres, rota e lado;
- eventos duplicados/rejeitados, colisões, path failures, CPU/tick, GC.

**Controle pareado:** executar mesmos seeds/composições/estratégia com parâmetros A e B, mantendo mapa e AI congelados. Usar experimentos adicionais por combinação, não somar 10 mudanças simultâneas e interpretar a diferença como efeito de uma única habilidade.

### 4.1 Pré-registro de hipótese e efeito esperado

Exemplo:
- Antes: Goku Super vence 8/10 cenários fixos contra Cell e TTK mediano é 3.2s (NÚMEROS FICTÍCIOS PARA ILUSTRAR).
- Proposta: reduzir razão de Ki de Q em 12%; previsão: TTK passa >3.8s nos mesmos cenários e vitória fica menos concentrada.
- Critério: diferenças por seed e phase sem regressão de skill/rank, tower behavior e escudos; nenhuma habilidade passa a falhar em hitbox.
- Experimento com bots não afirma **winrate humana**. Se diferença for pequena perto da variância, aumentar amostra.

### 4.2 Cortes e métricas

| Corte | Métrica | Por que importa |
|---|---|---|
| Tier/rank do jogador | winrate com intervalo de incerteza, picks e bans | iniciantes podem sofrer com kits complexos que experts dominam |
| Composição e counter | TTK, sobrevivência, contribuição de suporte | nerf no carry afeta outros heróis via sinergia |
| Fase da partida | nível, ouro, dano, lutas, objetivos, tempo de torre | item escala mais forte no late |
| Lado/rota | ganks recebidos, tempo ao objetivo, taxa vitória | mapa pode favorecer lado apesar de arte simétrica |
| Build | taxa de escolha e ganho marginal de cada item | “slot obrigatório” indica falta de diversidade |
| Antes/depois do patch | número de partidas após a mudança | adaptação não é instantânea |
| Saúde técnica | replay diverge? hitbox/visual coincidem? CPU/Tick | bug altera winrate e mascara balanceamento |

## 5. Publicação responsável e rigor de evidência

Cada relatório de balanceamento conterá:
- Hipótese, parâmetro(s), versão A/B e motivo da intervenção;
- Links do código, build, scripts, amostras e seeds;
- Métricas de efeito, erro/intervalo e cenários sem alteração;
- Matchups e fases prejudicadas e beneficiadas;
- Testes humanos (se existirem) separados de bot-bot;
- Decisão Keep/Rollback/Retest, com gatilho de rollback;
- Registro de limitações e riscos de IP/arte/performance.

## 6. O que ainda falta pesquisar

- Outro estudo longitudinal com dados de itens/composições e patches de LoL, de preferência integral e revisado;
- Evidência sobre efeitos de matchmaking e comunicação de equipe no aprendizado de mecânicas;
- Quantificação do tempo de decisão na loja e percepção de VFX/counters;
- Simulação de partida longa com o nosso próprio balanceamento e feedback humano;
- Revisão de bugs/alterações por temporada 2020–2026 numa **matriz por mecânica**, não apenas nomes dos patches.

Este dossiê constitui **leitura detalhada de um paper integral e de um postmortem**, não uma revisão sistemática exaustiva. A validação empírica do Dragon Ball MOBA ainda está pendente.
