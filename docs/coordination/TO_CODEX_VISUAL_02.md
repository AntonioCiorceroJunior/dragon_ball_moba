# Validação visual e sonora concentrada — ART-002

**Branch:** `chatgpt/visual-audio-overhaul-2026-10-17`. **Base:** BATCH-001/PR #4, validado anteriormente.  
**Objetivo:** gerar uma build e capturas visuais para avaliar se o jogo realmente melhorou. **Não reprogramar gameplay**; não copiar novos assets sem licença.

## Protocolo de poucos tokens

- Ler `AGENTS.md` e este arquivo. Resumo técnico em `reports/2026-10-17_VISUAL_AUDIO_PASS_02_chatgpt.md` somente se surgir dúvida. Não reler sete dossiês nem revisar arquitetura inteira.
- Fazer `git fetch`, checkout limpo da branch e `git status` preservando alterações de usuário. **Não mesclar** PR #2, #4 ou o PR visual.
- A lógica de domínio não mudou; não gastar tempo rodando smoke de .NET repetidamente se o CI do commit estiver verde.
- Executar `scripts/verify_publication.ps1`; `scripts/validate_unity.ps1 -Mode VisualPrepare`, `VisualPlay` e `VisualBuild`. A arte/som dependem do Unity e **não são aprovados pelo CI C# sozinho**.
- Se falhar compilação/Play, corrigir o **primeiro erro causal** e rodar somente o gate pertinente. Não despejar logs inteiros no contexto.
- Capturar obrigatoriamente 8 cenas: **seleção com 3 heróis**, mapa, rio/ponte, combate Ki, minion, santuário/torre, minimapa/HUD e loja. Pelo menos 1280×720 e 1920×1080 para menu/HUD.
- Ouvir Ki/impacto/compras/transformação/vitória, ambiente e mute com M. Conferir que não há clipping ou volume incômodo.
- Medir FPS em ondas de minions; se drops, identificar desenho minion/efeitos e reduzir custo.
- Registrar só resultados `PASS/FAIL/NOT TESTED`, capturas, 3–5 problemas principais, comandos e commits. **Não fazer merge**. Se a mudança piorou a aparência, dizer claramente e anexar evidência.

**Critério de aprovação:** imagens reais confirmam terreno texturizado, personagens em galeria, UI legível, minions distintos e efeitos; som existe no executável e é útil. Testes de jogabilidade críticos continuam verdes. **Sem capturas e ouvido humano, chamar visual de “pronto” seria inadequado.**
