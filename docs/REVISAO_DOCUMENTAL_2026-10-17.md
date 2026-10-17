# Auditoria documental e reorganização — 17/10/2026

**Escopo:** documentação da main do repositório GitHub, com confronto com código-fonte do protótipo e PR #2. **Natureza:** revisão editorial/técnica **sem** executar Unity, compilar C# ou consultar todos os links acadêmicos de novo. **Branch da reorganização:** `docs/reorganizacao-2026-10-17`.

## 1. Parecer: está mais completo?

**Sim, em abrangência de conteúdos e material de engenharia. Ainda incompleto em demonstração experimental, consistência de dados e detalhes de vários subsistemas.**

O material cobre agora: visão end-to-end de jogo (GDD mestre, ~130 KB), arquitetura histórica, arte e licença, sete dossiês temáticos de pesquisa com links, auditoria de 13 arquivos C# de framework de outro MOBA, estudo longitudinal de patches de LoL, planos de teste e comparações concretas com `Definitions.cs`/`GameWorld.cs`/`ArenaCameraController.cs`.

O material **não** comprova:
- execução do código novo de torre/colisão DBM-002;
- jogabilidade LoL-like, câmera de qualidade, loja real com receitas, regen natural, Oozaru e 15 personagens;
- build/integração/benchmark do framework C# aberto, ou auditoria integral de dependências;
- acesso ao código proprietário Riot, leitura integral de todos os papers, causalidade de todos os resultados publicados;
- equivalência de valores de balanceamento e de objetivos com patch LoL 2026;
- partidas humanas completas e dados de avaliação de diversão/competitividade.

**Nenhuma dessas lacunas é resolvida renomeando um documento.** O propósito da organização é tornar visível onde precisamos implementar ou pesquisar.

## 2. Inventário e nova função de cada classe de documento

| Classe | Fontes existentes | Nova função |
|---|---|---|
| **Índice central** | `docs/README.md` (novo), `docs/START_HERE.md` (revisado) | achar imediatamente estado real, plano, pesquisa e tarefa ativa |
| **Estado integrado** | `docs/STATUS_ATUAL.md` (novo) | snapshot do GitHub e testes conhecidos, separado do handoff histórico Codex |
| **Política de fonte** | `docs/DOCUMENTOS_FONTE_E_PRECEDENCIA.md` (novo) | conflitos GDD x código x pesquisa x PR x evidência |
| **Plano de desenvolvimento** | `docs/PLANO_MESTRE_ATUAL.md` (novo) | arquitetura modular, dependências, entregas e critérios de sucesso |
| **Visão de produto** | `docs/specifications/GAME_DESIGN.md` | preservar integralmente as histórias, 15 variantes, UI, personagens, itens e fusão |
| **Pesquisa bibliográfica** | `docs/research/01_` e `07_` | fontes Riot/acadêmicas com limitações, evolução, métodos de experimentação |
| **Pesquisa de sistema** | `docs/research/02_` e `04_` | contratos/matemática de stats, dano, Ki, economia, itens e counterplay |
| **Pesquisa de mundo** | `docs/research/05_` | movimento, torres, waves, mapa, câmera e navegação |
| **Engenharia de código aberto** | `docs/research/06_` | estudo de código C# de terceiro, reutilização conceitual, riscos de licença |
| **Plano legado detalhado** | `docs/research/03_` e `docs/specifications/MOBA_RESEARCH_BLUEPRINT_2026.md` | material de fundo/propostas históricas, **não fila executável oficial** |
| **Coordenação** | `docs/coordination/` | tarefa ativa, relatórios imutáveis, decisões e backlog |
| **Assets/licenças** | `docs/ASSET_MANIFEST.md`, `docs/specifications/ART_PIPELINE.md` | realidade de uso e critérios de futura importação |
| **Outros históricos** | `docs/PROJECT_STATUS.md`, `docs/RELATORIO_COMPLETO_DO_TRABALHO.md`, `docs/VISUAL_*.md` | histórico, não autoridade de status atual |

**Escolha editorial:** preservar URLs existentes e não mover arquivos 01–07 nem apagar relatórios. Reorganizar via índices, precedência e documento mestre. Isso evita links quebrados no GitHub, em conversas anteriores e nas notas do Codex.

## 3. Inconsistências reais descobertas e tratamento

| Severidade | Problema encontrado | Tratamento |
|---|---|---|
| Alta | `docs/START_HERE.md` dizia executar `prompts/05_COMPILE_PROTOTYPE.md` — caminho indisponível na `main` | Substituir por instrução ativa DBM-002 e links ao novo índice |
| Alta | `coordination/FROM_CODEX.md` e `PROJECT_STATUS.md` declaram DBM-001 “sem merge” | São snapshots históricos da última execução Codex. Novo `STATUS_ATUAL.md` registra PR #1 mesclado; NÃO adulterar relatórios |
| Alta | Pesquisa `DBM-005A` chama **seleção de herói**, mas fila formal DBM-005 é **animação/rig**, DBM-007 é **seleção/HUD** | Plano consolidado preserva IDs oficiais e trata os recortes divergentes como propostas sem ticket |
| Alta | GDD usa **660 HP** iniciais para Goku Criança; main usa **570**; blueprint usa **570 +82/nível** como experimento; main soma **+24/nível** | Regra explícita de versão/fonte em `DOCUMENTOS_FONTE_E_PRECEDENCIA.md`; decidir por balanceamento após DBM-003A |
| Alta | Pesquisa cita Oozaru e personagens com aparência como se estivessem planejados, risco de se ler como já entregues | Distinguir design de runtime; status enumera Oozaru ainda não implementado |
| Alta | Blueprint inicial muito extenso repete estudos posteriores e mistura código, valores de design e pesquisa | Mantido como histórico/preliminar e **despriorizado** no índice; dossiês servem de evidência, plano atual é sintético |
| Média | `TO_CODEX.md` ancora tarefa a uma quota antiga **12%** e horário de renovação, apesar do usuário dizer que a quota chegou a 100% | Remover informação de quota datada sem mudar escopo DBM-002 |
| Média | `docs/specifications/README.md` dava destaque à importação inicial DBM-000 | Atualizar como índice de projeto, mover instruções antigas para seção “histórico” |
| Média | `docs/research/01_` repete títulos `3.1` com níveis de heading incoerentes e `03_` tem heading `6.4` no mesmo nível de `6` | Ajustar headings editoriais sem alterar conteúdo das pesquisas |
| Média | Nome e números de receitas não são inteiramente iguais no GDD, blueprint, exemplificação e runtime | Declarar preços e ingredientes como seeds até serem decididos e validados; catálogo runtime apenas 4 itens |
| Média | `docs/PROJECT_STATUS.md` histórico e `docs/coordination/PROJECT_STATUS.md` possuem nomes semelhantes | Índice diferencia “histórico” e “retorno do Codex”; estado integrado tem nome próprio |
| Aberta | Algumas fontes acadêmicas aparecem apenas por resumo ou com DOI/link antigo; não houve validação de cada referência nesta auditoria | Manter evidência E1–E2 e backlog de verificação; não reivindicar revisão sistemática concluída |

## 4. Completeness matrix — conteúdo, não avaliação arbitrária por número de páginas

| Tema | Cobertura documental agora | Implementação/comprovação no jogo | Princípio para fechar lacuna |
|---|---|---|---|
| Atributos por nível, HP regen | **Boa conceitualmente:** fórmulas e contrato | **Incompleto:** sem regen natural | DBM-003A/testes numéricos |
| Dano físico/Ki, crítico, shield, vampirismo | Boa em requisitos e testes | Parcial: mitigação/escudo simplificados, sem life steal | DBM-003B/ataque state machine |
| Catálogo de itens, receitas, ícones | Boa em requisitos, sem catálogo final de design aprovado | Parcial: 4 itens diretos e sem receitas | DBM-003C/D, grafo e catálogo |
| Ki/treino `C` | Boa auditoria do código e necessidade de equilíbrio | Parcial: ganho XP e C, UI/anti-exploit insuficientes | Comparar XP/min C versus farm |
| 3 heróis e 15 variantes | GDD extenso + proposta de kits; detalhes variam | 3 heróis lógicos, kits genéricos | Design data-driven de P/QWER/forma e modelos |
| Oozaru e diferença de tamanho | Intenção clara, colisão de forma ainda incompleta | Não implementado como forma jogável | rig/anim/física/path/movimento |
| Câmera/seleção/HUD | Direção e lista de testes | Protótipo funcional simples, UX relatada como ruim | teste humano e patch focal |
| Mundo 3 rotas, jungle, torres, objetivos | Topologia e sequenciamento conceitual bons | Mapa de 3 colunas, selva limitada, torres simples | MapDefinition/A*/vision/tiers |
| AI minion/bot e visão | Pesquisa parcial | Sem fog/ward e sem IA estratégica | Dossiê 08 e testes de informação |
| Balanceamento/counters | Estudos e plano de simulação | Sem métricas de partida robustas | laboratório por seed/fases/jogadores |
| Multiplayer 5v5 | Arquitetura de alto nível | Não implementado | dossiê futuro + protótipo headless |
| Assets/áudio/rigs/licenças | Diretrizes + manifestos existentes | CC0 parcial, sem formas finais | asset register/versionamento/QA |

## 5. Precedência final e “como continuar sem confusão”

- Para saber **o que existe:** main + relatórios testados + `STATUS_ATUAL.md`.
- Para saber **o que o jogo será:** GDD + decisões aprovadas.
- Para saber **como implementar:** `PLANO_MESTRE_ATUAL.md` + dossiê específico.
- Para **executar Codex:** `AGENTS.md` + `coordination/TO_CODEX.md` + arquivos daquele ticket.
- Para justificar uma decisão de LoL: referência primária/paper com data e limite da evidência.
- Para ter rastreabilidade: não mover, renomear ou apagar relatórios sem demanda explícita.

## 6. Checklist de QA da própria reorganização

Verificar antes do merge:
- [x] Índice central encontra GDD, pesquisa, coordenação, assets, plano e status;
- [x] Nenhum arquivo de gameplay, cena, asset ou `.meta` mudou;
- [x] `TO_CODEX.md` continua DBM-002, sem nova tarefa ou merge;
- [x] `FROM_CODEX.md` e `coordination/PROJECT_STATUS.md` preservados (históricos);
- [x] Links relativos adicionados existem na árvore da branch;
- [x] Pesquisa 01–07 continua acessível nas URLs antigas;
- [x] Não existem instruções novas apontando para `prompts/05_COMPILE_PROTOTYPE.md`;
- [x] Dados do conflito 660/570 e +82/+24 rotulados com procedência;
- [x] GitHub PR doc-only aberto para revisão, sem confundir docs com testes Unity.

**QA documental executado após as alterações:** 9 documentos de navegação/gestão (incluindo README raiz) e 8 documentos de pesquisa/blueprint conferidos; **0 links locais quebrados** na árvore da branch. Comparação `main...docs/reorganizacao-2026-10-17`: **15 arquivos alterados, todos .md** (nenhum C#, asset ou cena). [PR #3](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/3) criado. Estes controles **não substituem** compilação ou testes Unity.

**Veredicto:** documentação muito mais útil que no início, porém não completamente exaustiva e ainda dependente de implementação/experimentação. O benefício dessa revisão é tornar a próxima tarefa inequívoca e transformar propostas sobre LoL em contratos auditáveis.
