# Substituição visual — resultado verificável
Data: 2026-10-16. Checkpoint anterior: bdf9dae55eb9cb609acc77403025ed212ab83ad7.

## Abrir o jogo correto
Executável: <repo>\src\client-unity\Builds\VisualTest\DragonMobaVisualTest.exe
Cena: <repo>\src\client-unity\Assets\_DragonMOBA\Scenes\VisualTest\ArenaVisualTest.unity
Versão visível: **VISUAL TEST 01 - Organic Arena**.
Build: **16/10/2026 20:51:49**, **168500908 bytes** totais reportados pelo Unity.
UTC: 2026-10-16T23:51:49.0512095Z. Detalhes: Builds/VisualTest/build-info.json.
O jogo inicia com GKU_SUPER usando o visual genérico CC0. Esc retorna à seleção existente.
As builds Windows/DragonMoba e ArtWindows/DragonMobaArt foram preservadas e continuam antigas.
Para copiar o novo jogo, copiar a pasta Builds/VisualTest inteira, não só o launcher .exe.

## FEITO — mudanças efetivamente ligadas
- Cena nova com OrganicArena e seis referências explícitas de visual do campeão lógico GKU_SUPER.
- PrototypeGame.CreateEntityVisual consulta OrganicArena.Character antes do fallback Resources.
  O prefab base usado é Assets/_DragonMOBA/VisualTest/PlayableCharacterVisual.prefab.
- ArtPresentation.BuildEnvironment encaminha a geração para OrganicArena na cena nova.
  A renderização dos retângulos é desativada; o collider de Solo permanece.
- Terreno procedural ativo com 25.665 vértices, bordas irregulares de caminhos, relevo de borda,
  rio com shader animado, três pontes, bases circulares e 125 instâncias de natureza.
- Torres/núcleos receberam arquitetura visual nova, mantendo entidades, posições e regras.
- Corpo CC0 realmente renderizado/animado; shader toon com roupa, botas/cinto, cabelo no osso
  e locks de cabelo originais, anel de seleção e aura com transparência.
- Controller de nove estados: Idle/Walk/Run/Attack/Hit/Death/Charge/Cast/Transform.
- Formas base e evoluídas distinguem cabelo, materiais/aura e tamanho dos locks.
  O teste acelera XP via GameWorld.GrantExperience; não altera FormId nem adiciona atalho ao jogo.
- A seleção dos três personagens e reinício continuam disponíveis; Kid/Cell ainda usam seus visuais anteriores.
- Nenhum arquivo de Assets/Scripts/Domain foi modificado; três hashes conferidos.

## TESTADO — provas
- VisualPrepare: compilação/importação/cena e referências diretas, sucesso.
- VisualPlay: três campeões, 12 formas, movimento, animação real dos ossos, collider único,
  materiais suportados, Q com dano, W com deslocamento, E com escudo, R com dano,
  Ki/C com partículas emitidas, XP, compra, tropas, ataque/dano de torre,
  morte/respawn, retorno à seleção/reinício e pool limitado: sucesso.
- Asserções específicas: mapa gerado ativo/frustum, ausência das faixas renderizadas,
  modelo estilizado realmente selecionado e dentro do enquadramento, solo clicável preservado.
- Destruição de torre: dano letal e visual efetivamente desativado, nos três campeões.
- Respawn real de Cell em nível5: 0/736HP por15,5s simulados e restauração de identidade/HP/Ki/inventário.
- VisualBuild: Windows x64 Development, sucesso.
- Novo executável aberto e janela1280×720 capturada. Cena/versão visíveis na imagem.
- Log nativo registrou RIGHT CLICK accepted=True e MOVEMENT CONFIRMED. A janela da loja
  foi capturada após P. Não se atribuem todos os cliques observados à automação.
- Sequência nativa completa de teclas não concluída: Windows perdeu/negou foco entre chamadas,
  e o script cancelou entradas. A compra e Q/W/E/R/C têm prova em Play Mode, não se afirma
  uma execução nativa automatizada completa desses comandos.
- Não houve erro C# ou exceção gerenciada de gameplay nos logs finais.
  Avisos existentes de token de licenciamento/D3D não impediram a execução.
- O assembly do jogo difere da build antiga: evidence/Binary-Identity.json.
  O .exe Unity é um launcher; sua data foi registrada após a build, e os dados ficam ao lado.

Logs na raiz Unity:
Logs/Validation-VisualPrepare.log, Validation-VisualPlay.log, Validation-VisualBuild.log,
Validation-Interactive-VisualTest-Final.log e Validation-Interactive-VisualTest-Executable.log.
Prova resumida de clique/movimento compilado: evidence/Native-Player-Proof.txt.

## Evidência visual
[Galeria antes/depois](evidence/Comparison.html).
- evidence/Before-Game-720.png: build anterior.
- evidence/After-Executable-720.png: janela real da build final.
- evidence/After-Executable-Shop.png: janela da nova versão com loja.
- evidence/After-Map-Overview.png: visão geral real em Play Mode.
- evidence/Character-super_base.png e Character-super_blue.png: duas formas em Play Mode.
- evidence/Form-super_*.png: demais variantes testadas.

A captura da câmera não contém IMGUI; as capturas do executável incluem HUD.
Nenhuma imagem foi substituída por mockup ou arte gerada.

## PENDENTE / limites reais
A apresentação é um primeiro protótipo original, não arte final de Goku/Cell.
Páginas de fãs retornaram403; arquivos/autoria/cadeia de direitos não verificados.
Foram usados humanoide/animações/natureza CC0 já disponíveis; ver ASSET_MANIFEST.md.
Vegetação/rochas são decorativas e não introduzem pathfinding/fog ou novos bloqueios.
Não se implementou multiplayer, ocultação por arbustos ou novos tipos lógicos de tropas.
Uma partida inteira, desempenho prolongado e escuta de áudio não foram comprovados.
Próxima ação: abrir JOGAR_VISUAL_TEST.cmd ou o executável acima; H mostra controles,
Esc permite escolher outro campeão. Próximo ticket: lapidar silhuetas, tropas e HUD compacto.
