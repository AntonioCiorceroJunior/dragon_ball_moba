# Último retorno — ART-002 (17/10/2026)

Branch `chatgpt/visual-audio-overhaul-2026-10-17`, [PR #5](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/5), sem merge. Código final `f4aedc087ed960a6fe820c76655d0610a2c8dbb5`, [CI #74 PASS](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/actions/runs/39070665780).

- [Relatório](reports/2026-10-17_ART-002_codex-validacao.md) e [capturas reais/áudio](evidence/ART-002/README.md) disponíveis para avaliação.
- Unity Prepare/Play/build Windows PASS; validação nativa e métricas anexadas. FPS: 239,83 em 720p/20 s e 239,61 em 1080p/30 s, 52 tropas no mundo.
- Corrigidos mute parcial, galeria ao retornar, identidade genérica dos heróis e pintura de rotas divergente do mapa.
- Usuário ampliou explicitamente o escopo: câmera em perspectiva, movimento/rotação suavizados e mapa diagonal original 144×144, três rotas e 18 torres. Não equivale ao mapa exato de LoL; valores de combate/progressão preservados.
- **Aceitação artística/auditiva pendente**: usuário rejeitou a primeira apresentação; modelos ainda intermediários. WAVs sem clipping nas amostras e mute silencioso não comprovam qualidade musical.

Próximo passo: ChatGPT/usuário avaliar imagens, som e câmera/escala. TO_CODEX preservado; nenhum próximo ticket iniciado. Backups, stashes e checkpoints locais preservados.

---

# Histórico — DBM-002 + BATCH-001

**Data:** 17/10/2026. **Estado:** gates automatizados aprovados; revisão manual parcial; aguardando ChatGPT. **Sem merge.**
**Branch:** `chatgpt/moba-integrated-systems-2026-10-17`; [PR #4](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/4), base [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2).
**Código validado:** `53e5ef010ca79fa7098e11c2375b81834ee3a37c`; [CI #61 aprovado](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/actions/runs/39062341692).
**Relatório:** [2026-10-17_BATCH-001_codex-validacao.md](reports/2026-10-17_BATCH-001_codex-validacao.md).

- Unity 6000.6.6f1: Prepare, domínio real, Play/arte e build Windows passaram; **21/21 verificações nativas** passaram.
- Corrigidos parsing da publicação, reembolso de venda no Mono e pan involuntário em batch; roteiro nativo adaptado às falhas reproduzidas.
- Conferidos sem flag: seleção dos três heróis, componentes/categoria/receita restante, venda, Ki, minimapa/Space e HUD 720p/1080p.
- **Não verificados manualmente:** Oozaru/colisão, combinação final da receita, combate com efeitos de itens, A/S e aceites completos de torre/níveis 6/11/16. Ver lacunas no relatório.
- Evidências publicáveis em `evidence/DBM-BATCH-001/`; logs completos ignorados em `src/client-unity/Logs/`. Build e backup permanecem locais.

Próximo passo: ChatGPT revisar PR #4, correções e pendências. TO_CODEX não foi reescrito; nenhum próximo ticket iniciado.
