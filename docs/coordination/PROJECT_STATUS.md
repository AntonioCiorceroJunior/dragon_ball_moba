# Estado técnico — DBM-001 validada em branch

**Atualização:** 2026-10-16, Codex. **Estado:** importação, smoke, build e 21 verificações nativas passaram; HUD corrigido. **Aguardando revisão; sem merge.**

- Branch: `chatgpt/DBM-001-gameplay`; [PR #1](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/1).
- Implementação recebida: `089a48f5188496711b957c282776ed0fa44ec4b6`.
- Correção/evidências: `c8240a2ae33643a131ed97fe4f858a42d33acd1d`; o commit deste status acrescenta somente documentação.
- Instrução executada: TO_CODEX da main `cd64b35e6ac8663b1864ddae2549139d62025a2d`, sem reescrever a versão anterior na branch.
- [Relatório desta execução](reports/2026-10-16_DBM-001_codex-validacao.md); [histórico DBM-000](reports/2026-10-16_DBM-000_publicacao-inicial.md).

## Projeto e build real

Raiz Unity: **`src/client-unity`**, no clone ativo `Repositorio/dragon_ball_moba` da pasta principal. Editor **6000.6.6f1**, Built-in/Input Manager legado. Cena: `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`; mapa gerado em Play.

Executável: `src/client-unity/Builds/VisualTest/DragonMobaVisualTest.exe`; launcher `JOGAR_VISUAL_TEST.cmd`. Build Windows x64 Development, **168.510.049 bytes totais**, UTC **2026-10-17T00:59:53.6958770Z** (16/10, 21:59:53 em São Paulo). O selo continua VISUAL TEST 01; [build-info](evidence/DBM-001/build-info.json) e [hash do assembly](evidence/DBM-001/Build-identity.json) distinguem esta build da DBM-000. Binários ficam locais; build anterior preservada em Backups.

## Resultados verificados

| Camada | Resultado |
| --- | --- |
| Unity | VisualPrepare, VisualPlay e VisualBuild passaram |
| Domínio | Run chama RunAll; pontos/ranks/limites, autoataque, retomada após Q e respawn passaram |
| Play Mode | Três campeões/12 formas, animação, QWER, Ki/XP, loja, tropas, torres, câmera, morte/respawn, seleção e reinício |
| Janela Windows | 21 verificações via SendInput passaram; [snapshots](evidence/DBM-001/Native-results.json) |
| HUD | Sobreposição e texto cortado corrigidos; [captura](evidence/DBM-001/locked.png) |
| Publicação | 26 hashes CC0, sete licenças, .meta, pacotes e padrões comuns de dados privados conferidos |

Q foi rejeitada enquanto bloqueada; Ctrl+Q e botão + funcionaram. Q rank2 foi rejeitado no nível2 e aceito em3; R evoluiu em6/11/16. Um clique gerou cinco ataques (intervalo mínimo33 ticks/1,1s); depois de Q houve mais dois sem novo clique. Chão/C cancelaram. Corpo do inimigo Cell perfeito continuou selecionável. Respawn nível16 preservou ranks/pontos; menu/nova partida reiniciou a progressão.

Preparação controlada na janela: XP via GrantExperience, posições, inimigo estacionário sem ataques/técnicas, transformação inimiga e dano letal. Sonda somente com `-dbm001-native-test`, sob símbolos de Editor/desenvolvimento; não ativa ao jogar normalmente. Não altera ticks, cooldowns, ranks diretamente ou FormId. Não se declara partida completa manual nem teste prolongado.

## Preservação e revisão

O Codex não alterou os três arquivos de domínio recebidos do ChatGPT. A referência DBM-000 permanece histórica; a nova [DOMAIN_BASELINE.csv](evidence/DBM-001/DOMAIN_BASELINE.csv) identifica o commit aprovado DBM-001 e é reconhecida pelo verificador. Mapa, arte, torres/colisões, inventário e dependências não foram reimplementados. Saídas regeneradas foram arquivadas/restauradas para evitar ruído no PR.

**Próximo passo:** ChatGPT revisar PR #1 e decidir sobre merge. `main`, TO_CODEX e fila não foram modificados pelo Codex; DBM-002 não iniciada. Partida completa, desempenho prolongado e multiplayer seguem não comprovados.

## Acesso local para teste — 16/10/2026
Atalho **Dragon Ball MOBA - Testar** criado e verificado na Área de Trabalho, apontando à build Windows DBM-001 existente. Sem nova compilação ou alteração de gameplay; teste de abertura pelo usuário permanece disponível. O atalho contém caminho local e não é publicado no Git.
