# Tarefa ativa — DBM-001: progressão Q/W/E/R e ataque básico automático

**Status:** PRONTA PARA CODEX (não iniciada)  
**Base:** após DBM-000, commit verificado `0c13080358d14b6403d55ba56e47f32f6d2b5a22`  
**Projeto:** `src/client-unity` · Unity 6000.6.6f1 · Built-in · Input Manager legado.

## Diagnóstico conferido no código publicado

- `Assets/Scripts/Domain/EntityState.cs`: possui `Level`, `SkillCooldown[4]`, `AttackTargetId`, mas não tem ranks nem pontos distribuíveis.
- `Assets/Scripts/Domain/Definitions.cs`: `CommandKind` não contempla evoluir habilidade; `AbilitySlot` já identifica Q/W/E/R.
- `Assets/Scripts/Domain/GameWorld.cs`: `CastAbility` não verifica se a habilidade foi desbloqueada. O mundo **já tenta fazer ataques básicos repetidos** via `AttackTargetId`, `UpdateMovement`, `AttackCooldown`, `DealDamage`; diagnosticar antes de reescrever.
- `Assets/Scripts/Runtime/PrototypeGame.cs`: clique direito faz raycast e procura `EntityHandle`; Ctrl+Q/W/E/R não evolui habilidades; HUD só exibe recarga.
- `ThinkBot` já lança Q; garantir que a IA também respeite os novos desbloqueios.

## Entrega SK — progressão de habilidades

1. Todos os heróis começam nível 1, com **um ponto não distribuído** e ranks Q/W/E/R = 0. Cada nível adquirido até 18 concede um ponto. Sem pontos extras depois do 18; reiniciar partida reinicia tudo; morrer/renascer preserva ranks e pontos.
2. Novo comando **de domínio C#** para aprimorar habilidade. Controlar por **Ctrl+Q, Ctrl+W, Ctrl+E, Ctrl+R** e botões visíveis de **+** próximos a cada habilidade quando elegível; não migrar toda a HUD.
3. Q/W/E no máximo rank 5; rank seguinte só pode ser alcançado a partir do nível `2 * nextRank - 1` (ranks 1/2/3/4/5 nos níveis 1/3/5/7/9). R no máximo rank 3, com ranks 1/2/3 nos níveis **6/11/16**. Gastar sem ponto, antes do nível ou além do máximo não altera estado.
4. Uma habilidade rank 0 **não pode ser conjurada**. Rejeitar antes de gastar Ki, aplicar recarga, cancelar ataque, causar dano ou emitir evento. Habilidades desbloqueadas continuam respeitando Ki, recarga, alcance e condições existentes.
5. Cada rank deve gerar um efeito verificável (por exemplo dano, escudo, distância da investida ou recarga), com parâmetros simples documentados. Evitar balanceamento final e reestruturação de 15 personagens.
6. Bots recebem evolução automática **determinística e legal**, sem bypass das regras do domínio. Não quebrar uso atual de Q.
7. HUD deve mostrar rank atual/máximo, pontos disponíveis, estado bloqueado, custo/recarga e feedback por tentativa inválida.

## Entrega AA — um clique, ataques contínuos

1. Reproduzir e localizar a falha relatada: cadeia `PrototypeGame.Update → GameCommand.Attack → GameWorld.Submit → UpdateMovement → Tick/DealDamage`. Não reimplementar lógica já funcional sem evidência.
2. Clique direito **uma vez** num inimigo vivo seleciona alvo persistente; fora do alcance, aproxima-se; ao entrar, ataca repetidamente conforme `AttackInterval` até alvo morrer, jogador mudar ordem ou ocorrer condição de cancelamento.
3. Clique direito no chão cancela alvo e move; segurar C interrompe ataque; morte do herói/alvo e troca de alvo não produzem golpes fantasmas; aliados e entidades inválidas não podem virar alvo.
4. Verificar seleção no **modelo 3D real**, incluindo hierarquia de colliders e `EntityHandle`. Ataques devem aplicar dano no domínio, não apenas disparar animação/VFX. Preservar simulação de 30 ticks/s.

## Testes obrigatórios

- **SK-01:** nível 1 = 1 ponto, todos ranks 0, conjuração bloqueada sem custo nem recarga.
- **SK-02:** após Ctrl+Q, Q rank 1, ponto 0, conjuração válida; outro upgrade não gasta ponto negativo.
- **SK-03:** Q rank 2 bloqueado antes do nível 3; R bloqueada antes do 6; R rank 2/3 apenas níveis 11/16. Limites máximos corretos.
- **SK-04:** XP que sobe vários níveis concede pontos exatos; rank superior traz benefício mensurável; morte/respawn preservam; reiniciar limpa.
- **SK-05:** bot aloca ranks legalmente e utiliza Q quando desbloqueada.
- **AA-01:** um único comando de ataque fora de alcance provoca aproximação e pelo menos dois golpes, respeitando cooldown.
- **AA-02:** cancelar com movimento ou C interrompe dano; morto/inválido/aliado não sofre golpe indevido.
- **AA-03:** clique efetivo no mesh/collider do alvo **no Play Mode** gera perseguição e dano sem novos cliques.
- **REG:** não regredir Ki, transformações, três campeões, loja, respawn, tropas, torres e câmera. Rodar comandos existentes `VisualPrepare`, `VisualPlay`, `VisualBuild`. Separar prova automatizada, Play Mode, build e interação com executável. O que não foi reproduzido deve constar como **não comprovado**.

## Procedimento e economia de tokens

1. Atualizar clone com segurança. Ler `AGENTS.md`, `FROM_CODEX.md` e este arquivo, depois apenas código/testes relevantes. Criar branch `codex/DBM-001-skill-autoattack`; não usar push forçado.
2. Trabalhar por marcos: **SK e testes primeiro; AA e testes depois**. Se limites de sessão impedirem completar ambos, publicar avanço comprovado e registrar **PARCIAL**, com próximos passos, sem aprovar a tarefa.
3. Pequenos diffs, nenhuma migração Unity, nenhum asset novo, nenhuma reconstrução do mapa, de torres, da loja ou do multiplayer.
4. Fazer commit(s) e publicar **PR de revisão** contra `main`, preferencialmente draft. **Não mesclar automaticamente**.
5. Na mesma branch atualizar `docs/coordination/FROM_CODEX.md` e `PROJECT_STATUS.md`; criar `docs/coordination/reports/YYYY-MM-DD_DBM-001_habilidades-autoataque.md`. Incluir SHA, links, arquivos, comandos/testes reais, evidências, limitações e rollback. Não reescrever `TO_CODEX.md` nem `TASK_QUEUE.md`.
6. Se não conseguir autenticar Git local, usar conector GitHub autorizado, sem guardar tokens/segredos no projeto.

**Fora de escopo:** navegação por obstáculos, colisões com torres e aggro de torre (DBM-002); inventário definitivo (DBM-003); novos modelos, sons, multiplayer.

**Definição de concluída:** comportamento SK/AA demonstrado, regressões testadas, relatório e PR disponíveis para revisão — não apenas compilação.
