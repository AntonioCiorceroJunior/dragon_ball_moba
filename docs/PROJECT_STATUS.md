# Estado anterior à centralização

Registro histórico importado. Estado vigente: [coordination/PROJECT_STATUS.md](coordination/PROJECT_STATUS.md). Tarefa ativa: [coordination/TO_CODEX.md](coordination/TO_CODEX.md). Não executar tickets históricos sem nova autorização.

---

# VIS-002 — FEITO / TESTADO — substituição visual real
Mapa orgânico e lutador CC0 animado efetivamente ligados ao instanciador.
Cena atual: Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity.
Build nova: Builds/VisualTest/DragonMobaVisualTest.exe. Versão VISUAL TEST 01.
1280×720 no executável capturado; antes/depois em docs/evidence/Comparison.html.
Domínio e cenas antigas preservados; shader/roupa/cabelo/aura e formas por nível testados.
Play Mode passou nos três campeões/12 formas, QWER/Ki/XP/compra/tropas/torres/morte/respawn/seleção.
Clique direito/deslocamento observados no log nativo; sequência completa de teclas nativas
interrompida por perda de foco, sem atribuir esse teste à automação completa.
[Resultados e evidências](VISUAL_REPLACEMENT_RESULTS.md); [licenças](ASSET_MANIFEST.md).
Próximo: polimento de silhuetas, tropas e HUD. Não declarar modelo oficial de Dragon Ball.

## Histórico anterior
# Atualização VIS-001 — 16/10/2026

Direção artística registrada em VISUAL_DIRECTIVES.md. Auditoria/checkpointGit5046d28 concluídos.
Câmera43graus, zoom/pan/foco/limites testados. Cell0/736HP com15,5s de respawn passou;
contador no HUD. Ajuda opcional e barras atrás dos painéis. Domínio inalterado.
Play Mode dos três campeões/12formas e build168152053bytes aprovados.
Capturas de HUD em1280×720 e1920×1080 inspecionadas; sem exceções nos logs.
[Progresso e escopo restante](VISUAL_RESTRUCTURE_STATUS.md).
Próximo: VIS-002 mapa orgânico. Modelos ainda provisórios.

## Histórico anterior
# Atualização — ART-001 — 16/10/2026

Integração visual autorizada concluída e validada no Unity 6000.6.6f1.
Sete pacotes CC0 rastreados; 12 formas humanoides; ArenaArt separada; cenário, VFX,
HUD e áudio. Domain e cena original preservados por SHA-256.
Play Mode: três campeões/12 formas, movimento/ossos, combate, Ki/XP, compra,
torre, morte/respawn, câmera e limite de efeitos passaram.
Regressão da cena original passou. Build ArtWindows: 168148649 bytes.
Evidências: [STATUS](../src/client-unity/docs/art/STATUS.md).
Impedimentos: [PENDENCIAS](../src/client-unity/docs/art/PENDENCIAS.md).
Próximo aceite: partida manual com entradas físicas e conferência visual/sonora.
Os dois packs originais do Drive excederam a cota; alternativas já integradas.

## Registro histórico anterior
# Estado do projeto — handoff compacto

Atualizado em: 2026-10-15
Fase: primeiro vertical slice offline C# compilado e validado automaticamente no Unity 6000.6.6f1; playtest interativo pendente.

## Feito (arquivos gerados)
- Documento Mestre v1.1 e plano Codex/Work preservados.
- Projeto Unity mínimo `src/client-unity/` com domínio C# sem `UnityEngine`.
- `GameWorld` offline a 30 ticks/s; comandos validados de movimento, ataque, C, Q/W/E/R, compras.
- Ki, XP por treinamento, níveis 1–18, estágios de transformação, Zeni, quatro itens básicos.
- 3 campeões placeholders (Goku Criança, Goku Super, Cell), 3 rotas, 6 torres, núcleos, ondas, bot adversário.
- Runtime com arena gerada por primitivas, câmera isométrica, HUD OnGUI e loja simples.
- Editor: gerador de cena e testes básicos sem pacote externo.
- `scripts/verify_unity_prototype.py`: integridade estática básica (não compila C#).

## Validação local — 15/10/2026
- Unity instalado: **6000.6.6f1 (d20c9ac777da)**. Importação e compilação C# concluídas; Input Manager antigo e renderização Built-in.
- Cena `Assets/Scenes/ArenaPrototype.unity` criada pelo gerador existente e incluída no Build.
- `DomainSelfTests.Run`: PASSOU (Ki, XP, cancelamento de carga, compras, formas, nível máximo e mitigação).
- Play Mode real: GKU_KID, GKU_SUPER, CELL_Z; 90 frames/44 ticks por personagem, movimento em FixedUpdate, câmera, materiais e objetos. Direct3D 12 / RTX 3060. Sem exceções de execução na rodada final.
- Build Windows x64 de desenvolvimento: sucesso, **157.624.129 bytes**; `Builds/Windows/DragonMoba.exe`.
- VS Code: solução/projetos gerados pelo Unity; SDK 8.0.318 embutido avalia ambos os projetos `netstandard2.1`; configurações, depurador e tarefas na pasta principal.
- Seis scripts C# originais idênticos ao ZIP por SHA-256; apenas ferramentas novas em `Assets/Editor`.
- Dependências: IMGUI 1.0.0 e IDE 2.0.26 adicionados; Test Framework 1.8.0/NUnit 2.1.0 resolvidos pelo Editor. Nenhum SDK global ou outro Editor instalado.
- Evidências: [preparação](../src/client-unity/Logs/Validation-Prepare.log), [Play Mode](../src/client-unity/Logs/Validation-PlayMode.log), [build](../src/client-unity/Logs/Validation-Build.log). [Guia completo](../../../README.md).

## Não comprovado / ainda faltando
- Playtest interativo completo, entrada física, compra via HUD, sessão de breakpoints/IntelliSense e execução interativa da build. O smoke test inicia partidas pelo método do menu e envia comandos de movimento ao domínio.
- Multiplayer, servidor dedicado remoto, login, matchmaking, fog of war, pathfinding real, balanceamento competitivo, fusões, 15 kits e modelos 3D licenciados.
- Base open source de terceiros permanece **não auditada** (AUD-001 TODO).

## Tarefa ativa
PROTO-VERIFY-001: parte automatizada concluída; aceite de partida manual parcial permanece pendente.

## Decisão provisória
Manter vertical slice próprio isolado para testar conceitos enquanto AUD-001 está pendente. Não migrar nem copiar código third-party sem auditoria/licença. Depois decidir qual código aproveitar.

## Uso econômico de Codex
Enviar somente `prompts/05_COMPILE_PROTOTYPE.md`; manter escopo 1-5 arquivos por ajuste e relatar bugs concretos, sem reabrir GDD inteiro.

Atualização adicional: HUD capturado pela janela real e contraste corrigido.
Tecla P abriu a loja; build corrigida e domínio passaram (168149205 bytes).
A janela foi fechada durante os testes seguintes; sequência completa de controles não concluída.
Evidências em src/client-unity/docs/art/INTERACTIVE_VALIDATION.md (a partir da raiz do protótipo).