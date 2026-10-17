# Dossiê 04 — Auditoria verificável: atributos, combate, progressão, itens e economia

**Projeto:** Dragon Ball MOBA · **Data:** 2026-10-16 · **Estado:** pesquisa e análise estática; **não foram executados testes Unity nesta entrega**.  
**Branch auditada:** `main` (DBM-001 integrada, DBM-002 em PR separado ainda sem aceite).  
**Fontes principais:** código publicado `src/client-unity/Assets/Scripts/Domain`; Riot 10.23, 12.10, 13.10, 26.1; /dev 2020–2024; Data Dragon 16.20.1; Riot Tech 2025; estudo de patches 2014–16 (FDG 2021).  
**Regra:** todas as fórmulas numéricas deste documento são marcadas **COMPORTAMENTO DO NOSSO CÓDIGO**, **REGRA OFICIAL DE PATCH**, **PROPOSTA NOSSA**. Nunca copiar automaticamente números de LoL.

## 1. Quadro de evidências rastreáveis (leitura direta)

| ID | Afirmação sustentada | Origem primária | Transferência e cautela |
|---|---|---|---|
| R-01 | Riot considerou os itens míticos excessivamente poderosos frente à identidade dos campeões, elevando valor do ouro e snowball | [/dev: 2024 Item Changes](https://www.leagueoflegends.com/en-us/news/dev/dev-2024-item-changes/), seção **Item Power Budgets**, 29/11/2023 | Kit e forma são o diferencial; itens têm papéis especializados e tradeoffs, sem automatizar combos |
| R-02 | Riot distingue itens *pattern-bound* dos *output-bound*: reforçar **como** o campeão luta versus substituir uma parte da execução | mesmo /dev, **Pattern Binding** | Espada de Cell reforça sustain por acertar ataques; não oferece stun universal gratuito que elimina contrajogo |
| R-03 | A revisão da loja de 2020 recomenda itens considerando inventário, estatísticas do adversário e situação; os ícones devem ser legíveis pequenos | [Preseason Item Shop Update](https://www.leagueoflegends.com/en-us/news/dev/preseason-item-shop-update/), **Item Shop Update** e **Item Icon Update** | Loja por receita, comparador e informação “bom contra”; sem automação de escolha, e sem ícones Riot |
| R-04 | Ability Haste substituiu CDR; cada ponto de haste aumenta frequência de conjuração no intervalo-base em 1% | [Patch 10.23](https://www.leagueoflegends.com/en-us/news/game-updates/patch-10-23-notes/), **Ability Haste**, 2020 | Proposta: cooldown = base/(1+haste/100); 50 haste faz 12s virar 8s, **não 6s** |
| R-05 | Aumento sistêmico de durabilidade veio acompanhado de redução de escudos/cura/sustain e ajustes após resultados observados | [Patch 12.10](https://www.leagueoflegends.com/en-us/news/game-updates/patch-12-10-notes/), **Champion Durability Update**, 2022 | Mudar só HP sem revisar lifesteal/TTK inflará sustain e endurecerá demais lutas |
| R-06 | Riot alterou encontro das waves nas lanes e minions travarem alvo em torres, modificando roaming/tower dives | [Patch 13.10](https://www.leagueoflegends.com/en-us/news/game-updates/patch-13-10-notes/), **Minions**, 2023 | Rota das waves e troca de alvo precisam de regras versionadas, além de dano das unidades |
| R-07 | Em 2026, Riot acelerou waves de mid/late e reduziu parte das recompensas por unidade para não inflar ouro/XP | [Patch 26.1](https://www.leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/), **Minions**, 2026 | Toda mudança de spawn deve testar Zeni/min, XP/min, pressão de torres, CPU |
| R-08 | Patch 26.1 prevê exceções à progressão usual: missão de top aumenta teto para 20 e crítico básico de muitos campeões passa a 200% | mesma [Patch 26.1](https://www.leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/), **Role Quests/Crit** | Nosso nível 18 e crítico proposto (1,75) não são espelho de LoL 2026; definir versão própria |
| R-09 | Riot Data Dragon expõe dados estáticos versionados, relações `from/into`, preços, venda, stats, imagens e labels, mas não todo código de passivas | [Riot Developer Portal](https://developer.riotgames.com/docs/lol), **Data Dragon / Items**, exemplo 16.20.1 | Adotar separação de dados e runtime; **não** copiar conteúdos, imagens ou scripts de LoL |
| R-10 | Loja e inventário da versão atual do Dragon Ball MOBA têm só 4 itens e incremento direto de atributos; não há remoção de bônus | `Definitions.cs` e `GameWorld.cs` no nosso repo | Qualquer nova receita precisa primeiro de `StatCalculator` + inventário estável |
| R-11 | Artigo de engenharia Riot descreve A* no LoL clássico e flow fields para hordas do modo Swarm, com waypoints/interpolação cliente/servidor | [The Tech Behind Swarm](https://www.riotgames.com/en/news/the-tech-behind-swarm), **Pathfinding & Movement**, 25/07/2025 | Não migrar nosso jogo para flow fields sem perf test; usar pathfinding correto para 3 rotas/10 heróis |
| R-12 | Artigo longitudinal sobre LoL usou 437 mil partidas entre 2014–2016, com efeitos heterogêneos de patches | [Heterogeneous Effects of Software Patches in a MOBA Game](https://arxiv.org/html/2110.14632v1), FDG 2021 | Balancear por matchup, nível de domínio e diferentes janelas pós-patch; resultados não demonstram valores de 2026 |

**Escopo da leitura:** R-01 a R-09, R-11 e R-12 foram confrontados com fontes oficiais/artigo acessíveis durante esta rodada; R-10 foi conferido diretamente no código atual. Isso não equivale a conhecer a implementação proprietária completa de LoL, não demonstra a precisão de todos os detalhes de cada patch e não transfere números automaticamente.

## 2. Auditoria do código exato, por arquivo e método

### 2.1 Definições e criação de personagens (`Definitions.cs:77–140`; `EntityState.cs:1–67`)

- `HeroDefinition` tem `MaxHp, MaxKi, BaseAttack, KiPower, BaseSpeed, AttackRange`. **Ausentes:** crescimento por nível próprio de cada herói, HPRegen/5, AttackSpeed derivada, heal/shield power, penetração, crítico, lifesteal, vampirismo, escala corpórea e atributos de counter.
- `ItemDefinition` tem `Id, Name, Price, MaxKi, KiRegen, Armor, Attack, TrainingBoots`. **Ausentes:** `components[]`, `combineCost`, `sellPrice`, `iconAsset`, `uniquePassiveId`, `stats` genéricos, compra ativa, cooldown, grupos exclusivos, restrições e versionamento.
- Catálogo real: `KI_CELL` (300 Zeni, +45 Ki máximo, +2.5 KiRegen), `ARMOR` (400, +30 Armor), `GLOVES` (350, +15 Attack), `TRAINING_BOOTS` (1100, tradeoff −8%/+8% de movimento após 300s). **Isso não corresponde ao catálogo extenso do GDD, que é apenas especificação.**
- Goku Criança: HP 570, ataque 55, KiPower 30, velocidade 6.2, ataque alcance 2.4; Goku Super: HP 520, ataque 51, KiPower 43, velocidade 6.5, range 5.8; Cell: HP 640, ataque 59, KiPower 33, velocidade 5.8, range 3.0.
- Todos nascem com `Armor=25`, `KiResist=25` e `KiRegen=2` por padrão. Todos heróis em `CreateHero` começam com `Zeni=1200`.
- O inventário atual é `List<string>`, e o limite 6 só é checado em `Purchase`; não existem slots reais nem tipos de item.

**Risco:** somar um item na mão e retirá-lo por subtração não reconstitui corretamente fonte/modificador após transformação ou outro bônus. O único método sustentável é **recalcular uma projeção de stats a partir de fontes imutáveis**, com tags de stacking.

### 2.2 Ataques (`GameWorld.Tick`, linhas aproximadas 57–103)

- Simulação atual: `FixedStep=1/30` no mundo (verificar inicialização do caller); `Tick(dt)` admite `dt <=0.1`, não assume necessariamente um fixed tick quando chamado pela API pública.
- Seleção de alvo repetido veio da DBM-001. A cada tick, quando vivo, em range e com `AttackCooldown<=0`, chama `DealDamage(entity, target, entity.PhysicalAttack, false)`, reinicia `AttackCooldown=AttackInterval` e emite `Attack`.
- O ataque **não possui windup separado** para aplicar dano depois do frame de preparação. `AttackInterval` default 1.1 segundos, sem `AttackSpeed` escalável, crítico, efeitos on-hit, ranged projectile ou controle de interrupção fina.
- O sistema de aquisição e visão não possui `VisibilitySnapshot` nem fog de guerra autoritativa.

**Hipótese verificável:** para herói recém-criado com `PhysicalAttack=55` e alvo com `Armor=25`, golpe sem escudo reduz HP em `55 × 100/125 = 44`. É cálculo a partir do código, não equivalência Riot. Para velocidade de ataque, objetivo futuro = intervalo derivado de `AttacksPerSecond`, com primeiro golpe após `WindupTicks`, janelas de cancelamento legais, múltiplos hits impossíveis por VFX duplicado.

### 2.3 Progressão (`GameWorld.GrantExperience` ~478–495)

- `ExperienceForNextLevel(level)=90 + 40×(level−1)`; exemplo level1→2 exige 90; level6→7 exige 290; level16→17 exige 690.
- Ao subir um nível, **todos os três** recebem `MaxHp +=24`, `Hp +=24` (cap), `PhysicalAttack +=2.8`, `KiPower +=3.1`. Não ganham HPRegen nem resistências por nível; isso explica parcialmente sensação de pouca progressão.
- Exemplo sem itens/formas: Goku Criança level6 tem `MaxHp=570+5×24=690` e `PhysicalAttack=55+5×2.8=69`; level18 tem `MaxHp=978` e `PhysicalAttack=102.6` **antes** de bônus de forma, já que level11/16 ativam bônus. Não atribuir valor de level18 total sem somar forma.
- `ApplyForm` soma bônus absolutos a `PhysicalAttack/KiPower/MoveSpeed` ao passar pelas etapas; mudança de forma e venda de equipamento não são eventos recalculados por um único `DerivedStats`.
- DBM-001 acrescentou `SkillRanks[4]` e pontos de upgrade, mas Q/E/R usam crescimento genérico por rank, ainda sem tabela específica de herói/forma.

### 2.4 Regeneração e Ki (`TickHeroTimers` ~188–241)

- Enquanto vivo, `hero.Ki = min(MaxKi, Ki + KiRegen×dt)`: regen passiva sempre.
- Fonte/base cura `80 HP/s`. **Não há nenhuma regen natural de HP fora da base**; ausência de campo `HpRegenPer5` em `EntityState`.
- C: após ao menos 0.15 segundos, em Ki abaixo do máximo, acrescenta `(15 + KiRegen)×dt` **além da regen passiva já aplicada antes** no tick. Com KiRegen base2, aproxima 19 Ki/s (2+17), não apenas 17.
- Com Ki cheio fora da base, atribui `1.8 XP/s`; após 30s de carga, `1.8×0.55 = 0.99 XP/s`. O `TrainingSeconds` decai lentamente quando C não é pressionado (`0.25s` para cada 1s sem carregar), então o abuso de alternância precisa de teste matemático e telemetria.
- Level16→17 requer 690XP; a 0.99XP/s = 697 segundos (~11.6min), supondo apenas C e taxa estabilizada. **Não se conclui que a partida precise durar isso**: ouro/farm também geram XP.

### 2.5 Dano/escudo/morte (`GameWorld.DealDamage` ~428–476)

- `Catalog.Mitigate(raw, armor ou KiResist)` aplica fórmula de resistência; positivo: `raw×100/(100+resist)`, negativo tem outra fórmula implementada.
- `Shield` absorve antes de `Hp`. A perda de HP não é registrada separadamente do dano pós-mitigação, embora `Damage` event tenha `Value` final. Não há categorias de golpe, penetração, true damage, lifesteal ou spell vamp.
- Qualquer dano válido sobre herói interrompe `Charging` e `ChargeSeconds`, mesmo que o escudo absorva 100%; isso precisa ser **decisão explícita de design**, não acidente.
- Quando HP chega a zero, `ProcessDeath` gera `Death`, premia killer herói com Zeni e atribui XP aos heróis vivos do time no raio13 do local de morte; a assistência não é contabilizada com atribuição específica.
- Zeni por last hit no protótipo: minion 28; herói 280; estrutura 160. XP: minion 24; herói 180; torre 110. Os números aplicam conforme `victim.Kind`, sem contribuição, suporte ou last hit separado.
- Não há ledger atômico e o campo `Zeni` é alterado diretamente.

### 2.6 Compra (`GameWorld.Purchase` ~517–536)

- Só compra na base, somente se `Inventory.Count<6` e `Zeni>=Price`. Compra válida **subtrai preço**, aplica stats **por adição** e inclui string no inventário.
- Não existe receita por multiconjunto, gasto incremental, venda, desfazer, passiva única, contador de transação, grupo de itens, item ativo ou apresentação por ícone.
- Botas de Treinamento ocupam slot temporariamente e desaparecem após contagem; +8% de velocidade é tratado em `UpdateMovement`. Exige teste de durabilidade do buff ao morrer e da pausa real da contagem de 300s conforme GDD.

## 3. Design de uma fundação sólida — proposta nossa, não código atual

### 3.1 Objetos e limites de responsabilidade

```text
ChampionDefinition + GrowthByLevel + FormModifiers + InventoryItems + StatusEffects
                |                   |
                +-- StatCalculator --+
                            ↓
              DerivedStatsSnapshot
                            ↓
            CombatSystem / Movement / UI

DamageRequest + source/target snapshot + seed
       → DamageResolver (físico/Ki/true, mitigação, escudo, HP)
       → DamageResult (perda real, absorção, cura, morte)
       → DomainEvents → VFX/SFX/HUD
```

**Nenhum efeito de apresentação poderá conceder vida/dano; nenhum item altera permanentemente o atributo-base.** Inventário armazena instâncias estáveis e o derivado é obtido somando fontes, não reaplicando bônus. Duração de buff pode ser representada em ticks com limite; habilidade e efeitos devem poder ser reprocessados de maneira determinística.

### 3.2 Fórmulas escolhidas para protótipo e exemplos verificáveis

**HP regen:** `regeneratedHP = (HPRegenPer5/5)×dt`. Se Goku Kid receber proposta `6HP/5 +0.65HP/5 por level`, no level6 terá `9.25 HP/5`, ou `1.85 HP/s`; cura em 10s =18.5 HP sem novas fontes. Todos esses são valores de **proposta**; devem ser testados com cooldowns e TTK.

**Dano físico/Ki:** `finalRaw = baseDamage[rank]+physicalRatio×PhysicalAttack+kiRatio×KiPower`; escudo absorve após `raw×100/(100+effectiveResist)`, separando dano real de absorção. Armadura não mitiga Ki e KiResist não mitiga físico. **Dano verdadeiro** só ignora resistência, não imunidade legítima.

**Lifesteal:** `heal = actualHpLossEligibleBasicAttack×lifeStealRate`. Exemplo 10% com 100 HP retirados → +10 HP; se escudo absorver tudo, 0. Spell vamp usar atributo/tags distintos e fator de área limitado, não presumir mesma regra de LoL em todos os patches.

**Haste:** `CD(base,haste)=base×100/(100+haste)` com `haste>=0`. Exemplo 12s com 50 haste =8s; 60 haste =7.5s; 100 haste=6s. Esses exemplos seguem a definição oficial de 2020, mas a decisão de manter esta fórmula no nosso jogo é nossa. **Não tratar 50 haste como −50% de cooldown.**

**Ataque speed:** `APS = clamp(BaseAPS × (1 + bonusAS), floor, cap)`; `AttackInterval=1/APS`; windup determinado pela animação/ratio sem duplicar hits. Crescimento de level e itens entram em `StatCalculator`, nunca escreverem diretamente em `AttackCooldown` já em andamento sem política.

**Critical:** valor-base próprio ainda não decidido: LoL patch26.1 elevou base crítico da maioria a 200% e possui exceções. Se adotarmos 175%, declarar explicitamente **nossa decisão**, não “igual LoL”. RNG por seed/evento, limites e dano real depois de resistências.

**Penetração:** para protótipo, escolher regra explícita `effectiveArmor=max(-X,(armor×(1-percentPen))-flatPen)`; casos de resistência negativa exigem função já implementada. Esta é proposta; validar interações de penetração plana, percentual e redução de resistência, sem assumir que replica todas as exceções Riot.

### 3.3 Ordem de cálculo de equipamentos e dano

```text
StatCalculator:
 baseHero(level) → formModifiers → itemFlat → itemPercent
 → permanentTraining → buffs/debuffs → cap → snapshot

DamageResolver:
 validateAttack → windup/attack interval → tags + base rank
 → coefficient scaling (physical/Ki) → crit eligibility
 → penetration + mitigation → shield → actualHPDamage
 → lifesteal/spellvamp (somente HP real) → on-hit/threat
 → death/reward/telemetry
```

**Não implementar a lista toda em um PR.** Etapa 1 `StatCalculator + HpRegen`; etapa 2 `DamageResolver`; etapa 3 `Items/Recipes/Transactions`; etapa 4 `HUD/icon/recommended`. Todas as etapas devem manter ponte com `GameWorld`, testes e compatibilidade de saves de partida se houver.

## 4. Itens e receitas como grafo, não atalhos de UI

### 4.1 Contrato de dado (proposta)

```json
{
  "id": "IT_O09",
  "namePTBR": "Espada da Corporacao Capsula",
  "iconAssetId": "items/it_o09",
  "totalCost": 3000,
  "components": ["IT_C01", "IT_C09"],
  "combineCost": 2050,
  "stats": {"physicalAttack": 50, "lifeSteal": 0.10},
  "passiveGroup": "capsule_sustain",
  "maxCopies": 6,
  "sellRate": 0.70,
  "dataVersion": 1
}
```

O esquema não é Riot Data Dragon. É o **nosso** `ItemDefinition`, inspirado somente na separação de dados. Com duas Luvas a350 e Cristal Vida a400, item final a2800 tem `combineCost = 2800-(350+350+400)=1700`. Comprar receita deve consumir componentes e substituir bônus, não acumular com o produto final.

### 4.2 Regras de transação

- `PurchaseResult` tipado com `OK, NotAtShop, InsufficientZeni, InventoryFull, RecipeCycle, MissingComponent, UniquePassiveViolation`.
- Receitas formam DAG: checar ciclos por DFS e ingrediente duplicado como **multiset** (não set simples).
- Se faltar apenas um componente mas houver dinheiro para item final, usuário pode clicar no item e pagar custo restante, consumindo os ingredientes elegíveis.
- Inventário com seis slots e opcionais slots especiais definidos; item ativo tem `Cooldown` e tecla, consumível muda quantidade.
- Venda remove fonte dos modificadores e credita taxa explícita; Undo restaura snapshot transacional quando regras permitirem; eventos duplicados por rede não fazem compra duas vezes.
- O cálculo dos atributos deve usar **apenas itens efetivamente equipados** (mais efeitos permanentes legítimos), nunca do histórico de compras.
- Botas treinamento: debuff −8% de speed por 300s elegíveis enquanto vivo fora da base, depois +8% permanente e slot liberado; comparar forma grande (Oozaru), penalidade e reaplicação de buffs.

### 4.3 Recomendações de counterbuild

- Inimigo com muito dano **físico**: sugerir Armor/HP; com **Ki burst**: KiResist/escudo; com sustain: anti-cura; com resistência alta: penetração; com assassin móvel: visão/slow/peel.
- A recomendação deve ser acompanhada de **razão observável**, ex.: “Cell causou 74% de seu dano recebido recente por Ki”. Não inventar números nem ler informação invisível do adversário.
- Recomendações **não bloqueiam** escolhas alternativas e não substituem estratégia do jogador (filosofia de loja Riot 2020).
- Ícones de espada, escudo, bota, Ki, poção e dispositivo devem ser originais/licenciados e reconhecíveis a 32–48px, com contraste e silhueta distinta.

## 5. Testes de contrato que devem anteceder os efeitos visuais

| ID | Fixture determinística | Resultado esperado |
|---|---|---|
| STAT-001 | Kid level1, HP300/570, fora da base, regen6/5 por 10s | 312 HP, nunca >MaxHP |
| STAT-002 | Kid level6, regen9.25/5 por 10s | +18.5 HP sem item |
| STAT-003 | Subir ao level6, sem item/transform | MaxHP690 e attack69 conforme comportamento atual compatível |
| STAT-004 | Equipar 2 luvas(+12AD), remover 1 | stats refletem **exatamente uma** luva |
| STAT-005 | Forma + item alteram ambos attack | sem aplicar bônus da forma duas vezes |
| STAT-006 | Morto durante 10s | regen não acontece; respawn HP cheio |
| DMG-001 | 100 físico contra Armor100 | perda pré-escudo50 |
| DMG-002 | 100 Ki contra KiResist100/Armor0 | perda pré-escudo50 |
| DMG-003 | 100 físico contra Armor0/KiResist100 | perda pré-escudo100 |
| DMG-004 | mitigado50 com escudo40 | perda10 HP; escudo0 |
| DMG-005 | mitigado50 com escudo60 | perda0 HP; escudo10 |
| DMG-006 | lifesteal10%, HP real100, atacante com espaço de cura | cura10 |
| DMG-007 | lifesteal10%, HP real0 absorvido por escudo | cura0 |
| DMG-008 | haste50 com CD12s | CD8s |
| DMG-009 | ataque com crit, RNG fixa | eventos/crítico reprodutíveis, sem duplicar hits |
| ITEM-001 | Luvas350+Luvas350+Cristal400, receita2800 | paga1700, ingredientes consumidos, bônus só do final |
| ITEM-002 | Zeni insuficiente/6 slots incompatíveis | sem mutação em ouro ou inventário |
| ITEM-003 | Venda e undo legítimos | exato saldo e stats revertidos |
| ITEM-004 | Compras com mesmo transactionId duas vezes | no máximo 1 compra |
| ITEM-005 | Itens de Ki/Armor não alteram dano errado | resistências e dano independentes |
| ECON-001 | 3 minions por lane/time, waves a cada14s | spawn determinístico, crescimento controlado |
| ECON-002 | Hero dá último hit; ally próximo | Zeni só ao último-hit conforme regra inicial; XP elegível |
| ECON-003 | Ajustar wave timer com recompensa fixa | detectar inflação Zeni/XP/min |
| KI-001 | C pressionado a Ki parcial | passive+active se ambos previstos, próximo estado correto |
| KI-002 | C com Ki cheio, fora da base, 30s+ | XP1.8/s→0.99/s, sem áudio/VFX fantasma |
| KI-003 | C na fonte | XP 0; Ki regen passiva continua |
| REG-001 | Q/W/E/R ranks pós DBM-001 | upgrades ainda válidos; R somente 6/11/16 |
| REG-002 | DBM-002 torres após integração | aggro e colisão não modificados pelo calculador de atributos |

**Plano de QA:** escrever testes C# puros primeiro e executar Unity depois. A biblioteca de teste não deve depender de modelos, cenas ou animação. **Os testes acima são especificações — ainda não foram executados.**

## 6. Falhas específicas que esta arquitetura previne

1. Compra de duas luvas causa +30 AD; combina espada e mantém +30 AD *além* da espada sem o item extra.
2. Vender armadura de loja não diminui Armor, porque atributo foi só incrementado.
3. Transformação no nível11 gera bônus duplicado quando o herói renasce e `ApplyForm` roda novamente.
4. Ki aplica dano com Armor, ignorando `KiResist`.
5. Escudo absorve dano mas lifesteal cura mesmo sem retirar HP.
6. Comprar HP máximo mata por redução de percentual/sistema de clamp errado.
7. Cooldown passa a 0 quando haste chega a100, devido a regra errada de `CD=(1-haste%)`.
8. Comprar receita custa preço total mesmo tendo componentes; usuário perde Zeni duas vezes.
9. Minions ganham velocidade de spawn e passam a gerar muito mais ouro/XP sem ajuste.
10. HP regenerando na morte ou sobrecarregando `MaxHp`.
11. Ícone aparece no inventário mas não existe `ItemInstance` no domínio.
12. Balanceamento medido apenas por dano final, sem shield, penetração, regen e recursos.

## 7. Gate de entrega e próxima investigação

**Entregável de pesquisa de hoje:** matriz de evidências, achados diretos do código, contratos/fórmulas ilustrativos, arquitetura de stats/dano/economia, casos de teste.  
**Não entregue nesta rodada:** compilação/Unity, implementação dos novos sistemas, medição de TTK/XP em partidas, auditoria integral de código Riot (não público), validação humana de builds.  
**Próximos dossiês de pesquisa:** (A) combate movimentação/orbwalking/visão/turret targeting; (B) economia de waves/objetivos e topologia original; (C) câmera/3D rig/som/input/UI; (D) rede/autoritatividade/replay/IA; (E) análise estatística de balanceamento e artigos acadêmicos **lidos integralmente quando disponíveis**.

**Decisão condicionada:** validar/mesclar DBM-002 e, depois, implementar DBM-003A (`DerivedStats` e `HPRegenPer5`) como primeira etapa de código. Não solicitar Codex apenas para ler documentação.
