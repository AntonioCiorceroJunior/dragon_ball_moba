# DBM-001 — implementação inicial pelo ChatGPT (aguardando validação no Unity)

**Data:** 2026-10-16  
**Responsável:** ChatGPT (edições diretas no GitHub)  
**Status:** **IMPLEMENTADO EM BRANCH — NÃO TESTADO NO UNITY**  
**Branch:** `chatgpt/DBM-001-gameplay`  
**Base:** `main` após DBM-000.  
**Tarefa:** `docs/coordination/TO_CODEX.md` (versão DBM-001).

## Alterações implementadas

1. `src/client-unity/Assets/Scripts/Domain/Definitions.cs`: comando `UpgradeAbility` e evento `SkillUp`.
2. `src/client-unity/Assets/Scripts/Domain/EntityState.cs`: `UnspentSkillPoints`, `SkillRanks[4]` e ponto inicial no nível 1.
3. `src/client-unity/Assets/Scripts/Domain/GameWorld.cs`: desbloqueios/ranks legais e validação no domínio; pontos por nível; R nos níveis 6/11/16; QWE em 1/3/5/7/9; habilidades bloqueadas antes de consumir Ki; ganhos por rank; evolução automática legal de bot; retomar ataque básico após cast e limpar alvos mortos.
4. `src/client-unity/Assets/Scripts/Runtime/PrototypeGame.cs`: Ctrl+Q/W/E/R para evoluir; botões +, ranks e pontos na HUD, feedback de bloqueio; seleção de alvo por RaycastAll e collider `EntityHandle` mais robusta.
5. `src/client-unity/Assets/Editor/DomainSelfTests.cs`: novas verificações determinísticas de progressão, pontos, ultimate e ataques repetidos com um comando, cancelamento e recarga.
6. `src/client-unity/Assets/Editor/ArtSmoke.cs`: smoke de QWER passa a desbloquear as quatro habilidades conforme nível 6, em vez de esperar habilidades liberadas desde o nível 1.

## Decisões de protótipo

- Q: +24 dano bruto por rank depois do primeiro.
- W: +1 unidade de deslocamento por rank depois do primeiro.
- E: +25 escudo por rank depois do primeiro, respeitando limite original de escudo.
- R: +40 dano bruto por rank depois do primeiro.
- Custos, recargas e transformações originais mantidos (retirada apenas de uma multiplicação redundante por 100/100).
- Se uma habilidade é conjurada, não se cancela automaticamente o alvo do ataque básico.
- O ataque automático já estava parcialmente implementado; mudanças específicas são destino após selecionar alvo, limpeza de alvo morto e captura robusta de clique. Não se reescreveu o laço de dano do mundo.

## Revisão estática efetuada

- Leitura e verificação contextual de `Definitions.cs`, `EntityState.cs`, `GameWorld.cs`, `PrototypeGame.cs`, `DomainSelfTests.cs`, `ArtSmoke.cs`, `OrganicArena.cs`, `VisualStageChecks.cs`.
- Alterações pontuais e conferência estática de delimitadores dos arquivos C# alterados.
- **Nenhum compilador C#, Unity Editor ou executável Windows foi utilizado nesta sessão**. Não se afirma sucesso de compilação, Play Mode, testes ou build.

## Validação obrigatória para Codex no Windows

1. Sincronizar a branch e abrir `src/client-unity` no Unity 6000.6.6f1.
2. Executar `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPrepare`.
3. Executar `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPlay`.
4. Executar `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualBuild`.
5. Testar manualmente na cena: Q/W/E/R inicialmente bloqueadas; Ctrl+Q destrava Q; ganho de pontos a cada nível; R nível 6; um único clique direito no inimigo gera ataques repetidos, respeita alcance e interrompe com clique no chão ou C.
6. Registrar e corrigir erros reais de compilação/regressão na **mesma branch**, com commits e evidências; criar atualização `FROM_CODEX.md`, `PROJECT_STATUS.md` e relatório complementar.
7. Não fazer merge automático. Abrir ou atualizar o PR draft e apresentar ao ChatGPT para revisão. Usar apenas o código atual do GitHub; não modificar a versão antiga do protótipo local.

## Critérios ainda não comprovados

- Compilação do Unity e geração do executável com estas alterações.
- Interação física de mouse/teclado no Windows e HUD em 1280x720/1920x1080.
- Bugs do collider do personagem 3D em partida real.
- Todos os testes `VisualPrepare`, `VisualPlay`, `VisualBuild`, performance e regressões.

## Próximo passo

Codex: sincronização, testes reais e correções na branch. ChatGPT: revisar PR/relatório e decidir merge após evidências. DBM-002 (torres/colisões) continua pendente.
