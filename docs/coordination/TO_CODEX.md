# Instrução ativa — DBM-002: validar PR #2 (executar após renovação da franquia Codex)

**Status:** código implementado pelo ChatGPT; AGUARDANDO TESTES UNITY  
**PR:** https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2  
**Branch:** `chatgpt/DBM-002-towers-collisions`  
**Base:** `main`; DBM-001 mesclada no commit `067ed1ceebfdb8facb59667afec00fd3c1657883`.  
**Editor:** Unity 6000.6.6f1, Built-in, Input Manager legado.

## Orçamento do Codex

O usuário informou que restam aproximadamente **12% da franquia** e que haverá renovação por volta de **01h12 (horário local)**. **Não execute esta tarefa antes de o usuário acioná-la**; preserve a franquia existente. Este documento é para a próxima janela de validação.

## Mudanças já feitas pelo ChatGPT

- `GameWorld.cs`: seleção de alvos de torre prioriza tropas vivas no alcance; agressão contra herói defendido provoca a torre por até 3s; dano provisório 125; temporizador com dt real; colisão circular sólida de torres/núcleos e movimento tangencial; dash W com subpassos.
- `EntityState.cs`: estado temporário de agressão da torre.
- `PrototypeGame.cs`: círculo de alcance de torre inimiga visível quando jogador se aproxima.
- `DomainSelfTests.cs`: testes `TR-01..05` e `COL-01..05`.
- `verify_publication.ps1` e `evidence/DBM-002/DOMAIN_BASELINE.csv`: hashes de domínio novos preservando histórico anterior.
- Relatório completo na branch: `docs/coordination/reports/2026-10-16_DBM-002_chatgpt-implementation.md`.

**Não reimplementar do zero.** A função do Codex é validar, corrigir erros concretos e publicar relatório.

## Passos para execução (somente quando o usuário pedir)

1. Verificar `git status` e salvar qualquer alteração local antes de sincronizar. Buscar `origin` e mudar com segurança para `chatgpt/DBM-002-towers-collisions`; não tocar na pasta antiga do protótipo.
2. Ler `AGENTS.md`, este documento da `main`, relatório DBM-002 da branch e diffs relevantes.
3. Executar na raiz do clone, com o Unity Editor fechado:
   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify_publication.ps1
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPrepare
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPlay
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualBuild
   ```
4. Confirmar que novos testes `TR-01..05` e `COL-01..05` rodaram, não só que houve mensagem de build; checar os resultados esperados em `DomainSelfTests.RunAll`.
5. Em executável real Windows: aproximar de torre inimiga, ver anel, receber dano dentro do alcance, não receber dano fora, ver minions serem priorizados; após atacar campeão aliado defendido, observar troca de aggro; click-to-move contorna torre; W não atravessa torres; torre destruída deixa de bloquear.
6. Regredir DBM-001 (Q/W/E/R, Ctrl upgrades, ataque básico após um clique), transformações, loja, tropa, respawn e seleção de personagem. Se testes manuais forem inviáveis, registrar `não comprovado`.
7. Corrigir erros identificados por testes **na mesma branch e PR #2**, com commits e evidências. Não atualizar `TO_CODEX.md` nem a fila.
8. Atualizar `FROM_CODEX.md` e `PROJECT_STATUS.md` na branch e criar `reports/YYYY-MM-DD_DBM-002_codex-validacao.md`. Informar mudanças, comandos, testes que passaram/falharam, build executável e limites.
9. **Não mesclar PR #2** nem começar DBM-003; ChatGPT revisará a validação.

## Limites desta entrega

Colisões apenas de **torres e núcleos vivos**; natureza, rochas procedurais e paredes de selva ainda não são obstáculos autoritativos. Dano, raios e aggro são valores provisórios. Não instalar novos frameworks, não mexer em sprites/modelos/sons ou multiplayer e não executar migração de versão do Unity.

**Definição de concluída:** testes de domínio + Play Mode + build e verificação das mecânicas, PR atualizado com evidências e relatório; sem alegar sucesso apenas por compilar.
