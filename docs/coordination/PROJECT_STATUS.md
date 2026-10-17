# Estado técnico — ART-002 em revisão

17/10/2026, Codex. Branch `chatgpt/visual-audio-overhaul-2026-10-17`, [PR #5](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/5). **Sem merge; aprovação visual/sonora humana pendente.**

Raiz Unity `src/client-unity`, Editor **6000.6.6f1**, Built-in/Input Manager legado; cena `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`. Código final `f4aedc087ed960a6fe820c76655d0610a2c8dbb5`, CI #74 PASS. Prepare/Play/build aprovados; [relatório](reports/2026-10-17_ART-002_codex-validacao.md) e [evidências](evidence/ART-002/README.md).

Build Windows Development local `src/client-unity/Builds/VisualTest/DragonMobaVisualTest.exe`: UTC **2026-10-17T21:19:05.3473104Z**, **168.638.501 bytes totais**. Binários/caches/logs não publicados.

Identidades próprias de Kid/Super/Cell, galeria responsiva, terreno/rotas coerentes, composição original de 48 s e mute de SFX implementados. Capturas reais cobrem oito tipos de cena e menu/HUD 720p/1080p. FPS observado 239,83/239,61; zero clipping nas amostras, silêncio após M. Não certifica partida longa ou qualidade auditiva.

Ampliação autorizada pelo usuário: perspectiva FOV 40°/elevação 55,5°, interpolação/rotação, arena original 144×144, bases opostas, três rotas e 18 torres. Stats/dano/progressão preservados; domínio Classic tem testes próprios, Training mantém fixtures anteriores. Não representa medidas exatas, topologia de selva ou todos os sistemas de LoL.

Pendências: acabamento de modelos/animações e som; aprovação da câmera/escala pelo usuário. Primeira apresentação foi considerada insuficiente; não declarar arte pronta. TO_CODEX intacto, nenhum próximo ticket iniciado.

---

# Histórico — DBM-002 + BATCH-001 em revisão

**Atualização:** 17/10/2026, Codex. **Estado:** importação, domínio, Play/arte, build e smoke Windows aprovados; aceitação manual parcial. **Sem merge.**

- Branch `chatgpt/moba-integrated-systems-2026-10-17`, [PR #4](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/4), empilhado sobre [PR #2](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/pull/2).
- Recebido `06e6472`; código corrigido/testado `53e5ef010ca79fa7098e11c2375b81834ee3a37c`; [CI #61 verde](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/actions/runs/39062341692).
- [Relatório compacto](reports/2026-10-17_BATCH-001_codex-validacao.md), [21 resultados nativos](evidence/DBM-BATCH-001/Native-results.json), [identidade da build](evidence/DBM-BATCH-001/Build-identity.json).

Raiz Unity: **`src/client-unity`**, clone ativo `Repositorio/dragon_ball_moba`. Editor **6000.6.6f1**, Built-in/Input Manager legado. Cena `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`.

Executável local: `src/client-unity/Builds/VisualTest/DragonMobaVisualTest.exe`, Windows x64 Development, **168.574.732 bytes totais**, UTC **2026-10-17T19:21:04.8510171Z** (16:21:04 em São Paulo). Build anterior em `src/client-unity/Backups/BATCH001-before-validation/`. Binários, caches e logs não publicados.

Validação: VisualPrepare/VisualPlay/VisualBuild aprovados; `RunAll` real emitiu DBM001/DBM002/DBM-BATCH DOMAIN PASSED; smoke dos três heróis/12 formas e **21/21 cenários Windows** aprovados. Execução normal conferiu seleção, loja/componentes/custo restante/venda, treino Ki, minimapa/Space e HUD 1280×720/1920×1080. Domínio C# aproveitado do GitHub Actions; SDK .NET local indisponível.

Correções pontuais: script de publicação corrompido, cálculo inteiro dos 70% de venda para Mono/CLR e entrada fantasma de câmera em batch. Roteiro nativo recentraliza a fixture, respeita tempo simulado e confirma a seleção atual. Hash de GameWorld recalculado por essa correção legítima; demais hashes, licenças e assets preservados.

**Não comprovado manualmente:** Oozaru tamanho/velocidade/colisão, receita completa na UI, efeitos de combate de itens, ataque-movimento A/S e matriz completa de torre/stats nos níveis 6/11/16. Cobertura automática parcial não equivale a partida completa nem teste prolongado.

**Próximo passo:** ChatGPT revisar PR #4 e pendências; decidir a integração na ordem adequada. Sem merge, alteração de TO_CODEX ou início do próximo ticket. Histórico anterior permanece em `reports/2026-10-16_DBM-001_codex-validacao.md`.
