# AutoArte v2 — progresso e provas

Atualizado: 2026-10-16. Trabalho autorizado pelo usuário em CODEX_EXECUTAR_TUDO.md.
Todas as etapas executáveis localmente foram concluídas.

- [x] Fase 0: raiz real, auditoria, backup e hashes (BASELINE.md).
- [x] Fase 1: sete ZIPs CC0 verificados, reutilizados e rastreados; dois packs Drive bloqueados por cota.
- [x] Fase 2: extração fora de Assets, triagem e 26 arquivos importados, incluindo licenças.
- [x] Fase 3: primeiro humanoide aprovado em Play Mode antes das 12 formas.
- [x] Fase 4: ArenaArt separada, natureza alternativa e estruturas próprias; três rotas preservadas.
- [x] Fase 5: aura, feixes, impactos, HUD com dados reais e áudio limitado/normalizado.
- [x] Fase 6: compilação, domínio, Play Mode de arte, regressão, imagens e build Windows.

## Evidências reproduzíveis

Execute scripts/validate_unity.ps1 na pasta DragonBall_MOBA_Prototype_v0_1 com -Mode:

| Modo | Prova | Resultado |
|---|---|---|
| ArtImport / ArtBase | ../../Logs/Validation-ArtBase.log | Ambos os avatars Humanoid válidos; seis estados, sem Root Motion |
| ArtPlay (base primeiro) | ../../Logs/Validation-ArtBasePlay.log | Movimento, animação real dos ossos, materiais e colisor único |
| ArtForms | ../../Logs/Validation-ArtForms.log | 12 prefabs nos caminhos Resources/CharacterForms do catálogo |
| ArtScene | ../../Logs/Validation-ArtScene.log | Cena copiada, materiais Built-in, modelos/sons/UI atribuídos |
| ArtPlay | ../../Logs/Validation-ArtPlay.log | GKU_SUPER 273 ticks; GKU_KID 260; CELL_Z 261; todos passaram |
| PlayMode | ../../Logs/Validation-PlayMode.log | Regressão da cena original; três campeões, 44 ticks cada |
| ArtBuild | ../../Logs/Validation-ArtBuild.log | Windows x64 Development: 168149205 bytes |

Os testes de domínio originais passaram nas execuções ArtPlay/PlayMode/ArtBuild.
Nenhum erro C# ou exceção gerenciada foi encontrado na rodada final.
O teste arrangeia posições/HP/XP para exercitar cenários sem esperar uma partida inteira.
Usa os comandos reais para movimento, carga, compra, ataque, Q e E.
A morte/respawn e a troca das 12 formas foram verificadas em Play Mode.
A troca visual preservou identidade, HP/Ki, velocidade, inventário e alvo após a progressão do domínio.
Verifica ossos animados (variação acima de 83 graus), altura de 1,82–2,39 m,
colisor único, partículas efetivamente emitidas, câmera e pool de no máximo 24 efeitos.
HUD: recursos atribuídos e leitura do estado existente; não houve asserção de pixels ou cliques no HUD.
Sons: picos analisados em runtime e ganho limitado a 0,3; intervalo mínimo de 160 ms;
audibilidade percebida não foi testada.

## Capturas reais

- ../../Logs/Art-GKU_SUPER.png e Art-Detail-GKU_SUPER.png
- ../../Logs/Art-GKU_KID.png e Art-Detail-GKU_KID.png
- ../../Logs/Art-CELL_Z.png e Art-Detail-CELL_Z.png

Capturas obtidas por Camera.Render/ReadPixels durante Play Mode, não ilustrações.
A captura de detalhe usa zoom temporário, restaurado imediatamente.
As imagens não incluem IMGUI. ScreenCapture no batchmode não produziu arquivo;
foi removida a dependência de screencapture que deixou de ser necessária.
Não há vídeo gravado. Inspeção detectou/corrigiu escala de importação e recorte/tintura das folhas.

## Preservação

- Backup: ../../Backups/BeforeArt-20261015-192130.zip
- SHA-256 do backup: 18F6F9E94CE3B3551022416A77C12018A17CD676557F66ECCEFC3C09A4CBCEB4
- Três arquivos Domain idênticos ao registro DOMAIN_BASELINE.csv.
- ArenaPrototype.unity idêntica ao backup:
  7055C96B9FA21BFBEC6DC5D06A1374DAC854D993562458E4E3C2D15DF5EB9BBA.
- PrototypeGame recebeu somente integração da apresentação; a simulação GameWorld não foi reescrita.
- Build anterior preservada em Builds/Windows; nova build em Builds/ArtWindows.
- Pasta não é um repositório Git; foi usado backup, sem commits artificiais.

## Recursos, compatibilidade e limites

ASSET_REGISTER.csv registra os sete arquivos ZIP e seus hashes.
IMPORTED_ASSETS.csv registra os 26 arquivos importados e hashes individuais.
ENVIRONMENT_DOWNLOADS.csv contém as respostas HTML rejeitadas; nunca foram importadas como FBX.
O manifesto do kit foi corrigido para um array plano de nove recursos e atualizado com status/hashes/destinos.
Os modelos genéricos e as formas não são modelos oficiais de Dragon Ball.

Unity 6000.6.6f1, Built-in e Input Manager antigo. IDE 2.0.26 e módulos embutidos 1.0.0;
Test Framework 1.8.0 e NUnit 2.1.0 resolvidos pelo Editor. Apenas particlesystem e
imageconversion foram acrescentados nesta etapa. Sem instalações globais.
A biblioteca de animações Standard do espelho do autor foi importada e retargetada com sucesso;
não foi trocada pela revisão mais nova apenas por existir uma versão posterior.

A revisão visual do HUD, entrada física, escuta de áudio, partida completa,
balanceamento e desempenho prolongado continuam sendo testes interativos, não resultados desta automação.
Para o primeiro teste, siga o README da pasta DragonBall_Moba.

## Inicialização da build final

A build Builds/ArtWindows/DragonMobaArt.exe foi iniciada com Direct3D 12 / RTX 3060,
permaneceu ativa por 10 segundos e não registrou exceções gerenciadas.
Somente a inicialização/menu foi exercitada no player; a partida automatizada foi testada no Editor.
Provas: ../../Logs/Validation-ArtPlayer.log e ../../Logs/ArtPlayer-Smoke.txt.
O processo de teste foi encerrado; nenhum Editor ou player ficou aberto.

## Validação adicional pela janela real

O HUD foi capturado fora do batchmode e a tecla P abriu a loja.
Foi identificado e corrigido o contraste dos painéis, preservando a cena original.
Build recompilada e domínio aprovados; novo tamanho: 168149205 bytes.
A sequência posterior foi interrompida pelo fechamento normal da janela.
Veja INTERACTIVE_VALIDATION.md para capturas, comandos e limites precisos.
As observações anteriores sobre ausência de captura do HUD referem-se somente ao batchmode.