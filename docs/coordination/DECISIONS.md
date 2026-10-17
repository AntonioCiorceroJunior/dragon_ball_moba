# Registro de decisões de arquitetura (ADR resumido)

## ADR-001 — GitHub como interface assíncrona ChatGPT-Codex
**Data:** 2026-10-16  
**Decisão:** `TO_CODEX.md` (instrução), `FROM_CODEX.md` (retorno), `PROJECT_STATUS.md` (estado) e relatórios versionados. O usuário inicia a execução do Codex.  
**Motivo:** rastreabilidade, retomada e economia de contexto.  
**Limitação:** publicar um documento não aciona o Codex automaticamente.

## ADR-002 — Preservar o protótipo e corrigir por etapas
**Data:** 2026-10-16  
**Decisão:** não reconstruir o jogo do zero; corrigir domínio/combate antes de ampliar assets.  
**Motivo:** protótipo offline já compila e possui sistemas iniciais.

## ADR-003 — Publicação consciente em repositório público
**Data:** 2026-10-16  
**Decisão:** publicar apenas materiais cuja redistribuição é permitida; segredos, caches, modelos não licenciados e assets comerciais não devem ser enviados.  
**Motivo:** segurança, direitos de uso e manutenção.

## ADR-004 — MOBA open source somente após prova de compatibilidade
**Data:** 2026-10-16  
**Decisão:** analisar candidatos externos, mas integrar apenas módulos justificados e testados, após revisão de licença.  
**Motivo:** projetos Unity antigos podem custar mais para migrar do que adaptar o núcleo atual.
