# Último retorno do Codex — DBM-001

**Data:** 2026-10-16. **Estado:** validação direcionada concluída; aguardando revisão do ChatGPT. **Sem merge.**
**Branch:** `chatgpt/DBM-001-gameplay`; [PR #1](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/1).
**Commit de correção/testes:** `c8240a2ae33643a131ed97fe4f858a42d33acd1d`; este handoff é seu descendente direto.
**Relatório:** [2026-10-16_DBM-001_codex-validacao.md](reports/2026-10-16_DBM-001_codex-validacao.md).

- Unity 6000.6.6f1, `src/client-unity`, Built-in/Input Manager legado; cena VisualTest existente.
- VisualPrepare, VisualPlay e VisualBuild passaram. Confirmada chamada de RunAll; testes de retomada após cast e ranks/pontos no respawn acrescentados.
- **21 verificações na janela real passaram** via Windows SendInput: Ctrl+Q, botão +, ranks, R 6/11/16, um clique seguido de ataques a cada 33 ticks (1,1 s), cancelamento chão/C, retomada após Q, corpo transformado, respawn e reinício.
- Corrigido bug constatado no HUD: título/+ sobrepostos e texto de bloqueio cortado.
- XP, posições, transformação inimiga e dano letal foram preparados por sonda opt-in de desenvolvimento; os comandos de jogo vieram de mouse/teclado reais. Não se declara partida completa jogada por humano.
- Build testada: `src/client-unity/Builds/VisualTest/DragonMobaVisualTest.exe`; **168.510.049 bytes totais**, UTC `2026-10-17T00:59:53.6958770Z`. `JOGAR_VISUAL_TEST.cmd` não ativa a sonda.
- [Resultados nativos](evidence/DBM-001/Native-results.json), [eventos](evidence/DBM-001/Native-events.txt), [HUD corrigido](evidence/DBM-001/locked.png). Assets/licenças preservados; nenhum pacote instalado.

Instrução seguida: TO_CODEX de `main` em `cd64b35e6ac8663b1864ddae2549139d62025a2d`; a versão anterior na branch não foi reescrita. `main` não recebeu commits desta execução. Publicação pelo conector autenticado, sem força.

**Próximo passo:** ChatGPT revisar o PR e decidir ajustes/merge. Não iniciar DBM-002. Histórico preservado nos relatórios DBM-000.
