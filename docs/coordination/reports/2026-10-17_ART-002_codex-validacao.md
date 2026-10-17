# ART-002 — validação para revisão

17/10/2026 · [PR #5](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/5) · branch `chatgpt/visual-audio-overhaul-2026-10-17` · **sem merge**.

Build executável e gates técnicos aprovados; **aceitação visual/sonora pendente**. Após rejeitar a apresentação inicial, o usuário autorizou designs próprios e alteração de câmera, movimento e formato/tamanho do mapa. Isso amplia o escopo inicial de arte; não foi iniciado outro ticket.

## Correções e commits

Recebido `4d9145f58e3fe2b75dae571f770648ef89909e6b`. Mudanças e saídas geradas preservadas em checkpoints/stashes locais; publicação sem force.

1. `bb59da9ff626693e4bac61164b17aee30c259539`: galeria pequena/fora de quadro ao retornar; M deixava SFX em andamento audíveis. Corrigidos framing/reset e mute/stop. Probe só Development/editor sob flag, registra frames/áudio sem comandos de domínio.
2. `875c2d04ccfacff6dc30207fddbc18d58ca52311`: identidades originais Kid/Super/Cell sobre rig CC0, perspectiva/suavização, mapa diagonal e composição original de 48 s. Corrigidas exceção reproduzida de MaterialPropertyBlock fora da thread principal e orientação das peças no rig.
3. `7e8bffeb78e6b887e58c8eed0516de5a1363c222`: captura revelou pintura vertical antiga apesar de domínio/minimapa diagonais. Pintura agora usa as mesmas rotas; regressão testa pontos da textura.
4. `f4aedc087ed960a6fe820c76655d0610a2c8dbb5`: captura 1080p revelou desalinhamento cartões/modelos e cabelo de Super oculto. Espaçamento responsivo e silhueta corrigidos.

Domínio alterado apenas para geometria solicitada: Classic 144×144 unidades Unity, bases opostas, três polilinhas, 18 torres, waves e limites coerentes. Training mantém fixtures anteriores. Testes Classic verificam spawns/lojas/rotas/clamp e ângulos arbitrários de movimento. Stats, dano, itens e progressão não foram rebalanceados. O movimento já aceitava vetores contínuos; interpolação/rotação suavizadas reduzem sua aparência discreta.

## Validação

Unity **6000.6.6f1**, raiz `src/client-unity`, cena `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`.

| Gate/comando | Resultado | Escopo |
|---|---|---|
| `scripts/verify_publication.ps1` | PASS | Metas, licenças CC0, conteúdo público e baseline ART-002; baseline BATCH preservado |
| `scripts/validate_unity.ps1 -Mode VisualPrepare` | PASS | Importação/compilação |
| `scripts/validate_unity.ps1 -Mode VisualPlay` | PASS | Domínio real DBM001/002/BATCH/Classic, três heróis/12 formas, seleção/retorno, câmera/pintura; antes do último ajuste de cabelo/espaçamento |
| `scripts/validate_unity.ps1 -Mode VisualBuild` | PASS | Build final Windows Development x64, 168.638.501 bytes totais, UTC 2026-10-17T21:19:05.3473104Z |
| `scripts/validate_native_gameplay.ps1` | PASS | 21/21 na build final, com foco mantido durante espera; [resultados](../evidence/ART-002/Native-results.json); fixture explícita, não partida manual completa |
| CI C# final | PASS | [#74](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/actions/runs/39070665780), f4aedc0; #72/#73 também verdes |
| .NET local | NOT TESTED | SDK indisponível; CI aproveitado, sem repetição |
| Oito cenas, menu/HUD 720p/1080p | PASS | [Janelas reais e procedência](../evidence/ART-002/README.md); câmera superior diagnóstica identificada |
| Audição humana final | NOT TESTED | WAVs anexados; feedback inicial negativo não foi convertido em aprovação |

Logs completos ignorados em `src/client-unity/Logs/ART002/` e `src/client-unity/Logs/Validation-*.log`. Build local `src/client-unity/Builds/VisualTest/DragonMobaVisualTest.exe`; backup recebido em `src/client-unity/Backups/ART002-before-validation/`. Avisos de API Unity obsoleta não bloquearam build; não tratados como bugs de gameplay.

O primeiro reteste Windows terminou em **FAIL por timeout**: apenas 376 ticks avançaram durante a espera de 120 s, sem comprovar falha na regra de respawn; ranks/pontos intactos. [Resultado preservado](../evidence/ART-002/Native-timeout-results.json). Uma cópia local do roteiro, com o mesmo `Focus` já usado nas ações aplicado também à espera, passou 21/21. Esse ajuste do roteiro foi incorporado; não houve mudança no jogo ou no temporizador. A cópia difere apenas nos caminhos relativos para funcionar dentro dos logs e nessa manutenção de foco.

## Medições

| Amostra | Resultado |
|---|---|
| 720p, 20,018 s / 4.801 frames | 239,83 FPS; p95 4,168 ms; 52 tropas vivas |
| 1080p, 30,053 s / 7.201 frames | 239,61 FPS; p95 4,163 ms; 52 tropas vivas |
| Ambiente/UI, 24 s | Pico PCM16 0,03290; RMS 0,01077; zero saturadas |
| Efeitos, 12 s | Pico 0,12003; RMS 0,01837; zero saturadas |
| Polifonia/mute, 5 s | Pico 0,13535 com prefixo audível; zero saturadas; intervalo 0,8–1,5 s: pico/RMS zero |
| M original, intervalo 0,8–1,5 s | Pico 0,11865; RMS 0,04308: vazamento reproduzido/corrigido |

Editor fechado durante FPS; tropas são contagem mundial. Amostras curtas não certificam partida longa, outro hardware ou máximo acima de 240. WAV mono 48 kHz registra mistura do jogo; sem microfone. Transformação/vitória foram acionadas como audição de apresentação, não eventos reais de partida.

## Pendências

- Modelos têm cores, silhuetas, bastão/cauda, asas/carapaça próprios; faces/anatomia/animações continuam intermediárias. Avaliar capturas antes de aprovar arte.
- Câmera FOV 40°/elevação 55,5° e estrutura diagonal seguem o pedido. **Não entregue mapa exato de LoL**: 144×144 é dimensão original, sem equivalência oficial comprovada; não inclui topologia completa de selva/obstáculos/inibidores e todos os seus sistemas. Referência geral: [Riot, como jogar](https://www.leagueoflegends.com/en-us/how-to-play/); nenhum asset copiado.
- Timbre, variedade, repetição e volume confortável exigem ouvido humano. Gravação ambiente cobre 24 dos 48 segundos da composição.
- Oito cenas e testes não comprovam partida completa, todos os efeitos de itens/receitas ou aceite manual de todas as formas.

Próximo passo: ChatGPT/usuário revisar imagens/WAVs e câmera/escala no PR #5. TO_CODEX intacto, sem merge, sem próximo ticket automático.
