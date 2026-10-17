# Visual + áudio — segunda passagem de apresentação (código aguardando Unity)

**Data:** 17/10/2026 · **Branch:** `chatgpt/visual-audio-overhaul-2026-10-17` (base: lote validado da DBM-002/BATCH-001, commit `40424df`).  
**Escopo:** arte técnica, cena, UI, som, câmera, animação cosmética e testes de cena. **Nenhuma alteração de regras de domínio, compras ou combate** nesta passagem.

## Problema atacado

A arena era um protótipo com terreno visual sujeito a perder a cor de vértice ao usar material Standard, soldados representados por esferas, muitos painéis IMGUI genéricos, feedback de golpes por linhas simples e três áudios importados com poucas variações. O menu normal aparecia antes da montagem visual do mapa.

## Alterações reais na branch

| Arquivo | Mudança e finalidade |
|---|---|
| `OrganicArena.cs` | Texture2D de superfície gerada das próprias cores do terreno e aplicada como `mainTexture`, UVs normalizados, vegetação preservada, rio azul-turquesa, margem iluminada, santuários e ruínas em pontos de leitura de mapa, luz ambiente e neblina limitadas, lanternas/santuários animados; sem colliders adicionais no domínio |
| `CharacterGallery.cs` | galeria real 3D com os três prefabs já importados, pedestais e iluminação suave; só funciona na seleção, desativada na partida |
| `PrototypeGame.cs` | construção do cenário durante o menu, 3 cards de campeão com informações e confirmação, tropas com modelo distinto, HUD com HP segmentado a cada 100/1000, cronômetro e indicação M para áudio |
| `MinionVisual.cs` | pequenos drones sentinelas com carapaça, asas, reator de Ki e oscilação sutil, duas cores de time; apenas render/collider de clique |
| `ArtPresentation.cs` | tema coerente de interface escura/cápsula com brilho metálico, botões com estados; feixes de Ki com núcleo luminoso e efeitos em dois traços; sistema sonoro integrado |
| `BattleSoundscape.cs` | **áudio original gerado por algoritmo na inicialização**, música ambiente discreta, disparo Ki, impacto, compra, carga, transformação, KO, vitória e alerta; M alterna silêncio. Não reutiliza músicas, dublagens ou sons protegidos |
| `ArenaCameraController.cs` | inclinação ortográfica 55,5° conforme direção de referência, com follow/free, foco/zoom existentes preservados |
| `ArtSmoke.cs`, `VisualStageChecks.cs` | novas verificações de `mainTexture`, fog, modelo minion, inicialização de áudio e novo ângulo de câmera |

**Escala de entrega:** passagem artística focada em aparência imediata. Sem mapa top/mid/bot diagonal, fog-of-war, novos assets de alto realismo, animação Oozaru própria ou sistema de UI comercial baseado em UIToolkit. Arte procedural e sons sintetizados são **provisórios de qualidade superior ao placeholder**, não assets finais.

## Relação com documento do Argon

Usamos os **tópicos compartilhados pelo usuário no chat**: apresentação Capsule Corp, personagem selecionável, câmera 55–56°, barras de HP com marcas 100/1000, feedback de Ki e originalidade visual.  
O arquivo `docs/coordination/REFERENCIA_COMPLETA_LOL_DRAGON_BALL_MOBA.md` **ainda não estava publicado no GitHub** ao criar este lote; sua leitura integral e comparação por patch estão pendentes. Este lote **não assume** horários, fórmulas, economia nem números de LoL sem ler/validar a fonte. O arquivo local do Argon não foi acessado e não foi copiado para esta branch.

## Verificações realizadas e pendências

**Executado nesta sessão:** leitura de arquivos C# e estrutura das referências, revisão estática de delimitadores de nove arquivos, confirmação de escopo GitHub via diff. **Nenhuma compilação Unity/Windows nem inspeção visual de frames foi executada nesta sessão.**

**Gate único para Codex/Unity:**
1. Sync branch de visual (após PR #4, sem merge automático), observar `git status`.
2. Confirmar Unity import/compilação (`VisualPrepare`) e Play (`VisualPlay`); `ArtSmoke` deve validar terreno, som e minions.
3. Capturar imagens reais **do menu, mapa geral, encontro no rio, combate com feixe Ki, torre/lanterna, loja, escala Goku Criança/Cell/Oozaru** em 1280×720 e 1920×1080.
4. Confirmar `VisualBuild` Windows e observar som diretamente: Ki, impacto, compra, transformação, vitória e mute M, sem saturação.
5. Checar FPS e GC em waves/combate; 3D menu e texturas devem ser descartadas/desativadas corretamente.
6. Corrigir apenas erros reproduzidos na mesma branch; relatório com comparação antes/depois por screenshots e referências a arquivos. **Não fazer merge sem revisão.**

## Riscos conhecidos

- Mudança de posição dos botões de seleção exige adaptar testes nativos que dependem de **coordenadas fixas** (testes de domínio não sofrem alteração).
- Iluminação/cor dependem de shader e material Unity reais, ainda precisam de captura. SFX sintetizado é primeira etapa; qualidade percebida depende de validação auditiva humana e pode exigir futuros áudios licenciados.
- Drones criam algumas primitivas/Renderers por unidade: medir lotes grandes; depois mesclar meshes/pooling se houver queda perceptível.
- Fog não é fog of war; estático visual não deve esconder informação real de jogo.
- O protótipo preserva limites de originalidade/licenças e não inclui ativos Dragon Ball/LoL não autorizados.

**Estado:** implementação em branch, **aguardando CI + validação Unity**, sem merge.
