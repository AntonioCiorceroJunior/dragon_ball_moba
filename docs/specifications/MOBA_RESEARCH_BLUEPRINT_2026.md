# Dragon Ball MOBA — Pesquisa de LoL, arquitetura de sistemas e plano de produção

**Versão:** 1.0 (pesquisa e proposta) · **Data de corte:** 16/10/2026 · **Idioma:** PT-BR  
**Estado:** documento técnico para revisão; **não representa funções já implementadas**.  
**Projeto ativo:** `src/client-unity`, Unity 6000.6.6f1, Built-in, Input Manager legado.  
**Documentos relacionados:** [GAME_DESIGN.md](GAME_DESIGN.md), [TECH_ARCHITECTURE.md](TECH_ARCHITECTURE.md), [ART_PIPELINE.md](ART_PIPELINE.md), [coordenação](../coordination/README.md).

> **Tese de implementação:** preservar o protótipo e a simulação pura C# enquanto construímos, em etapas verificáveis, uma experiência completa de MOBA. O objetivo é reproduzir a **profundidade sistêmica** e a legibilidade de um MOBA do nível de League of Legends, não copiar seu código proprietário, mapa exato, arte, interface protegida ou balanceamento atual. Dragon Ball também é propriedade intelectual de terceiros: prototipagem privada não concede licença para publicação comercial.

## Sumário executivo e decisão

1. O projeto já possui uma especificação mestre extensa, com 15 variantes, receitas de itens, Oozaru, fusão, mapa de 3 rotas, Ki e arquitetura de simulação. **A lacuna principal é código e integração efetiva**, não falta de ideias. Este texto complementa o GAME_DESIGN com auditoria do estado de 2026, fórmulas operacionais, dependências de engenharia e critérios de aceite.
2. O que faz LoL funcionar é a **interação de sistemas**: ondas de tropas → ouro/XP → itens e níveis → controle de terreno → visão e objetivos → decisões em equipe. Não basta juntar dezenas de efeitos isolados.
3. A prioridade é uma partida offline completa, com **3 heróis distintos, combate estável, progressão visível, loja de receitas e mapa navegável**, antes de 15 personagens, fusão e multiplayer.
4. **DBM-001 foi mesclada à main**: pontos/ranks de habilidades, requisito R 6/11/16 e autoataque de um clique. **DBM-002 é apenas PR #2 em rascunho**, contendo colisões de torres/núcleos e aggro provisório; não considerá-la concluída antes dos testes locais.
5. Para o Ki em C: hoje o código gera **1,8 XP/s após Ki cheio, caindo para 0,99 XP/s após 30 s contínuos**, fora da fonte. Como os requisitos de XP aumentam por nível, o benefício fica cada vez menos perceptível. Precisamos medir e mostrar XP/s, comparar com farming, e recalibrar — sem criar um botão de evolução AFK dominante.
6. O código atual inicia por uma tela simples de **3 botões**, mas o componente visual pode iniciar automaticamente conforme configuração de cena (`OrganicArena.startDemo`). A seleção deve tornar-se um **estado obrigatório antes da partida**, com retratos, kit, funções e prévia das formas, não apenas botões.
7. Toda afirmação sobre LoL deve ser entendida **no patch referenciado**. Em 2026, a Riot alterou missões de posição e recompensas; por exemplo, a atualização 26.1 prevê aumento do teto de nível do top para 20 após uma missão. Não congelar as regras do nosso jogo por números desatualizados de LoL.

## 1. Fontes, metodologia e limites da pesquisa

**Categorias de evidência:**

- **[R] Riot / fontes oficiais:** regras declaradas do LoL e, principalmente, o raciocínio de design e mudanças em patches. Fonte primária, mas não disponibiliza seu projeto completo/código de gameplay.
- **[T] Técnica:** publicações da Riot Technology, documentação Unity 6 e repositórios open source; mostram práticas/possibilidades, **não comprovam compatibilidade automática** com nosso projeto.
- **[A] Acadêmica:** artigos revisados por pares e conferências sobre balanceamento, equipes, previsão de partida e matchmaking; dão hipóteses e métodos, não receitas prontas nem evidência de que um valor numérico específico é ideal.
- **[P] Projeto:** código e relatórios do GitHub, diferenciando claramente especificado, implementado, compilado e testado.

**Método:** cruzar documentação oficial [R1–R11], técnicas [T1–T6], estudos [A1–A7] e leitura do repositório [P1–P4]; identificar princípio, traduzir para mecanismo próprio e especificar teste. **Números do nosso protótipo neste documento são propostas de balanceamento, nunca alegações de que são números do LoL.** Não foi realizada engenharia reversa do cliente de LoL e não há acesso ao seu código de servidor. Patches futuros podem alterar qualquer mecânica.

## 2. O que aprender com a evolução de League of Legends

| Momento / fonte | Mudança / motivo declarado pela Riot | Regra que devemos extrair |
|---|---|---|
| 2019: estrutura de balanceamento [R6] | Avaliar desempenho por diferentes grupos de habilidade, presença em picks/bans e limites de intervenção | Nunca equilibrar um herói apenas pelo dano bruto ou pelos resultados de 5 partidas. |
| 2019: ajustes de selva [R11] | Recompensas, tempos de respawn e incentivos ao farm foram alterados para controlar a influência das rotas | Ritmo e economia dos monstros mudam toda a macroestratégia. |
| 2020: nova loja e ícones [R7] | Categorias, recomendações, acesso rápido, contraste e legibilidade de ícones pequenos | Um item deve ser encontrável e entendível em segundos, com receita e uso visíveis. |
| 2021: design de counterplay [R8] | Ações de resposta, alertas, vulnerabilidade e pontos fracos claros em cada kit | Toda técnica poderosa precisa de aviso e contrajogo tático/estratégico. |
| 2022: indicadores de evolução [R9] | Recomendações visíveis de qual habilidade evoluir para reduzir fricção com iniciantes | HUD deve ensinar progressão, sem escolher pelo jogador. |
| 2024: retirada dos míticos [R3, R4] | Itens estavam consumindo parcela excessiva do poder do personagem e reduzindo sua identidade | **Itens complementam kit e forma, não substituem as habilidades exclusivas.** |
| 2024: remodelação do mapa [R5] | Aproximar condições dos lados, revisar vulnerabilidade a ganks e múltiplas entradas de objetivos | Um mapa não é só desenho: cada parede e arbusto altera vantagem, rotação e risco. |
| 2025: feitos de força/recompensas [R10] | Recompensas podem ser adiadas e conectadas a objetivos para controlar snowball | Projetar cuidadosamente quando uma vantagem vira poder combatível. |
| 2026: missões de função [R2] | Missões e recompensas de top/mid/bot/jungle/suporte estimulam exercer sua função, com exceções de nível/slots | Função deve ser valorizada por comportamento, mas quests só depois de economia e mapa maduros. |
| 2026: patch 26.20 [R12] | Mudanças pontuais no desempenho de itens, redução de ruído no placar e ajustes de habilidades | Após lançar sistemas, manutenção e clareza constantes são parte do produto. |

**Lição central:** LoL muda por hipóteses, instrumentação, testes e revisão contínua, não por copiar uma lista eterna de valores. Projetar mecanismos para atualização por dados/versões desde agora.

## 3. A matriz de profundidade — mecânicas que uma partida precisa sustentar

| Subsistema | Comportamento que o jogador percebe | Dependências |
|---|---|---|
| Progressão | Mais HP/regen/ataque/defesa/velocidade de ataque por nível; skills escalam pelo rank | Stats, XP, habilidades, HUD |
| Combate | Ataque automático, hitbox coerente, crítico, dano físico e Ki, resistências, cura, escudo, life steal, CC | DamagePipeline, eventos, alvo, autoridade |
| Movimentação | Clique com caminho, desvio, alcance, attack-move, stop, kite, dash e diferentes escalas de corpo | Navigation, colisão, input, câmera |
| Economia | Last hit, ouro passivo, assistências, preços, receitas, estoque, venda, undo | Events, ItemCatalog, inventory |
| Loja | Ícones, categorias, comparações de atributos, build recomendada, receita com componentes | Economy, UI, dados |
| Formas | Goku pequeno realmente menor, Oozaru grande/lento, mudança de rig/skills/hitbox | ChampionData, FormData, animações |
| Macrojogo | Top/Mid/Bot, selva, torres ordenadas, inibidores, objetivos, vision/fog e controle de waves | Mapa, IA, navegação, regras de vitória |
| Experiência audiovisual | Câmera jogável, HUD, barra/nome, sons legíveis, aura de Ki, feixes e golpes | EventBus e Presentation |
| Pré e pós-jogo | Seleção com retratos e previews, loadout, loading, vitória, placar e estatísticas | SessionFlow, catálogos, snapshots |
| Engenharia | Dados externos, simulação estável, replay, testes determinísticos, perf e eventual servidor | Domínio independente de Unity |

**Regra de design:** qualquer sistema adicionado tem estado, regra, sinal visual/sonoro, telemetria, testes automatizados e teste in-game. Criar asset sem conectá-lo à cena/jogo não é entrega.

## 4. Progressão, atributos e regeneração: contrato matemático

### 4.1 Atributos obrigatórios

Separar em quatro grupos, exibidos no painel de estatísticas (tecla a definir):

- **Sobrevivência:** `MaxHP`, `HPRegenPer5`, `Armor`, `KiResist`, `Shield`, `ShieldPower`, `Tenacity`, `HealAmplification`.
- **Ataque básico:** `PhysicalAttack`, `AttackSpeed`, `AttackRange`, `CritChance`, `CritMultiplier`, `ArmorPenFlat`, `ArmorPenPercent`, `LifeSteal`, `OnHitEffects`.
- **Técnicas:** `KiPower`, `AbilityHaste`, `KiPenFlat`, `KiPenPercent`, `SpellVamp`, `CooldownModifiers`.
- **Mobilidade/recursos:** `MoveSpeed`, `MoveAcceleration`, `TurnRate`, `MaxKi`, `KiRegenPer5`, `VisionRange`, `BodyRadius`, `ResourceType`.

**Dois tipos principais de dano**: físico (socos/espadas/bastão) e energético (Ki, equivalente funcional ao dano mágico). Dano verdadeiro deve ser raro, explicitamente sinalizado e não mitigado por Armor/KiResist. **Roubo de vida** aplica cura após **dano efetivamente causado** por ataques básicos elegíveis; **vampirismo de técnicas** aplica após dano elegível de habilidades, com coeficientes separados para área. Escudo absorve antes do HP; itens com passivas exclusivas não acumulam sem declaração.

### 4.2 Crescimento e pipeline de atributos (proposta)

A fórmula simples inicial deve ser `baseStat + growthPerLevel × (level - 1)`; não alegar que replica exatamente todos os campeões/patches do LoL. O resultado total por tick deve ser **recalculado a partir de fontes** — nunca adicionar o mesmo bônus por item duas vezes:

```text
Base (ChampionDefinition, nível)
 → FormModifiers (transformação atual)
 → ItemFlat / ItemPercent
 → PermanentTrainingBuffs
 → TemporaryBuffs / Debuffs / Auras
 → Caps, clamps, rounding policy
 → DerivedStatsSnapshot
```

Exemplo ilustrativo de **Goku Criança** (não é dado Riot): HP base 570, +82 HP/nível, regen base 6 HP/5s, +0,65 HP/5s por nível, ataque 55 +3,2/nível; velocidade base baixa/média, range curto. No nível 6, antes de itens: HP = 980; regen = 9,25 HP/5s ≈ 1,85 HP/s; ataque = 71. **Esses valores precisam de testes de TTK, não estão balanceados.** Diferenciar crescimento de HP máximo de cura: subir nível não deve necessariamente restaurar toda a vida, a menos que regra explícita.

### 4.3 Regeneração de vida base

```text
regenHpPerSecond = (HPRegenPer5 / 5) × (1 + percentRegenBonus)
heal = max(0, regenHpPerSecond × deltaTime)
HP = min(MaxHP, HP + heal)
```

- HP regen existe **desde nível 1**, inclusive sem item e sem voltar à fonte; não equivale a cura explosiva.
- Fonte aplica regeneração própria muito superior e regras de loja/segurança.
- Opcional: `OutOfCombatRegen` somente após N segundos sem dano (para itens específicos), separado da regeneração base.
- Cura recebida, passivas, anti-cura e escudos precisam de tipos/eventos distintos; regen não deve disparar efeitos de `OnSpellHeal` por acidente.
- Morte pausa a regen; respawn reseta HP conforme regra; nunca ultrapassar `MaxHP`.

### 4.4 Pipeline de dano — ordem autoritativa

```text
AttackOrCastRequest
 → validação de alvo, visão, alcance, vivo, custo, CC, rank
 → ataque/cast windup (pode cancelar conforme tipo)
 → HitResult (contém tags e tipo)
 → rawDamage = base[rank] + physicalRatio×Attack + kiRatio×KiPower
 → redução especial / bloqueio / esquiva legítima
 → penetração e resistência efetiva
 → mitigação (resistência >=0: 100/(100+resist))
 → escudo (tipado ou universal)
 → HP efetivamente perdido
 → life steal / spell vamp / efeitos on-hit / anti-heal
 → morte, kill/assist, ouro, XP, aggro de torre, VFX e replay
```

Técnicas e passivas devem especificar `DamageTags` (`BasicAttack`, `Ability`, `SingleTarget`, `Area`, `Dot`, `Physical`, `Ki`, `True`, `CanCrit`). **Regra de roubo de vida sugerida:** se ataque básico elegível removeu 100 HP, `LifeSteal=0.10` restaura até 10 HP ao atacante; se escudo absorveu tudo, não cura por esse dano. `SpellVamp=0.08` em habilidade elegível que removeu 200 HP cura 16 HP; habilidades de área podem ter fator de efetividade próprio (a calibrar).

**Mitigação de teste:** 100 de dano físico contra Armor 100 produz 50 antes de escudo. Se tiver 20% de penetração e 10 de penetração plana, resistência efetiva proposta `(100 × 0.8) − 10 = 70`; dano = `100 × 100/170 ≈ 58,82`. Definir ordem no código e testar para cada tipo de resistência. Resistência negativa usa a fórmula já presente em `Catalog.Mitigate`, com testes dedicados.

### 4.5 Cooldowns, velocidade de ataque e limites

- **Aceleração de habilidade proposta:** `CDfinal = CDbase × 100/(100 + AbilityHaste)`; +50 haste implica cooldown × 2/3. Não acumular redução diretamente em `SkillCooldown` quando item é equipado: usar `AbilityDefinition`, `DerivedStats` e evento de cast.
- `AttackInterval = 1 / AttacksPerSecond`; velocidade de ataque final tem limite e modificadores ordenados. Não permitir `0` nem número negativo; separar windup (animação antes do hit) do intervalo total.
- **Crítico ilustrativo:** chance 25%, dano ×1,75 em básico elegível; o número de 1,75 é **proposta para nosso jogo**, não afirmação geral sobre cada campeão do LoL.
- **Velocidade:** base × multiplicadores e clamp; slows múltiplos têm regra de empilhamento, limite e imunidades. Itens podem produzir desvantagens legítimas (ex.: botas de treino −8% antes do ganho), mas nunca dano/velocidade negativa por underflow.
- **Tempo:** simulação de 30Hz atual, com `tick` como relógio de efeito/duração. Não utilizar `Time.time` da Unity para regras de domínio; usar apenas para VFX/UI.

### 4.6 Crescimento de habilidades ao evoluir

DBM-001 **já adicionou** escalonamentos fixos por rank (Q +24 dano bruto, W +1 distância, E +25 escudo, R +40 dano), genéricos para três heróis. O próximo sistema deve migrar isso para **tabelas por personagem, forma e slot**, sem perder a progressão validada:

```text
AbilityDefinition {
  id, slot, allowedFormIds, maxRank,
  baseDamagePerRank[], physicalRatioPerRank[], kiRatioPerRank[],
  kiCostPerRank[], cooldownPerRank[], castRangePerRank[],
  targetingMode, castWindupTicks, projectileSpeed,
  controlEffects[], visualCueId, audioCueId, balanceVersion
}
```

Exemplo de `Q` artilharia: dano-base por rank [65, 95, 125, 155, 185] + 0,60 × KiPower, cooldown [9, 8.5, 8, 7.5, 7] s, custo 35 Ki. Rank de poder **não precisa aumentar todos os eixos ao mesmo tempo**; escolher 1–2 alavancas para evitar inflação. Ao mudar para forma avançada, mudanças podem ser overrides de raio, tipo de projétil ou tags, preservando o rank atual.

### 4.7 Testes mínimos da fundação de stats

1. HP/HPMax/HPregen crescem com nível; regen funciona fora da fonte e não excede máximo.
2. Equipa armadura → reduz dano físico; equipar KiResist → reduz dano de Ki; as resistências não se confundem.
3. 10% life steal + 100 HP realmente perdido → +10 HP no atacante (cap em MaxHP).
4. Escudo absorve antes do HP, evita life steal sobre dano absorvido; dano verdadeiro ignora resistências.
5. Haste 50 reduz cooldown de 9 para 6 segundos; nunca chega a CD zero por valores absurdos.
6. Rank 2 de uma habilidade aumenta dano/efeito mensuravelmente em cenário fixo.
7. Equipar, vender e recombinar item recalcula todos os stats, sem bônus fantasma.
8. Transformação e treino aplicam buff UMA vez; morrer/renascer preserva somente buffs permanentes previstos.
