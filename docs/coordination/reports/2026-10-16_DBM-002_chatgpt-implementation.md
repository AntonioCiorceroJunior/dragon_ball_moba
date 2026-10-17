# DBM-002 — implementação inicial ChatGPT: alcance, torres, aggro e colisões

**Responsável:** ChatGPT · **Estado:** CÓDIGO EM BRANCH; TESTES UNITY PENDENTES  
**Branch:** `chatgpt/DBM-002-towers-collisions` · **Base:** `main` após DBM-001 (commit de merge `067ed1ceebfdb8facb59667afec00fd3c1657883`).

## Alterações concretas

1. `GameWorld.cs`: torre com alcance explícito já existente (11,5 unidades), dano bruto aumentado de 68 para 125 (valor **provisório**, não balanceado), cooldown de torre descontado pelo `dt` real do tick em vez do `FixedStep` fixo. Seleção: priorizar tropas inimigas vivas dentro do alcance; atacar heróis quando não houver tropas; interromper alvos que saiam do alcance.
2. Provocação: quando um herói causa dano **efetivo** a um herói adversário dentro da área da torre que o defende, o defensor prioriza o agressor por até três segundos, desde que permaneça no alcance. Temporizador, ID e estado são guardados em `EntityState.cs`. Observação: modelo provisório de aggro; ainda não há memória de ameaça completa nem projéteis balísticos.
3. Movimento: torres e núcleos vivos possuem círculos sólidos para movimento; o domínio calcula colisão pelo segmento percorrido por tick e desvia tangencialmente, sem usar física do Unity. Clique diretamente no centro de uma estrutura é projetado para ponto externo; W avança por subpassos e não atravessa as estruturas.
4. `PrototypeGame.cs`: anel no terreno marcando o alcance de uma torre inimiga quando o jogador está próximo. **Somente visual**, não altera a regra de ataque.
5. `DomainSelfTests.cs`: novos testes TR-01..05 e COL-01..05 para dano dentro do alcance, desengajamento fora do alcance, prioridade por tropas, aggro de herói, desvio e estrutura destruída.
6. `docs/coordination/evidence/DBM-002/DOMAIN_BASELINE.csv` registra os hashes dos três arquivos de domínio no commit `270dfd7` (sem alterar `Definitions.cs`). `verify_publication.ps1` usa a referência DBM-002 quando presente, preservando versões DBM-001 e DBM-000.

## Geometria e limitações

- Raios provisórios: torre `1.45`, núcleo `1.65`, herói `0.5` e tropa `0.4`, em unidades de mundo. O alcance ofensivo da torre permanece `11.5` unidades (medidas centro a centro).
- Colisões implementadas **apenas contra torres e núcleos**, tanto aliados quanto inimigos; não incluem árvores, pedras procedurais, NPCs ou paredes de selva. Esses obstáculos precisam ser modelados em etapa futura, com dados de navegação autoritativos.
- A resolução de movimento por tangente é protótipo determinístico, não substitui NavMesh/A* completo para jungle 3D.
- Os aumentos de dano e números de aggro não foram balanceados para PvP.

## Verificação executada nesta sessão

- Revisão estática dos arquivos principais e equilíbrio de delimitadores C#; calibração independente em JavaScript de um caso de contorno circular, **não é teste Unity**.
- Hashes SHA-256 da referência calculados sobre conteúdo do GitHub (C# marcado com eol=lf); algoritmos verificados em vetores canônicos SHA-256. **O comando de publicação precisa confirmar a correspondência no clone Windows.**
- **Unity Editor, Play Mode, build Windows e partida real NÃO EXECUTADOS** por este ChatGPT.
- Não informar DBM-002 como concluída até passar os novos testes e validação visual/operacional.

## Validação pedida ao Codex após renovação de franquia

1. Sincronizar a branch do PR DBM-002 com o clone principal e confirmar Unity 6000.6.6f1. Não mexer na versão antiga do protótipo.
2. Executar `verify_publication.ps1` e `validate_unity.ps1` em `VisualPrepare`, `VisualPlay`, `VisualBuild`; `DomainSelfTests.RunAll` deve executar TR/COL.
3. No executável, testar aproximação de torre inimiga, indicação visual do anel, dano dentro de 11,5u, nenhum dano fora, prioridade de tropa, aggro ao atacar herói defendido, contorno de estrutura **com clique no chão**, W próximo à torre e passagem livre após destruição.
4. Corrigir falhas reproduzíveis **na mesma branch** e atualizar `FROM_CODEX.md`/`PROJECT_STATUS.md` e novo relatório, mantendo o PR em revisão sem merge automático.
5. Verificar regressões DBM-001 (autoataque e desbloqueios), formas e respawn; marcar não comprovados se não puder executar.

**Não iniciar DBM-003 nesta validação.** Economizar tokens; logs sintetizados.
