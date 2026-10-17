# Reestruturação visual — primeiro marco, 16/10/2026

Auditoria e checkpoint Git concluídos. ArenaArt agora usa câmera mais próxima,
43graus, scroll para zoom8–18, setas/botão central para mover, Espaço para centralizar,
Y para alternar seguimento e H para abrir a ajuda, inicialmente oculta.
Cell derrotado mostra o tempo restante para respawn. Domínio/regras preservados.
Build e regressões passaram; janela/HUD conferidos em1280×720 e1920×1080.
Mapa orgânico e silhuetas anime definitivas continuam como próximos marcos.
[Resultados e escopo restante](DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/docs/VISUAL_RESTRUCTURE_STATUS.md).

O guia de abertura abaixo continua válido. BuildArtWindows atualizada:168152053bytes.

---
# Dragon Ball MOBA — abrir e testar

Atualizado em 16/10/2026. Protótipo offline validado no Unity **6000.6.6f1**.
Humanoides e cenário são recursos genéricos CC0; não são modelos oficiais de Dragon Ball.

## Pasta exata para o Unity Hub

Em **Projects > Add > Add project from disk**, selecione:

```text
<repo>\src\client-unity
```

A pasta principal DragonBall_Moba contém documentos e kits. A raiz Unity é a pasta
**client-unity** acima, que contém Assets, Packages e ProjectSettings.

## Primeiro teste

1. Abra essa pasta com Unity **6000.6.6f1**, já instalado.
2. Abra **Assets/_DragonMOBA/Scenes/ArtValidation/ArenaArt.unity**.
3. Pressione **Play**, selecione a aba **Game** e escolha um dos três personagens.
4. Clique direito no chão para andar ou num inimigo para atacar. Use **Q/W/E/R**;
   segure **C** para carregar Ki e, com Ki cheio, treinar XP fora da fonte.
5. **P** abre a loja (compras perto da fonte); **H** mostra/oculta os controles.
   As formas mudam automaticamente ao atingir os níveis definidos no jogo.
6. Observe o Console e pare o Play antes de editar.

A cena original **Assets/Scenes/ArenaPrototype.unity** permanece idêntica ao backup.
Ela também utiliza os novos prefabs opcionais de personagem.
Os três scripts do domínio mantêm seus hashes originais.

Para testar sem abrir o Editor, execute
[DragonMobaArt.exe](./DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/src/client-unity/Builds/ArtWindows/DragonMobaArt.exe).
Ao copiar o protótipo, copie a pasta **Builds/ArtWindows inteira**.

## O que foi integrado

- Modelo humanoide Quaternius, animações Idle/Walk/Run/Attack/Hit/Death, Root Motion desativado.
- 12 formas com materiais/cores/aura próprios; escala infantil diferenciada.
- Árvores, arbustos e rochas CC0, transparência de folhas, estruturas próprias para torres/núcleos.
- Aura de carregamento, feixes e impactos com limite de 24 efeitos e 64 partículas por aura.
- HUD Kenney usando os valores reais já existentes de HP/Ki/XP, nível, cooldown e loja.
- Três amostras de áudio com volume ajustado ao pico e intervalo mínimo de 160 ms.
  A confirmação de seleção/compra reutiliza a amostra eletrônica de energia.
- Built-in e Input Manager antigo mantidos; sem instalação global ou mudança de pipeline.

## Validação executada

- Importação/compilação no Unity 6000.6.6f1 e testes de domínio existentes: passaram.
- Primeiro humanoide testado antes de gerar as outras formas.
- Play Mode na cena de arte: três campeões, 12 formas, movimento/ossos animados,
  tropas, ataque com dano, Q, E, torre, Ki, XP, compra, morte/respawn, câmera e limite de efeitos: passou.
- Regressão da cena original: os três campeões passaram.
- Build Windows x64 de desenvolvimento: **168.149.205 bytes** reportados pelo Unity.
- Inicialização da build final: 10 segundos com Direct3D 12, sem exceções gerenciadas detectadas (menu apenas).
- Capturas reais da câmera e da janela do jogo foram inspecionadas. A captura da janela inclui o HUD; o contraste foi corrigido e a tecla P abriu a loja.

A automação chama o método do menu e envia os comandos existentes ao domínio.
Não substitui uma partida manual completa, testes de cliques/teclas físicos, escuta dos sons,
validação prolongada do HUD, balanceamento ou avaliação prolongada de desempenho.
W/R continuam preservados, mas não receberam uma asserção específica nesta rodada.

[Progresso, logs e evidências](./DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/src/client-unity/docs/art/STATUS.md).
[Licenças e hashes](./DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/src/client-unity/docs/art/ASSET_REGISTER.csv).

## VS Code e repetição dos testes

Abra **DragonBall_Moba** no VS Code. As configurações, tarefas e depurador estão em .vscode.
A integração usa **com.unity.ide.visualstudio 2.0.26**, compatível com a extensão Unity do VS Code.
O SDK **8.0.318** embutido no Editor foi aproveitado; não foi instalado SDK global.
A solução e os projetos C# foram sincronizados pelo Unity.

Com este projeto fechado no Editor, use **Terminal > Run Task**:

| Tarefa | Resultado |
|---|---|
| Unity Art: ArtPlay | Testes completos da cena de arte |
| Unity Art: ArtBuild | Build separada em Builds/ArtWindows |
| Unity: testar Play Mode | Regressão da cena original |
| Unity: preparar e validar | Configuração básica, domínio e sincronização C# |

Os comandos estão em scripts/validate_unity.ps1, na pasta DragonBall_MOBA_Prototype_v0_1.
Exemplo a partir dessa pasta:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode ArtPlay
```

Para depurar, abra o projeto no Unity e use **F5 > Conectar ao Unity - Dragon MOBA**.
Se o duplo clique num script não abrir o VS Code, selecione **Edit > Preferences >
External Tools > External Script Editor > Visual Studio Code**.
Essa preferência global não foi alterada. IntelliSense e breakpoints não foram testados interativamente.

## Bloqueios reais

Os downloads dos packs originais **Ultimate Stylized Nature** e **Ultimate Modular Ruins**
retornaram **Google Drive — Quota exceeded**. Foram substituídos por Nature MegaKit CC0
do mesmo autor e estruturas próprias; isso não impede executar o protótipo.
[Links e evidências do bloqueio](./DragonBall_MOBA_Codigo_Unity_v0_1/DragonBall_MOBA_Prototype_v0_1/src/client-unity/docs/art/PENDENCIAS.md).

O backup anterior à arte está em **client-unity/Backups/BeforeArt-20261015-192130.zip**.
Nenhum arquivo de domínio, cena original ou build anterior foi excluído.
