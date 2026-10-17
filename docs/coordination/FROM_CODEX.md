# Último retorno do Codex

**Data:** 2026-10-16 — Codex.  
**Tarefa:** DBM-000 — publicação inicial segura.  
**Status:** concluída; fontes e documentos publicados, domínio e mecânicas preservados.  
**Commit dos fontes:** `04e33a986c7c545c5ae855f7ba4ef1a246ec9a8f`. O commit seguinte contém este handoff, o relatório e a preservação dos bytes das licenças.  
**Relatório:** [2026-10-16_DBM-000_publicacao-inicial.md](reports/2026-10-16_DBM-000_publicacao-inicial.md).

- Unity: `src/client-unity`, Editor **6000.6.6f1**, Built-in e Input Manager legado.
- Cena: `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`.
- Cópia limpa passou em VisualPrepare, VisualPlay e VisualBuild. Executável abriu; captura real incluída.
- 19 arquivos C# do Unity preservados; três hashes de domínio conferidos. 26 arquivos importados CC0 e sete licenças incluídos; checkout dos 26 hashes conferido.
- Documentos mestres canônicos em `docs/specifications/`; dados locais removidos das cópias públicas. Builds, caches, backups, downloads e pesquisas externas não publicados.
- Git local sem credencial: publicação pelo conector GitHub autenticado, com commits descendentes da base e avanço de referência **sem força**, em vez de `git push` via HTTPS local.
- Nenhuma nova partida completa ou sequência nativa completa de QWER foi validada nesta tarefa. Os testes de Play Mode não aprovam o balanceamento, a progressão individual de habilidades ou o autoataque definitivo.

**Próximo passo:** ChatGPT revisar esta entrega e preparar **DBM-001 — corrigir progressão de habilidades e autoataque**. Não iniciado. `TO_CODEX.md` e a fila continuam sob responsabilidade do ChatGPT e não foram reescritos.
