# Comece aqui — Dragon Ball MOBA (17/10/2026)

**Guia atual de navegação.** As antigas instruções do protótipo v0.1 (que apontavam para `prompts/05_COMPILE_PROTOTYPE.md`) foram superadas; não existe esse diretório na `main` atual.

## Para acompanhar o projeto

1. [Central de documentação](README.md): caminhos para visão de produto, pesquisa, plano, status e arte.
2. [Estado atual do GitHub](STATUS_ATUAL.md): DBM-001 já mesclada; DBM-002 em [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2), ainda aguardando validação.
3. [Plano mestre vigente](PLANO_MESTRE_ATUAL.md): próximos módulos de stats/regen, dano, itens, Ki, personagens, câmera e mapa.
4. [Revisão documental](REVISAO_DOCUMENTAL_2026-10-17.md): lacunas, contradições e regras de precedência.
5. [Pesquisa aprofundada](research/README.md): sete dossiês, evidências e limites.

## Para abrir o jogo

- No Unity Hub, adicionar **`src/client-unity`** do clone Git; não abrir a raiz do repositório como projeto Unity.
- Editor **Unity 6000.6.6f1**, **Built-in** com Input Manager legado.
- Cena: `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`.
- A build Windows é gerada localmente por `scripts/validate_unity.ps1 -Mode VisualBuild`, não baixada do GitHub.

Consulte [README principal](../README.md) e [README do Unity](../src/client-unity/README.md) para comandos detalhados. As capturas e builds do protótipo são histórico de versões, não prova de que os sistemas futuros já existem.

## Para usar o Codex

**Uma tarefa ativa:** [coordination/TO_CODEX.md](coordination/TO_CODEX.md) (DBM-002). Antes de iniciar, leia [AGENTS.md](../AGENTS.md), confirme `git status` e sincronize a branch correta; não trate o novo plano mestre como um segundo prompt de implementação.

A quota do Codex não altera o conteúdo da tarefa: o usuário decide quando iniciar a execução. Depois da validação, Codex publica relatório e ChatGPT revisa antes do merge.

**Não abrir tickets antigos `prompts/01_FIRST_TASK.md` ou `prompts/05_COMPILE_PROTOTYPE.md`**, que não estão na estrutura atual.
