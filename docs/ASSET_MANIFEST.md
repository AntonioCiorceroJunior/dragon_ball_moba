# Asset manifest — VisualTest
Acesso/verificação: 2026-10-16. Render pipeline Built-in, Unity6000.6.6f1.
Selecionados um conjunto coerente de natureza e um humanoide rigged CC0.
Downloads existentes foram reaproveitados; nenhum programa, SDK ou pacote pago foi instalado.

| Recurso / autor | Origem / licença | Arquivo local e uso real |
|---|---|---|
| Universal Base Characters Standard / Quaternius | https://quaternius.itch.io/universal-base-characters — CC0 1.0, License_Standard incluída | ExternalAssets/Staging/characters_quaternius.zip; Assets/_DragonMOBA/Art/Characters/Hero.fbx e Hair.fbx; rig/avatar válidos. Base dos seis prefabs usados por OrganicArena.Character |
| Universal Animation Library Standard / Quaternius | https://opengameart.org/content/universal-animation-library — CC0 1.0 | ExternalAssets/Staging/animation_quaternius.zip; Art/Animations/Humanoid.fbx; Idle/Walk/Run/Attack/Hit/Death/Charge/Cast/Transform vinculados ao VisualHero.controller |
| Stylized Nature MegaKit Standard / Quaternius | https://opengameart.org/content/stylized-nature-megakit — CC0 1.0 | ExternalAssets/Staging/nature_megakit_quaternius.zip; Tree/Bush/Rock e texturas, clonados em VisualTest/Forest*.prefab e efetivamente instanciados |
| UI / Input / Sci-fi Sounds / Impact / Kenney | https://kenney.nl/assets/ui-pack-sci-fi ; https://kenney.nl/assets/input-prompts ; https://kenney.nl/assets/sci-fi-sounds ; https://kenney.nl/assets/impact-sounds — CC0 | Subconjuntos e licenças em Assets/_DragonMOBA/Art/UI e Audio; referências do componente ArtPresentation existente |
| Terreno, pontes, sentinelas, shaders, locks de cabelo e anéis | Autoria local do projeto, sem extração/cópia de assets externos | OrganicArena.cs, FighterToon/OrganicGround/River.shader, HairLock.asset; gerados/vinculados ao runtime real |

Modelos FBX/PNG/OGG importados; malha e avatar humanoide foram inspecionados pelo Unity.
Os prefabs não são modelos oficiais de Dragon Ball. O nome do campeão lógico é preservado,
mas a apresentação é uma adaptação original do humanoide genérico CC0.
Hashes e licenças do subset original: src/client-unity/docs/art/IMPORTED_ASSETS.csv.
O Shader Toon usa bandas na malha e cabelo parentado ao osso; não foi usado modelo de fã.

## Fontes avaliadas, não incorporadas
- DevZhav/unity-moba: https://github.com/DevZhav/unity-moba
  commitd0f56fe5a2d30ffdd1929b9877f2e96ecd038fa9, GPL-3.0; Unity2018.2.6f1.
  README/licença/índice/versão em Research/OpenSource/DevZhav (ignorado no Git).
  O README declara três mapas; não se verificou licença individual de assets nem compatibilidade executada com Unity6.
  Nenhum arquivo do toolkit foi copiado para Assets.
- Yasgamesdev: https://github.com/yasgamesdev/MOBA_CSharp_Unity
  MIT, Unity2018.3.4f1. Auditoria anterior em VISUAL_RESTRUCTURE_AUDIT.md e EXTERNAL_AUDIT_SOURCES.csv.
  Referência arquitetural, não mapa pronto incorporado.
- Ultimate Stylized Nature / Ultimate Modular Ruins:
  https://quaternius.com/packs/ultimatestylizednature.html
  https://quaternius.com/packs/ultimatemodularruins.html
  Cota do Drive nas tentativas anteriores, respostas HTML rejeitadas.
  A alternativa CC0 acima foi escolhida; não há bloqueio do mapa por esse motivo.
- Fantasy Worlds Forest:
  https://marketplace.unity.com/packages/3d/environments/fantasy/fantasy-worlds-forest-free-stylized-forest-environment-open-worl-282610
  Página oficial declara Built-in/URP em2022.3.23f1; não se demonstrou Unity6.
  EULA Unity; sem pacote local/acesso oficial autorizado para importar.
  Não foi necessário baixar641MB: o subset CC0 já atende este marco.
- Goku: https://sketchfab.com/3d-models/goku-rigged-animated-3ed060df9f4a4134963e61f3ca95ee7d
- Vegeta: https://sketchfab.com/3d-models/vegeta-d43230bed9964b9aaa7671b752e8d608
- Cell: https://sketchfab.com/3d-models/cell-perfect-dragon-ball-z-2dbf22a793be4c13875bccec8af99d7e
  As três páginas retornaram403 na consulta. Arquivos, autoria e cadeia de direitos não verificados;
  nenhum download/modelo de fã foi incorporado. Não se pede login para continuar este protótipo,
  pois o humanoide CC0 já foi integrado e testado.

