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


## 5. Economia, equipamentos e loja — o principal sistema que falta

### 5.1 Princípios de economia e estratégia

Um MOBA gera decisões a partir de **custo de oportunidade**:

- Fazer last hit concede Zeni ao responsável e XP aos aliados elegíveis próximos; não conceder ouro total a quem não finalizou o golpe, salvo regras explícitas de suporte.
- Ouro passivo impede estagnação absoluta; farm de tropas, jungle, assistências e objetivos oferecem rotas de renda diferentes.
- Itens têm **picos de poder**: um componente de 350–900 Zeni dá estatística pequena; um item completo de 2500–3400 pode dar bônus único. **A receita usa os componentes já comprados**, cobrando somente a diferença.
- A compra só é permitida na base (salvo habilidade/regra explícita); loja aberta fora da base pode exibir catálogo, mas não executar compra.
- O inventário básico possui **6 slots**; consumíveis/wards precisam de slots e limites definidos; itens ativos disparados nas teclas 1–6.
- Ouro deve ser registrado num `GoldLedger`: `LastHit`, `Assist`, `Kill`, `Objective`, `PassiveIncome`, `Buy`, `Sell`, `Undo`, com ID de transação, tick e saldo auditável.
- **Counterbuild** deve ser real: armadura é a resposta a ataque físico; resistência de Ki a burst energético; anti-cura reduz sustain; penetração responde a resistências; mobilidade ajuda contra zonas; escudos ajudam contra explosão inicial.

Não copiar passivas específicas da Riot ou ícones originais. O que copiamos é a **lógica das escolhas**. A Riot já retirou o sistema de itens míticos por interferir na identidade de campeões [R3/R4], então evitaremos item universal obrigatório que derrote a especialização dos heróis.

### 5.2 Catálogo mínimo jogável — preços e números PROPOSTOS

**Esta tabela complementa o catálogo amplo do GAME_DESIGN §16, não substitui seus nomes nem sua direção temática.** Valores para protótipo, a confirmar com testes de TTK/tempo para primeiro item e renda média.

| ID | Item | Custo total | Atributos/efeito indicativo | Ingredientes |
|---|---|---:|---|---|
| `IT_C01` | Luvas Reforçadas | 350 | +12 Ataque | — |
| `IT_C02` | Célula de Ki | 350 | +80 Ki máximo | — |
| `IT_C03` | Placa de Armadura | 400 | +25 Armor | — |
| `IT_C04` | Núcleo Isolante | 400 | +25 KiResist | — |
| `IT_C05` | Cristal de Vida | 400 | +150 HP | — |
| `IT_C06` | Engrenagem Ágil | 350 | +12% AttackSpeed | — |
| `IT_C07` | Semente Revigorante | 400 | +3 HP/5s | — |
| `IT_C08` | Condutor de Ki | 500 | +20 KiPower | — |
| `IT_C09` | Essência Vampírica | 600 | +6% LifeSteal | — |
| `IT_C10` | Fragmento de Aceleração | 500 | +10 AbilityHaste | — |
| `IT_B01` | Botas Comuns | 300 | +mobilidade plana definida no MovementProfile | — |
| `IT_B03` | Botas de Armadura Saiyajin | 1200 | velocidade +30 Armor | Botas + Placa + 500 |
| `IT_B04` | Botas de Ki | 1200 | velocidade +30 KiResist | Botas + Núcleo + 500 |
| `IT_O01` | Luvas do Dragão | 2800 | +45 Ataque, +200 HP, bônus de dano em ataques repetidos | 2× Luvas + Cristal de Vida + 1700 |
| `IT_O04` | Cristal de Kaiô | 3000 | +70 KiPower, +20 Haste, bônus moderado às técnicas | Condutor + Fragmento + Célula + 1650 |
| `IT_O07` | Braçadeira de Precisão | 2700 | +35 Ataque, +25% crítico | Luvas + Engrenagem + 2000 |
| `IT_O08` | Lentes de Energia | 2800 | +60 KiPower, +15% penetração de Ki | 2× Condutor + 1800 |
| `IT_D01` | Colete do Exército de Freeza | 2800 | +500 HP, +55 Armor | Cristal de Vida + Placa + 2000 |
| `IT_D03` | Barreira Namekuseijin | 3000 | +350 HP, +55 KiResist, escudo em resposta a Ki (CD interno) | Cristal de Vida + Núcleo + 2200 |
| `IT_D05` | Núcleo de Regeneração | 3100 | +500 HP, +8 HP/5s, regen extra após sair de combate | Cristal de Vida + Semente + 2300 |
| `IT_O09` | Espada da Corporação Cápsula | 3000 | +50 Ataque, +10% LifeSteal; ataque básico cura pelo dano real | Luvas + Essência Vampírica + 2050 |
| `IT_U04` | Pulso de Interferência | 2800 | +40 KiPower, aplica anti-cura por técnica elegível por tempo limitado | 2× Condutor + 1800 |
| `IT_U02` | Amplificador de Escudos | 2700 | +30 KiPower, +15% potência de curas/escudos em aliados | Condutor + Fragmento + 1700 |

**Checagem de soma:** todas as receitas acima fecham no preço informado (ex.: Luvas do Dragão: 700+400+1700=2800). Efeitos ativos, venda e passivas únicas terão IDs próprios e cooldown interno. Se itens menores tiverem valores muito altos por Zeni investido, calibrar pelo desempenho observado, não por opinião.

### 5.3 Receitas por grafo acíclico e compra atômica

Exemplo: `IT_O01` requer 2 `IT_C01` + 1 `IT_C05` + 1700 Zeni. Comprando as duas luvas e o cristal anteriormente, o jogador deve pagar **1700**, consumir exatamente os três componentes e ganhar um item pronto; não manter os bônus dos ingredientes **mais** os do item final.

Fluxo recomendado:

```text
BuyRequest(itemId, transactionId, tick)
 → verificar jogador vivo/permissão de loja/proximidade/base
 → verificar definição e receita
 → escolher multiconjunto de componentes no inventário
 → custoEfetivo = preçoFinal − soma(preço dos componentes já possuídos e consumíveis na receita)
 → validar custo, espaço resultante e restrições (único, categoria)
 → transação atômica: debitar Zeni, remover componentes, adicionar item
 → RecalculateDerivedStats e emitir PurchaseEvent
 → snapshot HUD + ledger + telemetria
```

O termo "consumíveis na receita" aqui significa componentes equipados/elegíveis, não poções já utilizadas. Em compras avançadas, permitir **compra direta do item final** se o jogador tiver dinheiro e slots, usando automaticamente ingredientes disponíveis. **Undo:** só enquanto não saiu da área de compra e não houve ação invalidante (combate/uso de item/consumível/transformação relevante), com restauração exata de inventário e Zeni. Venda: 70% do valor base pago, arredondamento consistente; itens temporários com regra específica.

**Banco de dados de itens:** arquivo de conteúdo JSON ou ScriptableObject serializado para authoring, compilado para uma tabela/manifesto em runtime; o domínio C# lê dados **imutáveis e versionados**. O mesmo `ItemCatalog` alimenta HUD, loja, validador e dano. Não manter preços duplicados em `ShopPanel`.

Exemplo de dado autoral:

```json
{
  "id": "IT_O09",
  "name_ptbr": "Espada da Corporação Cápsula",
  "icon": "items/it_o09",
  "totalCost": 3000,
  "components": ["IT_C01", "IT_C09"],
  "combineCost": 2050,
  "stats": {"physicalAttack": 50, "lifeSteal": 0.10},
  "uniqueEffects": [],
  "maxOwned": 6,
  "dataVersion": 1
}
```

**Validações de CI:** IDs sem repetição, receitas sem ciclo, referências existentes, preços fechando, ícone existente/licenciado, mínimo e máximo de números válidos, passivas únicas com grupo e ausência de conteúdo protegido sem licença.

### 5.4 Design visual da loja e ícones

Regras de UI extraídas das decisões da Riot sobre a loja [R7]:

- Tamanho de ícone constante e reconhecível; silhouette distinta para espada, escudo, luva, armadura, botas e energia.
- Cores por **categoria**, mas nunca somente pela cor (acessibilidade).
- Busca em português, filtros "Todos", "Recomendados", "Ataque", "Ki", "Defesa", "Sustento", "Mobilidade", "Visão".
- Tooltip: estatísticas líquidas e **comparativo antes/depois**, cooldown, passiva, receita, custo restante, dificuldade e counter pretendido.
- **Árvore de construção** clicável: tocar numa espada final mostra os componentes; item no inventário realça caminho restante.
- Comprar/combinar/vender altera atributos de fato; HUD nunca deve mostrar ícone de item sem instância correspondente no inventário.
- Criar arte própria/CC0 para todos os ícones, usando atlas para reduzir draw calls; não copiar ícones originais do LoL.

### 5.5 Botas de treinamento — trade-off e ganho

Manter o contrato do GAME_DESIGN §16.4: débito 1100, **−8% velocidade enquanto o tempo elegível de 300 segundos corre vivo fora da fonte**, depois +8% permanente até o fim da partida, item desaparece e libera slot. Morte pausa. Não permitir vender botas e manter bônus. Indicador na HUD "Treinamento 02:41 | −8% velocidade", depois aviso de recompensa. Testar alternância de compra e mortes para impedir farm infinito de buffs.

## 6. Segurar C: a proposta para Ki e treinamento realmente relevantes

### 6.1 Diagnóstico do código atual — não é impressão

Em `GameWorld.TickHeroTimers`, após `C` por pelo menos 0,15 s:

- Se Ki **não está cheio**, recupera aproximadamente `15 + KiRegen` unidades por segundo (além da regeneração passiva aplicada antes).
- Quando Ki **fica cheio**, e o personagem está **fora da fonte**, passa a ganhar `GrantExperience(hero, 1.8 × multiplier × dt)`.
- O multiplicador após 30 s de treinamento contínuo cai a 0,55. Portanto, após 30 s, o ganho nominal é **0,99 XP/s**.
- A XP necessária para o próximo nível hoje é `90 + 40×(nível−1)`; ao nível 1, 90 XP; ao nível 16, 690 XP. Sozinho com treino tardio de 0,99 XP/s, esse último nível levaria **mais de 11 minutos** na fase penalizada, desconsiderando outras fontes de XP e crescimento temporário. Isso explica por que parece não valer a pena.
- Não existe HUD com barra separada "Treinando XP", XP/s, tempo até próximo nível por C nem feedback suficientemente diferenciado quando Ki está cheio.

### 6.2 Máquina de estados proposta

```text
Idle
  └─ C pressionado por >= 0,15s ─> ChargingKi
      ├─ Ki chegou ao máximo ─> TrainingXP
      ├─ C solto ─> Idle
      └─ dano/ação/movimento/CC/morte ─> Interrupted ─> Idle
TrainingXP
  ├─ C mantido, fora da fonte, não atordoado ─> XP por tick
  └─ mesma lista de interrupções ─> Interrupted ─> Idle
```

**Treinar deve poder subir de nível, mas deve ter custo estratégico de oportunidade**: perder last hits, visão, pressão e ficar parado exposto. Regra de anti-AFK, decaimento após janelas longas e proibição de XP na fonte. Se o personagem treina escondido em brush, o som/aura podem oferecer sinal observável a inimigos próximos *que teriam acesso legítimo à informação* — não violar fog of war.

### 6.3 Regra de XP proposta para playtest

Não fixar XP/s sem comparar o farm real. Começar com:

```text
targetTrainingXpPerSec(level) = 0.25 × ExpectedCompetentFarmXpPerSec(level)
trainingXp = targetTrainingXpPerSec × (1 - fatiguePenalty) × deltaTime
fatiguePenalty = 0 até 20s contínuos,
                 aumenta gradualmente até 0.45 após 60s
```

Uma base numérica provisória de simulação, **a ser aferida e não copiada de LoL**:

| Nível | Farm competitivo esperado (XP/s) | Treino alvo (XP/s) | Uso |
|---:|---:|---:|---|
| 1–3 | 7 | 1,75 | Regenerar Ki e preparar primeiros níveis sem abandonar farm |
| 4–7 | 12 | 3,0 | Treinar entre ondas ou enquanto espera objetivo |
| 8–11 | 18 | 4,5 | Escolha de risco vs. giro e pressão do mapa |
| 12–15 | 24 | 6,0 | Evoluir parado continua possível, porém mais lento |
| 16–18 | 28 | 7,0 | Completar evolução tardia é possível sem incentivo AFK excessivo |

Esses valores são **targets de experimento**: se o jogo de fato gera 4 XP/s por farm real no início, então 1,75/s treinando seria forte demais. O comportamento desejado define a proporção; a tabela não substitui telemetria.

**Obrigatório na HUD quando C é mantido:** `KI 120/120 • TREINANDO • +3,0 XP/s • Próximo nível em ≈ 42s • vulnerável`. Ganho verde/amarelo visível na barra de XP, animação de aura (não bolas primitivas), vocalização curta com loop/disparo de áudio controlado, efeitos localizados e baixo ruído visual. Deve haver um toggle de volume e intensidade audiovisual, sem repetição irritante de gritos.

### 6.4 Testes anti-exploit e UX

- Treinar na fonte dá 0 XP.
- C com Ki cheio dá XP mensurável; o jogador vê a barra aumentar.
- Dano, morte, ataque e movimentação interrompem imediatamente; não renderizar aura/áudio depois do estado cancelado.
- C pressionado e solto repetidamente não zera penalidade acumulada de treino.
- Treino após 30 s e nível 17 continua possível; tempo para próximo nível bate com fórmula.
- Treinar Ki não cura HP por padrão: regen base de vida é sistema separado.
- Instinto Superior/Oozaru/Android 18 podem ter overrides por variante, nunca uma regra universal de C que quebra identidades de personagem.

## 7. Fluxo de partida, seleção, personagens e identidade de forma

### 7.1 O que já existe versus o que precisa mudar

`PrototypeGame.DrawGui()` já contém uma tela básica com botões `Iniciar com Goku Criança`, `Goku Super` e `Cell Z`. Porém não é o **draft** do produto: falta selecionar posição, ver prévia de modelo, skins/formas, papel, dificuldade, descrição de P/Q/W/E/R, loadout, confirmação e tela de carregamento. O componente `OrganicArena` tem `startDemo`, que pode iniciar o jogo automaticamente dependendo da cena. **Averiguar esta configuração na cena real antes de afirmar que o jogo não tem seleção.**

Novo fluxo de estados:

```text
Boot → Home/Play → SoloVsBots (primeiro)
 → ChampionSelection (3 heróis, posição, forma preview, kit, role)
 → ConfirmedLoadout → Loading → PreGameSpawn → InMatch
 → Victory/Defeat → Results → Home
```

Na seleção, desabilitar "Iniciar" até personagem ser escolhido; obrigar o botão confirmar; não criar mundo nem disparar timer competitivo antes da confirmação. Ao reiniciar pela UI, voltar à seleção opcionalmente com o mesmo personagem pré-selecionado. Primeiro será um **cliente offline**; lobby, bans, partidas online, reconexão e matchmaking permanecem posteriores.

### 7.2 Heróis: diferentes kits, tamanhos e silhuetas

**Regra rígida:** `ChampionDefinition` define kit e stats; `FormDefinition` define aparência, modelo, hitbox autorizada, buffs e alterações de habilidade; `CharacterView` desenha isso. Trocar prefab não altera por si só o estado do domínio.

| Personagem/forma | Papel estratégico | Tamanho visual relativo | Colisão relativa | Movimento | Counterplay |
|---|---|---:|---:|---|---|
| Goku Criança base | lutador móvel melee | 0,70 do adulto | 0,85 da base humana | rápido/agilidade, sem voo | perde duelo longo para bruiser robusto |
| Goku Criança Nuvem | rotas/engage pontual | 0,70 + nuvem | 0,85 | animação flutuante, mas respeita terreno, exceto passagens explícitas | CC ou previsão de deslocamento |
| Goku Criança Oozaru | transformação temporária de cerco/área | 2,6–3,2 × altura adulta | 1,5–1,8 × raio base (limitado ao mapa) | −20% a −30% deslocamento; animação pesada | hitbox maior, tempo de resposta e DPS de longo alcance |
| Goku Super base | assassino/lutador técnico | 1,0 | 1,0 | dashes curtos e controle fino | stun, zoneamento, erros de Ki |
| Goku Super Blue/UI | lutador móvel avançado | 1,0 | 1,0 | auras/cadência próprias | controle e janelas de cooldown |
| Cell Imperfeito | lutador/absorção | 1,05 | 1,05 | ritmo médio | poke persistente, anti-sustain |
| Cell Perfeito | duelista de poder de Ki | 1,10 | 1,05 | animação diferente, ataque legível | kite, janelas de ultimate |

**Todos os números desta tabela são proposta nossa**, não escalas canônicas de Dragon Ball ou LoL. O Oozaru deve ter rig/modelo gigante **diferente** e hitbox adequada ao gameplay; não escalar uma cápsula ou mesh humanoide e fingir que está resolvido. Transformação só ocorre num ponto navegável; se não couber no corredor, bloquear transformação ou reposicionar em local válido predeterminado — **nunca dentro de parede ou torre**.

**Cuidado com Goku Criança e R:** o GAME_DESIGN §14.2 prevê uma R antes do nível 16 e Oozaru a partir do nível 16. Isso exige um `AbilityOverride` por nível/forma: `R` rank 1/2 não pode assumir a forma gigante cedo demais só por ter sido desbloqueada no nível 6.

### 7.3 P, QWER, classes e counters — matriz de kit

O protótipo atualmente dá a todos os heróis Q Rajada/W Impulso/E Escudo/R Explosão semelhantes. Substituir por três kits **funcionalmente distintos** antes de ampliar o roster:

| Herói | Passiva P | Q | W | E | R |
|---|---|---|---|---|---|
| Goku Criança | treino do Mestre Kame: velocidade de ataque moderada por rank | Bastão Mágico: skillshot físico curto | Salto Ágil: dash com regra clara de parede | Kamehameha Inicial: cast com aviso e dano Ki | Sequência do Torneio; nível16 habilita Oozaru temporário |
| Goku Super | Instinto de Batalha: janela de bônus após esquiva válida | Golpe Veloz: engage com alvo | Barreira Divina: escudo temporizado | Transmissão: teleporte condicionado à visão/alcance | Feixe de Ki evolutivo com aviso, muda por forma |
| Cell Z | Absorção limitada: cura condicionada a dano causado | Golpe drenante: dano + sustain pequeno | Passo oportunista: mobilidade curta | Barreira biológica: defesa curta | Explosão celular canalizada: AoE anunciada |

Os kits específicos de Goku seguem a direção do GAME_DESIGN §14; o exemplo de Cell é **proposta de prototipagem** e deve ser reconciliado com a ficha definitiva do documento mestre antes de implementar. Toda habilidade tem cast range, hitbox, custo, rank, tempo de preparação e recuperação, CC aplicável, tags de dano e contrajogo claro [R8]. Personagem com melhor mobilidade deve pagar preço em dano/sustentação/alcance para preservar **power budget** [R3/R8].

### 7.4 Identidade visual e áudio de cada personagem

- Nome + variante + barra de HP + nível + equipe sobre cabeça, a altura correta em todas as formas, sem poluição excessiva.
- Modelos com materiais **licenciados** e rigs válidos; idle, walk/run, básico, receber dano, morte, Ki, Q/W/E/R, transformação e indicador de virada. Nunca trocar skin provocando deslize de pés (`root motion` vs simulação).
- **Goku pequeno realmente pequeno** na comparação lado a lado; Cell diferente em todas as formas; Oozaru escala grande e passos lentos, partículas proporcionais.
- Retargeting humanoid onde viável; Oozaru pode exigir `Generic Rig` e clips próprios, com `AnimatorOverrideController` ou equivalente; modelo visual e radius de colisão não devem derivar um do outro implicitamente.
- Sons: `AudioCueId` por personagem/ação e forma, mixer com categorias e limites de repetição; vocalização durante C deve interromper ao soltar, receber dano ou morrer. Criar/gravar áudio original ou usar áudio com licença apropriada; não ripar vozes oficiais do anime.

## 8. Mapa, câmera, movimentação e ritmo

### 8.1 Formato: inspirar-se na TOPOLOGIA, não copiar Summoner's Rift

O protótipo hoje usa coordenadas lógicas aproximadamente `X=-32..32` e `Y=-41..41`, com três rotas em `X=-17/0/+17` e rio horizontal. A especificação mestre propõe um **mapa novo e maior**, simétrico competitivamente, 2D lógico/3D visual, com desenho inspirado na topologia de três rotas.

**Novo mapa original Dragon Ball MOBA (sem copiar o arquivo exato de Summoner's Rift):**

- **Formato global:** arena aproximadamente quadrada (alvo inicial: 240–300 unidades por lado, a aprovar por playtest), com **base azul ao sudoeste** e vermelha ao nordeste, cada uma contendo fonte, núcleo, dois guardiões finais e acessos de rotas.
- **Top:** caminho pela borda oeste e depois pelo extremo norte; **Bot:** pela borda sul e depois leste; **Mid:** diagonal central conectando bases diretamente, com curvas curtas.
- **Selvas simétricas por rotação de 180°:** dois quadrantes com trilhas internas e acampamentos de monstros, rotas de gank e escape calculáveis.
- **Rio diagonal** cruzando o mid e separando metades; margens, pontes/vãos e arbustos criam zonas de disputa.
- **Objetivo alto:** dinossauro/arauto e dragão voador/barão no quadrante norte do rio. **Objetivo baixo:** guardião elemental/dragão temático próximo do sul do rio. Monstros devem ter silhuetas originais/licenciadas.
- **Três torres por rota**, gerador/inibidor no interior, **duas torres finais** protegendo o núcleo; requer abrir uma rota para conseguir vencer. Ordem e vulnerabilidade explícitas. **O código atual possui só uma torre por rota/equipe e núcleo**; não confundir adicionar modelos com mudar a estrutura competitiva.
- Arbustos com ocultação de visão, paredes realmente intransponíveis, entradas múltiplas nas arenas de objetivo, cruzamentos e pontos de ward.
- Simetria competitiva (comprimento de rota, acessos, tempo a objetivos e espaços de gank), não simetria estética perfeita. Medir diferenças de vitória por lado; pesquisas sobre fairness e atualizações de mapa sugerem testar viés lateral [R5, A1].

**Plano de topologia (texto):**

```text
      canto noroeste  ─── ROTA TOP ─── canto nordeste [BASE VERMELHA]
            |    jungle N / objetivo maior    /     |
            |        rio diagonal      MID   /      |
            |       / arbustos e entradas  /         |
            |      / objetivo menor      /           |
 [BASE AZUL] ── ROTA BOT ───────── canto sudeste
       sudoeste
```

O desenho final será um **grafo de corredores e regiões** definido em dados, antes de qualquer arte: `LaneGraph`, `CampNode`, `ObjectivePit`, `VisionBrushPolygon`, `WallPolygon`, `TowerAnchor`, `SpawnAnchor`, `Waypoint`. Fazer testes de conectividade, distâncias e saídas de cavernas. Não importar uma réplica não licenciada do mapa LoL.

### 8.2 Engenharia de navegação e microjogo

**Ordens fundamentais:** `Move(point)`, `AttackTarget(entityId)`, `AttackMove(point)`, `Stop`, `HoldPosition`, `Cast`, `Recall`, `ChargeKi`. Cada ordem tem regras de interrupção. Mouse direito é ação primária; `A` é ataque-movimento e `S` é parar. A tecla `H` atualmente é ajuda no protótipo: implementar rebinding/configuração antes de assumir `H = Hold`.

- Clique no chão: escolher ponto navegável; sem atravessar paredes ou modelos sólidos. Desviar e contornar, evitando jitter em obstáculos ou cantos agudos.
- Clique em inimigo: seguir até distância `AttackRange + radii`, parar/virar, iniciar windup e repetir ataque; permitir **orbwalking/kiting**: após disparo, mover durante recovery sem disparar golpes extras.
- Clique em alvo móvel: recálculo de rota periódico com limite; `attack-move` escolhe alvos seguindo prioridade configurada (não atacar invisíveis).
- `Dash/Blink`: checar caminho, raio de saída e áreas proibidas; dash é movimento rápido e colisão contínua; blink pode cruzar obstáculos **somente se a habilidade permitir**.
- Separar `NavService` (caminho) do `MovementSystem` (integração por tick), e de `EntityPresentation` (interpolação/rotação/animator).
- Unity 6 oferece NavMeshAgent/NavMeshObstacle para pathfinding e carving [T2]; mas como o domínio atual é C# independente do Unity, decidir explicitamente entre: (A) navegação derivada de malha e **caminho calculado na Unity, validado por domínio** para offline; (B) pathfinding 2D puro autoritativo no domínio, usado também pelo futuro servidor. **Para o multiplayer futuro, a autoridade de caminho/colisão NÃO pode viver exclusivamente no cliente Unity.**
- A colisão circular da DBM-002 resolve torres/núcleos como etapa intermediária; **não pode ser declarada substituto de NavMesh/A***.

### 8.3 Câmera LoL-like

Uma câmera MOBA é ferramenta de **informação e controle**, não só um objeto seguindo o personagem.

| Controle/regra | Proposta técnica |
|---|---|
| Projeção | ortográfica inclinada com ângulo experimental entre 45° e 55°; evitar câmera totalmente vertical |
| Follow | pan suave e recenter `Space`; modo livre permanece quando usuário navega |
| Pan | arrastar botão central e opcional edge-scroll com dead zone e velocidade calibrada |
| Zoom | scroll com limites para preservar legibilidade de combate e evitar vantagem injusta de informação |
| Clamp | respeitar limites do mapa considerando área visível, não só posição central |
| Input | cliques no terreno continuam precisos em 1280×720, 1920×1080 e ultra-wide |
| Mira | indicadores 3D projetados no chão e cursor não desviam com altura do mesh/efeitos |
| Fights | não recentralizar automaticamente a cada habilidade, flash ou transformação |
| Acessibilidade | reduzir shake, velocidade do pan, desativar edge-scroll, teclas remapeáveis |

Testes: orbwalking/click precisão, visão da rota na borda do mapa, transformação Oozaru sem cortar o personagem, minimapa alterando foco, 60 FPS com frame pacing adequado. **Valores de ângulo/zoom são alvos nossos**, não dados internos da câmera LoL.

### 8.4 Nome, barra e minimapa

Implementar `WorldSpaceNameplate` com nome do herói, variante opcional curta, level badge, HP e escudo separados, condições (CC, charge/transform) e marcação de aliado/inimigo. UI deve fazer `billboarding`/projeção correta e ocultar nomeplates de entidades não visíveis por regras de fog. Em Oozaru, deslocar nameplate por referência ao topo do collider/rig, não `y=1.6` fixo.

Minimapa gerado do **grafo real do mapa**: torres, bases, objetivos, unidades visíveis, região de visão, pings, viewport da câmera; não apenas imagem decorativa.

### 8.5 Minions, waves, torres, objetivos e macroestratégia

- Quatro tipos de tropa: melee/tanque, ranged, cerco e super (após queda do gerador/inibidor). Rotas com waypoints, prioridade de alvos, alcance e cadência. Last hit/CS e XP distribuídos conforme regras separadas.
- Wave spawn em intervalos de relógio fixo; escalonamento do tempo de partida por `MatchPhase`: inicial (farm/laning), intermediária (roaming/objetivos), tardia (inibidores/cerco). **Primeiro ajustar tempos de rotação e sobrevivência, depois spawn de objetivos.**
- Torre dá aviso e usa regras autoritativas de seleção/agressão: minions normalmente na frente, punição a herói atacando aliado sob torre, distância exata, dano/cadência escaláveis, reset e invulnerabilidade durante janelas definidas. O PR DBM-002 iniciou esta lógica, mas ainda não implementa todos os casos do LoL.
- Visão: cones/áreas circulares + obstrução, brush como regra de ocultação, wards com duração, detecção, minimapa, pings e neblina de guerra. Bots jamais usam posições de inimigos não revelados.
- Selva: acampamentos, HP/armor, leash/reset, XP/Zeni, buffs, pathing e respawn; jungler decide gank vs farm; risco/benefício dos objetivos.
- Objetivos maiores dão vantagem significativa **temporária** (buff, empowerment de minions, cerco), sem eliminar a chance de counterplay. Em 2026, a Riot também usou missões de posição para incentivar funções; só adaptar essa camada após métricas sólidas de renda e XP [R2].


## 9. Arquitetura proposta: evoluir o código atual sem recomeçar

### 9.1 Diagnóstico do código versionado

| Arquivo atual | Estado concreto da leitura | Evolução necessária |
|---|---|---|
| `Assets/Scripts/Domain/Definitions.cs` | `GameCommand`, `CommandKind`, `EventKind`, `HeroDefinition`, `ItemDefinition`, `Catalog` | Separar definições autorais de contratos de comando/evento; schemas completos |
| `Assets/Scripts/Domain/EntityState.cs` | Estado de combate, nível, XP, Ki, inventário simples, ranks, posição e cooldowns | `StatBlock`, `StatusEffectCollection`, `ItemInstance`, `MovementState`, `ChampionVariantState` |
| `Assets/Scripts/Domain/GameWorld.cs` | Tick 30Hz, spawn, torres, AI básica, damage/XP, loja, form, Ki; classe monolítica | Extrair sistemas gradualmente, preservando testes e comportamento público |
| `Assets/Scripts/Runtime/PrototypeGame.cs` | Input, câmera, GUI, spawn de visual e efeitos coligados | `GameSessionFlow`, `InputCommandMapper`, `HUDPresenter`, `EntityViewFactory` |
| `Assets/Scripts/Runtime/OrganicArena.cs` | Terreno, rio e vegetação procedurais; coordenadas retas e mapa de teste | `MapDefinition`, `MapSceneLoader`, `NavMap`, `MapVisualPresentation` |
| `Assets/Scripts/Runtime/CharacterPresentation.cs` | Sincronização de aparência/animações | `ChampionView`, perfis de rig e locomotion por forma |
| `Assets/Editor/DomainSelfTests.cs` | Testes determinísticos executados pelo Unity Editor | Expandir testes focados por sistema, fixtures tipadas e invariantes |
| `Assets/Editor/ArtSmoke.cs` | Smoke Play Mode e visuais | Separar testes básicos, progressão, navegação, HUD, mapa e build |

**Recomendação:** arquitetura modular em C# puro, com adapters Unity e dados serializados. O domínio permanece fonte da verdade do jogo. **Não introduzir ECS/DOTS ou substituir o render pipeline por obrigação arquitetural.** A Unity disponibiliza DOTS/Netcode [T6], mas os custos de conversão precisam ser comprovados por perfil de performance, não deduzidos do tamanho do projeto.

### 9.2 Esboço de módulos — sem movimentar tudo nesta entrega

```text
src/client-unity/Assets/
  Game/
    Config/              BalanceProfile, ChampionData, ItemData, MapData
    Domain/
      Contracts/         GameCommand, DomainEvent, Identifiers, enums
      Combat/            DamageResolver, Mitigation, Healing, Shields, StatusEffects
      Stats/             StatDefinitions, StatCalculator, StatSnapshot, GrowthTables
      Abilities/         AbilityDefinitions, CastStateMachine, CooldownSystem
      Economy/           ItemCatalog, RecipeResolver, Inventory, GoldLedger
      Progression/       XPSystem, SkillRanks, KiTraining, FormSystem
      Movement/          CommandQueue, PathService interface, CollisionResolver
      World/             WorldState, MatchClock, TickPipeline, Teams, WinCondition
      AI/                MinionBrain, TowerTargeting, JungleBrain, ChampionBot
      Vision/            FogOfWar, SightGrid, BrushSystem, WardSystem
    Application/
      Session/           SessionStateMachine, ChampionSelection, MatchBootstrap
      PresentationAPI/   ReadOnlySnapshots, EventProjection, Tooltips
    UnityAdapters/
      Input/             MouseInput, Hotkeys, UI blocking
      Navigation/        NavMesh importer, 2D walkability cache
      Rendering/         ChampionView, CharacterScaler, VFX, Nameplates
      UI/                ChampionSelectUI, HUD, ShopUI, Minimap, Results
      Audio/             AudioCueRegistry, KiVocalization, Mixer
      Build/             SceneBootstrap, validation hooks
    Tests/
      Domain/            Progression, Damage, Recipes, Ki, Tower, Movement
      Integration/       Input to command, scene, view, shop
      Regression/        fixed seed, recorded match, restore/load
```

**Atenção:** esse layout é alvo, **não operação imediata de mover centenas de arquivos**. Primeiro criar novos módulos ao redor das APIs existentes, migrar uma responsabilidade por PR, deixar `GameWorld` como fachada e só removê-lo parcialmente quando testes equivalentes passarem. Isso protege a DBM-001 e as futuras validações da DBM-002.

### 9.3 Comandos, eventos, snapshots e relógio

Domínio:

```text
Input: GameCommand { Tick, ActorId, Kind, TargetId?, Position?, AbilityId?, ItemId?, ClientSequence? }
Loop: Receive → Validate → Timers → Path/Move → AI/Target → Cast → Damage → Death
      → XP/Gold → ItemEffects → Structures/Objectives → Vision → Snapshot
Output: WorldSnapshot + DomainEvents
```

**Invariantes obrigatórias:** evento com tick/ID único; comando impossível não produz custo/efeito; HP, Ki, ouro, ranks e stacks não negativos; morte/processamento do Nexus idempotentes; integridade do inventário; tamanho de corpo pode impedir entrada em corredor; sem danos múltiplos por VFX, animação ou pacote duplicado.

Relógio: `MatchTick` inteiro, `TicksPerSecond=30` na primeira fase; cooldown e duração de buff armazenados em ticks sempre que possível. O artigo de engenharia da Riot sobre relógio unificado documenta riscos de fontes diversas de tempo e precisão [T1]. Internamente, preferir algo como `ExpiresAtTick` a múltiplos floats acumulados com `Time.time`.

```csharp
// Esboço de contrato; NÃO é patch para colar sem integração.
public readonly struct CombatSnapshot {
    public readonly int Tick;
    public readonly int EntityId;
    public readonly float Hp, MaxHp, Ki, MaxKi;
    public readonly float PhysicalAttack, KiPower, Armor, KiResist;
    public readonly float HpRegenPer5, AttackSpeed, AbilityHaste;
    public CombatSnapshot(int tick, int entityId /* ... */) { /* ... */ }
}
```

Para visualização, entregar somente `ReadOnlySnapshots` à Unity. Eventos de dano disparados pelo domínio originam hit sparks, áudio e shake; eles **não autorizam** VFX a descontar HP de novo. Separar lógica de visão e de renderização (não mostrar entidade inimiga invisível por engano).

### 9.4 Design data-driven e versionamento

Propor `ContentManifest` com `balanceVersion`, `mapVersion`, `abilitySchemaVersion`, `itemSchemaVersion`, `assetManifestVersion`. Cada partida fixa as versões ao iniciar, e replays gravam IDs/seed; mudar valores durante patch nunca modifica a partida já em andamento.

Ferramentas de autor:

- `ItemRecipeValidator` verifica DAG sem ciclos, custos e inventário;
- `ChampionDefinitionValidator` verifica P/Q/W/E/R, crescimento, IDs de animação e forms;
- `MapValidator` verifica caminhos, colisões, simetria, spawns e visão;
- `BalanceSimulator` executa lutas sintéticas com seeds reproduzíveis;
- `ContentLicenseValidator` evita publicar ícones/modelos sem prova de licença.

Modelos, sons e ícones usam `AssetManifest` com origem, licença, hash, atribuição se aplicável, papel (form/skill/item/map) e fallback. Asset missing deve gerar erro verificável no pipeline, não cápsula silenciosa para produto final.

### 9.5 Direção do futuro multiplayer

O `GameWorld` já é C# puro e offline; isso cria **boa separação inicial**, mas ainda não equivale a servidor pronto. Para multiplayer futuro:

1. Extrair `SimulationCore` sem UnityEngine, sem estado global mutável não controlado.
2. Teste headless de duas equipes simuladas; comandos por sequência/tick.
3. Criar servidor autoritativo isolado que valide movimentos, casts, XP, ouro e fog.
4. Escolher transporte e lib após pequeno benchmark de atraso, conexão e segurança. Unity Netcode for GameObjects é opção para projetos GameObject [T3]; Netcode for Entities implica decisão maior de arquitetura [T6]; projeto MOBA_CSharp_Unity é referência MIT legada [T4].
5. Snapshot/delta, interpolação visual, previsão limitada e reconciliação; tick de simulação não precisa igualar FPS.
6. Login/lobby/matchmaking só depois de uma partida real estável com 10 bots; autenticação e backend não devem impedir MVP offline.

**Não copiar protocolo do LoL**. As publicações de engenharia Riot sobre determinismo ajudam a formular nossos requisitos [T1], mas não fornecem sua implementação completa.

### 9.6 Repositórios abertos que merecem auditoria

| Repositório / licença declarada | Valor possível | Risco e decisão |
|---|---|---|
| [yasgamesdev/MOBA_CSharp_Unity](https://github.com/yasgamesdev/MOBA_CSharp_Unity), MIT, Unity 2018.3 | Referência a servidor C# independente, buffs, bush, visão, skills/itens data-driven, pathfinding e UDP | **Alta relevância conceitual**, mas legado e dependências antigas. Auditar módulos, benchmarks e atribuição; não substituir projeto de imediato. |
| [bensalilijames/Clash](https://github.com/bensalilijames/Clash), MIT declarada | Estados de personagem, ataques, buffs, itens e minions | Depende de uLink, tecnologia antiga; avaliar código somente como referência. |
| [InFaNsO/MOBA_Unity](https://github.com/InFaNsO/MOBA_Unity), MIT declarada | Arena/protótipo Unity 2019 | Pequeno e antigo; não presumir produção pronta. |
| [DevZhav/unity-moba](https://github.com/DevZhav/unity-moba), GPL-3.0 | Exemplo de cliente, 3 mapas, lobby, serviços | Licença copyleft com obrigações, possível incompatibilidade da redistribuição. Usar para estudo; não incorporar componentes sem análise jurídica/técnica. |
| [Unity-Technologies/EntityComponentSystemSamples](https://github.com/Unity-Technologies/EntityComponentSystemSamples) | Exemplos oficiais DOTS/Netcode/Physics | Material didático; não migrar para ECS só porque há exemplos. |

A licença do código **não autoriza automaticamente** redistribuir modelos/ícones incorporados de terceiros ao repositório externo. Registrar versão/commit analisados e uma `AUD-MOBA` com score: portabilidade, segurança, dependências, custo de migração, cobertura funcional, licença e performance. Até lá, arquitetura atual evolutiva continua sendo a escolha padrão.

## 10. Balanceamento como produto: counters, dados e ciência

### 10.1 Counterplay e classes

Design de cada herói tem `StrengthProfile` e `WeaknessProfile`; cada counter deve existir em **três níveis**, conforme filosofia de counterplay Riot [R8]:

1. **Na habilidade:** disparo com windup, feixe anunciado, zona de impacto legível, tempo de recarga e possibilidade de esquiva/interrupção.
2. **Na compra de item:** defesa de Ki contra mage, armadura contra físico, anti-cura contra sustain, visão contra emboscada, mobilidade para escapar de zona.
3. **Na estratégia:** controlar visão/rotas, forçar cooldown, lutar perto de minions, punir transformação lenta ou não contestar objetivo desfavorável.

Exemplo de matriz experimental:

| Arquétipo | Vence em circunstâncias | Perde para / resposta |
|---|---|---|
| Lutador de sustain / Cell | trocas longas com pouca interrupção | anti-cura, kite e CC |
| Assassino móvel / Goku Super | alvo isolado, cooldowns disponíveis | tank/escudo, controle, visão, posição defensiva |
| Oozaru / cerco em área | fight frontal agrupada | ranged DPS, terreno estreito, lentidão/CC |
| Mago de Ki (Freeza) | zona/objetivo, poke preparado | assassino chegando por flanco, KiResist |
| Tank/suporte (Piccolo) | teamfight em torno de carry | penetração, dano percentual, jogar objetivos opostos |
| Carry de ataque (Android 18) | combate prolongado bem protegido | burst/CC ao alcance antes de acumular dano |

Counters **não** devem ser vitórias automáticas: campeão forte contra outro ainda pode perder por habilidade, desvantagem de ouro ou macro. Separar fraqueza estratégica de ameaça puramente numérica.

### 10.2 Métricas de qualidade e balanceamento

Métricas de partida a registrar por `balanceVersion`:

| Grupo | Métrica | Decisão habilitada |
|---|---|---|
| Combate | tempo médio até matar (TTK), dano/min por tipo, dano mitigado, curas/escudos, críticos, life steal | identificar explosões instantâneas, tanks imortais, sobrecura |
| Progressão | XP/min e Zeni/min por fonte, level aos minutos 5/10/15, nível último das skills, tempo real carregando C | comparar XP do treino e farm |
| Economia | tempo até primeiro componente e item completo, frequência de compra/venda, receitas favoritas, saldo parado | precificar itens e evitar builds dominantes |
| Personagens | winrate por matchup/função/faixa de domínio, pick/ban, curva de maestria, duração média de forma | achar kits sem counterplay ou fracos |
| Objetivos | primeiro ward, first tower, objetivo contestado, tempo de rotação, lado da base, placar no momento do objetivo | medir snowball e assimetria |
| UX e perf | input-to-feedback, divergência de collider e efeito, CPU/tick, FPS frame times P95/P99, crashes | priorizar correções reais |

Os estudos [A1–A7] sustentam usar composição de equipe, função, dados históricos, simulação e objetivos explícitos de equilíbrio. **Não provam que existe uma fórmula universal para todos os MOBAs.** Uma winrate de 60% em 10 jogos não justifica nerf; usar intervalos de confiança, estratificação por nível de habilidade, posição, partida e patch. Bots podem ser usados para testar invariantes e tendências de balanceamento, mas **não substituem humanos**.

### 10.3 Qualidade do MVP com metas de piloto (valores propostos)

- 100% dos comandos inválidos rejeitados sem mutar ouro/HP/Ki; zero colisões irreversíveis entre structures e actors.
- 100% das 3 variantes básicas possuem **modelo distinto, escala e kit próprio**, além de barra/nome e no mínimo animações funcionais.
- Partida offline 1v1 + tropas completa do início à vitória/derrota, sem botão de depuração necessário.
- C tem XP/s verificável, anti-AFK e feedback visual distinto entre `ChargingKi` e `TrainingXP`.
- Pelo menos 20 itens reais com ícones e 8 receitas; 6 slots; nenhum item de dano só visual.
- Map validator: todas as 3 rotas conectadas, caminhos entre bases e objetivos, nenhum spawn sob obstáculos, simetria de tempos de rotação dentro de tolerância a medir.
- Em PC de referência, `FPS >= 60` na maior parte da partida é objetivo, **não garantia**; medir P95/P99 frame time, CPU da simulação e número de entidades. Criar preset low para máquinas modestas.
- Escolha de personagem deve ser obrigatória, com confirmação; no final, mostrar XP, Zeni, CS, kills, dano por tipo, itens e objetivos.

## 11. Literatura técnica e científica — implicações para o nosso jogo

| Fonte | Método / ideia respaldada pelo resumo publicado | Uso prático / limite |
|---|---|---|
| **[A1] Champion Recommendation System (ICTC/IEEE 2020)** | Modelos de recomendação de campeões com dados de partidas profissionais e sequência de pick/ban | Futuramente sugerir sinergias/counters no draft; **não** inferir matchup balanceado só por pick de pro player. |
| **[A2] CUPID (ACM PACM HCI 2024)** | Matchmaking considerando equidade e satisfação de posição | Quando houver multiplayer, separar skill e preferência de função; não construir matchmaking antes de volume de jogadores. |
| **[A3] Metagame Autobalancing (IEEE CoG 2020)** | Otimização por simulação visando um gráfico-alvo de relações entre estratégias | Criar matriz de counters e rodar simulações A-vs-B, sem exigir todos terem exatamente 50% contra todos. |
| **[A4] Simulation-Driven Balancing of Competitive Game Levels (IEEE ToG 2024)** | Gerador + agente de balanceamento + simulação-recompensa para avaliar justiça em arenas competitivas | Comparar lados do mapa e tempos de rota em 1000 simulações, não aprovar mapa só por parecer simétrico. Não é artigo específico de LoL. |
| **[A5] Role-Sensitive Analysis of Positional Contributions (IEEE Access 2025)** | Com dados de partidas profissionais de LoL, contribuição varia com ataque, defesa, laning e aquisição de ouro | Scoreboard/telemetria deve valorizar CS, objetivos, cura/defesa e visão, não só kills; resultados não garantem generalização aos nossos bots. |
| **[A6] What makes an ideal team? (Entertainment Computing 2023)** | Discute número de participantes e dificuldade/competitividade em MOBAs | 5v5 é produto de estratégia e coordenação; fazer 1v1 com bots primeiro para validar mecânicas, mas projetar espaço para 10 participantes. |
| **[A7] A Gospel for MOBA Game: Ranking-Preserved Hero Change Prediction (IEEE ToG 2022, Dota 2)** | Mudanças constantes de heróis e previsão de ajustes para diversificação e balanceamento | Versionar parâmetros e medir antes/depois por patch; estudo é sobre Dota 2, não fornece regras de LoL. |

**Leitura crítica:** artigos de previsão podem acertar vencedores usando padrões correlacionais sem explicar causalidade do balanceamento. Estudos de meta e draft não substituem playtest de sensação de impacto, câmera e leitura de efeitos. Para engenharia de código, documentação técnica da Unity/Riot e testes próprios são mais úteis do que papers de machine learning.
