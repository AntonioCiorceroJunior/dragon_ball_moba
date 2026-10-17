# Estado técnico do projeto

**Última atualização:** 2026-10-16 — Codex, DBM-000.  
**Estado:** projeto-fonte Unity e documentos mestres importados; cópia limpa compilada e testada.  
**Commit dos fontes:** `04e33a986c7c545c5ae855f7ba4ef1a246ec9a8f`; **handoff verificado no clone remoto:** `684b5e467a48211b2d88fda98e47daef0d38f519`.  
**Relatório:** [DBM-000 — publicação inicial](reports/2026-10-16_DBM-000_publicacao-inicial.md).

**Conferência posterior:** [clone novo do GitHub](reports/2026-10-16_DBM-000_verificacao-remota.md) passou nas verificações de publicação e integridade. O commit desta atualização contém apenas documentação e provas adicionais.

## Projeto e execução

- Raiz real do Unity no repositório: **`src/client-unity`**. Na pasta de trabalho original, a cópia ativa está em `Repositorio/dragon_ball_moba/src/client-unity`.
- Editor: **Unity 6000.6.6f1**; Built-in; `activeInputHandler: 0`.
- Cena: `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`.
- Identificação: **VISUAL TEST 01 - Organic Arena**. Mapa gerado durante Play.
- Build local: `src/client-unity/Builds/VisualTest/DragonMobaVisualTest.exe`; executar `JOGAR_VISUAL_TEST.cmd` após compilar. Binários não estão no Git.
- Build DBM-000: **168.500.853 bytes totais**, em **2026-10-17T00:21:32.7697390Z** (16/10 às 21:21:32, São Paulo).
- Comando, na raiz do clone: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualBuild`.

## Verificações desta execução

| Verificação | Resultado e prova |
| --- | --- |
| Importação e preparação na cópia limpa | VisualPrepare passou |
| Play Mode na cópia limpa | Três campeões/12 formas, animação, combate, Ki/XP, compra, câmera, tropas, torres, morte/respawn, seleção e reinício passaram |
| Compilação Windows x64 Development | VisualBuild passou; [build-info](evidence/DBM-000/build-info.json) |
| Executável recém-compilado | Abriu em 1280 × 720; [captura real](evidence/DBM-000/Executable-1280x720.png) |
| Código preservado | No clone remoto, 19 arquivos C# com o mesmo conteúdo; 14 iguais byte a byte e cinco apenas normalizados de CRLF para LF; três hashes de domínio iguais |
| Publicação e licenças | Scanner de padrões comuns, tamanhos, `.meta`, pacotes, sete licenças e 26 hashes CC0 passou; checkout dos hashes passou |
| Integridade remota | Árvore do commit dos fontes igual ao índice revisado: `06dfbbb6653deb2adfa7706d3574fdf1f9c9ab6e` |
| Clone independente da main | Passou: 450 arquivos, 176 `.meta`, 26 hashes CC0, sete licenças e três hashes de domínio; snapshot `684b5e4` |

Evidências resumidas em [evidence/DBM-000/](evidence/DBM-000/). Logs completos ficam locais em `src/client-unity/Logs/`; não foram publicados por conterem dados do ambiente. A verificação de padrões comuns não constitui auditoria completa de segurança.

## Conteúdo e dependências

Domínio C# continua isolado de UnityEngine: `Definitions.cs`, `EntityState.cs`, `GameWorld.cs`. O jogo é offline; não foi implementado servidor remoto. Arquivos de código, cenas, prefabs, materiais, shaders, `.meta`, Packages e ProjectSettings foram incluídos.

Humanoide/animações/natureza Quaternius e recursos Kenney têm avisos CC0 incluídos. Nenhum modelo oficial, modelo de fã não autorizado, asset extraído de jogos, toolkit externo ou pacote da Asset Store foi incorporado nesta tarefa. [Manifesto](../ASSET_MANIFEST.md).

Documentos mestres: [specifications/README.md](../specifications/README.md). Propostas históricas nesses documentos não equivalem a funcionalidades concluídas; Built-in permanece como implementação atual. `AGENTS.md`, `TO_CODEX.md`, decisões e fila remotos foram preservados.

## P0 e verificações que permanecem abertas

- **DBM-001:** progressão individual de Q/W/E/R, desbloqueios e autoataque/perseguição contínua; ainda não corrigidos nesta tarefa.
- Torres, aggro, alcance e colisões: DBM-002; loja/inventário completo: DBM-003.
- Partida completa, balanceamento, testes prolongados e multiplayer não comprovados.
- Testes controlados em Play Mode não substituem entrada física completa nem aceitação de gameplay.

## Próxima ação

DBM-000 entregue para revisão. Aguardar instrução do usuário/ChatGPT para **DBM-001 — corrigir progressão de habilidades e autoataque**. Não executar automaticamente outra tarefa nem reescrever `TO_CODEX.md`.
