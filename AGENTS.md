# Regras permanentes — Dragon Ball MOBA (Codex / VS Code)

1. Antes de editar código, leia `docs/coordination/README.md`, `docs/coordination/TO_CODEX.md` e `docs/coordination/PROJECT_STATUS.md`. Leia apenas os documentos técnicos necessários à tarefa atual.
2. Faça **uma tarefa funcional por execução**. Não altere sistemas sem relação com o objetivo.
3. Preserve o Unity local configurado e o protótipo executável. Crie checkpoint/branch antes de mudanças extensas.
4. Não faça `git push --force`, nem apague histórico, branches, backups ou assets sem autorização.
5. Não publique assets de Dragon Ball/LoL extraídos, modelos de fãs sem licença de redistribuição, senhas, tokens, dados pessoais ou arquivos gerados do Unity. O repositório é público.
6. Códigos, cenas, prefabs e `.meta` necessários ao Unity devem ser versionados juntos. Mantenha downloads e caches fora do Git; documente dependências.
7. Execute testes apropriados; se não puder testar, registre explicitamente o impedimento. Não confunda compilar com comprovar jogabilidade.
8. Ao fim **de toda execução**, atualize `docs/coordination/PROJECT_STATUS.md` e `docs/coordination/FROM_CODEX.md` e crie um relatório em `docs/coordination/reports/` com commits, testes, evidências, pendências e próximo passo.
9. `TO_CODEX.md` é mantido pelo ChatGPT; **não o reescreva**. Dúvidas e sugestões devem ser registradas em `FROM_CODEX.md`.
10. O trabalho é assíncrono: só execute novas tarefas quando receber instruções pelo Codex/usuário. Não pressuponha que ChatGPT ou Codex sejam agentes continuamente ativos.
11. Economize contexto: leia diffs e arquivos estritamente relacionados; prefira patches pequenos; rode testes direcionados e resuma logs.
12. Nas tarefas de gameplay, mantenha separação entre regras de domínio, apresentação Unity e infraestrutura; evite reescrita ampla.
