# Uma rodada de validação integrada — DBM-002 + BATCH-001

**Atualizado:** 17/10/2026 · **Estado:** código editado pelo ChatGPT em branch; testes Unity ainda não executados.
**GitHub:** [PR #4](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/4), branch `chatgpt/moba-integrated-systems-2026-10-17`, empilhado sobre [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2).
**Instrução operacional substituta SOMENTE nesta validação:** após acionamento pelo usuário, leia `AGENTS.md`, este arquivo da branch, relatório `reports/2026-10-17_BATCH_001_chatgpt.md` e diff do PR #4. **Não reimplemente sistemas** e **não faça merge**.

## 1. Preparação e integridade (PC do usuário)

1. Usar exclusivamente o clone ativo em `Repositorio/dragon_ball_moba`; registrar `git status` e preservar alterações locais.
2. Sincronizar com `git fetch` e criar checkout limpo da branch `chatgpt/moba-integrated-systems-2026-10-17` (baseada no código DBM-002). Não mudar Unity do projeto e não misturar pastas antigas.
3. Garantir Unity 6000.6.6f1 com Editor fechado antes de comandos batch.
4. `dotnet run --project tests/DomainSmoke/DomainSmoke.csproj -c Release` se houver .NET8. Se não houver, usar resultado GitHub Actions e rodar domínio no Editor.
5. `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify_publication.ps1`. O baseline correto é `docs/coordination/evidence/DBM-BATCH-001/DOMAIN_BASELINE.csv`. Não editar hashes para esconder erros.
6. Rodar `scripts/validate_unity.ps1` em `VisualPrepare`, `VisualPlay` e `VisualBuild`; **confirmar execuções reais** de `DomainSelfTests.RunAll` (`DBM001 DOMAIN PASSED`, `DBM002 DOMAIN PASSED`, `DBM-BATCH DOMAIN PASSED`), smoke de arte e build Windows. Inspecionar logs de erro.
7. Rodar `scripts/validate_native_gameplay.ps1` para regressão de 21 cenários DBM-001, preservando o flag opt-in `-dbm001-native-test`.

## 2. Matriz manual de aceitação no executável normal (SEM flag de teste)

| ID | Fluxo de usuário | Aceite |
|---|---|---|
| SEL-01 | Abrir jogo novo normalmente | aparece seleção de 3 heróis; **não** começa automaticamente |
| SEL-02 | Selecionar Goku Criança/Cell e confirmar | entra com heroId correto e HUD, sem duplicar arena |
| STAT-01 | Perder HP fora da base e aguardar | HP regenera; HUD mostra regen/s |
| STAT-02 | Subir nível 6/11/16 | HP/ataque/regen/AS aumentam e skills evoluem legalmente |
| SHOP-01 | P abrir loja, selecionar categoria, comprar componente | Zeni diminui e stat muda; ícone procedural representa item |
| SHOP-02 | Comprar 2 luvas + cristal vida; combinar Luvas do Dragão | custo restante = 1700; componentes desaparecem, stats finais sem duplicação |
| SHOP-03 | Vender item na fonte | devolve 70%, stats antigos desaparecem e HP não excede MaxHP |
| COUNTER-01 | Comprar Lâmina de Supressão e atingir outro herói | aplica redução de cura de 35% por 3s; debuff expira |
| TWR-06 | Fazer uma segunda tropa entrar mais perto da torre com primeira tropa viva | torre mantém a primeira até morrer/sair, exceto hero aggro |
| COMBAT-01 | VAMP_ESSENCE e ataque básico em inimigo | cura apenas dano ao HP; shield bloqueia sustain |
| COMBAT-02 | HASTE_SHARD, subir Q e conjurar | cooldown de Q diminui conforme haste e Ki é consumido |
| COMBAT-03 | Ataque-movimento A+clique e S | auto-adquire alvo, realiza básicos repetidos, para ao pressionar S |
| KIT-01 | Comparar Q/R entre Goku Kid, Super e Cell | os três têm efeitos/dano diferentes; níveis alteram habilidade |
| KI-01 | Carregar Ki com C até 100% fora da fonte | mostra XP/s e a XP sobe; efeito some após soltar |
| CHAR-01 | Goku Criança próximo do Super | Criança realmente menor, com collider/mira funcionais |
| CHAR-02 | Goku Criança no nível 16 → Oozaru | forma 3D muito maior, anda mais devagar e não aparece dentro da torre |
| CHAR-03 | Cell base e transformado | modelo/forma corretos, animações e visuais existentes preservados |
| CAM-01 | Pan com bordas/setas/meio; Space/Y e zoom | clique/raycast continuam precisos, não recentraliza indevidamente |
| MAP-UI-01 | Visualizar minimapa e clicar em uma região livre | destaca unidades e desloca a câmera sem deslocar o personagem |
| HUD-01 | Nome/nível/vida/escudo de dois jogadores | informações verdadeiras e legíveis 1280×720 e 1920×1080 |
| DBM2-01 | Torre inimiga em alcance, tropas, agressão, W e colisão | aceitação da [DBM-002](TO_CODEX.md) aplicada também na branch integrada |
| REG-01 | Restart, retorno à seleção, morte/respawn, transformação e basic attack | nenhuma regressão relevante |

**Importante:** ícones são **procedurais simples**; Oozaru é uma **prototipagem com rig reaproveitado**; minimapa reflete somente a arena atual e **não tem fog** (todos os heróis aparecem). Novo layout de mapa, jungle, visão real, multiplayer e kits finais NÃO são objeto deste lote.

## 3. Resolução de falhas

- Corrigir **compilação, Play Mode, testes reais, asset references, UI e colisões** na MESMA branch `chatgpt/moba-integrated-systems-2026-10-17`, com commits.
- Se a DBM-002 apresentar bug herdado, corrigi-lo nesta branch e rotular como falha DBM-002; coordenar cherry-pick para PR #2 apenas depois da revisão.
- Preservar `.meta`, catálogos, licenças CC0 e baseline de domínio (recalcular quando houver alteração legítima de código, registrar motivo).
- Não tentar implementar mapa 5v5 novo nem infraestrutura multiplayer neste teste. Não instalar novos frameworks ou refatorar centenas de arquivos.
- Registrar **falhou / passou / não verificado**; nenhum resultado inventado.

## 4. Handoff e decisão

Criar `docs/coordination/reports/YYYY-MM-DD_BATCH-001_codex-validacao.md` com comandos/versão Unity, testes, logs, capturas e commits. Atualizar `FROM_CODEX.md` e `PROJECT_STATUS.md` **na branch**, referenciando PR #4 e base PR #2. Não sobrescrever `TO_CODEX.md` de main por conta própria.

**Não dar merge** nem fechar PR #2/#4 automaticamente. ChatGPT revisará evidências e integrará na ordem correta. Uma execução do Codex valida o lote completo — não uma partida MOBA final de 15 heróis.
