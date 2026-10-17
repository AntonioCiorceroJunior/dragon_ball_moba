# Dragon MOBA — versão nova: VISUAL TEST 01

## Jogar
Dê duplo clique em **JOGAR_VISUAL_TEST.cmd**, nesta pasta.
Ele abre exclusivamente a build nova, com mapa orgânico e lutador animado.

Executável exato:
```text
<repo>\src\client-unity\Builds\VisualTest\DragonMobaVisualTest.exe
```

O jogo começa com o campeão lógico GKU_SUPER usando um visual genérico CC0.
**Esc** retorna à seleção dos personagens existentes.
Clique direito move/ataca; **Q/W/E/R** usam técnicas; **C** carrega Ki;
**P** abre a loja, **H** mostra ajuda. Scroll ajusta zoom, setas/botão central movem a câmera,
Espaço centraliza e Y alterna seguimento.

Confira o selo **VISUAL TEST 01 - Organic Arena**.
As pastas Builds/Windows e Builds/ArtWindows contêm versões anteriores.
Ao copiar o jogo, copie a pasta **Builds/VisualTest inteira**.

## Abrir no Unity
No Unity Hub, adicione a pasta:
```text
<repo>\src\client-unity
```
Use Unity **6000.6.6f1** e abra:
`Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`.
Pressione Play. O mapa é gerado pelo runtime dessa cena e o lutador é ligado explicitamente ao instanciador.
Built-in e Input Manager antigo foram mantidos; as cenas antigas continuam preservadas.

## Provas e testes
[Comparação antes/depois, mapa e formas](DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/docs/evidence/Comparison.html).
[Resultados, build, testes e limitações](DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/docs/VISUAL_REPLACEMENT_RESULTS.md).
[Origem e licenças](DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/docs/ASSET_MANIFEST.md).

Compilação, domínio, Play Mode dos três campeões/12 formas, QWER/Ki/XP/compra,
tropas, torres, morte/respawn, seleção/reinício e build Windows passaram.
O executável foi aberto e capturado; cliques/movimento foram confirmados no log.
A sequência completa de teclas nativas foi limitada por perda de foco; os comandos foram
validados em Play Mode. Não houve avaliação de partida completa ou áudio percebido.

Para repetir, com o projeto fechado no Editor, na pasta DragonBall_MOBA_Prototype_v0_1:
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPlay
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualBuild
```

## VS Code
Abra DragonBall_Moba no VS Code. Configurações/tarefas/depurador existentes foram mantidos.
Os projetos C# são gerados pelo Unity. Use F5 para conectar ao Editor.
Não foi instalado SDK global nem importado framework externo.
A versão anterior deste guia foi preservada em docs/ROOT_README_BEFORE_VISUAL_TEST.md,
na raiz DragonBall_MOBA_Prototype_v0_1.

