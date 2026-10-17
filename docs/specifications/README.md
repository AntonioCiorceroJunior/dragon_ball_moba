# Documentos de produto e design — Dragon Ball MOBA

**Revisão:** 17/10/2026. Esta pasta concentra a **visão de jogo** e guias de especialidade. Eles descrevem objetivos de produto e engenharia, não garantem funcionalidades presentes no executável.

## Fonte de design principal

**[GAME_DESIGN.md](GAME_DESIGN.md)** — documento mestre: experiência, telas, 15 variantes, kits, Ki, formas, Oozaru, compra/receitas, mapa, minions, visão, objetivos, fusão, multiplayer futuro e QA. **Preservado integralmente**; valores como HP de personagem são metas/sementes até uma decisão registrada, não automaticamente os números do código.

## Guias complementares

| Documento | Finalidade | Ressalva |
|---|---|---|
| [TECH_ARCHITECTURE.md](TECH_ARCHITECTURE.md) | arquitetura e tecnologias historicamente propostas | intenção antiga pode mencionar ferramentas/pipelines distintos do runtime Unity Built-in |
| [ART_PIPELINE.md](ART_PIPELINE.md) | modelos, rigs, animação, integração e licenças | manifesto real está em [ASSET_MANIFEST.md](../ASSET_MANIFEST.md) |
| [VISUAL_DIRECTIVES.md](VISUAL_DIRECTIVES.md) | direção visual e apresentação | não prova integração de todo modelo/skin |
| [ASSETS_PRIORITARIOS.md](ASSETS_PRIORITARIOS.md) | seleção/prioridades de assets | recursos desejados, **não** necessariamente importados |
| [CODEX_WORKFLOW.md](CODEX_WORKFLOW.md) | práticas históricas de trabalho incremental | protocolo atual é [coordination/README.md](../coordination/README.md) |
| [IMPORT_SOURCES.csv](IMPORT_SOURCES.csv) | proveniência dos arquivos importados na DBM-000 | preservar para auditoria |
| [MOBA_RESEARCH_BLUEPRINT_2026.md](MOBA_RESEARCH_BLUEPRINT_2026.md) | plano preliminar extenso e exemplos | não é a arquitetura ou fila executável vigente |

## Arquitetura e pesquisa vigentes

- **[PLANO_MESTRE_ATUAL.md](../PLANO_MESTRE_ATUAL.md)** — consolidação de arquitetura e ordem de implementação, sem substituir o GDD.
- **[Biblioteca de pesquisa](../research/README.md)** — evidência Riot, artigos e código aberto, auditoria de gameplay e limitações.
- **[DOCUMENTOS_FONTE_E_PRECEDENCIA.md](../DOCUMENTOS_FONTE_E_PRECEDENCIA.md)** — divergências entre valores do GDD, código real e blueprint, com decisões ainda em aberto.
- **[STATUS_ATUAL.md](../STATUS_ATUAL.md)** — funcionalidades comprovadas e PRs ainda pendentes.

## Histórico de importação

Na DBM-000, documentos mestres em Word/Markdown e arquivos de arte foram importados de versões locais; o Markdown foi priorizado e ZIPs duplicados não foram publicados. Esse procedimento está concluído e documentado em [relatório DBM-000](../coordination/reports/2026-10-16_DBM-000_publicacao-inicial.md). Não repetir tarefas de importação automaticamente.

Os documentos de trabalho não concedem direito de redistribuir personagens ou marcas de Dragon Ball/LoL.

[Central de documentação](../README.md).
