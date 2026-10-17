# Políticas de documentação e precedência — Dragon Ball MOBA

**Data:** 17/10/2026. A documentação passou a ter papéis distintos para evitar decisões incompatíveis.

## 1. Tipos de informação — nenhum substitui outro

| Tipo | Significado | Como identificar | Documento/artefato de referência |
|---|---|---|---|
| **CÓDIGO MAIN** | Comportamento versionado na ramificação principal | método, campo, implementação e testes ligados a commit | `src/client-unity` + `main` |
| **CÓDIGO EM PR** | Alteração ainda não aprovada/mesclada | URL do PR, branch, mudanças, gate pendente | PR #2 para DBM-002 |
| **VALIDADO** | Executado com evidência em contexto identificado | logs, comando, resultado e build/commit | `coordination/reports/`, `coordination/evidence/` |
| **DESIGN** | Comportamento desejado, inclusive números de exemplo | texto mestre de produto e decisão explícita | `specifications/GAME_DESIGN.md` |
| **PROPOSTA TÉCNICA** | Possibilidade de arquitetura ou balanceamento | hipótese com tradeoff e aceitação definida | `PLANO_MESTRE_ATUAL.md`, `research/` |
| **PESQUISA EXTERNA** | Fonte que apoia princípio ou método | título, ano, URL, escopo e limite de evidência | `research/01_` a `07_` |
| **HISTÓRICO** | Registro de uma versão, decisão ou entrega antiga | carimbo de data, branch e ausência de papel normativo vigente | blueprint 2026 preliminar, status antigos, relatórios anteriores |

**Regra central:** design não prova implementação; commit não prova build; smoke não prova partida completa; resumo de paper não equivale a leitura integral.

## 2. Autoridade por pergunta

- **O que o personagem faz hoje?** Ler código da `main` e relatórios de teste do mesmo commit. Se aparecer em PR, declarar como **proposto**.
- **O que queremos no produto?** Consultar GDD Mestre. Se houver duas versões de um kit ou número, o GDD é a **intenção de produto** até decisão atual explícita em `coordination/DECISIONS.md`; não substituir silenciosamente.
- **Como implementar?** Usar `PLANO_MESTRE_ATUAL.md` e dossiê correspondente; validar contra código real. O Codex só executa o `TO_CODEX.md`.
- **Qual regra do LoL é a correta?** Registrar patch, modo, data e fonte primária. Não assumir que números de 2020 valem em 2026.
- **Um estudo sustenta causalidade?** Conferir o método e limitações no estudo; separar proposta de evidência de campo.
- **Podemos publicar o asset?** Consultar licença do próprio asset e origem; uma licença MIT de código não libera modelos, músicas, personagens e marcas de terceiros.

## 3. Conflitos concretos encontrados

| ID | Tema | GDD / proposta | Código main conferido (17/10) | Resolução documental — ainda não alteração de código |
|---|---|---|---|---|
| C-01 | Goku Criança HP inicial | GDD §14: **660 HP** em tabela de planejamento | `Catalog.Hero("GKU_KID").MaxHp=570` | Registrar 660 como **semente de balanceamento do GDD**, 570 como **realidade do protótipo**. Decidir base definitiva em DBM-003A após teste de TTK |
| C-02 | Crescimento de HP | Blueprint preliminar: exemplo **+82 HP/nível** | `GrantExperience`: **+24 HP/nível** | Não mesclar números; propor `ChampionGrowthData` por herói, com versão de balanceamento e testes |
| C-03 | Regeneração | Pesquisa sugere `HPRegenPer5` (exemplo 6 no lvl 1; +0,65/lvl) | Só cura 80 HP/s na fonte; **nenhuma regen base fora** | Definir regen em DBM-003A; proposta não executada |
| C-04 | Itens e receitas | GDD §16 e blueprint descrevem dezenas de itens, componentes, ícones e builds | `Catalog` possui **4** IDs; compra soma atributos diretamente, sem venda/receita | GDD = intenção; DBM-003A/B/C/D = etapas para fazer compra real |
| C-05 | Transformação Oozaru | GDD §14–15 prevê forma gigante e restrição de terreno | `GKU_KID` termina em `kid_mastery` no protótipo; sem Oozaru jogável | Não declarar Oozaru existente; precisa FormData, rig, escala, collider e kit |
| C-06 | Seleção de campeão | GDD §6 exige seleção antes da partida | Runtime contém botões de seleção, mas `OrganicArena.startDemo=true` pode iniciar Goku Super automaticamente | Criar `SessionFlow` com seleção obrigatória; não chamar tela atual de draft completo |
| C-07 | Mapa | GDD §8: 3 rotas, selva, objetivos, geometria planejada | Coordenadas três colunas, rios/arte procedural e colisões limitadas | Mapa diagonal quadrado está em pesquisa/proposta; fazer blockout e navegação antes de decoração |
| C-08 | Torre/colisão | Pesquisa descreve alcance, aggro, pathing e prioridade de alvo | Na `main` ainda usa regras anteriores; PR #2 traz colisão circular e aggro preliminares | PR #2 não vira estado atual até merge e validação |
| C-09 | Status de DBM-001 | `coordination/PROJECT_STATUS.md` e `FROM_CODEX.md` foram escritos **antes** do merge e dizem “sem merge” | PR #1 está **mesclado** na main, `067ed1c` | Tratar handoff como histórico do Codex; usar `STATUS_ATUAL.md` e `TASK_QUEUE.md` para status integrado; não sobrescrever relatórios |
| C-10 | Prompt de início | `docs/START_HERE.md` antigo mandava executar `prompts/05_COMPILE_PROTOTYPE.md` | `prompts/` não está na árvore Git main; DBM-002 é tarefa ativa | Guia reescrito, removendo passos que não existem no repositório |

## 4. Como resolver futuros conflitos

1. Registrar **tema + origem + número/definição de cada lado + versão do código/documento**.
2. Marcar impacto: muda gameplay, custo, licença, QA, arquitetura, arte ou apenas nomenclatura.
3. Escolher: **manter design**, **ajustar por evidência**, **postergar**, **experimentar** — decisão com responsável e justificativa, sem fingir que houve aceite de balanceamento.
4. Documentar em `coordination/DECISIONS.md` com ID `ADR-...` quando a decisão for aprovada.
5. Atualizar `PLANO_MESTRE_ATUAL.md` e testes apenas após a decisão; preservar documento histórico.
6. Nunca sobrescrever log ou relatório de validação de uma versão anterior para simular um status mais recente.

## 5. Preservação dos links históricos

Os sete dossiês `docs/research/01_...` a `07_...` e `specifications/MOBA_RESEARCH_BLUEPRINT_2026.md` **não foram movidos nem apagados**. O conteúdo antigo permanece para rastreabilidade; a organização principal agora é feita pelos índices e por este documento de precedência, evitando quebrar URLs já compartilhadas.

## 6. Regras para o Codex e ChatGPT

- `AGENTS.md` continua obrigatório.
- `coordination/TO_CODEX.md` contém **somente a tarefa ativa**, não o roteiro inteiro.
- `coordination/FROM_CODEX.md` e `coordination/PROJECT_STATUS.md` são handoff técnico produzido pelo Codex, possivelmente referindo-se à última execução dele, e não um registro automático da main.
- `STATUS_ATUAL.md` é um retrato do GitHub conferido pelo ChatGPT na data escrita, também sujeito a desatualização: checar PR/main novamente em nova sessão.
- O plano e a pesquisa nunca contam como testes Unity; o usuário aciona o Codex para rodar builds e validação Windows.
