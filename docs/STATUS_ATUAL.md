# Estado verificável — Dragon Ball MOBA

**Última conferência GitHub:** 17/10/2026. Este arquivo é um **snapshot**; consultar o GitHub antes de qualquer mudança.

| Item | Situação confirmada | Evidência |
|---|---|---|
| Repositório | `AntonioCiorceroJunior/dragon_ball_moba`, `main` | [GitHub](https://github.com/AntonioCiorceroJunior/dragon_ball_moba) |
| Código Unity | `src/client-unity`, Unity 6000.6.6f1, Built-in, Input Manager legado | [Projeto](../src/client-unity/README.md) |
| DBM-000 | Publicação original completa; relatórios e verificações registrados | [Relatório](coordination/reports/2026-10-16_DBM-000_publicacao-inicial.md) |
| DBM-001 | **Integrada à main** pelo [PR #1](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/1), commit `067ed1ceebfdb8facb59667afec00fd3c1657883` | [Relatório Codex](coordination/reports/2026-10-16_DBM-001_codex-validacao.md) |
| DBM-002 | [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2) **aberto/draft, não mesclado**; altera torres/aggro/colisão; verificação no Unity ainda pendente conforme registro atual | [Tarefa ativa](coordination/TO_CODEX.md) |
| DBM-003 | Backlog: sistema de stats, itens e receitas | [Fila](coordination/TASK_QUEUE.md) e [plano](PLANO_MESTRE_ATUAL.md) |
| Pesquisa | 7 documentos temáticos + status de evidências; sem testes Unity nesta pesquisa | [Centro de pesquisa](research/README.md) |

## O que já foi validado — e o que NÃO

**Documentado como validado na DBM-001:**
- Importação, Play Mode, build Windows e 21 verificações de UI/teclado/mouse no executável; smoke de seleção, ataque automático após um clique e ranks de habilidades.
- As verificações foram controladas com sonda de teste em build de desenvolvimento. Isso **não é** partida completa com humanos nem prova de balanceamento competitivo.
- Os binários locais não ficam no repositório; a importação/build precisa ser repetida no computador atual quando Codex validar a DBM-002.

**Ainda pendente:**
- Aceite Unity/Windows da DBM-002 e merge posterior;
- Regeneração básica de HP fora da fonte, stats por fontes, dano completo, itens com receitas, ícones e loja;
- Seleção pré-partida obrigatória, kits distintos dos três personagens e Oozaru gigante;
- Mapa quadrado com rotas diagonais, selva, visão, objetivos, torres em camadas;
- Servidor multiplayer e playtests estatisticamente úteis.

## Ação operacional seguinte

**DBM-002 = única tarefa ativa no Codex.** O usuário informou que a franquia do Codex renovou para 100%; isso **não executa uma tarefa automaticamente**. Quando o usuário decidir, o Codex deve seguir [TO_CODEX.md](coordination/TO_CODEX.md), sincronizar PR #2, executar validações e reportar no PR, **sem merge automático**. Somente após revisão do relatório abrir DBM-003A.

Os arquivos `coordination/PROJECT_STATUS.md` e `FROM_CODEX.md` são snapshots imutáveis da última execução DBM-001 do Codex, escritos antes do merge; não são evidência de que o PR #1 ainda esteja aberto. Seu conteúdo não será reescrito por uma reorganização de documentação.

[Documentação principal](README.md) · [Revisão documental](REVISAO_DOCUMENTAL_2026-10-17.md).
