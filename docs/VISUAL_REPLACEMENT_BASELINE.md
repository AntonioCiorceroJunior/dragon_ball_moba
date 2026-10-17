# Baseline — substituição visual real
Data: 2026-10-16. Checkpoint: bdf9dae55eb9cb609acc77403025ed212ab83ad7.
Raiz Unity: <repo>/src/client-unity
Unity6000.6.6f1 / Built-in / Input Manager antigo.
Nenhum processo Unity ou DragonMobaArt ativo foi encontrado no início desta execução;
não se atribui ao usuário uma instância em execução sem prova.
BuildSettings: ArenaArt.unity primeiro, ArenaPrototype.unity segundo.
ArenaArt contém o bootstrap PrototypeGame e ArtPresentation.
Awake cria câmera; StartMatch cria GameWorld, BuildEnvironment e views.
BuildEnvironment recria Solo/Rota/Rio Central/Floresta/Fontes por primitivas.
ArtPresentation só acrescentava árvores/pedras/torres sobre esse mapa.
CreateEntityVisual busca Resources/CharacterForms/HeroId/FormId; faltando, usa cápsula.
Conclusão: importar assets não substituiu o gerador efetivamente usado.

Builds existentes:
- Builds/Windows/DragonMoba.exe: ArenaPrototype, protótipo original.
- Builds/ArtWindows/DragonMobaArt.exe: ArenaArt, ainda mapa retangular.
O timestamp de executáveis Unity pode refletir o launcher compartilhado; conteúdo real
da partida está nos dados/assemblies e cena da build, não apenas no .exe.
A nova build será Builds/VisualTest/DragonMobaVisualTest.exe, com identificação própria,
build-info.json e cena Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity.

Evidência anterior preservada: docs/evidence/Before-Game-720.png (captura real da build anterior).
Git estava limpo. Nenhum código de domínio deve ser alterado neste marco.

Resolução da captura anterior:1280x720. Não havia Console interativo aberto; os logs de validação e a baseline Git foram usados como referência. Instruções atuais: VISUAL_REPLACEMENT_RESULTS.md.
