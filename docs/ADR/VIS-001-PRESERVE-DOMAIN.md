# ADR VIS-001 — preservar domínio e evoluir apresentação
Data: 2026-10-16. Status: adotado.

A auditoria está em ../VISUAL_RESTRUCTURE_AUDIT.md.
Manter GameWorld/EntityState/Catalog e seus contratos; implementar câmera como componente
opcional da cena de arte. Evoluir terreno e modelos em marcos verificáveis.
Não integrar os frameworks de Unity2018/Unity5 e stacks externos de rede neste marco:
o ganho de migrar contratos/dependências/serviços não foi demonstrado.
Recursos de visão/arbustos e novos tipos de tropas precisam de lógica e testes próprios.
O modelo compartilhado com cores diferentes continua identificado como provisório.
Provas: checkpoint5046d28; baseline; regressão Cell; Play Mode e capturas720p/1080p.

