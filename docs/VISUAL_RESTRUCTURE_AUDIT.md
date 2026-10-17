# VIS-001 — auditoria da reestruturação
Data: 2026-10-16. Diretrizes recebidas: VISUAL_DIRECTIVES.md.
Checkpoint anterior às mudanças de gameplay/apresentação: **5046d28dec67bb437feb321517e55efcd31094ae**.
Git inicializado localmente; nenhum push. Assets importados (~29,8 MB), scripts, cenas,
ProjectSettings e Packages versionados. Downloads, cookies, caches e builds excluídos do Git.

## Situação local observada
Unity 6000.6.6f1; Built-in; Input Manager antigo; simulação offline GameWorld a 30 Hz.
PrototypeGame envia comandos e apresenta snapshots. CharacterPresentation controla animação/aura;
ArtPresentation cuida de cenário, pool de 24 efeitos, som e skin IMGUI.
Não existe servidor remoto operacional integrado a essa cena.
Não há NavMesh/pathfinding com obstáculos, fog of war nem ocultação por arbustos.
Minions atuais compartilham um único tipo lógico; cerco/distância exigem um ticket de regras.
As posições das três rotas são X=-17/0/17, torres em Z=+-19 e núcleos em Z=+-38.
Essa geometria lógica permanece a referência para a reconstrução visual.

Assets: seis FBX (corpo, cabelo, biblioteca de animação e três modelos de natureza);
12 prefabs de forma usam o mesmo corpo e materiais diferentes. O cabelo importado ainda
não diferencia a silhueta. Há seis estados no Animator; preparação/carga/transformação
ainda não têm estados dedicados. Esse conjunto não atinge a referência anime solicitada.
Nature MegaKit CC0, subset Kenney e três sons já disponíveis; não precisam de novo download.
Ruínas e natureza originais continuam bloqueadas por cota do Drive.
Câmera anterior: ortográfica 20, deslocamento (0,31,-20), inclinação aproximada 57 graus,
sem zoom/pan. HUD grande, sem minimapa. Torres primitivas e tropas esféricas.

## Auditoria externa — código inspecionado, sem integração
Fontes fixadas por commit; 23 arquivos de licença, versão, dependências e sistemas baixados
apenas em external/review (ignorado pelo Git e fora de Assets). Índices completos de ambas
as árvores consultados; origem/hash de cada arquivo em EXTERNAL_AUDIT_SOURCES.csv.
Não se executou nem se compilou esses projetos. Não se auditou a totalidade de cada repositório.

### yasgamesdev/MOBA_CSharp_Unity
Commit f29f2f3da1f1f843292fa167785dbd446fbf8d52; licença raiz MIT.
Cliente Unity **2018.3.4f1**. Separação cliente/servidor C# útil como referência.
Movement interpola posição recebida; PlayerController envia ações à camada de rede.
RTSCamera oferece seguimento, pan, scroll e foco. Tower seleciona alvos e cria TowerBullet;
Minion usa estados e relay points. Skill/Combat/Item são componentes do servidor.
VisionParam representa visibilidade por time e prioridade.
Dependências observadas: servidor net461, CsvHelper12.1.2, MessagePack1.7.3.4,
Newtonsoft12.0.1, YamlDotNet5.3.0; README também aponta ENet/VelcroPhysics/SharpNav.
Essas dependências e contratos não pertencem ao nosso mundo. Compatibilidade com Unity6
não foi demonstrada; portar o framework seria uma migração ampla.
Decisão: referência de arquitetura/waypoints/visão; não importar módulos neste marco.

### tammukul/UNION-OpenSource-MOBA
Commit 77b090b70ef95280d508da1e65a7f747c6b5ac2a; licença raiz Apache-2.0.
Projeto efetivo em Project, Unity **5.1.0f3**.
MovementManager recebe touch e delega ao jogador; PlayerAIManager usa NavMeshAgent/Rigidbody e máquina de estados.
TowerManager trata alvo/tiro/dano com camada de rede. ShooterManager resolve disparos.
PlayerNetworkLayer usa PhotonView/RPC/serialização; loja usa PlayFabClientAPI.
Input móvel e fluxo com serviços externos não se encaixam no protótipo offline.
Não foi identificado um subsistema de três waves/tipos de minion ou fog nas fontes amostradas;
não se afirma ausência em toda a árvore. Bibliotecas vendorizadas exigem auditoria própria.
Decisão: não integrar PUN/PlayFab/Facebook/OpenIAB nem migrar o projeto.

| Sistema | Nosso protótipo | Referência aproveitável | Decisão |
|---|---|---|---|
| Movimento/câmera | V2 e câmera fixa | Pan/zoom/foco do primeiro projeto | Implementação local pequena |
| IA/tropas | Bot simples; 3 rotas retas | Waypoints e FSM | Ticket posterior; preservar regras |
| Torres | Prioriza tropas, dano no mundo | Separar projétil da regra de dano | Evoluir só apresentação agora |
| Habilidades/itens | QWER, Ki, loja local | Composição de efeitos | Catálogo atual continua |
| Visão/arbustos | Ausente | Visibilidade por time | Futuro sistema do domínio, com testes |
| Rede | Offline | Servidor C# independente / PUN RPC | Sem migração ou multiplayer neste marco |

Licenças raiz não substituem a revisão individual de plugins e assets.
Não houve cópia de código ou recursos dos repositórios para Assets.

## Cell com zero HP
A captura isolada não prova defeito: GameWorld define Hp=0, RespawnTime=8+1.5*Level e
restaura HP/Ki/posição depois do timer. O teste anterior encurtava o timer para .05s.
Novo teste deve percorrer o timer real de Cell em nível 5 (MaxHp736), conservar identidade
e inventário e confirmar o evento Respawn. A apresentação precisa explicar o intervalo
com um contador visível. Não alterar o tempo de respawn apenas para esconder a captura.

## Próximos marcos verificáveis
1. Baseline compilada + regressão real de Cell; indicar respawn no HUD.
2. Câmera inclinada, zoom/pan/foco e limites, sem modificar o domínio.
3. Mapa orgânico em cena separada: relevo decorativo fora de rotas, margens curvas,
   água com animação e distribuição natural; navegação/colisores permanecem até testes próprios.
4. Silhuetas/rig/shader anime e estados dedicados; variantes por forma.
5. VFX específicos, torres/robôs e HUD compacto/minimapa; visão real precisa de lógica testada.
A qualidade das imagens fornecidas é o alvo artístico; não se declara equivalência com a arte atual.

Fontes primárias:
- https://github.com/yasgamesdev/MOBA_CSharp_Unity
- https://github.com/tammukul/UNION-OpenSource-MOBA
- Arquivos exatos e commits: EXTERNAL_AUDIT_SOURCES.csv.

