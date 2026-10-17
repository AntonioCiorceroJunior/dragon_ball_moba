# VIS-001 — primeiro marco da reestruturação
Data: 2026-10-16. Diretrizes: VISUAL_DIRECTIVES.md.
Auditoria: VISUAL_RESTRUCTURE_AUDIT.md. Fontes: EXTERNAL_AUDIT_SOURCES.csv.
Checkpoint prévio: **5046d28dec67bb437feb321517e55efcd31094ae**.

## Etapas concluídas
1. Auditoria da arquitetura/arte/versão e dos dois projetos MOBA; 23 arquivos externos lidos,
   fixados por commit, sem importação ou execução. Não há benefício demonstrado para migração.
2. Câmera inclinada a 43 graus, ortográfica inicial11,5, zoom8–18, setas/botão central para pan,
   Espaço recentraliza, Y alterna seguimento/livre, suavização e viewport limitado ao mapa72×88m.
   Ativa somente na cena de arte; câmera original preservada.
3. Cell0/736HP: teste em nível5 percorreu os15,5s simulados reais, verificou HP/Ki, evento,
   baseZ=-34, identidade e inventário. Não se reproduziu bug no timer.
   HUD agora explica a derrota com contador de respawn.
4. Ajuda inicialmente oculta (H); barras de vida desenhadas atrás dos painéis.

## Arquivos de implementação
- Assets/Scripts/Runtime/ArenaCameraController.cs — novo componente opcional.
- Assets/Scripts/Runtime/PrototypeGame.cs — hook, contador, ajuda e ordem de desenho.
- Assets/Editor/VisualStageChecks.cs e ArtSmoke.cs — regressões direcionadas.
- scripts/interactive_smoke.ps1 — resolução, zoom/setas/Espaço, janela por PID e captura completa.
- Nenhuma alteração em Assets/Scripts/Domain.

## Testes realmente executados
- Baseline do checkpoint: Logs/Validation-RestructureBaseline.log, na raiz Unity.
- Compilação Unity6000.6.6f1 e testes de domínio: passaram.
- ArtPlay: Cell com timer real; três campeões/12 formas, combate, Ki/XP, compra,
  morte/respawn, materiais, VFX e câmera: passaram.
- Câmera em cada campeão: zoom mínimo/máximo, pan, limites, recentralização e seguimento.
- Build Windows final: **168152053 bytes**.
- Janela real em **1280×720 e 1920×1080**, com HUD/barra inferior completos,
  inspecionada nas capturas. Ajuda oculta e sem sobreposição das barras nos painéis.
- Sem erro C# ou exceção gerenciada detectados nos logs finais.
- Scroll/seta/Espaço foram enviados à janela; a aprovação de cada função é apoiada
  pelas asserções em Play Mode. Não se atribuem todas as ações observadas à automação.

Provas na raiz Unity:
Logs/Validation-ArtPlay.log; Logs/Validation-ArtBuild.log;
Logs/Validation-Interactive-final-1080.log; Logs/Validation-Interactive-final-720.log;
Logs/Interactive-final-1080.png; Logs/Interactive-final-720.png.
As imagens mostram o protótipo real, sem ilustrações substituindo o resultado.

## Próxima etapa — VIS-002: mapa orgânico
Trocar representação retangular por terreno de borda, margens curvas, água animada,
vegetação agrupada e leitura de selva, preservando posições lógicas e colisões.
Navegação, bots e tropas precisam de regressão após a montagem.
Arbustos decorativos não constituem ocultação real: visão exige estado/testes no domínio.
Áreas para monstros podem ser desenhadas; monstros e novos tipos lógicos de tropas são tickets próprios.

## Ainda pendente nas diretrizes
Mapa/torres/robôs, silhuetas anime distintas, estados de carga/preparação/transformação,
variantes de cabelo/modelo/postura, VFX específicos e HUD compacto/minimapa/placar/configurações.
Não se declara equivalência artística entre as referências e os modelos/mapa atuais.
O conjunto disponível não contém os modelos definitivos dos três personagens.
Não há impedimento para iniciar o mapa com os recursos existentes; os dois packs
originais do Drive seguem bloqueados por cota, com alternativa CC0 já disponível.

