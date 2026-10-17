# Validação pela janela real — 2026-10-16

Ferramenta local: scripts/interactive_smoke.ps1.
Captura somente a área cliente da janela DragonMobaArt, após confirmar processo/caminho e foco.
Entradas sintéticas são enviadas por Win32 SendInput; não são chamadas diretas ao domínio.
Se a janela perder o foco, a ferramenta cancela a entrada/captura. Teclas são liberadas em finally.

## Resultados comprovados

- Build visível executou uma partida e desenhou o HUD IMGUI.
- Tecla P enviada pela automação abriu a loja.
- Capturas mostram HP/Ki/XP/nível/Zeni, estado de carregamento, cooldowns e textos da loja.
- A primeira captura revelou texto branco sobre painéis claros.
- Correção mínima em PrototypeGame.OnGUI: aplicar GUI.backgroundColor escuro somente na cena com ArtPresentation; restaurar o valor anterior após desenhar.
- Build recompilada: 168149205 bytes, testes de domínio passaram.
- Captura posterior confirma o contraste melhorado, incluindo os itens inferiores da loja.
- Log da build interativa sem exceções gerenciadas; encerramento normal do processo.

## Evidências

- ../../Logs/Interactive-menu.png — HUD anterior (a partida já estava iniciada).
- ../../Logs/Interactive-shop.png — loja anterior, contraste insuficiente.
- ../../Logs/Interactive-menu-final.png — HUD corrigido (partida já iniciada).
- ../../Logs/Interactive-shop-final.png — tecla P e loja com contraste corrigido.
- ../../Logs/Validation-Interactive.log — execução com janela e encerramento normal.
- ../../Logs/Validation-ArtBuild.log — build corrigida e domínio.

Os nomes "menu" identificam a etapa de captura; as imagens mostram uma partida, não a tela de seleção.
Não atribuir a seleção do personagem ou as outras ações observadas à automação:
ela não enviou o clique de seleção nessas sessões.

## Limites desta rodada

Foram enviados cliques direitos, mas a sequência terminou com perda de foco e fechamento da janela.
As tentativas seguintes de E/C foram canceladas porque o processo não estava mais ativo.
Portanto, esta rodada não comprova compra por clique, controles completos ou partida completa.
A qualidade audível não foi avaliada. O HUD agora possui evidência visual real,
superando a limitação anterior de captura em batchmode.

Documentação das APIs usadas:
https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput
https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow

