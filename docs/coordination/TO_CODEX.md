# Tarefa ativa — DBM-001: validar o PR #1 no Unity e corrigir regressões

**Status:** CÓDIGO IMPLEMENTADO PELO CHATGPT — AGUARDANDO CODEX / TESTES REAIS  
**PR:** https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/1  
**Branch do código:** `chatgpt/DBM-001-gameplay`  
**Base:** `main` (não modificar `main` antes da revisão)  
**Unity:** 6000.6.6f1 · Built-in · Input Manager legado.

## Contexto

O ChatGPT já alterou diretamente o código-fonte publicado no GitHub na branch `chatgpt/DBM-001-gameplay` e abriu o PR #1 como rascunho. **Não é necessário reimplementar a tarefa do zero.** O trabalho de Codex agora é: sincronizar, compilar, executar os testes, verificar gameplay com mouse/teclado, corrigir falhas reais e publicar resultados.

### Implementado no PR

- `EntityState`: pontos e ranks de habilidade.
- `Definitions`: comando para evoluir habilidades.
- `GameWorld`: autorização de upgrade/rank, ultimate nível 6/11/16, auto-upgrade legal do bot, bloqueio do cast rank0, ganhos por rank e preservação do autoataque após cast.
- `PrototypeGame`: Ctrl+Q/W/E/R, botões +, HUD de ranks/pontos, raycast de inimigos.
- `DomainSelfTests` / `ArtSmoke`: testes e smoke adaptados.
- Relatório de implementação (branch): `docs/coordination/reports/2026-10-16_DBM-001_chatgpt-implementation.md`.

## Passos obrigatórios do Codex

1. Localizar o clone ativo `Repositorio/dragon_ball_moba`. Fazer `git status`; preservar modificações não commitadas, se houver.
2. Buscar atualizações via `git fetch` e trocar com segurança para a branch existente `chatgpt/DBM-001-gameplay`. Se falta credencial Git HTTPS, usar conector GitHub autorizado. Não criar branch concorrente para reimplementar a mesma tarefa.
3. Ler `AGENTS.md`, este documento da `main`, o relatório da implementação na branch, e os diffs do PR #1.
4. Rodar com Unity **fechado no Editor**:
   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPrepare
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPlay
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualBuild
   ```
5. Conferir que `DomainSelfTests.RunAll` e o smoke `ArtSmoke` realmente rodam na validação; se algum não rodar, executá-lo explicitamente. Corrigir compilação/testes que falharem. Registrar logs úteis sem caminhos privados.
6. **Teste manual no executável novo**:
   - Começa com ponto1 e Q/W/E/R bloqueadas; Q não gasta Ki enquanto bloqueada.
   - Ctrl+Q desbloqueia Q; uso gasta Ki, tem cooldown.
   - XP concede pontos; Q rank2 só no nível3; R somente 6/11/16. Botões + funcionam.
   - Clique direito em inimigo **uma vez**: desloca até o alvo e acerta várias vezes no ritmo correto. Clicar no chão e segurar C cancelam ataques.
   - Usar técnica não precisa fazer novo clique para voltar a atacar o alvo válido.
   - Selecionar no corpo do personagem 3D de fato funciona, inclusive após transformação.
   - Reinício e respawn respeitam ranks/pontos; torres, tropas, loja, transformações continuam funcionando como antes.
7. Capturar evidências (texto dos eventos, logs e screenshots do Unity/executável quando possível). Corrigir bugs encontrados **na mesma branch**, com commits vinculados ao PR #1.
8. Criar relatório complementar `docs/coordination/reports/YYYY-MM-DD_DBM-001_codex-validacao.md`, atualizar `docs/coordination/FROM_CODEX.md` e `PROJECT_STATUS.md` **na branch** com resultados concretos, commits, evidências, limitações e próximo passo.
9. Enviar link do PR e resumo; **não executar merge automaticamente** e não iniciar DBM-002 sem revisão.

## Regras

- Não alterar estrutura do mapa, pipeline de arte, multiplayer, torres/colisões ou inventário nesta tarefa.
- Testes no domínio não substituem cliques físicos na aplicação. Indicar explicitamente o que foi `passou`, `falhou` ou `não verificado`.
- Economizar contexto: corrigir os erros encontrados, sem fazer refatorações grandes ou revisitar todos os documentos.
- Não usar push forçado, reset destrutivo, nem modificar arquivos protegidos de coordenação fora das atualizações permitidas.

**Definição de concluída:** compila, smoke passa, os critérios de gameplay são demonstrados, relatório e commits estão no PR #1; ChatGPT revisa antes do merge.
