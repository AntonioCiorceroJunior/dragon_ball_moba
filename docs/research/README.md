# Biblioteca de pesquisa MOBA — índice por assunto

**Revisão editorial:** 17/10/2026. Os documentos 01–07 foram preservados nos caminhos originais para não quebrar links e citações compartilhados. **Pesquisa não é especificação aprovada nem código executável.**

**Consulta recomendada:** [Plano mestre atual](../PLANO_MESTRE_ATUAL.md) para ordem de trabalho; [política de precedência](../DOCUMENTOS_FONTE_E_PRECEDENCIA.md) para diferenciar fatos, propostas e histórico; [estado da pesquisa](STATUS_INVESTIGACAO.md) para lacunas e níveis de evidência.

## 1. Referências e ciência: por que LoL evoluiu

| Documento | Aplicação principal | Limite |
|---|---|---|
| [01 — Fontes oficiais, evolução e estudos](01_FONTES_AUDITADAS_E_EVOLUCAO_MOBA.md) | Riot /dev, patches, engineering, papers, métodos e licenças | Mistura textos integrais e abstracts; ler classificação/limitações |
| [07 — Longitudinal e BalanceLab](07_ESTUDOS_LONGITUDINAIS_BALANCEAMENTO_EXPERIMENTAL.md) | Estudo de patches LoL e postmortem Riot; desenho de experimentos A/B | Não reproduz dataset nem valida nossos personagens com humanos |

## 2. Sistemas de gameplay

| Documento | Perguntas que ajuda a responder |
|---|---|
| [02 — Especificações de mecânicas](02_SISTEMAS_MOBA_ESPECIFICACAO_IMPLEMENTAVEL.md) | Que sistemas de stats, combate, economia, Ki, seleção, UI, mapa e visão queremos? |
| [04 — Auditoria de combate, progressão e itens](04_AUDITORIA_PROFUNDA_COMBATE_PROGRESSAO_ECONOMIA.md) | O que o código atual faz em `GameWorld`? Como provar regen, mitigação, receitas e life steal? |
| [05 — Navegação, torres, waves e câmera](05_AUDITORIA_MOVIMENTO_TORRES_WAVES_MAPA_CAMERA.md) | Como evoluir pathfinding/aggro/mapa e que lacunas restam no PR #2? |

## 3. Engenharia e reuso de código

| Documento | Como utilizar |
|---|---|
| [06 — Leitura de MOBA C# aberto](06_AUDITORIA_CODIGO_ABERTO_MOBA_CSHARP_UNITY.md) | Exemplos reais de máquina de ataque, stats derivados, itens e minions; **não importar código antigo sem testes/licença** |
| [03 — Arquitetura e plano de testes da pesquisa v2](03_ARQUITETURA_BACKLOG_E_PROTOCOLO_DE_ACEITE.md) | Contratos e sugestões detalhadas por sistema; **sequência de IDs é histórica/proposta**, não fila ativa |

## 4. Documento extenso anterior (não usar como fonte única)

[Blueprint MOBA 2026 — preliminar](../specifications/MOBA_RESEARCH_BLUEPRINT_2026.md) foi escrito antes das auditorias mais específicas e permanece para consulta de ideias, tabelas e exemplos. Pode usar IDs de tarefa e valores diferentes do plano vigente. **Em divergências, comparar GDD, código, decisões e [plano consolidado](../PLANO_MESTRE_ATUAL.md), nunca sobrescrever silenciosamente.**

## 5. O que já é sólido e o que ainda não foi demonstrado

**Material disponível:** fontes históricas Riot com patches identificados; auditoria de métodos do domínio; leitura de classes C# de terceiros; fórmulas candidatas; exemplos de counterbuild; contrato de mapa; matriz de testes.

**Ainda não comprovado:** acesso ao código proprietário de LoL, leitura integral de todos os estudos citados, simulações de meta e partidas reais do nosso jogo, código novo de HPregen/receitas/vampirismo, IA de visão, teste de unidade/nav de terceiros em Unity6 e licença de personagens/vozes.

Consulte [STATUS_INVESTIGACAO.md](STATUS_INVESTIGACAO.md). Classificação E1/E2 indica evidência documental/estática; E3/E4 exige teste executado. Esta reorganização não alterou o grau real de evidência.

## 6. Método para continuar a pesquisa

Cada novo dossiê deve declarar: **pergunta, escopo, patch/versão, fonte primária com URL, método/trecho efetivamente consultado, achados, hipóteses, limites e teste falsificável**. Links para páginas pagas não autorizam afirmar leitura integral. Registrar auditoria de bibliografia e resultados de experimento, não apenas novas listas de recursos.

**Próximas pesquisas ainda abertas:** visão/fog/wards, servidor/snapshot/replay, pipeline de rig/VFX/voz e licenças, economia de jungle/objetivos/late game, mais artigos completos e playtests. Elas não bloqueiam a validação da DBM-002.

[Voltar à central](../README.md).
