# BATCH-001 — Alterações de código integradas pelo ChatGPT (aguardando Unity)

**Data:** 17/10/2026 · **Estado:** implementação em PR draft; compilação de domínio via GitHub Actions, validação Unity/Windows **ainda não executada** por ChatGPT.  
**Branch:** `chatgpt/moba-integrated-systems-2026-10-17` (base da DBM-002), [PR #4](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/4).  
**Regras:** não mesclar antes da validação; PR #2 continua pendente. **A nova main de documentação não é a base desse PR empilhado**; integrar em ordem para preservar documentação.

## 1. Código criado/modificado

| Arquivo | Mudanças | Status |
|---|---|---|
| `Domain/Definitions.cs` | 25 itens (4 legados + 21 novos), 13 receitas, categorias e atributos, comandos `Sell`, `Stop`, `AttackMove`, forma `kid_oozaru` nível16 | Proposto |
| `Domain/EntityState.cs` | regen, AS, LifeSteal, SpellVamp, crit, haste, penetração, renda passiva, AttackMoveActive e feedback de treino | Proposto |
| `Domain/StatCalculator.cs` + .meta | reconstrução de stats por nível/forma/itens, clamp de HP/Ki, regen básica por level, defesa e AS crescentes | Novo |
| `Domain/GameWorld.cs` | regen HP, renda Zeni após30s, training XP/s escalado, crit determinístico, sustain por dano real, penetração e cooldown com haste, receitas, venda, Stop/AttackMove, QWER diferenciadas, Oozaru raio/caminho seguro | Proposto |
| `Runtime/PrototypeGame.cs` | seleção obrigatória em jogo normal (fixture de teste preservada), loja rolável com categorias, preço restante e venda, ícones procedurais, HUD regen e XP/s, nameplates, Goku pequeno/Oozaru gigante, A+click, S e minimapa que focaliza câmera | Proposto |
| `Runtime/ArenaCameraController.cs` | edge pan opcional e `FocusAt` para clique no minimapa | Proposto |
| `Runtime/PrimitiveHeroVisual.cs` + .meta | fallback 3D original para heróis sem prefab licenciado disponível | Novo, reserva |
| `Resources/CharacterForms/GKU_KID/kid_oozaru.prefab` + .meta | cópia técnica do prefab licenciado existente `kid_mastery`, rig humanoid preservado e silhueta suplementar via runtime; **não é arte final de grande macaco** | Provisório |
| `Editor/DomainSelfTests.cs` | validações de regen, receitas, venda, life steal, haste, Oozaru fora da torre, AttackMove/Stop | Testes adicionados |
| `tests/DomainSmoke/` | executável .NET 8 que compila Domain C# fora do Unity e testa invariantes | Novo |
| `.github/workflows/domain-smoke.yml` | CI de domínio no push/PR | Novo |
| `scripts/verify_publication.ps1` e baseline BATCH | verificação de hash SHA256 4 fontes de domínio e exceção controlada para docs/research | Atualizado |

**Compatibilidade:** o arquivo `ArtSmoke.cs` continua testando rigs humanoid dos heróis. A nova forma Oozaru usa prefab rigged para preservar o contrato existente. O programa normal não inicia a partida sem confirmação; o teste automatizado nativo com flag `-dbm001-native-test` ainda pode iniciar automaticamente.

## 2. Regras efetivamente implementadas (valores nossos, provisórios)

- `HPRegenPer5`: 6 no nível1, +0,65 por nível, além da cura da fonte 80HP/s. HP/ataque/KiPower preservam +24 HP, +2,8 ataque, +3,1 KiPower por level (não balanceados).
- Resistências: +0,9 por level; ataque: +1,2% attack speed relativo por level, além de itens; `CritChance` de itens é aplicada com roll determinístico por contagem de ataques (prototipagem, não sistema pseudoaleatório oficial do LoL).
- `AbilityHaste`: CD = CDbase × 100/(100+haste). 10 haste reduz Q 4,0→3,636s.
- `LifeSteal`: somente ataques básicos elegíveis, por HP realmente removido; `SpellVamp`: dano Ki de técnicas, por HP realmente removido. Efeitos de Cell em Q/R têm sustain adicional; item `ANTI_HEAL_BLADE` aplica **35% de redução de cura por 3 segundos** ao danificar herói. Não há ainda true damage/on-hit/CC completo nem resistência a efeitos.
- Receitas consomem componentes existentes no inventário e cobram só diferença; vender devolve 70% do preço nominal. **Não existe undo**, stacks avançados nem IDs atômicos de transação; são próximos tickets.
- Treino após Ki cheio: taxa começa em `1.8 + 0.22×(level-1)` XP/s, multiplicador 0,55 após30s. Não foi calibrado por partidas com farm real; a HUD exibe XP/s.
- Zeni passivo após30s de partida: +3/s enquanto herói vivo. Compras somente na fonte.
- Kid base escala visual ×0,8; Oozaru ×2,45 com raio lógico0,95, velocidade menor. A transformação ocorre automaticamente ao atingir nível16 no protótipo; **não é ainda uma ultimate temporária fiel ao GDD**.
- Q/W/E/R agora têm nomes/efeitos parciais por herói; **não são ainda kits completamente exclusivos**, sem passiva P implementada, CC e skillshot perfeito.
- Ícones simples procedurais baseados em categoria e modelo original; são placeholder, não arte de ícones exclusiva item a item.

## 3. Evidências nesta fase

- Revisão estática de arquivos C#, verificação lexical de delimitadores e cruzamento de itens/ingredientes: catálogo com **25 IDs únicos**, **13 receitas** e sem componentes inexistentes nas definições escritas.
- GitHub Actions `Dragon MOBA pure C# domain smoke`: `success` nos **dois runs do commit b465d113** (39060414683 e 39060413218), com .NET8, incluindo os testes adicionados. Um primeiro run em commit anterior falhou por **fixture inválida de torre sob fogo cruzado** (39059967699), corrigida mudando posições e confirmada por CI posterior. A evidência não substitui Unity/Windows. Links: [run validado A](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/actions/runs/39060414683) e [run validado B](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/actions/runs/39060413218).
- `DomainSelfTests` adicional foi **escrito**, mas nenhum Unity Editor nem build Windows desta branch foi rodado aqui.
- Não há screenshots do executável atual nem evidência de impacto de balanceamento em partida real.
- PR #2 continua sem prova Unity; validação integrada será feita no mesmo checkout, em uma rodada Codex.

## 4. Limitações e riscos a não ocultar

- **Não finalizado:** novo mapa diagonal top/mid/bot e NavGraph/A*, jungle/objetivos/buffs, fog/wards, torres em tiers/inibidores, quatro tipos de minions, 15 heróis, sistemas de runas, multiplayer/servidor, loja com undo, itens ativos e assets definitivos.
- Catálogo de itens e QWER têm valores de protótipo, sem playtest TTK, farm/XP/min ou counters humanos. Não dizer que está igual a LoL 2026.
- Oozaru tem rig humanoide reaproveitado com esculturas simples, não modelo original completo com animações próprias e textura de pelo.
- Minimap de **três rotas atuais**, clicável, com torres/minions/heróis; não existe ainda fog/ward, portanto revela posições de ambos os times (apenas protótipo offline). Torre mantém o minion alvo vivo até ele sair de alcance, exceto quando herói aciona aggro.
- A ordem de merge é crítica: PR #4 baseado no PR #2. Quando testes forem aprovados, PR #2 primeiro, depois atualizar base de PR #4 para `main` e mesclar. PR #3 de documentação já está na main e deve permanecer.
- Pipeline de rendering/câmera é IMGUI e art Built-in; não reescrever cena/render pipeline antes de avaliar build.
- `scripts/verify_publication.ps1` passa apenas quando baseline corresponde aos fontes; esse mecanismo não substitui licença/segurança completa.

## 5. O que cabe ao Codex numa única validação

Executar [TO_CODEX_BATCH_001.md](../TO_CODEX_BATCH_001.md) na branch deste PR e registrar logs, screenshots, testes `VisualPrepare`, `VisualPlay`, `VisualBuild`, smoke nativo e cenários de compra/combate/câmera/personagens. Corrigir a branch até passar, sem merge. Priorizar **erros de execução observados** e regressões. Não pedir ao Codex pesquisa ou implementação do mapa novo nesta janela.

## 6. Definição de conclusão

- [x] Compilação e smoke .NET8 do domínio aprovados no commit `b465d113`; após alterar documentos revalidar o hash do código antes da integração.
- [ ] Publication checker passa com .meta e baseline (sem caminhos privados).
- [ ] Unity importa sem erros.
- [ ] Testes domínio/art e Play Mode passam (incluindo Oozaru e 3 personagens).
- [ ] Build Windows executa e permite seleção normal, compra/receitas, HUD e câmera.
- [ ] Regressões DBM-001/002 testadas; PR atualizado e relatório Codex com evidências.
- [ ] ChatGPT revisa e integra na sequência correta, sem merge automático.

**Até cumprir todos os itens, este documento descreve IMPLEMENTAÇÃO NÃO VALIDADA, não entrega pronta.**
