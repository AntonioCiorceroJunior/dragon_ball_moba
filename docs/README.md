# Documentação oficial — Dragon Ball MOBA

**Reorganizada em 17/10/2026.** Esta é a porta de entrada para consultar o projeto sem confundir visão futura, código real, hipóteses de pesquisa e relatórios históricos.

## Escolha o que precisa fazer

| Necessidade | Documento principal | O que encontrar |
|---|---|---|
| Saber o estado real do projeto | [STATUS_ATUAL.md](STATUS_ATUAL.md) | Main, PRs, builds/testes documentados e próximos gates |
| Entender **o que vamos construir e em qual ordem** | [PLANO_MESTRE_ATUAL.md](PLANO_MESTRE_ATUAL.md) | Arquitetura consolidada, prioridades, decisões e dependências |
| Descobrir qual documento tem precedência | [DOCUMENTOS_FONTE_E_PRECEDENCIA.md](DOCUMENTOS_FONTE_E_PRECEDENCIA.md) | Fonte da verdade, conflitos e convenção de status |
| Consultar a visão completa do jogo | [specifications/GAME_DESIGN.md](specifications/GAME_DESIGN.md) | 15 variantes, Ki, habilidades, personagens, mapa, fusão e sistema de partida |
| Estudar LoL e engenharia de MOBAs | [research/README.md](research/README.md) | Pesquisa, evidências, fórmulas, código aberto, hipóteses e lacunas |
| Ver instrução ativa do Codex | [coordination/TO_CODEX.md](coordination/TO_CODEX.md) | Tarefa DBM-002, PR e validação Unity |
| Ver backlog e relatórios | [coordination/README.md](coordination/README.md) | DBM-000…010, decisões, handoffs e testes |
| Ver arte, modelos e licenças | [ASSET_MANIFEST.md](ASSET_MANIFEST.md) e [specifications/ART_PIPELINE.md](specifications/ART_PIPELINE.md) | O que foi importado e o que falta |

## Hierarquia de trabalho

1. **Código + estado remoto GitHub + testes de build:** o que realmente existe, funciona e foi verificado. O resultado mais recente de uma branch ainda NÃO está na main.
2. **Tarefa ativa e coordenação:** `AGENTS.md`, `coordination/TO_CODEX.md`, `TASK_QUEUE.md`, relatórios e evidências correspondentes. Não transformar backlog de pesquisa em tarefa ativa.
3. **Visão de produto:** `specifications/GAME_DESIGN.md`; determina a experiência desejada, mas seus números são propostas até serem aprovados e implementados.
4. **Plano técnico consolidado:** `PLANO_MESTRE_ATUAL.md`; coordena implementação sem substituir o GDD.
5. **Pesquisa:** sete documentos e matriz de evidência `research/`, usados para justificar escolhas e indicar hipóteses ainda não testadas.
6. **Histórico:** relatórios antigos e o blueprint preliminar ficam preservados; não orientam o Codex quando divergirem do protocolo vigente.

**Não há dois documentos igualmente oficiais para a mesma finalidade.** Se houver conflito, registrar a diferença em [DOCUMENTOS_FONTE_E_PRECEDENCIA.md](DOCUMENTOS_FONTE_E_PRECEDENCIA.md) ou nas [decisões](coordination/DECISIONS.md) antes de alterar o jogo.

## Situação ao reorganizar

- **DBM-000:** publicada e testada no Windows por Codex.
- **DBM-001:** PR #1 **mesclado** na main (commit `067ed1c`); testes dirigidos no executável publicados.
- **DBM-002:** [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2) **aberto como rascunho**, código de torres/colisões proposto, ainda sem aceite Unity registrado na main.
- **DBM-003A (estatísticas/regen):** próxima proposta arquitetural, **não iniciada**.
- A pesquisa LoL / MOBAs ampliou documentação, **não implementou esses sistemas**.

## Leitura econômica

- **Usuário:** `STATUS_ATUAL` → `PLANO_MESTRE_ATUAL` → detalhe específico.
- **ChatGPT:** diff/estado GitHub → `coordination/` → seção relevante de `research/`.
- **Codex:** `AGENTS.md` → `coordination/TO_CODEX.md` → arquivos relacionados ao ticket; **não ler toda a biblioteca** a cada execução.

[Relatório desta revisão e lacunas remanescentes](REVISAO_DOCUMENTAL_2026-10-17.md).
