# Instrução ativa para o Codex

**ID:** DBM-000  
**Título:** Publicar o projeto Unity existente no GitHub com segurança  
**Status:** PRONTO PARA EXECUÇÃO  
**Repositório:** https://github.com/AntonioCiorceroJunior/dragon_ball_moba  
**Responsável pela execução:** Codex no VS Code (máquina local)

## Contexto
O projeto Unity funcional está no computador do usuário, dentro da pasta `DragonBall_Moba`, com scripts, cenas, assets já integrados e testes. O repositório remoto foi recém-criado, contém arquivos de coordenação e **é público**. O ChatGPT não tem acesso direto às pastas locais, portanto o Codex deve fazer a sincronização.

## Entrega obrigatória
Publicar **o projeto-fonte Unity utilizável** (código C#, cenas, prefabs, materiais próprios/licenciados, configurações, `.meta`, scripts de teste e documentação), evitando arquivos regeneráveis, backups, instaladores e conteúdo que não possa ser redistribuído.

## Etapas
1. **Inventário, sem alterar nada:** localizar a raiz atual do Unity (contém `Assets`, `Packages`, `ProjectSettings`), a raiz atual do Git local, o `.gitignore`, cenas e scripts de build. Listar tamanhos e assets de terceiros.
2. **Segurança/licenças:** verificar se há tokens, senhas, caminhos pessoais, arquivos sensíveis, modelos extraídos de jogos ou assets de redistribuição não autorizada. Se identificar arquivos incertos, **não fazer push deles**: registrar bloqueio e alternativa. Confirmar que o usuário está ciente de que o repositório é público; não tornar público material duvidoso.
3. **Sincronização sem destruir histórico:** a pasta local já pode conter um `.git` com commits próprios, sem ancestral comum com `origin/main`. **Nunca usar `--force` nem `reset --hard`**. Preferir clonar `origin/main` para uma pasta de trabalho limpa e copiar somente os fontes aprovados, excluindo `.git` interno e arquivos ignorados. Preservar os caminhos relativos necessários para scripts funcionarem; se reorganizar, atualizar scripts/README.
4. **Preservar coordenação:** manter todos os arquivos de `docs/coordination/` e `AGENTS.md` da branch `main`; não substituí-los por cópias antigas. O código pode ir para `game/` ou para uma estrutura lógica documentada, conforme o layout real do projeto.
5. **Preparar projeto versionável:** usar o `.gitignore` existente no repositório; incluir `.meta` e versões de `ProjectSettings`/`Packages` relevantes. Dependências externas não redistribuíveis devem ficar documentadas para instalação independente. Evitar Git LFS ou grandes pacotes sem necessidade e sem validação de licença.
6. **Validar:** confirmar que Unity Hub abre o caminho publicado, compilar se possível e executar testes direcionados. Se houver limitações de custo/tempo, registrar precisamente o que foi e não foi testado.
7. **Publicar com segurança:** fazer `git status`/`git diff --cached --stat` e revisar conteúdos antes do commit; commit com mensagem clara; `git push` normal. Se remoto avançou, buscar atualizações e resolver conflito preservando trabalho do outro agente.
8. **Handoff:** atualizar `docs/coordination/PROJECT_STATUS.md` e `docs/coordination/FROM_CODEX.md`; criar `docs/coordination/reports/YYYY-MM-DD_DBM-000_publicacao-inicial.md`. Citar commit, árvore de pastas, origem dos assets, validações, arquivos excluídos e bloqueios.

## Critérios de aceitação
- Repositório contém fonte Unity suficiente para outro desenvolvedor abrir o projeto, ou impedimentos documentados com instruções para instalar dependências externas.
- GitHub preserva documentos de coordenação e o histórico existente.
- Build/cena em uso, editor Unity e método de execução identificados no README/status.
- Nenhuma credencial, cache grande ou asset com direito de redistribuição não comprovado foi publicado.
- Relatório entregue e apontando explicitamente próximo passo: **DBM-001 — corrigir progressão de habilidades e autoataque**.

## Restrições
Não refazer arte, mapa ou mecânicas agora. Não migrar o Unity. Não adicionar frameworks externos nesta tarefa. Não reescrever o código que já funciona. Se houver conteúdo com licença incerta, interromper apenas o envio dele; continuar tudo o que for seguro.
