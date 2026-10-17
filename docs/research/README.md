# Centro de pesquisa — Dragon Ball MOBA

**Data:** 16/10/2026. **Status:** pesquisa documental e plano de arquitetura auditáveis, sem alterar gameplay.

## Leitura recomendada em ordem

1. **[01 — Fontes auditadas e evolução do LoL](01_FONTES_AUDITADAS_E_EVOLUCAO_MOBA.md)**: separa evidência oficial, artigo técnico, estudo acadêmico, repositório aberto e hipótese. Inclui registro verificável de fontes, metodologia, limites dos papers e aspectos legais.
2. **[02 — Especificação implementável dos sistemas de partida](02_SISTEMAS_MOBA_ESPECIFICACAO_IMPLEMENTAVEL.md)**: stats, regen, dano, lifesteal, escudos, abilities, itens/receitas, treinamento com C, seleções, Oozaru, mapa, pathfinding, visão, objetivos, câmera, UI e áudio.
3. **[03 — Arquitetura, backlog e protocolo de aceite](03_ARQUITETURA_BACKLOG_E_PROTOCOLO_DE_ACEITE.md)**: módulos C#/Unity, migração incremental, diferenças com o código existente, sequência concreta de tickets, contrato de teste, critérios de sucesso e pesquisas pendentes.

## Novos dossiês de auditoria em nível de código

4. **[04 — Combate, atributos, progressão e economia](04_AUDITORIA_PROFUNDA_COMBATE_PROGRESSAO_ECONOMIA.md)**: matriz de evidência Riot 2019–2026, leitura de métodos e números atuais do GameWorld, fórmulas propostas, inventário/receitas, riscos e casos de teste determinísticos. Os cálculos são separados entre regra do nosso código e proposta.
5. **[05 — Movimento, torres, waves, mapa e câmera](05_AUDITORIA_MOVIMENTO_TORRES_WAVES_MAPA_CAMERA.md)**: confronto com Riot Tech Swarm/A*, patch de waves, colisões e tower aggro da branch DBM-002, riscos específicos de retarget, proposta de pathfinding e testes do mapa original.
6. **[06 — Auditoria de código-fonte de MOBA open source](06_AUDITORIA_CODIGO_ABERTO_MOBA_CSHARP_UNITY.md)**: leitura real de 13 arquivos C# do projeto MOBA_CSharp_Unity (MIT), incluindo máquina de ataque, agregação de status, item factory, torre, minion e riscos de lógica/performance. Não é alegação de build testado.

Estes dossiês são **mais específicos** que o blueprint v1/v2 e apresentam hipóteses que podem ser refutadas. Ainda existem áreas não auditadas integralmente, como todas as variantes do LoL, artigos pagos integrais, rede/servidor, o restante do código open source e balanceamento real por partidas humanas.

7. **[07 — Pesquisas longitudinais e balanceamento experimental](07_ESTUDOS_LONGITUDINAIS_BALANCEAMENTO_EXPERIMENTAL.md)**: leitura da íntegra do paper de He et al. (2021), cotejo com postmortem Riot de 2025, distinção entre causalidade e correlação, roteiro de experimento A/B e métricas do nosso jogo. Não é uma metanálise de todos os estudos existentes.

## Relação com documentos anteriores

- [GAME_DESIGN.md](../specifications/GAME_DESIGN.md) continua sendo a **visão original de produto**, com 15 variantes, fichas, fusão, Oozaru e loja. A pesquisa v2 não o substitui.
- [MOBA_RESEARCH_BLUEPRINT_2026.md](../specifications/MOBA_RESEARCH_BLUEPRINT_2026.md) é uma **proposta preliminar** extensa. A pesquisa v2 corrige referências e distingue o que foi efetivamente verificado. Em caso de conflito de verificação de fonte, prevalece o registro v2; em caso de conflito de produto, registrar decisão antes de implementar.
- A instrução ativa para o Codex permanece [DBM-002 em TO_CODEX.md](../coordination/TO_CODEX.md); [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2) ainda depende de validação Unity. Não iniciar DBM-003 sem aprovação do fluxo.

## O que este conjunto não afirma

- **Não** lemos o código proprietário completo da Riot, e estes documentos não oferecem acesso a ele.
- **Não** foram executados testes Unity/Windows, nem partidas humanas nesta pesquisa.
- O texto integral de vários artigos acadêmicos não estava disponível gratuitamente; as interpretações são fundamentadas em **resumos verificáveis**, marcadas como tal.
- As propostas de números, nomes de itens, mapa, balanceamento e escalas são nossas, com avaliação quantitativa ainda pendente.
- A licença de um repositório GitHub não implica automaticamente licença de seus modelos 3D, imagens, vozes e músicas.
- A existência de documentação não significa que as funcionalidades estão no jogo.

## Próxima porta técnica

**DBM-002 → DBM-003A (estatísticas + regeneração de vida)**: primeiro testar/mesclar torres/colisões; depois centralizar cálculo de atributos para permitir espadas, escudos, resistência de Ki, roubo de vida e receitas sem efeitos duplicados.

## Protocolo de atualização de pesquisa

Quando houver novo estudo/patch ou evidência, registrar **data/versão, fonte, afirmação específica, aplicabilidade, mudança no plano e teste para falsificar hipótese**. Não substituir números silenciosamente. A fase de playtesting deverá complementar fontes publicadas com dados coletados do nosso próprio jogo.
