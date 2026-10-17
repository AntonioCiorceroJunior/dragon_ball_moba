# Dossiê 06 — Leitura do código-fonte de um MOBA aberto em C#: lições e limites

**Data:** 2026-10-16. **Objeto:** [yasgamesdev/MOBA_CSharp_Unity](https://github.com/yasgamesdev/MOBA_CSharp_Unity), pasta servidor C#.  
**Tipo de investigação:** **código-fonte real consultado**, não apenas README; análise estática, **sem clonar, compilar ou testar o projeto antigo**.  
**Licença:** arquivo LICENSE na raiz declara MIT; uso de material que venha a ser copiado requer preservação da nota de copyright/licença e auditoria de cada dependência/asset. **Nenhum código ou asset externo foi copiado para nosso jogo nesta etapa.**

## 1. Organização efetivamente inspecionada

Arquivos lidos no repositório externo (link relativo a partir da [pasta Game](https://github.com/yasgamesdev/MOBA_CSharp_Unity/tree/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game)):

| Arquivo | O que implementa / resultado da leitura |
|---|---|
| [UnitComponent/Combat/Combat.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/UnitComponent/Combat/Combat.cs) | Componente genérico para timers, charge, custo, stats float/bool/visão, cooldown e parâmetros de animação. Carrega parte dos dados por YAML. |
| [Abilities/Attack.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/UnitComponent/Combat/Abilities/Attack.cs) | Máquina de ataque Move → BeforeAttack → AfterAttack; range considera **raio de ambos os corpos** e ataque tem tempo antes/depois do hit. |
| [Unit/Unit.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/Unit/Unit.cs) | Reúne componentes e recalcula stats; bufferiza dano, concede recompensas, compra, vende e usa itens. |
| [UnitComponent/Combat/Items/Item.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/UnitComponent/Combat/Items/Item.cs) | Cada item gera fontes de stats absolutos/percentuais a partir de ItemTable CSV; é uma **entidade** no dono. |
| [UnitComponent/Combat/Effects/UnitStatus.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/UnitComponent/Combat/Effects/UnitStatus.cs) | Nível e EXP por tabela de CSV; camada de atributos aplicados através de efeitos, inclusive ataque e movimento. |
| [CSVReader/ItemTable.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/CSVReader/ItemTable.cs) | Descreve BuyingPrice/SellingPrice e campos Add/Mul de HP/MP/Attack/Defence/MagicAttack/MagicDefence/AttackRate/MovementSpeed. |
| [Unit/ItemFactory.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/Unit/ItemFactory.cs) | Fábrica por CombatType para efeitos especializados como Katana, Potion, RegenArmor, SpecialBoot. |
| [Unit/Buildings/Tower.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/Unit/Buildings/Tower.cs) | Torre lê unidades na física ao alcance, filtra time/visibilidade/alvo, escolhe mais próximo e emite TowerBullet ao iniciar tiro. |
| [Unit/Characters/Minion.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/Unit/Characters/Minion.cs) | Estados Start, Move, Attack e Fin; percorre relayPoints e interrompe movimento ao adquirir ameaça. |
| [Pathfinding/PathfindingEntity.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/Pathfinding/PathfindingEntity.cs) | Adaptador de NavMeshPathfinder, Load(path) e GetPath(start,end); não dá detalhes de toda implementação só neste arquivo. |
| [Items/Katana.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/UnitComponent/Combat/Items/Katana.cs) | Efeito próprio baseado em razão de HP máximo por HP atual; não deve ser copiado sem clamp/validação. |
| [Effects/Slow.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/UnitComponent/Combat/Effects/Slow.cs) | Slow ajusta modificador de velocidade, duração e expiração separadas de personagem, útil como padrão de efeito. |
| [Unit/Characters/Champions/Champion.cs](https://github.com/yasgamesdev/MOBA_CSharp_Unity/blob/master/MOBA_CSharp_Server/MOBA_CSharp_Server/Game/Unit/Characters/Champions/Champion.cs) | Monta habilidades Q/W/E/R via SkillFactory e configura respawn da unidade. |

**Versão:** caminhos na branch master consultados em 16/10/2026. Não fixei um commit imutável nem validei o binário antigo, portanto **não recomendar importação direta** com alegação de compatibilidade. README descreve Unity 2018.3.4f1 e tecnologias antigas (ENet, SharpNav, VelcroPhysics, YAML etc.), que requerem auditoria antes de adaptar ao Unity 6.

## 2. Descoberta essencial: atributos derivados, não bônus permanentes

Em Unit.SetStatus, o framework:
1. Inicializa listas de FloatParam, BoolParam, VisionParam e AnimParam;
2. Coleta valores de todos os componentes Combat ativos;
3. Para cada tipo de atributo, calcula addSum de contribuições aditivas e mulSum de contribuições percentuais;
4. Define o status final usando addSum × (1 + mulSum);
5. Para bool/vision/animação, elege entradas com prioridade.

**Isso é uma decisão estrutural confirmada no código**, útil para nosso protótipo, porque no Dragon Ball MOBA atual o código faz, por exemplo, PhysicalAttack += item.Attack e Armor += item.Armor ao comprar, mas **não possui sistema de venda completo para reverter fontes**.

**Nossa recomendação original:** não copiar a classe Unit.cs nem usar LINQ/dicionários alocados por frame automaticamente (o método cria muitos dicionários/lists). Implementar DerivedStatCalculator próprio, com definições de HeroGrowth/Form/Item/Buff, cache invalidado por evento e teste exato de remoção do item, sem alocação por tick em hot path. Adotar prioridade explícita para flags de CC e visão, guardando fonte e validade de cada efeito.

### Testes derivados (para nosso projeto)

- Hero BaseAttack55 + LuvasA (+12) + LuvasB (+12) =79; retirar uma →67; mudar forma com +8 →75; retirar última luva →63. Recalcular do começo, não subtrair vários bônus de versões distintas.
- Buff +20% que dura 5s deve desaparecer exatamente quando Tick>=ExpiresAtTick; morte limpa apenas buffs classificados como removíveis.
- Efeito exclusivo de item não duplica em duas instâncias; status booleano mais prioritário prevalece de forma reprodutível.

## 3. Ataques: análise de máquina de estados REAL

Attack.cs possui estados **Move**, **BeforeAttack** e **AfterAttack**:
- Valida atacante/alvo, time, HP, alvo visível e não untargetable/disarmed;
- Ao mandar atacar, inicia movimento até target.Position;
- Calcula alcance por distância entre centros **menos os raios dos dois corpos**, comparando ao AttackRange;
- Prepara um windup baseado em AttackRate×AttackTimeRate; então aplica dano ao alvo e entra no recovery;
- Move para acompanhar alvo quando distância cresce; faz repath em intervalo UpdatePathSpan;
- Cancela ação quando atacante/alvo inválido (inclusive perda de visibilidade).

**Comparação com nosso código:** GameWorld.Tick atual aplica dano imediatamente quando alvo está em AttackRange e AttackCooldown<=0, sem windup ou recovery separados; não há escolha de status Untargetable/Unarmed nem range por radii. Essa diferença explica parte da sensação de ataques secos e sem resposta de animação.

**Transferência proposta:** desenhar máquina própria AttackerState {Approach,Windup,Strike,Recovery}, AttackTimeline configurável por campeão/forma, HitEvent tick exato, cancelamentos legais, manter autoataque da DBM-001 como contrato. Não copiar código antigo literalmente: dependência de XNA/ECS legado, visão/colisão/animadores incompatíveis.

### Novos testes

- Um clique num inimigo fora de range → path → windup → pelo menos 2 ataques no intervalo.
- Mover durante windup cancela se permitido; mover após Strike não acelera AttackInterval.
- Ataque inválido contra oculto/untargetable/disarmed não aplica dano, Ki nem eventos duplicados.
- Oozaru tem mais raio e animação mais lenta; distância de alcance é física de bordo, não só distância entre centros.

## 4. Economia: por que o framework é referência, não solução pronta

Unit.BuyItem exige OnBase, preço suficiente e slot livre; instancia Item via Factory e debita ouro. Unit.SellItem remove a entidade Item e credita SellingPrice×Count. Item.cs contribui com atributos para agregação de status.

**Ganho real da arquitetura:** vender retira a **fonte do modificador**, portanto o próximo recálculo de stats pode corrigir atributos. O nosso jogo ainda precisa de ItemInstance/RecipeGraph/GoldLedger, que **não** são comprovados pelas classes lidas deste projeto antigo.

**Cuidado:** nenhum dos trechos lidos comprova DAG de receitas, custo incremental de item completo, undo atômico ou antidupla compra. Portanto **não descrever o projeto aberto como portador de loja igual a LoL**. Nossa implementação de receitas virá de design original, com DFS de grafo, multiset de ingredientes e transações idempotentes.

## 5. Torres e minions: evidência comparativa

**Tower.cs externo**: faz consulta espacial às unidades no raio, filtra time/visibilidade/HP/untargetable, ordena por distância e instancia TowerBullet com targetId e parâmetros do tiro. Diferentemente do nosso protótipo, existe entidade específica de projétil de torre. Porém esse código **não demonstra prioridade por minion, sticky targeting ou hero aggression de LoL**; seleciona mais próximo. Precisamos de TowerPolicy original.

**Minion.cs externo**: avança em relayPoints, adquire alvos num raio AttackRadius e muda para estado Attack; após combate retoma caminho. Nosso ThinkMinion segue alvo direto e não mantém waypoint formal. Modelo de minion por states + lane waypoints é ideia viável, desde que testes determinem prioridade e vision.

**PathfindingEntity.cs externo**: faz adapter de NavMeshPathfinder para GetPath. O arquivo é fino; não podemos declarar que estudamos/avaliamos o algoritmo inteiro só por ele. É um argumento a favor de separar NavPlanner da simulação e do view.

## 6. Problemas do código externo que NÃO devemos importar

1. **Katana.cs** calcula bônus proporcional a MaxHP/HP. Quando HP está muito baixo, o multiplicador cresce sem teto; não há clamp local no método. Recomendo clamp e teste HP→0 em qualquer ideia de passiva de risco/recompensa.
2. **Unit.SetStatus** recria dicionários e listas ao calcular stats; num combate com muitos minions e muitos efeitos por tick pode causar GC/CPU. Nosso calculator deve evitar allocations em hot path.
3. **Tower.cs** cria SortedDictionary<float,Unit> chaveado por distância. Distâncias iguais ou quantizadas podem colidir no dicionário. Precisamos de tie-break estável por EntityId e prioridade, não chave float exclusiva.
4. **Dano recompensado por história**: Unit.ConfirmHPDamage divide XP/gold pelo número de atacantes em buffer da confirmação; isso pode não representar last hit, assistências por janela e recompensas MOBA modernas. Usar ledger com causa/tick/regras próprias.
5. **Recursos antigos:** Unity 2018, XNA, Mono e bibliotecas que podem estar sem manutenção. Um servidor inicial em C# que funciona em Mono antigo não é evidência de projeto estável no .NET moderno, nem integra à Unity6 por copiar pasta.

As observações acima são **riscos inferidos da leitura dos métodos**. Não foram reproduzidos em testes, nem são um julgamento sobre a qualidade integral do projeto.

## 7. Matriz de transferência para o Dragon Ball MOBA

| Ideia concreta | Ação recomendada | Dependência e prova |
|---|---|---|
| Agregação de stats por fontes | **Adotar conceitualmente**, criar StatCalculator original em DBM-003A | Testes de level/form/item/buff com venda |
| Attack state machine separada | **Adotar conceitualmente** depois de estabilizar autoataque | Testes de windup/recovery/cancel/moving target |
| QWER por fábrica/configuração | **Estudar e desenhar AbilityDefinition próprio** | 3 heróis com P/QWER realmente diferentes |
| Status effects com duração/prioridade | **Adotar conceitualmente** em módulo separado | Stun, slow, shield e CC sem conflito |
| TowerBullet com alvo e travel | **Adotar conceitualmente** após validação DBM-002 | projétil não causa dano duas vezes |
| Minions com relayPoints e estados | **Adotar conceitualmente** no mapa novo | waves percorrem 3 lanes e priorizam alvos |
| CSV/YAML editável por design | **Adotar princípio data-driven**, formato próprio | validadores de IDs/receitas/versões |
| Código Unity2018 completo | **Não importar agora** | custo de migração/licença/compatibilidade desconhecido |

## 8. Decisão e limites deste dossiê

**Resultado:** conseguimos estudar código de um MOBA real de código aberto em vez de apenas listar repositórios. Há evidência específica para **agregação de atributos, máquina de ataque, inventário, timer, torre com projétil e minions com waypoints**. Isso permite construir implementações próprias mais robustas, sem importação precipitada.

**Ainda falta:** auditar árvores completas de dependências, verificar versão exata do commit e demais módulos (visão, física, NavMeshPathfinder, serialização, rede, autenticação), compilar o projeto externo em ambiente isolado e comparar benchmark. Não afirmar que esse projeto é um LoL open source ou que fornece mecânicas modernas completas.

**Próximo ticket técnico:** DBM-003A derivará estrutura de fontes e stats, mas só depois de DBM-002 validada e mesclada. Essa ordem foi escolhida para minimizar regressões do domínio.
