# Tarefa ativa na branch integrada — DBM-002 + BATCH-001

**Data:** 17/10/2026 · **Branch de validação:** `chatgpt/moba-integrated-systems-2026-10-17` · [PR #4](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/4), empilhado sobre [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2).

**Instrução operacional vigente neste checkout:**
[**TO_CODEX_BATCH_001.md**](TO_CODEX_BATCH_001.md).

O arquivo de tarefas herdado da DBM-001 estava desatualizado nesta branch (PR #1 já foi integrado à main). Foi substituído por esta referência para impedir que o Codex retome a tarefa antiga.

## Entrega esperada em uma única rodada do Codex

1. Sincronizar a branch acima de maneira segura, sem descartar alterações locais.
2. Executar publicação, smoke .NET, domínio e arte no Unity 6000.6.6f1, build Windows e testes nativos de DBM-001/DBM-002.
3. Fazer uma revisão manual da seleção, dos três personagens, lojas/receitas, stats/regen, dano, Ki, torre/colisões, Oozaru provisório, câmera, **minimapa interativo** e ataque-movimento.
4. Corrigir erros concretos **na mesma branch**, com testes e commits.
5. Publicar relatório de execução e evidências em `docs/coordination/reports/`, atualizar `FROM_CODEX.md` e `PROJECT_STATUS.md` da branch.
6. **Não mesclar** PR #2 ou PR #4; aguardar revisão do ChatGPT.

**Nota:** o `TO_CODEX.md` da main é administrado separadamente. Esta alteração pertence somente ao PR #4 e não ordena início automático de Codex nem substitui a necessidade de o usuário acionar a execução.
