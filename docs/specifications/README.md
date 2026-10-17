# Especificações do Dragon Ball MOBA

Este diretório guarda documentos de design e engenharia que orientam o desenvolvimento.

## Importação inicial pelo Codex (DBM-000)

Localizar na pasta local `DragonBall_Moba` os documentos mestres e guias já produzidos (Word, Markdown, instruções de integração de assets e relatórios técnicos). Trazer **preferencialmente as versões Markdown mais recentes**, mantendo o conteúdo e registrando as fontes originais. Evitar arquivos duplicados, pacotes ZIP enormes e documentos que contenham informações privadas.

Sugestão de organização:

- `GAME_DESIGN.md` — regras, personagens, interface, loja, mapa, progressão, transformações e fusões.
- `TECH_ARCHITECTURE.md` — Unity/C#, arquitetura, código reutilizável, rede, testes e integração.
- `ART_PIPELINE.md` — modelos, rigs, animações, VFX e licenças.
- `CODEX_WORKFLOW.md` — desenvolvimento incremental e economia de contexto.

**Não inventar conteúdo ausente:** importar os documentos existentes e informar no relatório o que ficou pendente.

## Documentos importados — DBM-000

- [GAME_DESIGN.md](GAME_DESIGN.md): documento mestre de design, origem docs/GDD_MASTER.md do protótipo local.
- [TECH_ARCHITECTURE.md](TECH_ARCHITECTURE.md): guia de arquitetura, origem docs/ARCHITECTURE_GUIDE.md.
- [ART_PIPELINE.md](ART_PIPELINE.md): direção e critérios de arte, origem DragonBall_MOBA_Art_Starter_v1/docs/ART_DIRECTION.md.
- [VISUAL_DIRECTIVES.md](VISUAL_DIRECTIVES.md): diretrizes visuais posteriores preservadas no protótipo.
- [ASSETS_PRIORITARIOS.md](ASSETS_PRIORITARIOS.md): seleção planejada do kit de arte; não é uma lista de recursos efetivamente importados.
- [CODEX_WORKFLOW.md](CODEX_WORKFLOW.md): fluxo anterior de trabalho incremental e economia de contexto.

Conteúdo histórico preservado; propostas de URP, rede e arte não descrevem automaticamente o estado implementado. O projeto atual usa Built-in. AGENTS.md e docs/coordination/ têm precedência para tarefas e handoff. O uso real de recursos está em [ASSET_MANIFEST.md](../ASSET_MANIFEST.md).

Proveniência e hashes originais: [IMPORT_SOURCES.csv](IMPORT_SOURCES.csv). Fontes Word/ZIP duplicadas não foram publicadas. Os documentos importados não concedem direitos sobre personagens ou marcas citados.

## Pesquisa de referência e roadmap (16/10/2026)

- **[MOBA_RESEARCH_BLUEPRINT_2026.md](MOBA_RESEARCH_BLUEPRINT_2026.md)** — pesquisa técnica e científica do League of Legends, evolução de mecânicas, fórmulas de stats/dano/regeneração, itens e receitas, treinamento de Ki, seleção e kits de personagens, Oozaru, topologia do mapa, câmera, arquitetura modular, testes e ordem de implementação. **Documento de proposta, não código já implementado.** Complementa o GAME_DESIGN; respeitar o PR DBM-002 pendente antes de iniciar DBM-003.

**Revisão posterior:** [Pesquisa MOBA v2 — auditoria de fontes, especificação de mecânicas e arquitetura testável](../research/README.md). Ela complementa o documento mestre e sinaliza limites de evidência do blueprint inicial.
