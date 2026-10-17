---
title: "DRAGON BALL MOBA — ESPECIFICAÇÃO MESTRE"
subtitle: "Game Design Document + Technical Design Document + Plano de Produção"
author: "Documento de planejamento de produto e engenharia"
date: "15 de outubro de 2026"
lang: pt-BR
---

# Índice geral

Este índice identifica os capítulos do documento. No Word, também é possível navegar pelos títulos no Painel de Navegação.

- **01** — Visão de produto, público e critérios de sucesso
- **02** — Jornada completa: clique no EXE até fechar o aplicativo
- **03** — Launcher, atualização, instalação e integridade
- **04** — Conta, identidade, preferências e perfis
- **05** — Client social, home, amigos e lobby
- **06** — Fila, matchmaking, aceite e seleção de personagens
- **07** — Entrada na partida, câmera, HUD e controles
- **08** — Mundo, navegação, rotas e estruturas
- **09** — Selva, criaturas, objetivos e mudanças do mapa
- **10** — Visão, informação e navegação
- **11** — Simulação de combate: regras e fórmulas
- **12** — Ki, treinamento, níveis e economia
- **13** — Sistema de transformações, animações e visual
- **14** — Elenco inicial: 15 fichas jogáveis
- **15** — Fusão, Oozaru e mecânicas cooperativas
- **16** — Itens, loja, receitas, cosméticos e consumíveis
- **17** — Escolas de Treinamento (runas) e técnicas universais
- **18** — Bots, treino, modos e progressão da conta
- **19** — Fim da partida, pós-jogo e encerramento
- **20** — Arquitetura de software e divisão de responsabilidades
- **21** — Protocolo de rede, sincronização e reconexão
- **22** — Backend, APIs, persistência e fluxos críticos
- **23** — Organização do repositório, convenções e ferramentas internas
- **24** — Pipeline de arte, animação, som e licenças
- **25** — Segurança, privacidade, moderação e conformidade
- **26** — Implantação, serviços e custos operacionais
- **27** — Testes, aceitação e critérios de qualidade
- **28** — Estratégia de desenvolvimento, marcos e prioridades
- **29** — Inventário final de sistemas e cobertura de equivalências MOBA
- **30** — Apêndices de implementação e exemplos
- **31** — Fontes e referências verificadas (15/10/2026)
- **32** — Histórico, governança e próximos passos
- **33** — Operação Work + VS Code + Codex e otimização de contexto

---


# Controle do documento

**Versão:** 1.1 — especificação de referência para pré-produção. **Plataforma inicial:** Windows 10/11, x64, mouse e teclado. **Gênero:** MOBA isométrico 3D, 5v5, três rotas, selva e partidas com duração-alvo de 25 a 40 minutos. **Classificação de decisões:** `[D]` decisão de design tomada para a primeira implementação; `[P]` proposta sujeita a teste; `[V]` verificação técnica/licença obrigatória; `[F]` etapa futura. Um número proposto é um alvo de teste, não uma garantia de equilíbrio.

**Objetivo deste documento.** Servir de fonte única de referência da abertura do executável ao encerramento do processo: instalador, atualizador, login, client social, matchmaking, seleção, carregamento, partida, resultado, reconexão, configurações, telemetria, arquitetura de software, produção de assets, distribuição, segurança, operação, testes e roadmap. Abrange também todas as interfaces entre esses elementos.

**Limite importante.** Um documento completo de produto não implica que cada valor de dano, custo e latência esteja comprovado. Tais parâmetros serão versionados como dados de balanceamento, medidos no protótipo e atualizados por testes. Este material contém especificações de implementação e propostas explícitas onde ainda não há comprovação técnica.

**Propriedade intelectual.** A mecânica do gênero MOBA pode orientar o design, mas não devemos extrair ou redistribuir código proprietário, arquivos de personagens, mapa, HUD, ícones, efeitos ou áudio de League of Legends. Dragon Ball, personagens, nomes, visuais e materiais de jogos oficiais são protegidos por direitos de terceiros. Uma licença de repositório aberto cobre apenas aquilo que seus autores realmente podem licenciar. **Protótipos internos com placeholders próprios/licenciados são a via preferida; distribuição pública de materiais Dragon Ball depende de direitos/autorização específicos.** O projeto deve poder trocar o conteúdo temático sem reescrever seu núcleo técnico.

## Decisões principais e pontos de validação

| ID | Tema | Definição | Estado |
|---|---|---|---|
| DEC-01 | Motor | Unity 6.3 LTS + C#; Universal Render Pipeline (URP) | D |
| DEC-02 | Base de código | Auditar MOBA_CSharp_Unity, MIT; portar apenas subsistemas viáveis | V |
| DEC-03 | Rede | Servidor dedicado autoritativo; cliente envia comandos, não resultados | D |
| DEC-04 | Backend | ASP.NET Core em .NET 10 LTS, PostgreSQL, Redis opcional | P |
| DEC-05 | Regras | 5v5, três rotas, selva, torres, geradores e núcleo | D |
| DEC-06 | Elenco | 15 variantes distintas, seis Gokus e dois Vegetas | D |
| DEC-07 | Progressão | Níveis 1–18 no lançamento; nível 20 via missão é opcional futuro | D/F |
| DEC-08 | Ki | Tecla C carrega; Ki cheio habilita treino de experiência lento e vulnerável | D |
| DEC-09 | Transformação | Troca visual, de efeitos e propriedades em marcos definidos | D |
| DEC-10 | Fusão | Duas pessoas operam um único personagem; apenas fase avançada | P |
| DEC-11 | Comunicação | Chat de equipe, pings e bloqueio; voz opcional posterior | D/F |
| DEC-12 | Ativos de terceiros | Inventário de licenças e evidências de origem antes de integrar | D |
| DEC-13 | Loja real | Somente cosméticos licenciados e sem pagamento por vantagem; fora do MVP | F |
| DEC-14 | Base do jogo | Reaproveitar conceitualmente sistemas conhecidos; UI/arte próprias | D |

# 1. Visão de produto, público e critérios de sucesso

## 1.1 Experiência pretendida

Um jogador abre o executável, autentica sua conta, encontra amigos, seleciona uma função, entra em uma fila, escolhe uma versão de Dragon Ball, configura Escola de Treinamento e técnicas universais, assiste ao carregamento, participa de uma partida MOBA 5v5, evolui até formas de combate avançadas, disputa objetivos e recebe um resultado completo. Poderá também jogar sozinho contra bots ou treinar offline/localmente, conforme a fase do projeto.

**Pilares de experiência:** (1) legibilidade estratégica antes de espetáculo visual; (2) fantasia reconhecível de cada saga; (3) escolhas de risco/recompensa ligadas a Ki, treinamento, mobilidade e transformação; (4) justiça competitiva; (5) baixo tempo entre decisão e resposta dos controles; (6) continuidade da sessão mesmo diante de falhas recuperáveis.

## 1.2 Escopo por modalidade

| Modalidade | Entrega inicial | Regras especiais |
|---|---|---|
| Ferramenta de treino | Sim | Alvos estáticos, bots, nível e Zeni ajustáveis; pausável |
| 1v1 em rota | Protótipo | Uma rota, 2 personagens, sem ranking |
| 5v5 contra bots | Vertical slice | Mesma lógica de servidor, bots substituem humanos |
| 5v5 personalizada | Beta privado | Convites, sala, composição livre e regras configuráveis |
| 5v5 normal | Beta público condicionado | Matchmaking, abandono, estatísticas |
| Ranqueada | Após balanceamento | Classificação, integridade competitiva, restrições e temporadas |
| Replay / espectador | Posterior | Política de atraso para evitar vantagem informacional |

O escopo inicial não inclui consoles, mobile, cross-play, mercado de skins, torneios oficiais, mecânicas pay-to-win nem compatibilidade com o cliente real do LoL.

## 1.3 Qualidade esperada e indicadores

- [P] 60 FPS estáveis como alvo em hardware Windows intermediário; perfil econômico 30 FPS, preservando simulação do servidor.
- [P] Em região adequada, p95 do RTT de rede abaixo de 100 ms; experiência aceitável dependerá da infraestrutura e do local do jogador.
- [P] Disponibilidade de serviços de conta acima de 99,5% em beta, monitorada, não prometida antes da operação.
- [P] Zero progressão autoritativa concedida pelo cliente sem validação do servidor.
- [P] 95%+ das partidas de teste completam fluxo de início ao pós-jogo sem falha de software bloqueante.
- [D] Nenhuma compra com dinheiro real altera poder ou matchmaking.

# 2. Jornada completa: clique no EXE até fechar o aplicativo

## 2.1 Máquina de estados de alto nível

`DESINSTALADO → INSTALANDO → BOOTSTRAP → VERIFICAÇÃO/ATUALIZAÇÃO → AUTENTICAÇÃO → CLIENT_SOCIAL → FILA → PARTIDA_ACEITA → SELEÇÃO → CARREGAMENTO → PARTIDA → RESULTADO → CLIENT_SOCIAL → SAÍDA`.

Transições especiais: `BOOTSTRAP/LOGIN/SELEÇÃO/PARTIDA → ERRO_RECUPERÁVEL`, com `TENTAR_NOVAMENTE`; `PARTIDA → DESCONECTADO → RECONEXÃO → PARTIDA`; `QUALQUER_TELA_SEM_PARTIDA → SAÍDA`; `PARTIDA → SOLICITAR_SAÍDA → ADVERTÊNCIA/RECONEXÃO`. Toda transição emite evento de telemetria com `session_id`, estado anterior, novo estado e código de motivo, sem registrar senha.

## 2.2 Fluxo e comportamento por tela

| Etapa | Entrada | Tela, feedback e animação | Sucesso | Erros, escape e recuperação |
|---|---|---|---|---|
| Instalador | Arquivo assinado | Pasta de destino, tamanho, progresso, licença; sem barras falsas | Cria atalhos e desinstalador | Falta de disco; permissão negada; rollback |
| Launcher/Bootstrap | Duplo clique | Splash próprio de 1–2s, indicador discreto de estado | Carrega manifesto assinado | Instância duplicada → focar existente; processo travado → log |
| Verificador | Launcher pronto | Diagnóstico de versão, arquivos e conexão; passos reais | Define build aplicável | Sem internet → modo treino disponível se assets locais válidos |
| Atualizador | Manifesto remoto mais novo | MB total, velocidade real, pausa/repetição, notas de versão | Faz download, hash, instalação atômica | Disco cheio, CDN falha, hash inválido → não executa pacote |
| Inicialização gráfica | Conteúdo válido | Logo do jogo, controles e áudio carregados progressivamente | Abre autenticação ou sessão persistida | Driver/API não suportados → diálogo e modo seguro |
| Login | Sem sessão | E-mail, senha/passkey se implementada, mostrar/ocultar senha | Obtém tokens e perfil | Erro neutro, limitação de tentativas, redefinição por e-mail |
| Home | Autenticado | Avatar, nome, notícias, amigos, status e CTA Jogar | Abre áreas secundárias | Backend parcial → feedback de indisponibilidade por serviço |
| Lobby | Clique em Jogar | Criar grupo, convidar amigos, funções e prontidão | Entra na fila | Jogador incompatível, expulsão amigável, dissolução |
| Matchmaking | Grupo pronto | Busca animada com duração informativa, botão cancelar | Encontrou 10 vagas | Desconexão, cancelamento, tentativa automática controlada |
| Aceite | Match encontrado | Modal central, contagem e 10 indicadores de aceitação | Todos aceitam | Recusa/timeout libera jogadores à fila, penalidade graduada |
| Seleção | Aceite completo | Timer, ordens de pick, filtros, descrição, runas e skins | Todos bloqueiam escolhas válidas | Timeout → padrão explícito/abandono segundo modo |
| Loading | Draft fechado | Retratos, formas iniciais, dicas, prontidão e conexão | Servidor validou 10 clientes ou política de espera | Falha de asset, reconnect, timeout e cancelamento protegido |
| Entrada na partida | Snapshot inicial | Câmera sobre base, apresentação curta e HUD; pular se permitido | Controles ativos ao fim da fase pré-jogo | Jogador ausente → pausa/tolerância do modo |
| Partida | Cronômetro em andamento | Mundo 3D, HUD, áudio, combate e menus | Núcleo destruído ou decisão válida | Queda de rede, tela de reconexão, alt-tab, FPS baixo |
| Vitória/Derrota | Resultado autoritativo | Câmera sobre núcleo, animação contextual e tela de resultado | Grava resultado e estatísticas | Retry idempotente de salvamento |
| Pós-jogo | Cena de resultados | Recompensas, estatísticas, reportar, jogar de novo | Retorna ao lobby/home | Backend indisponível → resultado pendente localmente |
| Encerramento | Sair | Confirmar se há partida ativa, salvar preferências, liberar recursos | Processo termina | Não bloquear saída por rede; fechar conexões com timeout curto |

## 2.3 Atalhos e interrupções globais

`Esc` fecha modal de maior prioridade ou abre configurações na partida; `Alt+Tab` preserva estado; `Alt+F4` solicita encerramento e informa consequências de abandonar; `Enter` foca chat onde permitido; `F11` troca janela/tela cheia se habilitado; captura de tela usa atalho configurável. Ao minimizar, reduzir FPS do menu; em partida, não interromper simulação nem áudio de alertas críticos se as preferências permitirem. Falha ao sincronizar uma configuração não impede sair.

## 2.4 Padrões visuais de transições de interface

Tempo-alvo de microinteração de botão 80–180 ms; abertura de painel 180–280 ms; troca de tela 250–450 ms; notificações discretas 3–6 s, persistência enquanto necessárias. Opção de reduzir movimento desliga zooms, tremores e partículas ornamentais. Spinners representam tarefas indeterminadas; progressos com porcentagem requerem bytes ou passos mensuráveis. Controles ficam desabilitados somente enquanto há operação pendente; cada operação terá estados `idle`, `loading`, `success`, `failure`, `retry`.

# 3. Launcher, atualização, instalação e integridade

## 3.1 Componentes e responsabilidades

**Launcher separado do jogo:** app leve em .NET/Windows, responsável por autenticação inicial opcional, manifesto, integridade, configuração de região, instalação/patch, diagnósticos e inicialização do executável Unity. Não confundir Launcher com UI de lobby: na primeira entrega, lobby e client social podem estar dentro do Unity para acelerar desenvolvimento; mais tarde podem migrar para um client separado. **Game.exe:** renderização e experiência. **MatchServer:** processo headless isolado. **Backend API:** contas e sessões. **CDN/Storage:** atualizações e assets.

## 3.2 Protocolo de patch

Manifesto JSON assinado contendo `version`, `min_client_version`, `channels`, `files[{path, size, sha256, url}]`, `content_catalog_version`, `release_notes`, `mandatory`, `expiry`. O launcher verifica assinatura e hash, faz download por arquivos/chunks em pasta temporária, retoma quando possível e efetua substituição atômica. Não permite caminhos relativos `../`, DLLs arbitrárias vindas de origem não verificada nem execução de scripts remotos sem assinatura. Releases possuem canais `dev`, `staging`, `beta` e `stable`.

## 3.3 Falhas previstas

- Conexão ausente: botão Repetir e opção Treinamento Offline, se implementada e versão compatível.
- Manifesto expirado/inválido: bloquear atualização e apresentar código de erro com link de suporte.
- Download interrompido: retomar por arquivo ou baixar novamente sem corromper instalação anterior.
- Atualização obrigatória durante partida: manter cliente e servidor compatíveis até terminar partida ou impedir novas filas, nunca expulsar silenciosamente uma partida válida.
- Disco insuficiente: mostrar espaço necessário antes de baixar.
- Versão não compatível com o servidor: mensagem clara; impedir matchmaking e oferecer atualizar.

# 4. Conta, identidade, preferências e perfis

## 4.1 Registro e login

Fluxo: criar conta com e-mail, identificador público e senha forte (ou passkey quando incluída), confirmar e-mail, aceitar documentos aplicáveis, entrar, redefinir senha, encerrar sessão, alterar credenciais e excluir conta. E-mails verificados são únicos; apelidos públicos seguem regras de unicidade; não expor presença de contas por mensagens de erro. Preferir identidade gerenciada confiável ou biblioteca sólida a autenticação artesanal. Armazenar senhas somente com hash adaptativo robusto (Argon2id se suportado na escolha final), salt exclusivo; tokens de acesso de curta duração, refresh rotativo, revogação por dispositivo e transporte TLS.

## 4.2 Dados salvos da conta

Identificação interna UUID; nome público; avatar; região; idioma; acessibilidade; preferências por dispositivo; histórico resumido; estatística por campeão; inventário cosmético; listas de amigos/bloqueio; restrições de moderação; nível da conta e progressão não competitiva; consentimentos versionados; dispositivos/sessões recentes; flags de experimentos. Não armazenar endereço completo ou dados financeiros desnecessários.

## 4.3 Tela de configurações

| Aba | Controles previstos | Comportamento |
|---|---|---|
| Vídeo | Resolução, modo janela, monitor, escala, VSync, cap FPS, sombras, partículas, antialiasing | Confirmar mudanças de resolução em 15s ou reverter |
| Áudio | Master, efeitos, música, voz, alertas, dispositivo, dynamic range | Prévia imediata e botão restaurar |
| Controles | Remap QWER/DF/C/itens, câmera, movimento e ataque, smart cast | Detecta conflitos e reserva atalhos críticos |
| Interface | Escala HUD/minimapa, barra de Ki, nomes, números de dano, tooltips | Mostra prévia e preserva legibilidade |
| Acessibilidade | Daltonismo, contraste, texto, legendas, reduzir movimento, tremor e flashes | Nunca depende apenas da cor |
| Social | Chat, convites, status, privacidade, bloquear, silenciar | Respeita preferências por sessão |
| Conta | Segurança, sessões, idioma, termos, exportação/exclusão | Operações sensíveis pedem reautenticação |

# 5. Client social, home, amigos e lobby

## 5.1 Home

Componentes: topo com perfil e status; menu Jogar, Campeões, Coleção, Histórico, Treinamento, Notícias, Configurações; painel de missões não competitivas; amigos e convites; versão/servidor; notificações. Uma camada opaca mostra estado de carregamento, sem impedir interação em áreas que já funcionam. Em ausência de rede, somente entradas offline são habilitadas.

## 5.2 Amizades e presença

Pesquisar pelo nome público; solicitar, aceitar e remover amizade; ignorar; bloquear; mostrar estados `online`, `em lobby`, `em seleção`, `em partida`, `ocupado`, `offline`; configurar privacidade. Nenhum status privado visível ao bloqueado. Convites têm expiração e confirmação para não inserir jogador automaticamente em grupo.

## 5.3 Lobby de equipe

Criar lobby; nome e modo; limite de cinco; líder de grupo; convidar por lista; definir preferências de função principal/secundária; confirmar pronto; trocar líder; sair; expulsar com confirmação; fila só começa quando composição válida. Membros ficam visíveis com status de rede. Se líder desconectar, liderança passa por regra determinística. A partida personalizada aceita dez vagas humanas ou bots conforme autorização do modo.

## 5.4 Chat, pings e conduta

Chat de amigos e equipe; chat geral da partida opcional desabilitado por padrão em beta; antispam, mute, block, denúncias, regras de retenção e acesso administrativo auditável. Pings de perigo, ajuda, a caminho, objetivo, visão inimiga, tempo de recarga e posicionamento; limitação de spam. Comunicação não pode transmitir localizações que a própria equipe não conhece legitimamente. Voz poderá usar SDK de serviço gerenciado posteriormente, com controles de consentimento e moderação.

# 6. Fila, matchmaking, aceite e seleção de personagens

## 6.1 Matchmaking

Fila por região, modo, tamanho do grupo, estimativa de habilidade e preferência de função. `QueueTicket` inclui `party_id`, `mode`, `region`, `members`, `roles`, `client_build`, `rating_bucket`, `created_at`. Matchmaker não aceita sessões não autenticadas, clientes desatualizados ou jogadores ainda em outra partida. Critérios alargam janela gradualmente para reduzir tempo, com teto configurável; não misturar regiões de alta latência sem aceite explícito. Proteger contra entrada múltipla e corrida de cancelamento.

## 6.2 Aceite

Modal de 10–15s [P], som não invasivo, botão Aceitar/Recusar, contagem dos 10 estados (sem identidade adversária). Só criar sessão definitiva após todos os aceites; se alguém recusar, recolocar elegíveis na fila sem perder prioridade excessivamente. Desconexão durante aceite segue timeout; punição graduada após padrão de abuso, não após falha isolada diagnosticada.

## 6.3 Draft e identidade de variantes

**Nome de seleção:** saga como parte da identidade (`Goku — Criança`, `Goku — Saiyajins`, `Goku — Super`). Cada variante tem ID único e balanceamento próprio, ainda que compartilhe nome/personagem narrativo. Seleção com filtro por função, dificuldade, alcance e saga. Exibir retrato **da forma inicial da variante**, além de prévia sequencial das formas desbloqueadas ao subir de nível. Escolhas únicas por equipe por `variant_id`; mesma variante permitida no lado oposto em modo normal. Não introduzir 10 banimentos com apenas 15 variantes. Em ranqueado futuro, aumentar elenco ou limitar bans a número compatível.

## 6.4 Ordem e bloqueio

MVP: atribuir função, selecionar variante, escolher duas técnicas universais, Escola de Treinamento e visual cosmético. Ordenação de draft configurável; jogadores podem sinalizar intenção; botão Bloquear encerra edição. Servidor valida licença do cosmético, variante disponível, página de treinamento e técnicas permitidas. Se timer acabar, escolher padrão seguro dentre variantes disponíveis previamente marcadas, ou tratar como abandono conforme modo. Retomar a seleção após reconexão sem alterar escolha bloqueada.

## 6.5 Escolas de Treinamento (runas)

Cinco árvores temáticas: **Tartaruga** (precisão/dano contínuo), **Força Saiyajin** (explosão/abates), **Controle de Ki** (habilidades/recarga), **Resistência** (proteção/tanques), **Tecnologia Capsule** (utilidade/economia). Cada página possui 1 efeito principal, 3 efeitos menores e 2 vantagens secundárias de outra árvore. Exemplos detalhados na seção de itens. Presets são salvos na conta, versionados com patch, recebem migração automática quando perks forem removidos e são validados no servidor antes da partida.

## 6.6 Tela de carregamento

Cards próprios para dez jogadores (cinco de cada lado), time e função, retrato do personagem e forma inicial, splash original licenciado, indicador de conexão, dica dinâmica e pequena linha evolutiva de formas disponíveis. Não mostrar local da equipe adversária nem informação de rede privada. Para cada cliente: `assets_ready`, `scene_loaded`, `auth_ready`, `snapshot_received`, `input_enabled`. Falha isolada aciona política de tolerância, substituição por bot apenas em modos que permitem e possível anulação antes de começar.

# 7. Entrada na partida, câmera, HUD e controles

## 7.1 Introdução da partida

Servidor cria arena e estado inicial, spawns dos 10 participantes, temporizador pré-jogo e timers dos monstros. Cliente mostra câmera cinematográfica curta da base e duas equipes de modo não bloqueante (pular disponível), som de início, nome dos campeões, objetivo de destruir núcleo e minimapa. **Nenhuma câmera não interativa bloqueará movimentação depois que o relógio competitivo começar.** Jogadores ganham controle antes do primeiro spawn de tropas; animações narrativas e falas são puramente cosméticas.

## 7.2 Layout da HUD

- Centro inferior: retrato da forma atual, HP, Ki/Energia, nível, barra de XP, Q/W/E/R e respectivas recargas, D/F, C para treinamento, itens ativos 1–6, botão de retorno B.
- Inferior direita: minimapa com torres, aliados, estruturas, objetivos visíveis e pings; quadrado de câmera; ícones de buffs.
- Superior: placar dos times, tempo, kills, torres, objetivos, latência e FPS opcionais.
- Lateral: feed de eliminações, notificações de objetivo, informações de companheiros e status de fusão.
- Painel TAB: campeões, formas atuais conhecidas, itens públicos, ouro pessoal, pontuação, CS, K/D/A, tempos visíveis e estatísticas de equipe.
- Tooltips ao pressionar Shift: fórmulas de dano, alcance, custo, tipo de alvo, dano por rank, efeitos de transformação e status de controle.

## 7.3 Controles padronizados

| Entrada | Função padrão | Regra |
|---|---|---|
| Mouse direito | Mover/atacar alvo clicado | Pathfinding até posição válida |
| Mouse esquerdo | Selecionar, loja, interação UI | Sem comprar acidentalmente ao fechar modal |
| Q W E R | Habilidades do campeão | Indicador de alcance, smart cast configurável |
| D F | Técnicas universais | Persistem por partida |
| C (segurar) | Carregar Ki / treinar | Interrompe por movimento, ataque ou dano |
| B | Retornar à base | Canalização cancelável |
| 1–6 | Itens ativos | Ícones por slot, remapeáveis |
| A | Ataque-movimento | Respeita seleção de alvo |
| S | Parar | Cancela ordem de movimento |
| H | Manter posição | Não persegue alvos automaticamente |
| Espaço | Centralizar câmera | Opcional segurado/alternância |
| Tab | Placar | Mostra/oculta tabela |
| G / Alt+clique | Roda de pings | Limite contra spam |
| Enter | Abrir chat | Bloqueia comandos do jogo enquanto digita |
| Esc | Opções / fechar | Diálogo confirmar em ações destrutivas |

## 7.4 Câmera

Isométrica fixa com zoom dentro de limites definidos; pan nas bordas opcional; follow por Espaço; pan pelo minimapa permitido dentro das restrições de visão; sem inclinação livre que exponha detalhes ocultos. câmera acompanha fusão por ambos os clientes, com política de shake/redução de movimento individual. Efeitos de voo não elevam o personagem tanto a ponto de esconder sinais no chão.

# 8. Mundo, navegação, rotas e estruturas

## 8.1 Geometria lógica

O mundo é renderizado em 3D, mas o combate é resolvido em um plano de navegação 2D/2.5D, com elevação cenográfica controlada. Arena simétrica competitivamente, três rotas `TOP`, `MID`, `BOT`, floresta entre elas, curso de água central e dois lados espelhados. Dimensões-alvo [P]: 300×300 unidades lógicas (1 unidade aproximada ≈ 1 metro para cálculos internos, sem equivalência rígida); caminho base-base ~180–260 u por rota; nível de câmera permite 18–24 u visíveis no raio típico do herói. Esses números só serão congelados com testes de tempo de rotação, de abates e duração de partidas.

Camadas de colisão: `Ground`, `Walls`, `Brush`, `Units`, `Projectiles`, `Structures`, `Objectives`, `VFXOnly`, `UIRaycast`. Voo visual não ignora paredes; atravessar obstáculos exige habilidade explicitamente marcada `can_cross_terrain`. Pathfinding no servidor calcula caminho, cliente faz previsão visual, corrige divergências suavemente, salvo teleporte legítimo.

## 8.2 Rotas e pontos de interesse

- Top — **Montanhas Rochosas**: rota solo, maior isolamento, acesso a objetivo superior, zonas de flanco.
- Mid — **Arena do Torneio**: rota mais curta, prioridade para rotação, cruzamento com quatro acessos à selva.
- Bot — **Planícies Capsule**: rota dupla, cobertura vegetal e acesso ao objetivo inferior.
- Rio/vale — **Passagem Central**: duas entradas principais por lado, passagem de visão e patrulha de criatura neutra.
- Selvas — pares equivalentes de acampamentos, acesso a buffs e rotas de gank; pontos de parede/vegetação projetados para múltiplas formas de aproximação.
- Bases — fonte, loja, núcleo, duas torres finais, três geradores ligados às rotas, entradas defendidas.

**Norma de mapa:** o primeiro minuto nunca deve permitir assassinatos inevitáveis por salto através de paredes. Testes devem analisar redirecionamento de projéteis, colisão de personagens gigantes, observabilidade dos arbustos e distância para ganks. Todas as posições críticas serão IDs num catálogo de mapa, não coordenadas espalhadas pelo código.

## 8.3 Torres de Defesa de Energia

Três torres por rota [P] (externa, interna e de base), mais duas de núcleo, total simétrico. Vida, armadura, dano, alcance e sequência de mira configuráveis. Adquire alvos por prioridades: adversário elegível dentro do raio → tropas atacantes primeiro; se campeão adversário danificar campeão aliado em área protegida, reavalia agressão; exceções deliberadas documentadas. Torre apresenta aviso de alvo e crescente intensidade de raios. Anti-invasão inicial, placas de blindagem e recompensa compartilhada configuráveis. Ataques em torres devem ser telegráficos com intervalo legível; habilidades podem conceder imunidade apenas se explicitamente designadas.

Modelo ilustrativo [P]: alcance 8 u; disparo inicial 130 de dano bruto, intervalo 1,0 s, aumento escalonado enquanto foca o mesmo campeão até teto. **Todos os parâmetros residem em `structures_balance.json`**, nunca em prefab disperso. Cancelar dano por morte da torre obedece estágio do projétil: projétil já disparado pode acertar conforme regra formal da versão.

## 8.4 Geradores, Núcleo e fonte

**Gerador de Defesa (inibidor):** somente vulnerável após torre da base cair; quando destruído, tropas inimigas da rota recebem melhoria temporária; regenera após tempo [P] 300 s. **Núcleo de Energia:** vulnerável somente com condições estruturais satisfeitas; destruição encerra a simulação de forma autoritativa. **Fonte:** regenera vida e Ki, compra itens, protege spawn; proteção não deve ser explorável para ataque externo. Mundo persiste durante um breve período de conclusão para animação, mas dano e compras param no estado `MatchEnding`.

## 8.5 Tropas e ondas

Ficção: Robôs de Treinamento Capsule. Tipos: `Melee` (corpo a corpo), `Ranged` (disparo de Ki), `Siege` (artilharia a cada n ondas), `Super` (gerador inimigo destruído). Spawns conjuntos por rota a cada [P] 30 s; 3 melee + 3 ranged por onda como base; adição de cerco em cadência configurável. Last hit dá Zeni; experiência é concedida por proximidade quando tropa morre, mesmo sem last hit, sujeito a raio. Unidades usam regras de agro para ataques do adversário, priorização e reset do alvo. O caminho é composto por waypoints, com correções de colisão e prevenção de deadlock. Contagem de tropas e última batida podem ser reproduzidas em testes determinísticos de servidor.

## 8.6 Macroestratégia obrigatória

Manter gestão de ondas (freeze, slow push, fast push), pressão de rota, trocas, gank, contra-gank, dive, controle de visão, objetivos coletivos, flanco e split push. O design não define essas estratégias por comandos específicos; o comportamento de torres, ouro, tempos e rotas precisa permiti-las emergir. Equipes devem ter caminhos defensivos reais mesmo atrás em ouro, sem mecanismo artificial que premie intencionalmente perder.

# 9. Selva, criaturas, objetivos e mudanças do mapa

## 9.1 Acampamentos comuns

1. **Saibamens (pequenos):** grupo de criaturas corpo a corpo, baixo HP, respawn rápido, ouro/XP.
2. **Robô Guardião Capsule:** alvo único resistente, alcance médio, ouro/XP.
3. **Criatura de Namekusei:** HP intermediário, dano em área leve, ouro/XP.
4. **Guardião Carmesim:** buff temporário de ataques e perseguição, contagem de duração.
5. **Guardião Espiritual:** buff de recuperação de Ki e aceleração de habilidades.
6. **Patrulheiro do Rio:** cria área de visão e deslocamento neutro; posição/tempo de respawn previsíveis.

Cada acampamento configura `camp_id`, posição, tabela de loot, XP, vida, dano, armadura, aggroRadius, leashRadius, respawnDelay, tag, animações e estados (`Idle`, `Engaged`, `Resetting`, `Dead`, `Respawning`). A IA do monstro não pode perseguir indefinidamente nem permitir que um jogador puxe duas áreas fora de regra. O servidor registra último golpe e participação em objetivo para distribuir recompensas.

## 9.2 Objetivos maiores e correspondências

| Objetivo estratégico | Criatura do jogo | Comportamento do encontro | Recompensa / consequência |
|---|---|---|---|
| Pressão inicial sobre torres | Larvas Mutantes | Grupos múltiplos na caverna | Acúmulo de bônus de cerco |
| Invocação de aríete | Dinossauro Colossal | Ataques frontais, área de recuo | Item de invocação para investir contra torres |
| Fortalecimento periódico | Guardiões Elementais | Cinco afinidades rotativas | Acúmulos permanentes por equipe |
| Bônus de quatro objetivos | Poder Elemental Supremo | Não é monstro separado | Bônus único associado ao elemento predominante |
| Objetivo superior final | Dragão Primordial | Criatura voadora original, grande arena | Amplificação temporária de tropas e cerco |
| Execução de fim de jogo | Guardião Ancestral | Área de perigo elevada | Efeito temporal de execução em vida muito baixa |

Nenhuma dessas criaturas corresponde a Cell, Freeza ou outro campeão selecionável. **Shenlong/Porunga** aparecem opcionalmente como efeito narrativo de objetivos completados; não são feridos nem precisam ser derrotados. Um sistema de sete Esferas do Dragão como condição extra para desejo é **futuro**, não obrigatório no lançamento, para evitar objetivos concorrentes excessivos.

## 9.3 Ciclo temporal de objetivos

Tabelas de spawn são armazenadas por modo. Exemplo provisório, a validar por playtest: buffs comuns 01:30, Patrulheiro do Rio 03:30, Larvas 05:00, Guardião Elemental 05:00, Dinossauro 08:00, Dragão Primordial 20:00, Ancestral 30:00. O evento final usa temporizadores de respawn definidos por dado e exclusividade de ocupação das arenas. Não copiar valores de patches do LoL cegamente: o mapa, os heróis e o tempo de deslocamento serão diferentes.

## 9.4 Variação elemental do terreno

Após certos objetivos, o mapa adota afinidade `Fire`, `Wind`, `Earth`, `Water` ou `Lightning`. Mudanças possíveis: vegetação/obstáculos temporários seguros, corredores de velocidade, visão de áreas especiais, regen em pontos de água, ou tempestades visuais sem dano arbitrário. O impacto estratégico deve ser simétrico e legível no minimapa. **Parâmetros visuais de mudança** separados de **modificações de colisão** para impedir desync e vantagem por qualidade gráfica.

# 10. Visão, informação e navegação

## 10.1 Regras de visão

Toda equipe tem `TeamVisionGrid`, derivada de personagens vivos, tropas, estruturas, wards e habilidades. O servidor decide se uma entidade inimiga pode ser revelada. Compartilhamento de visão apenas para a equipe; bushes ocultam personagens salvo condições. Tipos: visão normal, revelação, visão verdadeira, camuflagem e invisibilidade; efeitos de detecção têm raio e duração. O cliente **não recebe estado detalhado de unidades não visíveis**; isto é requisito de segurança e anti-cheat, não apenas máscara gráfica.

## 10.2 Dispositivos e plantas

| Equivalente | Nome temático | Interação | Regras de segurança |
|---|---|---|---|
| Ward de visão | Radar Capsule | Colocar num ponto válido, duração finita | Não revela fora do raio ou através de regras opacas |
| Ward detectora | Radar de Controle | Visão + detecta sensores próximos | Limite por jogador/equipe |
| Detector móvel | Rastreador de Ki | Varredura em torno do herói | Tempo de recarga e aviso |
| Flor reveladora | Flor-Sensor | Ativação em cone para mapear | Visão concedida conforme cálculo do servidor |
| Planta de impulso | Cápsula de Impulso | Empurra entidades elegíveis | Física discreta e segura |
| Planta de cura | Fruta de Namekusei | Pequenas cargas de recuperação | Não ultrapassa máximo de HP/Ki |
| Ponto amplificador | Farol de Radar | Aumenta cobertura de ward próxima | Temporizador e lado informado |

## 10.3 Sinalização de informação

Ping de tempo restante de objetivo só exibe dado público ou visto; inimigo desaparecido é marcador manual; a tabela mostra itens inimigos apenas quando foram observados de forma legítima, se essa for a política definida. Replays de observadores recebem filtros de visibilidade por equipe e atraso quando necessário.

# 11. Simulação de combate: regras e fórmulas

## 11.1 Autoridade e relógio de simulação

Servidor executa sequência fixa de etapas em `tick` configurável [P] 30 Hz para o primeiro vertical slice; 60 Hz para teste de exigência competitiva. Ordem conceitual: `ReceiveInput → Validate → UpdateTimers → Move → AcquireTargets → ResolveCasts → Projectiles → Hits → Damage/Shield/Heal → Deaths → Experience/Gold → Objectives → Vision → Replicate`. Tempo de animação visual não altera janela autoritativa de acerto. Todos os eventos possuem `tick_id`, `entity_id`, `ability_id`, `source_id`, `target_id`, seeds versionadas quando há aleatoriedade e dados suficientes para replay/sanity check.

## 11.2 Atributos e terminologia

`HP/HPMax`, `Ki/KiMax`, `AttackPower (APhysical)`, `KiPower`, `Armor`, `KiResist`, `AttackSpeed`, `CritChance`, `MoveSpeed`, `AbilityHaste`, `ArmorPen`, `KiPen`, `Tenacity`, `Lifesteal`, `Omnivamp`, `ShieldPower`, `HealPower`, `Range`, `VisionRange`. Evitar sigla AP ambígua: na UI usar **Ataque** e **Poder de Ki**, em código `physicalAttack` e `kiPower`.

## 11.3 Modelos matemáticos iniciais

- **Crescimento linear:** `stat(level) = base + perLevel × (level − 1)`; curvas não lineares são possíveis por tabela, mas devem ser justificadas.
- **Recarga com aceleração:** `effectiveCooldown = baseCooldown × 100/(100 + haste)`; exceções explícitas para cargas/ultimates.
- **Mitigação para resistência ≥ 0:** `damageFinal = rawDamage × 100/(100 + effectiveResistance)`; resistências negativas seguem função separada documentada (não reutilizar sem teste).
- **Crítico:** evento de ataque básico com multiplicador inicial 1,75 [P]; efeitos específicos definidos por habilidade.
- **Velocidade:** acúmulos aditivos/multiplicativos ordenados, limites de aceleração, suavização de extremos, desacelerações e fatores de slow.
- **Cura e escudo:** clamp em limites, redução por anti-cura e interação com amplificadores, sem números negativos ou overflow.
- **Experiência:** tabela `xp_to_next_level[level]`, recompensa por tropa/monstro/herói, divisão por número de aliados elegíveis e modificador de treinamento de Ki.

As fórmulas serão implementadas em biblioteca de domínio com testes unitários de fronteira: dano zero, armadura negativa, cura acima do máximo, crítico garantido, slow concorrente, colisão com alvo morto e operações fora de ordem.

## 11.4 Habilidades e ciclo de execução

Toda habilidade tem `CastRequest`, validação (vivo, Ki, cooldown, silenciado, alvo, alcance, mapa, forma, status), `Windup`, `Fire`, `Projectile/Hit`, `Recovery`, `Cooldown`, eventos de VFX/SFX e cancelamento. Tipos: instantânea, projétil, canalizada, movimento, área persistente, aliado, passiva, transformação e ultimate. Previsão local exibe mira de modo imediato, mas a resolução é confirmada pelo servidor. Imunidades, interrupções e prioridade de controle não podem depender da ordem de chegada arbitrária de mensagens de rede.

## 11.5 Controles de grupo

Conjuntos: `stun`, `root`, `slow`, `silence`, `knockup`, `knockback`, `taunt`, `suppress`, `fear`, `charm`, `disarm`, `blind`, `untargetable`, `invulnerable`. Cada um tem interação explícita com ações de mover, atacar, lançar, carregar C, utilizar itens, retorno B e fusão. Tenacidade não reduz controles classificados como irredutíveis quando definidos; cada exceção consta de `crowd_control_rules.yaml`. Animação de reação não prolonga controle além da duração autoritativa.

## 11.6 Morte, abate, respawn e recompensa

Ao HP ≤ 0: valida prioridade de eventos, dispara `DeathEvent`, interrompe canalizações, remove buffs temporários elegíveis, mantém transformações de nível, desabilita input, mostra câmera/spectate de aliados conforme visão, calcula ouro/XP por kill/assist. `deathTimer = base(mode, level, matchTime)` em tabela, com teto testado. Reconexão durante morte preserva timer. Feed de abates mostra versão do campeão, mas não revela inimigos escondidos fora do conhecimento da equipe antes do evento relevante.

# 12. Ki, treinamento, níveis e economia

## 12.1 Máquina de estados do carregamento

`Idle → ChargingKi → (KiFull) → TrainingXP`; de qualquer estado, `DamageTaken/Move/Attack/Cast/HardCC/Death → Interrupted → Idle`. Início exige segurar `C` por mínimo [P] 0,15 s contra acionamento acidental. Ki recuperado por segundo [P] `15 + regenBonus`, sujeito a modificação por personagem e decréscimo de mobilidade zero. Enquanto treina XP, partículas distintas e indicador amarelo; ruído/áudio pode informar inimigos próximos a um raio equilibrado. Não há proteção durante treinamento, e carregamento é cancelado antes de aplicar dano recebido para impedir exploração de frame.

## 12.2 Experiência por treinamento

Quando Ki cheio, `trainingXpPerSecond = min(baseAtLevel × modifiers, capForLevelRemaining / timeWindow)`; inicialmente [P] gerar 20–30% da taxa efetiva de ganho de XP de farm adequado, sob condições comparáveis. **Não usar teto absoluto de 15% por nível** — proposta anterior incompatível com desejo de poder evoluir somente treinando. Antiexploit: treino na fonte não gera XP, alternância de C não reseta penalidades, AFK é detectado por participação real no mapa, e há retorno decrescente por janelas longas (ex.: após 30s contínuos). Nos níveis finais, é possível ganhar XP treinando, mas muito ineficiente; o ritmo real será calibrado com dados de partida.

## 12.3 Escolha estratégica e informação de risco

UI informa Ki atual, tempo estimado para carga completa, XP/s esperado, alerta de vulnerabilidade e cancelamento. O jogador pode escolher treinar em arbusto, mas o risco de perder ouro/pressão e o som/efeito local continuam. Itens aceleram **Ki**; itens que aceleram **experiência de treinamento** são inicialmente proibidos para evitar estratégia dominante. Considerar itens de treinamento com penalidade temporária (botas/pesos) como economia de mobilidade por recompensa tardia, sem acúmulo infinito.

## 12.4 Energia artificial e personagens especiais

Android 18 utiliza recurso `ArtificialEnergy` com barra e lógica visual específica: reserva estável e regeneração passiva, não ganha XP ao segurar C da mesma forma; seu C realiza `Treinamento de Rotina` parado, com taxa equivalente porém sem aura de Ki e com eventual recarga das cargas especiais. Cell pode receber energia de absorção por habilidade, nunca roubar níveis permanentes de alvo; Buu pode recuperar HP, mas não aumentar HP máximo permanentemente sem teto.

## 12.5 Ouro/Zeni e loja

Renda: passiva, último golpe, eliminações, assistências, torres, criaturas, objetivos e missões. `GoldLedger` autoritativo grava `income_event` e `purchase_event`; operações idempotentes, saldos nunca negativos. Compras normalmente permitidas na fonte, com exceções explícitas de personagem/item. Reembolso de item recém-comprado, venda parcial e desfazer compras obedecem política única (p.ex., desfazer antes de sair da fonte e se efeito não consumido), calculada pelo servidor. Cliente sempre mostra preço, componentes faltantes, custo para combinar, estoque e item indisponível quando não cabe no inventário.

# 13. Sistema de transformações, animações e visual

## 13.1 Estrutura do personagem

Entidade autoritativa mantém `championVariantId`, `formId`, `formStartTick`, `level`, `transformUnlocks`, `appearanceId`, `rigProfile`, `formModifiers`, `abilityOverrides`. Transformação é uma mudança de estado, não um personagem novo, exceto fusão. O número de transformações por campeão é variável; personagens sem transformações têm `MasteryMilestones` com bônus de técnica equivalentes.

## 13.2 Pipeline de assets por forma

Pasta por `variant/form`: FBX/GLB fonte, malha final, materiais/texturas PBR estilizadas, rig humanoide/personalizado, avatar de animação, clip base, overlays faciais, VFX graph/particle prefabs, SFX, retrato 2D, ícone minimapa, ícone HUD e metadados. Preferir compartilhar esqueleto e animações entre formas compatíveis; Oozaru e fusão gigante possuem rig próprio. A diferença entre forma base e forma transformada deve ser visível pela silhueta, paleta, cabelo, postura e leitura do HUD, não apenas pela aura.

## 13.3 Eventos de transformação

1. Server detecta `levelThresholdReached` ou habilidade de forma ativável.
2. Valida condições, configura `nextFormId` e emite `FormTransitionStarted` com tick e duração.
3. Cliente pré-carrega variante visual; toca blend de postura, aura e SFX com opção de reduzir efeitos.
4. No tick autoritativo, aplica modificações de forma; troca materiais/malha/rig conforme perfil; preserva posição, HP proporcional ou absoluto conforme regra por campeão, buffs, alvo e orientação.
5. Emite `FormTransitionCommitted`; HUD, minimapa, retrato, skills e indicadores atualizam ao mesmo tempo; log de replay recebe ambos os eventos.
6. Se asset falhar, usar modelo de fallback visível e seguro, nunca bloquear a simulação; registrar erro de conteúdo.

## 13.4 Regras de animação e cancelamento

Clips-padrão: idle terrestre, idle voo, walk/run, dash, salto, ataque básico A/B, dano leve/pesado, knockup, knockback, stun, C charging, transformação por estágio, QWER, recall, morte, respawn, vitória, derrota, emote e fusão. Para cada clip: duração, janela de hit, possibilidade de cancelamento, root motion permitido?, camadas (upper body/full body), blending, evento de áudio e fallback. Root motion **não controla sozinho** o deslocamento autoritativo; animador segue resultados de simulação. Transformações automáticas não devem tornar o jogador inalvejável ou cancelar decisões sem justificativa.

## 13.5 VFX/SFX e legibilidade

Combate 5v5 com formas de aura grandes exige orçamento de partículas e limites de transparências, além de prioridade de informação: círculo de habilidade hostil > alvo > projétil > forma > partículas decorativas. Habilidades perigosas precisam de telégrafos próprios (borda, formato, som), acessíveis sem diferenciar apenas pela cor. Reduzir flashes fortes, tremor e intensidade de aura em modos de acessibilidade. Vozes e trilhas oficiais não serão extraídas de mídias protegidas; efeitos temporários devem ter origem permitida.

## 13.6 Catálogo de formas e identidade visual

As 15 variantes requerem uma matriz de assets: cada forma tem `form_id`, `unlock_level`, `model_asset`, `portrait_asset`, `minimap_icon_asset`, `animation_profile`, `vfx_profile`, `sfx_profile`, `palette`, `silhouette_id`. Teste automatizado falha caso uma forma referencie arquivos inexistentes. Para skins cosméticas alternativas, cosmético e transformação são **camadas ortogonais**: um pacote de skin fornece variantes visuais para todas as formas daquela variante jogável, ou herda com fallback explicitado. Nunca aplicar skin de uma saga a outra sem suporte.

# 14. Elenco inicial: 15 fichas jogáveis

## 14.1 Convenções de ficha

Todos possuem `P` passiva, `Q/W/E` básicas e `R` suprema, além de D/F universais e C treinamento. Habilidades são adquiridas/melhoradas com pontos de nível; R em níveis 6/11/16 [P], exceto quando a forma exige troca específica. A tabela `balance/champions/{id}.yaml` define base, crescimento, fórmulas, recarga, custos, atributos por forma, alvos, tags e flags de controle. **Custos e recargas abaixo são sementes para protótipo**, não valores aprovados para ranqueadas. Até cada forma estar pronta, é permitido usar placeholder visível com `FormUnavailableInPrototype` apenas nos modos internos.

### 14.1.1 Valores-base propostos

| ID | Variante | Classe | HP | Ki/energia | Velocidade | Alcance básico | Dificuldade |
|---|---|---|---:|---:|---:|---:|---|
| GKU_KID | Goku Criança | Escaramuça/Selva | 660 | 100 | 345 | 1,6 u | Média |
| GKU_RADITZ | Goku Z — Raditz | Tanque/Suporte | 700 | 110 | 325 | 1,6 u | Alta |
| GKU_SAIYAN | Goku Z — Saiyajins | Lutador/Selva | 650 | 120 | 340 | 1,7 u | Alta |
| GKU_FRIEZA | Goku Z — Freeza | Mago de alcance | 600 | 150 | 330 | 5,5 u | Média |
| GKU_BUU | Goku Z — Buu | Duelista | 645 | 135 | 340 | 1,7 u | Alta |
| GKU_SUPER | Goku Super | Assassino/Lutador | 620 | 150 | 355 | 1,7 u | Muito alta |
| VGT_SAIYAN | Vegeta Z — Saiyajins | Lutador/Tanque | 680 | 110 | 330 | 1,7 u | Média |
| VGT_SUPER | Vegeta Super | Mago/Lutador | 625 | 160 | 335 | 4,5 u | Alta |
| GOH_CELL | Gohan Jovem — Cell | Lutador/Carry | 610 | 130 | 335 | 3,5 u | Média |
| PIC_HERO | Piccolo — Super Hero | Tanque/Suporte | 720 | 125 | 325 | 2,5 u | Média |
| FRZ_Z | Freeza Z | Mago de controle | 590 | 170 | 325 | 5,5 u | Alta |
| CELL_Z | Cell Z | Lutador/Selva | 670 | 135 | 330 | 2,0 u | Média |
| TRK_SUPER | Trunks do Futuro — Super | Assassino/Lutador | 630 | 120 | 350 | 1,8 u | Alta |
| A18 | Android 18 | Atiradora | 600 | 100 energia | 340 | 5,6 u | Média |
| BUU_FAT | Majin Buu | Tanque/Suporte | 745 | 115 | 320 | 2,0 u | Média |

Por nível, `HP +75–105`, `Ki +0–10`, ataque e resistência específicos em tabelas. Alcance em unidades lógicas. Personagens terrestres/voadores diferem em animação e habilidades, não em imunidade básica. **Seis Gokus e dois Vegetas são intencionais**: o conjunto inicial privilegia variantes de sagas; expansões deverão melhorar diversidade de estilos e universos.

## 14.2 Goku Criança (`GKU_KID`)

**Fase:** Dragon Ball clássico. **Função:** selva/top; terrestre, curta distância; vence por improviso e mobilidade progressiva. **P — Treino do Mestre Kame:** cada nível de habilidade adquirida concede pequeno bônus de velocidade de ataque; efeito não acumula além do teto. **Q — Bastão Mágico:** estica bastão em linha, dano físico e alcance maior; custo 20 Ki, CD 7 s. **W — Salto Ágil:** salto curto que atravessa pequenas unidades, não paredes altas; custo 30, CD 13 s. **E — Kamehameha Inicial:** projétil canalizado curto, dano energético; custo 45, CD 11 s. **R — Poder do Grande Macaco:** transformação temporária Oozaru desbloqueada no nível 16; antes disso R executa `Sequência do Torneio`, combos de controle moderado; custo 90, CD 110 s. **Marco nível 11:** Nuvem Voadora muda animação de deslocamento, dá habilidade curta de passagem por obstáculos permitidos mediante recarga; não é voo livre. **Fraquezas:** pouca pressão à distância e vulnerabilidade ao controle. **Counters previstos:** Freeza Z e Goku Freeza controlam espaço; bom contra artilharia imóvel se alcançar. **Assets mínimos:** base, Nuvem Voadora e Oozaru gigante (rig próprio).

## 14.3 Goku Z — Raditz (`GKU_RADITZ`)

**Fase:** confronto com Raditz, antes do primeiro treino de Kaiô. **Função:** suporte/tanque de contenção; terrestre. **P — Espírito de Sacrifício:** ao proteger aliado com habilidade, ganha resistência temporária, com tempo interno. **Q — Golpe de Contenção:** soco em linha que reduz velocidade; custo 25, CD 8 s. **W — Proteção do Irmão:** desloca-se a um aliado próximo e concede escudo, sem invulnerabilidade; custo 45, CD 15 s. **E — Imobilização Heroica:** agarrão curto canalizado que prende alvo e o próprio Goku; custo 55, CD 18 s, pode ser interrompido. **R — Sacrifício Decisivo:** grande contenção de um alvo; defesa de aliados e risco alto, com dano ao próprio HP limitado e informado; custo 85, CD 130 s. **Marcos 6/11/16:** aumento de alcance defensivo, duração de barreira e segurança da contenção, sem SSJ. **Fraquezas:** baixa mobilidade, alto comprometimento. **Counters:** alvos muito móveis e interrupções; favorece equipe contra assassinos que mergulham no carry.

## 14.4 Goku Z — Saiyajins (`GKU_SAIYAN`)

**Fase:** treino de Kaiô, combate contra Nappa/Vegeta. **Função:** lutador/selva. **P — Espírito de Combate:** dano de habilidade bem-sucedida acumula pequenas cargas temporárias para fortalecer ataque seguinte, limitado. **Q — Kamehameha:** feixe de Ki de alcance médio, custo 45, CD 9 s. **W — Passo do Kaiô:** avanço curto, custo 30, CD 12 s. **E — Golpes Acelerados:** sequência corpo a corpo com reset parcial de básico, custo 35, CD 10 s. **R — Kaioken:** modo de amplificação temporária com níveis ×2/×3/×4 liberados nos níveis 6/11/16, aumenta dano/mobilidade mas drena Ki/s e parte do HP; custo inicial 30, CD 100 s após acabar. **Fraquezas:** precisa administrar HP e Ki; pode ser contido durante janela agressiva. **Counters:** anti-sustain e tanques capazes de sobreviver ao burst.

## 14.5 Goku Z — Freeza (`GKU_FRIEZA`)

**Fase:** Namekusei. **Função:** mago/atirador de Ki. **P — Determinação de Namekusei:** acertos consecutivos à distância marcam alvo, pequeno dano adicional no terceiro acerto. **Q — Kamehameha de Namekusei:** rajada em linha, custo 45, CD 8 s. **W — Defesa de Ki:** escudo curto com custo 55, CD 17 s. **E — Movimento Instantâneo:** reposicionamento curto, não atravessa paredes proibidas; custo 50, CD 18 s. **R — Super Kamehameha:** canalização com marca no chão e dano alto; custo 100, CD 125 s. **Forma:** SSJ1 automática no nível 6, aprimora efeitos e adiciona bônus moderado de poder/alcance, com crescimento técnico nos níveis 11/16. **Fraquezas:** vulnerável a saltos e interrupções antes de completar cast. **Counters:** Trunks e Goku Super podem aproveitar janela de canalização.

## 14.6 Goku Z — Buu (`GKU_BUU`)

**Fase:** final de Dragon Ball Z. **Função:** duelista/controle de espaço. **P — Técnica Aperfeiçoada:** alternar ataques básicos e habilidades reduz levemente tempo restante de deslocamento, com teto. **Q — Combinação Saiyajin:** três golpes em sequência, custo 35, CD 7 s. **W — Transmissão Instantânea:** teleporte curto para alvo/posição válida com aviso; custo 55, CD 18 s. **E — Kamehameha Direcionado:** feixe de habilidade, custo 50, CD 11 s. **R — Genki Dama:** preparação exposta e projétil pesado, dano escalável com Ki investido até limite; custo 100, CD 150 s. **Formas:** SSJ1 nível 6, SSJ2 nível 11, SSJ3 nível 16; SSJ3 aumenta custo de técnicas e sustenta dano em troca de Ki. **Fraquezas:** exposição durante R e alto gasto de Ki. **Counters:** Piccolo/Freeza com interrupções.

## 14.7 Goku Super (`GKU_SUPER`)

**Fase:** Dragon Ball Super. **Função:** assassino/lutador de alta dificuldade. **P — Instinto de Batalha:** movimentação correta após evitar ataque concede carga de eficiência ao próximo básico, CD interno. **Q — Golpe Veloz:** avanço-ataque de curto alcance, custo 35, CD 8 s. **W — Barreira Divina:** defesa breve e janela de contra-ataque, custo 55, CD 18 s. **E — Transmissão de Combate:** teleporte curto com alvo visível, custo 60, CD 20 s. **R — Kamehameha Evolutivo:** feixe cuja VFX/fórmula muda por forma, custo 100, CD 120 s. **Formas:** nível 6 God (aura vermelha e Ki eficiente); 10 Blue (poder estável); 13 Blue+Kaioken (modo temporário de risco); 16 Instinto Superior Incompleto (esquiva condicional, sem esquiva automática perfeita); 18 Instinto Superior Completo (esquiva limitada por cargas/tempo interno). **Fraquezas:** HP inicial baixo, defesa exige timing, alto Ki. **Counters:** controles de área e dano persistente; não pode esquivar automaticamente de tudo.

## 14.8 Vegeta Z — Saiyajins (`VGT_SAIYAN`)

**Fase:** invasão Saiyajin. **Função:** lutador resistente/top. **P — Orgulho Saiyajin:** ganhar resistência breve quando atingido por múltiplos adversários, com recarga. **Q — Galick Gun:** raio carregável, custo 45, CD 11 s. **W — Golpe de Elite:** investida que empurra alvo, custo 40, CD 13 s. **E — Rajadas Impiedosas:** cone de Ki, custo 55, CD 14 s. **R — Lua Artificial/Oozaru:** cria evento de lua e transformação temporária em nível 16; níveis de R menores usam explosão/ameaça de área; custo 100, CD 145 s. **Fraquezas:** lentidão relativa, grande hitbox em Oozaru. **Counters:** atiradores móveis e controles de área.

## 14.9 Vegeta Super (`VGT_SUPER`)

**Fase:** Dragon Ball Super. **Função:** mago de explosão/lutador. **P — Orgulho Intacto:** dano por Ki sobre alvos marcados aumenta moderadamente após preparação. **Q — Big Bang Attack:** esfera explosiva, custo 55, CD 10 s. **W — Parede de Energia:** barreira direcional curta, custo 45, CD 17 s. **E — Investida do Príncipe:** avanço e dano físico moderado, custo 40, CD 15 s. **R — Final Flash:** canalização de feixe longo, com aviso claro, custo 110, CD 135 s. **Formas:** God nível 6, Blue nível 11, Blue Evolution nível 16; melhorias posteriores de maestria no nível 18. **Fraquezas:** alta dependência de acertar skillshots, cast interrompível. **Counters:** Goku Criança e Trunks quando entram pelo flanco.

## 14.10 Gohan Jovem — Cell (`GOH_CELL`)

**Fase:** jogos de Cell. **Função:** carry de escalamento. **P — Potencial Oculto:** ganha carga de `Determinação` por participar de combate/defender aliados, limitada, consumida em habilidade. **Q — Masenko:** rajada frontal, custo 45, CD 8 s. **W — Contra-Golpe Instintivo:** proteção rápida e acerto contra atacante próximo, custo 35, CD 15 s. **E — Passo de Batalha:** deslocamento curto e bônus de ataque seguinte, custo 40, CD 13 s. **R — Kamehameha Pai e Filho:** feixe direcionado de longa preparação, custo 110, CD 140 s. **Formas:** SSJ1 nível 6, SSJ2 nível 11; níveis 16–18 melhoram domínio e efeito de R, sem SSJ3. **Fraquezas:** fase inicial, posicionamento e controle. **Counters:** Freeza Z o pune cedo; protege-se com Piccolo.

## 14.11 Piccolo — Super Hero (`PIC_HERO`)

**Fase:** filme Super Hero. **Função:** tanque/suporte. **P — Regeneração Namekuseijin:** regeneração de HP condicionada a não receber dano por curto período. **Q — Braços Extensíveis:** agarrão direcionado, custo 40, CD 11 s. **W — Barreira Namekuseijin:** escudo aliado, custo 55, CD 17 s. **E — Impacto Tático:** onda em cone que empurra/lentifica, custo 50, CD 15 s. **R — Makankosappo:** disparo perfurante de preparação longa e alto dano, custo 100, CD 130 s. **Formas:** Potencial Liberado nível 11, Orange Piccolo nível 16; aumento de tamanho dentro de limite, não muda colisão sem compensação. **Fraquezas:** perseguição longa, dano contínuo comparativamente baixo. **Counters:** lutadores com mobilidade constante; forte contra ultimates canalizadas.

## 14.12 Freeza Z (`FRZ_Z`)

**Fase:** Namekusei, quatro estágios de forma. **Função:** mago de zona/controle. **P — Crueldade Calculada:** ataques e habilidades contra inimigos lentos causam dano adicional moderado, com cooldown interno. **Q — Death Beam:** perfuração linear, custo 45, CD 8 s. **W — Telecinese:** deslocamento limitado de alvo/objeto elegível, custo 60, CD 18 s. **E — Investida Aérea:** reposicionamento lateral e breve slow ao contato, custo 35, CD 14 s. **R — Death Ball:** esfera de Ki com grande área e tempo de aviso, custo 110, CD 140 s. **Formas:** forma inicial, segunda no nível 6, terceira no 11 e final no 16; cada forma tem silhueta diferente, rig e retrato próprios. **Fraquezas:** HP baixo, recargas altas de controle. **Counters:** Trunks e Goku Saiyajins se entrarem após W.

## 14.13 Cell Z (`CELL_Z`)

**Fase:** evolução de Cell. **Função:** lutador/selva escalável. **P — Absorção Bioenergética:** cura leve e carga de aprimoramento em eliminações de monstros/campeões, sem roubar estatísticas permanentes de vítimas; limite. **Q — Investida Perfeita:** golpe rápido com linha de efeito, custo 40, CD 9 s. **W — Barreira Genética:** escudo e resistência curta, custo 45, CD 16 s. **E — Kamehameha de Cell:** feixe médio, custo 55, CD 12 s. **R — Explosão de Energia:** expansão perigosa em área, custo 105, CD 130 s. **Formas:** Imperfeito inicial, Semiperfeito nível 11, Perfeito nível 16; desbloqueio por nível, **não depende de absorver personagens jogáveis**. **Fraquezas:** vulnerável a anti-cura, controle e compra de tempo até níveis altos. **Counters:** Piccolo e Freeza controlam sua aproximação.

## 14.14 Trunks do Futuro — Super (`TRK_SUPER`)

**Fase:** Trunks de Dragon Ball Super. **Função:** assassino/duelista. **P — Espadachim do Futuro:** após mover distância curta, próximo ataque recebe bônus limitado de alcance/dano. **Q — Corte de Espada:** golpe frontal com reset do básico, custo 30, CD 7 s. **W — Burning Attack:** projétil com marca de alvo, custo 45, CD 11 s. **E — Investida Rápida:** dash linear, custo 40, CD 14 s. **R — Sword of Hope:** golpe de energia vertical em área estreita, custo 105, CD 135 s. **Formas:** SSJ nível 6, Rage nível 16, melhoria de técnicas nível 11. **Fraquezas:** alto risco ao entrar, limitado contra tanques. **Counters:** Piccolo, Buu e Vegeta Saiyajins resistem; forte contra magos vulneráveis.

## 14.15 Android 18 (`A18`)

**Fase:** Androids/Z. **Função:** atiradora de dano sustentado. **Recurso:** Energia Artificial; HUD próprio. **P — Reator Infinito:** regeneração passiva de energia e maior estabilidade de ataques, mas cooldowns e limite de gasto mantidos. **Q — Rajada de Energia:** ataque reforçado à distância, energia 30, CD 6 s. **W — Barreira Artificial:** proteção breve que absorve ataques, energia 45, CD 18 s. **E — Reposicionamento Preciso:** deslocamento curto/slow em alvo, energia 35, CD 14 s. **R — Combo de Disparos:** sequência de ataques com direção controlada e oportunidade de esquiva do inimigo, energia 90, CD 115 s. **Marcos:** aprimoramentos técnicos nos níveis 6/11/16 (sem transformação artificial inventada). **Fraquezas:** perigosa quando presa por controle e sem W. **Counters:** Goku Super e Trunks quando alcançam; forte contra tanques sem gap close.

## 14.16 Majin Buu (`BUU_FAT`)

**Fase:** Majin Buu gordo. **Função:** tanque/suporte de controle. **P — Corpo Maleável:** regeneração elevada e pequena redução de dano após receber múltiplos golpes, com teto. **Q — Braços Elásticos:** alcance estendido para puxar/atacar, custo 40, CD 10 s. **W — Massa Protetora:** escudo em si/aliado e cura moderada, custo 50, CD 16 s. **E — Transformação em Doce:** CC de curtíssima duração sobre alvo elegível, custo 65, CD 22 s, sujeito a imunidades. **R — Aprisionamento de Massa:** controle de área telegráfico, custo 100, CD 135 s. **Marcos:** níveis 6/11/16 melhoram alcance, regeneração e zona; absorção de campeões como transformação permanente **não** incluída no MVP. **Fraquezas:** movimentação lenta, anti-cura e danos percentuais. **Counters:** Android 18 e Goku Freeza podem destruí-lo a distância se não forem alcançados.

## 14.17 Matriz inicial de sinergias e antipadrões

| Par | Resultado esperado | Contrajogo |
|---|---|---|
| Piccolo + Gohan Jovem | Proteção do carry em escalamento | Pressão precoce e negação de farm |
| Goku Super + Vegeta Super | Potencial de fusão na fase final | Janela antes do nível 18; CC durante ativação |
| Freeza Z + Android 18 | Zona e dano contínuo à distância | Flancos e ataques coordenados de aproximação |
| Cell + Majin Buu | Sustentação e pressão prolongada | Anti-cura, dano percentual e objetivos rápidos |
| Goku Raditz + Goku Freeza | Contenção para feixe canalizado | Dispersão e silenciar a canalização |
| Trunks contra Freeza Z | Eliminação de mago frágil | Controle telecinético, wards e peel |

Esses pares são hipóteses qualitativas. Contadores não são multiplicadores secretos; emergem do kit, alcance, recarga, mobilidade, estatísticas e visão. Testar matchups cruzados automatizados e com jogadores de habilidade comparável.

# 15. Fusão, Oozaru e mecânicas cooperativas

## 15.1 Fusão de dois jogadores

Fusão é sistema de fase avançada e precisa de protótipo isolado antes de entrar no modo competitivo. Dupla inicial elegível: `GKU_SUPER` + `VGT_SUPER`; requisito: ambos vivos, nível 18, distância ≤ 5 u [P], fora de controle pesado, Ki ≥ 60% e habilidade de fusão da equipe disponível. Interação de convite curto no HUD: Goku propõe, Vegeta confirma até 5 s. Inicia `FusionWindup` de 2 s; o servidor valida novamente ao fim. Interrupção por morte, afastamento, CC definido ou timeout não consome uso integral (pode aplicar CD curto antispam). A fusão ativa por 45 s [P], inicialmente uma vez por equipe/partida.

## 15.2 Responsabilidades simultâneas

**Jogador A / corpo:** clicar para mover, selecionar foco de ataques básicos, acionar Q/E e pings de movimento. **Jogador B / energia:** mira e executa W/R e barreira especial, gerencia recurso especial de fusão, pings de alvo; o cursor dele pode usar retícula separada. Ambos visualizam mesma entidade em duas câmeras centradas, mas HUD de habilidades pessoais diferentes. Não há disputa do mesmo comando: no servidor, cada componente de controle tem exatamente um `owner_controller_id`; inputs não autorizados são rejeitados. Troca de papéis exige aceite dos dois e cooldown. A assistência com teclado/mouse e latência diferente deverá ser testada para garantir controle responsivo dos dois.

## 15.3 Entidade e consequências

No servidor: preservar snapshots dos dois heróis; torná-los `MergedDormant`; criar `FusionChampionEntity` com `fusion_id`, `parentA`, `parentB`, HP e Ki próprios, níveis relativos, transformações permitidas, dois canais de input. Seus itens não se somam integralmente: montar atributos de fusão por fórmula balanceada sobre power budget dos pais. Jogadores ausentes deixam de controlar fusão após timeout, e a dupla pode desfazer antecipadamente para não manter corpo inerte. **Morte da entidade fusionada causa morte de ambos**, com timers calculados individualmente; buffs descartados conforme regra. Ao separar, encontrar dois pontos seguros adjacentes; caso parede ou torre ocupe espaço, usar slots de fallback válidos. Nenhuma separação dentro de parede.

## 15.4 Comunicação e reconexão

Ícones no HUD `corpo/energia`, status do outro jogador, painel breve de instruções, ping rápido "Preparar suprema", indicador de Ki da fusão e limite de duração. Se um jogador desconectar, ativar janela [P] 5s de piloto automático **somente para defesa e movimentação segura**; depois permitir desfusão, sem outorgar controle completo do ausente ao outro. Reconnect devolve papel original se ainda fusionado. Registrar replay de ambas entradas.

## 15.5 Oozaru e alterações de escala

Goku Criança e Vegeta Saiyajins podem ficar gigantes temporariamente. A malha e aura crescem, mas raio de colisão é um atributo explícito; não bloquear corredores essenciais nem ficar preso entre obstáculos. Se o tamanho exceder corredor, forma não é ativável nesse ponto ou personagem é deslocado para área segura antes de concluir o efeito (regra definida pelo mapa). Aumento de ataque em área contrasta com perda de velocidade e hitbox maior; indicadores de alcance adaptam-se ao novo tamanho.

# 16. Itens, loja, receitas, cosméticos e consumíveis

## 16.1 Princípios econômicos

Zeni são gerados na própria partida e zeram na próxima. Itens podem ser comprados na fonte e têm condições de categoria/nível, custo e slot. Inventário padrão de seis slots mais slots acessórios específicos conforme modo. Componentes e receitas formam um DAG (grafo sem ciclos); combinar consome componentes e Zeni restante. Consumir itens em momentos inválidos não desconta saldo. Venda de item concede percentual [P] 70% do custo, salvo itens consumíveis e treinamentos; opção Desfazer preservada até ações invalidantes. Botão `Recomendado`, `Todos`, `Por atributo`, `Build favorita`, busca em português e tooltip detalhada.

## 16.2 Catálogo econômico inicial (preços provisórios)

| Código | Nome | Categoria | Custo Zeni | Efeito-base inicial | Observação |
|---|---|---|---:|---|---|
| IT_S01 | Uniforme de Treino | Inicial | 450 | +HP, +ataque leve | Ideal lutadores |
| IT_S02 | Luvas de Combate | Inicial | 450 | +dano físico, +recuperação | Início agressivo |
| IT_S03 | Foco de Ki | Inicial | 450 | +Ki e +poder de Ki | Magos |
| IT_S04 | Protetor Capsule | Inicial | 450 | +defesa e HP | Suportes/tanques |
| IT_S05 | Ferramenta do Caçador | Selva | 450 | dano apropriado a monstros | Reservada a selva |
| IT_S06 | Dispositivo de Suporte | Suporte | 400 | renda condicionada e visão | Tarefa de suporte |
| IT_C01 | Luvas Reforçadas | Componente | 350 | +ataque | Ingrediente |
| IT_C02 | Célula de Ki | Componente | 350 | +Ki | Ingrediente |
| IT_C03 | Placa de Armadura | Componente | 400 | +armadura | Ingrediente |
| IT_C04 | Núcleo Isolante | Componente | 400 | +resistência de Ki | Ingrediente |
| IT_C05 | Cristal de Vida | Componente | 400 | +HP | Ingrediente |
| IT_C06 | Engrenagem Ágil | Componente | 350 | +velocidade de ataque | Ingrediente |
| IT_B01 | Botas Comuns | Movimento | 300 | +velocidade | Pode evoluir |
| IT_B02 | Botas do Vento | Movimento | 1.100 | maior velocidade | Sem atravessar paredes |
| IT_B03 | Botas de Armadura Saiyajin | Movimento | 1.200 | velocidade + armadura | Proteção física |
| IT_B04 | Botas de Ki | Movimento | 1.200 | velocidade + resist. energética | Magia |
| IT_B05 | Botas de Treinamento de Kame | Treinamento | 1.100 | −8% velocidade por 5min ativa; depois +8% perm. | Consumidas; 1 vez/partida |
| IT_O01 | Luvas do Dragão | Ofensivo | 2.800 | ataque e dano sustentado | Melee |
| IT_O02 | Manoplas Saiyajins | Ofensivo | 3.100 | ataque, velocidade, efeito após habilidade | Lutador |
| IT_O03 | Bastão de Energia | Ofensivo | 2.900 | dano/alcance conforme skill | Controle |
| IT_O04 | Cristal de Kaiô | Ki | 3.000 | poder de Ki, aceleração | Técnicas |
| IT_O05 | Núcleo Blue | Ki | 3.200 | poder de Ki e Ki máximo | Alto custo |
| IT_O06 | Amplificador de Feixe | Ki | 2.900 | dano escalado em canalizações | Limite de efeito |
| IT_O07 | Braçadeira de Precisão | Ataque | 2.700 | crítico e ataque | Carry |
| IT_O08 | Lentes de Energia | Ki | 2.800 | penetração de resistência de Ki | Anti-tanque |
| IT_D01 | Colete do Exército de Freeza | Defesa | 2.800 | armadura e HP | Físico |
| IT_D02 | Armadura Saiyajin Reforçada | Defesa | 3.200 | HP, armadura, escudo condicional | Tanque |
| IT_D03 | Barreira Namekuseijin | Defesa | 3.000 | resist. Ki e shield | Proteção |
| IT_D04 | Capa de Kami-Sama | Defesa | 2.700 | resistências situacionais | Diversa |
| IT_D05 | Núcleo de Regeneração | Defesa | 3.100 | HP e regen fora do combate | Anti-burst |
| IT_U01 | Radar de Assistência | Suporte | 2.300 | buffs de visão | Limites de alcance |
| IT_U02 | Amplificador de Escudos | Suporte | 2.700 | cura/escudo em aliados | Sem stacking infinito |
| IT_U03 | Engenho do Tempo | Utilidade | 2.900 | aceleração e efeito pós-ação | CD interno |
| IT_U04 | Pulso de Interferência | Controle | 2.800 | aplica redução de cura conforme condições | Anti-cura |
| IT_U05 | Protetor do Núcleo | Suporte | 2.900 | aura defensiva próxima | Aura não acumula |
| IT_K01 | Cápsula de Recuperação de Ki | Consumível | 70 | recuperação curta de Ki | Estouro limitado |
| IT_K02 | Semente dos Deuses Parcial | Consumível | 95 | recuperação gradual de HP | Cancela sob condição explícita |
| IT_V01 | Radar Capsule | Visão | Grátis/recarga | ward de visão | Limite por jogador |
| IT_V02 | Radar de Controle | Visão | 75 | detectar wards | Slot específico |
| IT_V03 | Rastreador de Ki | Visão | upgrade | varredura temporária | CD controlado |

Todo item deve ter `id`, `name_ptbr`, descrição curta/longa, `price`, `sellPrice`, `tags`, `maxStack`, `recipe`, `stats`, `uniquePassives`, `active`, `cooldown`, `useRestrictions`, `iconAsset` e `dataVersion`. Valores de ataque/defesa do catálogo serão preenchidos após estabelecer curva de poder e cenários de build; preços sozinhos não definem eficiência.

## 16.3 Receitas-modelo e regras de empilhamento

`Luvas do Dragão = Luvas Reforçadas ×2 + Cristal de Vida + custo de combinação`. `Cristal de Kaiô = Célula de Ki + Núcleo de Isolante energético (novo componente se necessário) + custo de combinação`. Receitas são validadas por ferramenta automática e precisam fechar exatamente o preço final. Identificadores únicos `unique_group` impedem dobrar efeitos exclusivos por múltiplas cópias; quando duas fontes afetarem o mesmo estatuto, seguir ordem declarada: atributos base → bônus absolutos → bônus percentuais → limite → condicionais finais.

## 16.4 Sistema de treinamento por item

As Botas de Treinamento ativam debuff imediatamente após compra válida; contagem de cinco minutos considera tempo **vivo e fora da fonte**, não tempo de parede do sistema; morte congela, pausa de partida congela. Não é permitido vender/recuperar debuff para obter bônus instantâneo. Ao finalizar, item desaparece, gera `PermanentTrainingBuff` e libera slot. O efeito persiste até o fim da partida, mas não na conta. A implementação deve mostrar `04:59` restante e tooltip com benefício exato. Todo item futuro de treinamento segue o mesmo contrato `UpfrontCost → DrawbackPhase → EarnedReward`, com regras de abortar claras.

## 16.5 Regras de cosméticos

Skins são aparência, nunca estatística. Uma skin de uma variante define como cada forma será exibida; se só houver modelo base, o catálogo marca incompatibilidade com variante/formas avançadas ou usa fallback original licenciado. Loja cosmética com dinheiro real é **posterior à licença e à segurança de pagamentos**. Antes disso, coleção local contém apenas assets internos permitidos e pacotes de teste com auditoria de origem.

# 17. Escolas de Treinamento (runas) e técnicas universais

## 17.1 Formato da página

Uma Escola principal, uma runa-chave e três menores escolhidas dentro das fileiras; Escola secundária concede dois efeitos de outras fileiras; fragmentos de estatísticas são aplicados como modificadores adicionais. Escolhas são realizadas no lobby/seleção, exportadas para `LoadoutSnapshot`, assinadas/validadas pelo backend e interpretadas pelo servidor da partida. Ao trocar de versão de balanceamento, presets podem ficar inválidos e retornar ao padrão com aviso claro.

## 17.2 Catálogo inicial de efeitos

| Escola | Runa-chave | Efeito temático | Exemplos de menores |
|---|---|---|---|
| Tartaruga | Sequência Perfeita | Acertos repetidos ampliam dano até teto | Recuperar Ki após hit; dano por constância; precisão em alvos isolados |
| Força Saiyajin | Explosão de Orgulho | Dano em curta janela dispara efeito adicional | Caçada a alvo ferido; velocidade após abate; penetração após combo |
| Controle de Ki | Harmonia Energética | Alternar técnicas acumula energia e libera descarga | Redução de recarga; Ki extra; zona de acerto |
| Resistência | Corpo de Aço | Troca longa gera defesa temporária | Escudo inicial; recuperação em combate; resistência a controle |
| Tecnologia Capsule | Preparação Tática | Item/feitiço ativado gera utilidade moderada | Desconto de consumível; visão melhorada; aceleração após retorno |

Cada efeito possui `trigger`, `cooldown_internal`, `stack_limit`, `duration`, `formula`, `tags`, compatibilidade e ícone. Não permitir runa-chave que faça transformação exclusiva; marcos das variantes permanecem de domínio do campeão.

## 17.3 Técnicas universais (D/F)

| Técnica | Papel | Condição | Recarga proposta |
|---|---|---|---|
| Deslocamento Relâmpago | Salto curto instantâneo | Posição válida e visível conforme regra | 300s |
| Transmissor Capsule | Transporte longo | Canalização e destino permitido | 300s |
| Cápsula Restauradora | Cura pequena individual/aliada | Alcance limitado | 240s |
| Aceleração de Combate | Velocidade temporária | Redução de eficácia em extremo | 210s |
| Campo de Supressão | Dano/velocidade reduzidos no alvo | Alvo e alcance válidos | 210s |
| Energia Residual | Dano ao longo do tempo e anti-cura moderada | Não acumula livremente | 180s |
| Escudo Emergencial | Escudo autônomo breve | Não impede CC | 180s |
| Libertação de Energia | Remove controles autorizados | Exceções explícitas | 210s |
| Golpe de Precisão | Forte dano a monstro/objetivo | Usado pelo caçador da selva | Cargas e evolução |

Esses tempos são apenas sementes de balanceamento. Narrativamente, Goku Criança realiza um salto veloz em vez de teleportar, mas o **hitbox**, distância e janela competitiva de Deslocamento Relâmpago são equivalentes para todos.

# 18. Bots, treino, modos e progressão da conta

## 18.1 Inteligência artificial

Bots implementam `Perception → Decision → Action` sob a mesma visão permitida ao time. Papéis de IA: `LaneBot` (last hit, recuar, trocar), `JungleBot` (rota, camp e objetivo), `SupportBot` (proteger, ward, seguir), `ObjectiveDirector` (estimular decisões coletivas sem obter informação ilegal). Níveis: iniciante com alta latência de reação, intermediário com rotinas por função, avançado com coordenação limitada. Bots não podem enxergar inimigos ocultos nem ignorar Ki/cooldown. Para treino, parâmetros configuráveis incluem HP infinito, ouro livre, nível escolhido, reset de CD, spawns e velocidade de simulação.

## 18.2 Progressão externa à partida

Conta ganha XP por participação válida; desbloqueios são **somente modos, aprendizagem e cosméticos**, não bônus de dano. Maestria por variante acompanha partidas, desempenho, marcos e cosméticos. Estatísticas por temporada e por modo separadas; partidas de teste interno e bots não contaminam ranking. Histórico tem match ID, data, duração, personagem, forma final, resultado, KDA, CS, Zeni, dano, visão, objetivos e timeline simplificada.

## 18.3 Abandono, rendição e integridade

Rendição exige janela mínima de tempo e maioria configurável do time. Interrupção inicial por falha significativa de conexão pode permitir remake sem classificar derrota injusta. Saída antes de terminar partida sinaliza abandono potencial, com período de reconexão e consideração de incidentes confirmados; sanções transparentes e graduais. Não prometer detecção perfeita de AFK e griefing. Partidas personalizadas podem ter regras livres; competitivas usam política única publicada no client.

# 19. Fim da partida, pós-jogo e encerramento

## 19.1 Sequência de término

Quando núcleo inimigo chega a HP zero, servidor emite `MatchCompleted(matchId, winner, reason, finalTick)` uma vez, aplica freeze de combate, snapshot final e escreve resultado em storage transacional. Cliente mostra morte do núcleo em animação de até 5 s [P] com qualidade adaptativa, som de vitória/derrota, silencia input ofensivo, gera tela `Vitória`/`Derrota`. Não considerar partidas encerradas somente por animação local ou por queda do cliente.

## 19.2 Tela de resultados

Resumo com time vencedor, tempo, personagens e formas finais; placar detalhado de dano por tipo, dano mitigado, cura/escudos, CS, Zeni total, itens finais, visão e sentinelas, objetivos, KP, mortes, experiência obtida via carregamento de Ki, duração de treinamento, transformações, fusões e sua eficácia. Gráficos de evolução de ouro/XP por minuto apenas onde houver dados; timeline de objetivos; créditos de performance sem humilhação de colegas. Botões `Continuar`, `Jogar novamente`, `Adicionar amigo`, `Denunciar`, `Salvar replay` [F]. Informação pública restrita por privacidade.

## 19.3 Retorno ao client

Ao clicar Continuar: fechar conexão do MatchServer, liberar cena e efeitos em memória, reconciliar dados de perfil com backend, apresentar notificações de progressão e retornar ao mesmo grupo social quando possível. Se backend cair, cliente pode mostrar resultado recebido como **pendente de sincronização**, e reenviar com idempotency key; nunca fabricar vitória baseada em dados locais não confirmados.

## 19.4 Encerramento voluntário

Sem partida ativa, `Sair` salva configurações, finaliza voz/chat, cancela filas e desconecta com timeout breve. Em seleção, avisa efeito de abandonar; em partida, avisa que a partida continua e oferece Cancelar/Desconectar. O Windows pode encerrar compulsoriamente; servidor trata timeout e reconnect. Logs locais rotacionados com retenção limitada; token sensível nunca salvo em texto aberto. Ao atualizar o launcher enquanto o jogo está aberto, não substituir executáveis em uso.

# 20. Arquitetura de software e divisão de responsabilidades

## 20.1 Visão arquitetural

Recomenda-se separar **interface e renderização**, **simulação autoritativa**, **serviços de sessão** e **dados persistentes**. Isto permite reaproveitar parte de um MOBA C# aberto sem acoplar o funcionamento competitivo ao ciclo de renderização do Unity. O protótipo de referência `MOBA_CSharp_Unity` declara servidor C# independente do Unity, com ENet-CSharp, MessagePack, SharpNav e cliente Unity 2018.3.4f1. É um candidato a auditoria, não uma base já aprovada; terá de ser compilado, perfilado, ter dependências atualizadas e licenças rastreadas [Fonte 2].

### 20.1.1 Diagrama lógico

```text
Windows Launcher (.NET)
   | HTTPS/TLS: manifest + auth + updates
   v
CDN/Storage ----------- Identity + Account API (.NET 10)
                            |                 |
Unity Client (URP) ----- Gateway/API ----- PostgreSQL
   |   \                         \        Redis [opcional]
   |    \-- Social/Presence/Queue ---- Matchmaking
   |                                      |
   +--- UDP/TCP protocol [autenticado] --> Dedicated MatchServer (C#)
                                              |      |
                                        Combat Sim  Match Results
                                              |      |
                                       Rules/Data  Backend Storage
```

Em desenvolvimento: processo local de servidor, API e banco via Docker Compose, cliente Unity no Windows. Em produção: API e matchmaking escaláveis horizontalmente; servidor de partida isolado por processo/contêiner; resultado persistido em serviço próprio; telemetria centralizada. CDN e bytes de assets não trafegam pelo servidor de gameplay.

## 20.2 Stack proposta e alternativas

| Camada | Tecnologia preferida | Alternativa | Justificativa / decisão |
|---|---|---|---|
| Cliente 3D | Unity 6.3 LTS, C#, URP | Godot 4 [não prioritário] | Pipeline 3D, equipe e reuso do código em Unity |
| UI principal | UI Toolkit para client; uGUI para HUD de alto desempenho onde necessário | UI Toolkit integral | Testar compatibilidade e perf. em cenário real |
| Input | Unity Input System | Adaptador legado temporário | Remapeamento e dispositivos |
| Câmera | Cinemachine | Controller próprio | Follow/zoom/edge pan sem câmera livre |
| Assets dinâmicos | Unity Addressables | AssetBundles gerenciados | Variantes de forma e updates de conteúdo |
| Dados de balanceamento | JSON/YAML → build validada | ScriptableObjects gerados | Diff no Git e shared logic de servidor |
| Simulação | C# em servidor independente | Headless Unity para protótipo | Segurança e reuso do framework aberto |
| Transporte | ENet-CSharp [V] | Unity Transport/LiteNetLib [V] | Testar manutenção, licenças, loss e desempenho |
| Serialização | MessagePack-CSharp [V] | Protobuf | Pacotes compactos versionados |
| Backend HTTP | ASP.NET Core/.NET 10 LTS | Serviço gerenciado | Mesma linguagem, ferramentas maduras |
| Banco | PostgreSQL | Banco gerenciado compatível | Transações, índices e consulta |
| Cache/fila | Redis [se necessário] | Serviços gerenciados | Presença, fila efêmera e locks curtos |
| Observabilidade | OpenTelemetry + métricas/logs estruturados | Ferramentas SaaS | Tracing e diagnóstico central |
| Infra local | Docker Compose | Scripts nativos | Reprodutibilidade |
| CI | GitHub Actions | GitLab CI | Builds, testes e artefatos assinados |
| Asset art | Blender, Krita, GIMP, Audacity/Reaper conforme licença | Ferramentas equivalentes | Pipeline interoperável e custo controlado |
| Comunicação opcional | Vivox [F/V] | Outro provedor | Voz/chat moderáveis; exige revisão de preço/SDK |

**Compatibilidade:** Unity 6.3 LTS suportado até dezembro de 2027 [Fonte 3]; .NET 10 LTS até novembro de 2028 [Fonte 4]. Versionar precisamente Unity editor, SDK e packages no repositório. Não iniciar upgrade de Unity sem matriz de testes de cliente, shader, animações e importadores.

## 20.3 Camadas de código

- `Domain`: entidades puras (`Champion`, `Ability`, `Buff`, `Item`, `Wave`, `Objective`, `MatchState`), sem dependência Unity; validações e fórmulas.
- `Simulation`: tick, comandos, física 2D, colisão, pathfinding, timers, visibilidade, IA e resolução de eventos.
- `Protocol`: envelopes de mensagens, mensagens compiladas, versões, snapshots e compressão.
- `ClientPresentation`: cenas, UI/HUD, áudio, Animator, VFX, câmera, input e predição visual.
- `GameData`: schemas versionados e pipeline de validação de catálogos, assets e balanceamento.
- `Backend`: auth, contas, amigos, matchmaking, sessões, histórico e moderação.
- `Infrastructure`: bancos, cache, deployments, métricas, logs, configuração e segredos.
- `Tools`: editor de mapa, importação de formas, simulador de partida, bots e pipeline de QA.

Regra de dependência: `Presentation → Protocol / DTO`, `Server → Domain + Simulation`, `Backend → Contracts`; `Domain` não importa `UnityEngine`, API HTTP, banco ou biblioteca gráfica. Documentar regras que devem ser **idênticas** em simulação e predição; centralizar constants e versões.

## 20.4 Entidades de domínio e principais eventos

| Entidade | Campos obrigatórios relevantes | Eventos |
|---|---|---|
| PlayerSession | userId, matchId, teamId, inputOwner, authProof | Connected, Lost, Reconnected, Left |
| Champion | instanceId, variantId, formId, level, HP/Ki, buffs, items | Spawned, Damaged, Leveled, Transformed, Died |
| AbilityCast | castId, skillId, tick, source, target/point, phase | Requested, Validated, Fired, Interrupted, Resolved |
| ItemInstance | uniqueId, catalogId, owner, stacks, cooldown | Purchased, Used, Sold, Upgraded |
| MinionWave | waveId, lane, side, spawnTick, unitIds | Spawned, Cleared, ReachedStructure |
| Objective | id, state, hp, spawnTick, ownerTeam | Spawned, Engaged, Killed, BuffGranted |
| TeamVision | teamId, cells, revealSources | SourceAdded, SourceRemoved, Recomputed |
| Fusion | fusionId, parents, inputRoles, deadline, status | Proposed, Accepted, Merged, Split, Killed |
| Match | id, rulesVersion, mapVersion, players, status | Created, Started, Paused, Finished, Archived |

Somente comandos validados geram eventos autoritativos. Uma mesma solicitação de compra/comando idempotente não pode gerar dois efeitos ao receber retransmissão.

# 21. Protocolo de rede, sincronização e reconexão

## 21.1 Contrato de transporte

Cliente obtém `match_access_token` de curta duração via API autenticada, junto a endpoint regional, `match_id`, `player_slot`, `protocol_version` e `content_hash`. Canal de simulação usa transporte de datagramas escolhido após benchmarking; handshake valida token, versão de conteúdo e papel. Mensagens levam `sequence`, `clientTick`, `ack`, `commandType` e payload reduzido. O cliente envia **intenções** (`MoveTo`, `AttackTarget`, `CastAbility`, `UseItem`, `StartCharge`, `StopCharge`, `Ping`, `Buy`) em vez de posição, dano ou ouro final.

## 21.2 Predição e correção

Mover pode usar predição local de trajetória e animação para resposta rápida, com reconciliação ao snapshot do servidor. Inimigos e tropas são interpolados com buffer para suavizar jitter; teleportes legítimos são eventos instantâneos e não interpolados como caminhada. Habilidades de acerto delicado usam resolução do servidor e lag compensation conservador, com limites e testes de exploração. Render FPS variável; tick do servidor fixo. Envio de estado via snapshots completos ocasionais + deltas com baseline, priorizados por interesse/visibilidade.

## 21.3 Validações anti-exploit

Verificar: distância percorrida por tick, cooldown, custo Ki, HP, posição válida, alvo visível, orçamento de ações por segundo, inventário, Zeni, escolha de personagem, função da fusão e premissas de colisão. Client não recebe posições ocultas; usar verificação anti speed hack e teleporte impossível. Comandos muito antigos/futuros ou duplicados são ignorados com métricas. Chaves de sessão expiram, mitigação de replay protege ações críticas e rate limits são por token/usuário/IP conforme avaliação de risco.

## 21.4 Estados de perda de conexão

1. Perda curta (<3s [P]): HUD alerta sinal, predição local limitada, não acumular comandos infinitos.
2. Perda moderada (3–20s [P]): camada de reconexão, jogo segue no servidor; herói adota último comando válido ou IA defensiva de modo específico.
3. Reconexão: autenticar novo socket, receber snapshot completo, inventário/forma/HP/posição/cooldowns e fila de eventos relevantes, reconstruir objetos sem duplicar efeitos.
4. Timeout prolongado: servidor marca abandono potencial, mas não apaga personagem em partida competitiva; tratamentos dependem de regras do modo.
5. Conclusão durante desconexão: ao retornar ao client, buscar resultado por `match_id` em vez de tentar entrar em servidor destruído.

## 21.5 Budget de rede e performance

**Hipóteses iniciais:** servidor com 10 heróis, dezenas de tropas, objetivos, projéteis e efeitos; tick 30 Hz; snapshots de 10–20 Hz por entidade relevante [P]. Perfilar p50/p95/p99 de tempo de tick; uso de memória; bytes/s por jogador; pacotes perdidos; retransmissões; jitter; correções de posição; frequência de alocação GC. Não inventar metas de bytes antes da primeira medição. Gate de lançamento: um servidor aguenta 10 jogadores em simulação de pior caso (luta 5v5 + ondas de tropa + chefe) mantendo tempo de tick dentro do orçamento com margem operacional.

# 22. Backend, APIs, persistência e fluxos críticos

## 22.1 Serviços lógicos

Um **monólito modular** em ASP.NET Core é preferível a microsserviços completos no começo: módulos `Identity`, `Profiles`, `Social`, `Party`, `Queue`, `Draft`, `MatchHistory`, `Catalog`, `Moderation`, `Admin`. Um processo separado `MatchmakerWorker` prepara partidas e agenda instâncias do MatchServer. Comunicação interna via interfaces e eventos confiáveis; banco único inicialmente, com tabelas por domínio. Separar serviços apenas quando gargalos operacionais comprovados justificarem.

## 22.2 Endpoints de referência

| Método e caminho | Ação | Segurança / idempotência |
|---|---|---|
| POST /v1/auth/register | Criar conta | rate limit e verificação e-mail |
| POST /v1/auth/login | Iniciar sessão | rate limit, política de risco |
| POST /v1/auth/refresh | Girar refresh token | token rotativo e revogação |
| POST /v1/auth/logout | Revogar sessão | idempotente |
| GET /v1/profile/me | Perfil do usuário | auth |
| PATCH /v1/profile/settings | Salvar configurações | etag/versionamento |
| GET /v1/champions | Catálogo das 15 variantes | versão pública assinada |
| GET /v1/items | Catálogo de itens | versão pública |
| POST /v1/party | Criar grupo | auth |
| POST /v1/party/{id}/invite | Convidar jogador | permissões de líder |
| POST /v1/queue/tickets | Entrar na fila | unique ticket por usuário |
| DELETE /v1/queue/tickets/{id} | Cancelar busca | idempotente |
| POST /v1/match/{id}/accept | Aceitar partida | valida convite e deadline |
| POST /v1/draft/{id}/lock | Fixar campeão/build | valida catálogo e tempo |
| GET /v1/matches/{id} | Histórico/resultado | privacidade/autorização |
| POST /v1/reports | Denúncia | limites e prevenção de abuso |
| GET /v1/client/manifest | Versão e assets | assinatura e cache/CDN |

Todos os endpoints exigem contrato formal OpenAPI, validação de payload, limites de tamanho, códigos de erro estáveis e rastreamento `correlation_id`. Não colocar lógica autoritativa de dano no backend HTTP; pertence ao MatchServer.

## 22.3 Modelo de dados relacional inicial

```text
users(id, email_normalized, password_hash_ref, created_at, state)
profiles(user_id, public_name, avatar_id, locale, account_level, account_xp)
sessions(id, user_id, refresh_token_hash, device_id, expires_at, revoked_at)
friendships(user_a, user_b, status, created_at)
blocks(blocker_id, blocked_id, created_at)
parties(id, leader_id, mode, region, version)
party_members(party_id, user_id, preferred_roles, ready)
queue_tickets(id, party_id, mode, region, status, created_at)
match_sessions(id, region, rules_version, map_version, status, started_at, ended_at)
match_players(match_id, user_id, team_id, champion_variant, loadout_hash, result)
match_results(match_id, winner_team, duration_ticks, checksum, payload_ref)
match_player_stats(match_id, user_id, kills, deaths, assists, cs, gold, vision, ...)
loadouts(id, user_id, champion_variant, rune_page_json, spells_json, version)
cosmetic_entitlements(user_id, cosmetic_id, grant_source, granted_at)
moderation_reports(id, reporter_id, reported_id, match_id, category, state)
audit_events(id, actor_id, action, resource, reason, created_at)
```

Índices em sessão, `email_normalized`, partidas do usuário, estado da fila e timestamps. Migrações versionadas, dados de teste sintéticos, backups com restauração ensaiada. Transações para resultado e contabilidade persistente de cosméticos. Anonimização/exclusão de conta sujeita a política legal de retenção de auditoria.

## 22.4 Resultado de partida idempotente

MatchServer fecha resultado assinado com `matchId`, `rulesVersion`, participantes, estado final, hash de eventos e identificador de processamento; backend usa unique constraint em `match_id` e grava exatamente uma vez na perspectiva de negócio. Retentativas de rede não duplicam recompensa de conta. Partidas sem assinatura/instância registrada entram em quarentena para avaliação.

# 23. Organização do repositório, convenções e ferramentas internas

## 23.1 Monorepositório sugerido

```text
dragon-moba/
  docs/                  # GDD, TDD, ADRs, diagramas, licenças
  client-unity/          # Projeto Unity 6.3
    Assets/Game/Core/
    Assets/Game/UI/
    Assets/Game/Characters/
    Assets/Game/Maps/
    Assets/Game/Audio/
    Assets/Game/VFX/
  launcher/              # Instalador, patcher e diagnóstico
  server-match/          # C# simulação autoritativa
  backend-api/           # .NET 10 módulos de conta e social
  matchmaker-worker/     # Filas, aceite, alocação e partida
  shared-contracts/      # Proto/MessagePack schema, DTOs
  balance-data/          # Heróis, itens, runas, objetivos
  art-source/            # Arquivos-fonte com origem licenciada
  tools/                 # Validadores, migrações, simuladores
  infra/                 # Compose, IaC e observabilidade
  tests/                 # Unit, integration, e2e e load
  .github/workflows/     # CI/CD
  third_party_licenses/  # LICENSE, NOTICE, proveniência
```

## 23.2 Git, branches e qualidade

`main` sempre compilável, `develop` opcional, branches curtas `feat/...`, `fix/...`, `docs/...`, PR com revisão e status de testes; tags `v0.x.y`; SemVer para backend/client; versão de protocolo e conteúdo separadas. Arte binária pesada em Git LFS com política de tamanho, evitando subir assets sem permissão. Code owners por domínio; Conventional Commits opcional; ADR (Architecture Decision Record) para decisões irreversíveis. `dotnet format`/Roslyn analyzers, regras de estilo C#, editorconfig, validação JSON Schema e linter de catálogos. Código de terceiros isolado em `vendor/` ou fork explícito, sem misturar alterações enormes sem rastreio.

## 23.3 Ferramentas para o designer

- **Balance Editor:** interface para stat bases, progressões, itens, custos, cooldowns e comparações de dano efetivo.
- **Champion Preview:** mudar níveis/formas em tempo real, validar assets faltantes, comparar silhuetas.
- **Map Editor:** waypoints, spawns, collision layers, bush shapes, wards, monstros, objetivos e campo de visão.
- **Bot Scenario Runner:** rodar partidas headless 1.000× com seeds, registrar winrate e duração.
- **Replay Inspector:** reconstruir tick/evento, câmera de combate e discrepâncias.
- **Asset Validator:** licença, rig, scale, LOD, texturas, nome, polycount, atlas, performance e export.

# 24. Pipeline de arte, animação, som e licenças

## 24.1 Tratamento de modelos/skins existentes

As skins feitas para LoL geralmente são **mods atrelados à malha, esqueleto, shaders ou animações do campeão-alvo de LoL**; não são automaticamente modelos importáveis e autorizados para o Unity em um jogo independente. Um mod de Goku para Nasus pode demonstrar transição de formas, mas seus arquivos podem depender de material de Riot e de Dragon Ball. **Fluxo permitido:** catalogar link, autor, versão, origem do modelo, licenças documentadas; pedir permissão específica do criador quando necessário; confirmar que ele tem direito de sublicenciar cada componente; aprovar para uso somente após revisão. Sem isso, usar placeholders originais licenciados ou modelagem própria.

## 24.2 Matriz de proveniência do asset

Cada asset terá `asset_id`, `source_url`, `author`, `copyright_owner`, `license`, `license_file`, `attribution`, `permission_evidence`, `commercial_allowed`, `derivatives_allowed`, `redistribution_allowed`, `model_origin`, `ingest_date`, `approval_state`, `reviewer`. Estados: `quarantined`, `internal_reference`, `approved_internal`, `approved_public`, `rejected`. Pipeline de release **bloqueia** assets não `approved_public` no build público. Licença MIT/Apache do **código** de um repositório não elimina direitos sobre imagens, música ou nomes que ele possa conter.

## 24.3 Metas iniciais técnicas de assets

- Modelos dos heróis com LOD 0/1/2; limite de triângulos e materiais definido após profiling em 10 campeões, criaturas e VFX.
- Texturas comprimidas em formatos corretos para Windows; atlas por personagem/forma quando viável; cargas prévias limitadas ao loadout da partida.
- Animações humanoides compartilhadas em rigs compatíveis; máquinas de estados e override por forma; form giant tem controlador próprio.
- Áudio 2D para eventos do client, espacial atenuado para habilidades no mapa, prioridades de voz e combate; evitar som de todos os 10 heróis no volume máximo ao mesmo tempo.
- Catálogo de imagens para ícones HUD, retratos 2D, miniaturas de seleção, tela de load e minimapa: mesma forma precisa ser identificável em todos os contextos.

## 24.4 Production sheet obrigatório por personagem/forma

Checklist: nome/ID; história/variante; silhouette front/top isométrica; paleta; shape language; rig; 16+ clips de movimento/combate/estado; 5 ícones de habilidade; retrato; splash; VFX por habilidade; transform start/loop/end; SFX; áudio de fala original/licenciado; LOD; teste acessibilidade; testes GPU/CPU; reprodutibilidade na cena de preview; provenance. Critério de pronto exige funcionar em jogo, não apenas render bonito isolado.

# 25. Segurança, privacidade, moderação e conformidade

## 25.1 Modelo de ameaça

Ativos: identidade do jogador, tokens, disponibilidade do matchmaking, integridade da partida, dados de desempenho, canais de chat, cosméticos e infraestrutura. Adversários: usuário com cliente modificado, scripts de automação, bot de cadastro, spammer, atacante de DDoS, insider indevido e terceiros com assets sem licença. Fronteiras de confiança: launcher↔CDN, client↔API, client↔MatchServer, MatchServer↔backend, admin↔produção. Nunca confiar em estado de vida/dano/Zeni vindo do cliente.

## 25.2 Controles mínimos

TLS em serviços HTTP; autenticação e autorização por recurso; tokens curtos e rotativos; assinatura de release; secrets via secret manager; revisão de dependências (SCA) e SBOM; sanitização de texto/chat e rate limiting; senhas com hash robusto; backup/restore; registro de ações administrativas; autenticação reforçada para painel de moderação; política de atualização de patches críticos; portas de servidor expostas estritamente conforme necessidade. Segurança de jogo não depende só de anti-cheat no kernel; priorizar integridade de simulação no servidor, verificação de visão e análise de anomalias.

## 25.3 Privacidade e dados de jogadores

Coletar estritamente o necessário para criar conta, operar partidas e investigar abuso. Especificar finalidade, base legal aplicável (incluindo LGPD quando pertinente), retenção, exclusão, exportação, controle de acesso e medidas de proteção. Telemetria técnica utiliza IDs pseudonimizados; mascarar IP em exportações quando possível; não salvar texto completo de conversas indefinidamente; jamais coletar teclas fora do jogo. Canais de voz futuros terão consentimento, informação de gravação e política de moderação antes da ativação.

## 25.4 Direitos autorais e risco de marca

Riot proíbe o uso não autorizado de sua PI em jogos/apps, inclusive mapa, personagens, ícones, objetos e arte [Fonte 5]. Os nomes/personagens e visuais Dragon Ball também não devem ser presumidos liberados apenas porque fãs criaram modelos. O protótipo **não** será distribuído com assets questionáveis. Para viabilizar uma publicação comercial, duas opções estratégicas: obter licenças para propriedade intelectual relevante e desenvolver elementos próprios, ou converter o conteúdo para universo original preservando as mecânicas de MOBA. Uma revisão jurídica específica precede beta público e monetização.

# 26. Implantação, serviços e custos operacionais

## 26.1 Ambientes

`LOCAL` para desenvolvimento com Docker Compose, bots e credenciais fake; `CI` em pipeline efêmero; `STAGING` reproduz configuração regional e release; `PROD` com armazenamento persistente, monitoramento, backups e políticas de acesso. Tags e endpoint são distintos para não fazer teste de carga no servidor de produção por acidente. Cada ambiente possui chaves e segredos separados.

## 26.2 Escalonamento

API stateless pode escalar horizontalmente atrás de load balancer. Matchmaker gerencia fila em memória/Redis com garantia contra entrada duplicada e observabilidade. Um `GameAllocator` escolhe host de MatchServer por capacidade e região; uma instância de partida atende exatamente uma partida (isolamento simples para a primeira versão) e é destruída após persistir resultado e obter confirmação. Servidores de partidas não dependem de conexão HTTP contínua ao backend para cada ataque; possuem política de funcionamento degradado até o encerramento da sessão.

## 26.3 Logs e métricas

Registrar taxa de conclusão de partida, crashes, reconexões, tempo de fila p50/p95, aceite, deserção de draft, duração do loading, FPS client por preset, uso de memória, RTT, packet loss, tempo p95 de tick, divergência de posição, anomalias de dano e erros de conteúdo. Dashboards separados por patch e região; alertar quando houver crescimento de falhas após deploy. Identificadores correlacionam client, backend, alocador e MatchServer sem incluir dados sensíveis em logs.

## 26.4 Orçamento orientado a capacidade, não a palpites

Não há custo mensal confiável sem saber simultaneidade, regiões, provedor, frequência de partidas e consumo real de CPU/banda. Modelo de custo: `CustoTotal = HorasServidoresPartida + API + Banco + Cache + Storage/CDN + Observabilidade + Voz/Moderação + Tráfego + Suporte`. Dimensionar a partir de teste de 100 partidas simultâneas **simuladas** para estimar recursos por partida, e só então projetar gastos por 100, 1.000 e 10.000 jogadores concorrentes. Incluir mão de obra, licenças, certificação, testes e produção artística: geralmente serão custos mais difíceis de reduzir que hospedagem do protótipo.

# 27. Testes, aceitação e critérios de qualidade

## 27.1 Pirâmide de testes

**Unitários:** fórmulas de dano, cooldown, Ki, XP, ouro, compra/undo, restrições de forma, aggro, pathfinding, controle, morte e fusão. **Integração:** client ↔ servidor com versões, API ↔ banco, matchmaking ↔ alocador, resultado ↔ conta. **E2E:** click launcher → atualizar → login → queue → draft → loading → match → resultado → sair. **Carga:** 10 players + bots + 200 unidades e áreas máximas simultâneas. **Chaos:** derrubar socket, matar processo server, reiniciar backend, simular CDN lento, perda de pacotes 5%, atraso 150–250ms. **Usabilidade:** leitura de skills, indicador de alvo da torre, transformação sob combate e configurador de runas.

## 27.2 Matriz de casos críticos verificáveis

| ID | Cenário | Resultado esperado |
|---|---|---|
| QA-001 | Abrir launcher com conexão válida | Checar update, abrir tela correta |
| QA-002 | Manifesto com hash inválido | Bloquear arquivo e reportar erro |
| QA-003 | Dois cliques rápidos no EXE | Uma sessão do launcher; foco na existente |
| QA-004 | Login incorreto repetido | Erro neutro; rate limit sem revelar usuário |
| QA-005 | Aceite de 9 de 10 jogadores | Não criar partida definitiva; retornar elegíveis |
| QA-006 | Três jogadores escolhem mesma variante no mesmo time | Bloqueio ou reconciliação de escolha válida |
| QA-007 | Runas inválidas para versão | Substituir padrão com notificação antes de iniciar |
| QA-008 | Asset de forma ausente no loading | Fallback seguro; telemetria; não corrupção de partida |
| QA-009 | Segurar C com Ki 20% | Recuperar até 100%; após iniciar treino de XP |
| QA-010 | Receber dano enquanto C | Cancelar treino no tick apropriado |
| QA-011 | Usar C na fonte | Recuperar Ki, não receber XP de treino |
| QA-012 | Treino de Ki por vários níveis | Evoluir lentamente, sem ganho de Zeni ilegal |
| QA-013 | Comprar Botas de Treino e morrer | Contador congela; debuff e buff não duplicam |
| QA-014 | Transformar durante morte | Evento não aplicado até estado permitido |
| QA-015 | Ativar forma sem asset pronto | Fallback visual; atributos corretos no servidor |
| QA-016 | Voo contra parede intransponível | Não atravessa sem skill que permita |
| QA-017 | Habilidade de torres sob alvo prioritário | Aggro previsível e reproduzível |
| QA-018 | Atacar adversário não visível por packet forge | Server rejeita alvo proibido |
| QA-019 | Roubo de objetivo no último tick | Último golpe autoritativo único |
| QA-020 | Loja com dois cliques simultâneos | Uma compra por idempotência; saldo correto |
| QA-021 | Troca de fase de Freeza | Malha, HUD, ícone e habilidades atualizam |
| QA-022 | Goku Super atinge nível 18 | UI completa + MUI sem invulnerabilidade infinita |
| QA-023 | Fusion request fora de alcance | Recusa sem criar entidade órfã |
| QA-024 | Fusão interrompida no windup | Pais intactos; cooldown correto |
| QA-025 | Um piloto da fusão desconecta | Outro não ganha permissões indevidas; desfusão segura |
| QA-026 | Fusão morre | Duas mortes e contabilidade consistente |
| QA-027 | Cliente perde rede e reconecta | Snapshots restauram HP, XP, Ki, forma e itens |
| QA-028 | Núcleo destruído duas vezes por pacotes | Um resultado final e uma recompensa |
| QA-029 | Backend cai no pós-jogo | Tela informa pendência e reconcilia depois |
| QA-030 | Fechar cliente durante partida | Servidor mantém jogo e reconhece reconnect |
| QA-031 | Desativar flashes e tremor | Transformações continuam legíveis |
| QA-032 | Skin externa sem permissão em build público | CI falha; asset não empacotado |

## 27.3 Critérios de pronto por funcionalidade

Um recurso só está `Done` com documentação de regra, implementação servidor/client conforme necessidade, configurações data-driven, testes automáticos, telemetria mínima, estados de erro, acessibilidade relevante, revisão de performance e checklist de licença. "Animação pronta" não significa "transformação pronta" sem replicação de estado, HUD, logs e teste de reconexão.

## 27.4 Playtest e balanceamento

Coletar `winrate` por campeão/função/faixa de experiência, tempo médio de primeira kill, dano por minuto, ouro/XP por minuto, XP pelo C, duração de carregamentos, frequência de cada transformação, efetividade de itens, pick rate e objetivos. Segmentar premade vs solo, região e tipo de partida; não tirar conclusões de 10 jogos. Se a estratégia dominante for carregar Ki parado, corrigir incentivo; se transformações de nível 18 raramente ocorrerem, revisar tempo de partida, XP e marcos. Rodadas curtas de testes com changelog para rastrear causas.

# 28. Estratégia de desenvolvimento, marcos e prioridades

## 28.1 Decisão sobre código aberto: prova de viabilidade em duas trilhas

**Trilha A — Modernizar MOBA_CSharp_Unity:** clonar, verificar licença MIT e dependências, criar build reproduzível do servidor C#, migrar cliente para Unity 6.3 e validar movimento, vision, bushes, skills, items e rede. Registrar tudo que falhar, complexidade de migração e bugs. **Trilha B — Protótipo mínimo verde:** Unity 6.3 + personagem cápsula, C# servidor simples, movimento/cast, torres e socket. Comparar tempos de implementação, manutenção, desempenho, extensibilidade e quantidade de código legado reescrito. **Gate:** se a trilha A exigir reescrita da maioria dos componentes críticos, reaproveitar apenas ideias/algoritmos autorizados e seguir B. O UNION MOBA é referência secundária, porque o README o apresenta como demo Android, Photon PUN e PlayFab, com dependências legadas [Fonte 6].

## 28.2 Fases, entregáveis e critérios de saída

| Fase | Entregável principal | Aceite obrigatório |
|---|---|---|
| 0. Pré-produção | Este documento, riscos, licença, arquitetura, fork auditado | Decisões bloqueantes e protótipo de networking resolvidos |
| 1. Prova de movimento | 1v1 numa rota, câmera e pathfinding | Input responsivo, server authoritative, sem teleportes ilegais |
| 2. Vertical slice de Ki | Goku Criança + Goku Super + Cell, carregamento e XP | Transformações replicadas e C testado |
| 3. Loop MOBA | Mapa três rotas, tropas, torres, loja, base e vitória | Partida completa com bots sem passo manual |
| 4. Selva e estratégia | Monstros, visão, objetivos, wards, técnicas | Objetivos e bugs de visão cobertos por testes |
| 5. Elenco completo | 15 kits e formas, assets aprovados, balance editor | 15 variantes jogáveis e stress test visual |
| 6. Client completo | Launcher, contas, party, draft, carregamento, pós-jogo | E2E launcher até saída com 10 bots/usuários |
| 7. Multiplayer beta | 5v5 real, reconexão, matchmaking, observabilidade | Testes de carga e segurança do servidor |
| 8. Fusão cooperativa | Gogeta/Vegito em modo de teste | Dois input owners, split e reconnect estáveis |
| 9. Beta público | Distribuição e suporte, licenças, moderação | Aprovação legal/asset, estabilidade e operação |
| 10. Ranqueado | MMR, temporadas, sanções e matchmaking refinado | Volume e balanceamento suficientes |

**Estimativa indicativa:** um vertical slice simples pode levar semanas a poucos meses conforme equipe e reaproveitamento; um MOBA 5v5 polido, 15 variantes e artes transformáveis exige muitos meses ou anos de engenharia, arte, QA e operação. Não há cronograma responsável antes do teste técnico de Fase 0 e capacidade disponível da equipe. Não prometer lançamento público com licenças não resolvidas.

## 28.3 Primeiros 20 tickets de engenharia

| Prioridade | Ticket | Critério de aceite |
|---|---|---|
| P0 | ADR-001 stack e monorepo | ADR aprovada e builds locais documentados |
| P0 | AUD-001 clonar/fixar commit do MOBA aberto | BUILD/testes executados e inventário de licenças |
| P0 | NET-001 protótipo de conexão e autenticação | 2 clientes/servidor, comandos validados |
| P0 | MOV-001 movimento isométrico | pathfinding, colisão, correção de posição |
| P0 | SIM-001 loop tick e eventos | perfis de tick e ordem estável |
| P0 | DATA-001 schema champion/form | validador rejeita inconsistências |
| P0 | GAME-001 combate básico | ataques e HP autoritativos |
| P0 | KI-001 `ChargingKi` | Ki regenera por C, interrompe dano |
| P0 | KI-002 `TrainingXP` | treino concede XP justo, sem XP na fonte |
| P0 | FORM-001 `FormTransition` | modelo, HUD, stats e replay sincronizados |
| P0 | CHAR-001 Goku Criança | 5 skills e 3 assets visuais-placeholder |
| P0 | CHAR-002 Goku Super | múltiplas formas com assets-placeholder |
| P0 | CHAR-003 Cell | progressão em 3 formas |
| P1 | UI-001 HUD de combate | HP, Ki, XP, skills e tooltips |
| P1 | MAP-001 rota com duas torres | alvo correto e condição de destruição |
| P1 | ECON-001 XP/last hit/Zeni | saldo e compra validados |
| P1 | BOT-001 bot de rota | movimenta, ataca, recua, não trapaça visão |
| P1 | QA-001 pipeline unit/integration | smoke test por PR |
| P1 | BUILD-001 empacotar client/servidor | script reproduzível e versão |
| P1 | LEG-001 catálogo de origem de assets | bloquear material não autorizado |

## 28.4 Pessoas e funções necessárias

Fase inicial possível com engenheiro generalista C#/Unity, designer técnico e artista 3D/animador por demanda; exige liderança rigorosa de escopo. Para produção de 15 campeões e online persistente: engenharia de gameplay, rede/backend, UI/UX, arte 3D, animação e VFX, áudio, QA, infraestrutura/DevOps, balanceamento, suporte/comunidade e jurídico de IP. Uma pessoa usando IA pode acelerar documentação, scripts, testes e protótipos, mas a integração e a responsabilidade operacional permanecem humanas.

# 29. Inventário final de sistemas e cobertura de equivalências MOBA

A tabela liga mecanismos típicos de um MOBA de três rotas à especificação. Equivalência não significa copiar expressão artística, números ou um patch específico do LoL; o objetivo é cobrir sua função estratégica.

| Família de mecânica | Itens incluídos | Seção | Situação |
|---|---|---:|---|
| Entrada/instalação | installer, launcher, patch, integrity, safe mode, logout, exit | 2–4 | Especificado |
| Conta/client | auth, perfil, config, presença, social, lobby, notificações | 4–5 | Especificado |
| Pré-partida | fila, aceite, draft, campeão, loadout, skins, loading | 6 | Especificado |
| Comandos | movimento, ataque, QWER, DF, itens, recall, camera, chat | 7 | Especificado |
| Mundo | top/mid/bot, rio, selva, bases, paredes, bushes | 8 | Especificado |
| Estruturas | torres, placas, geradores, núcleo e fonte | 8 | Especificado |
| Farm/ondas | melee/ranged/siege/super, CS, XP, ouro, aggro | 8/12 | Especificado |
| Neutros | camps, buffs, patrulheiro, larvas, dinossauro, guardiões, chefes | 9 | Especificado |
| Informação | fog, wards, plantas, sensores, pings, recall | 7/10 | Especificado |
| Atributos | HP, Ki, ataque, resist., critical, haste, regen, sustain | 11 | Especificado |
| CC/mecânicas | stun, silence, roots, knockups, heals, shields, dash, invisibility | 11 | Especificado |
| Recompensas | kill/assist, shutdown, farm, objectives, XP, ouro | 11–12 | Especificado |
| Progressão | níveis 1–18, marco de forma, treinamento, skills | 12–14 | Especificado |
| Estratégia | wave control, roaming, gank, split, dives, teamwork | 8–10 | Especificado |
| Inventário | components, recipes, actives, consumables, boots, vision | 16 | Especificado |
| Preparação | runas, árvore principal/secundária, técnicas universais | 17 | Especificado |
| Social/modos | bots, normal, custom, ranqueado, surrender, remakes | 5/18 | Fases mapeadas |
| Especial DB | Ki, formas, Oozaru, voo, fusão coop | 12–15 | Especificado |
| Pós-partida | animação final, stats, history, recompensas, reconnect | 19 | Especificado |
| Engenharia | net, backend, DB, CI/CD, observabilidade, segurança | 20–27 | Especificado |
| Conteúdo | modelagem, rigs, skins por forma, VFX, licença | 13/24/25 | Especificado |

**Assuntos deliberadamente não congelados:** valores finais de stats/dano/itens; tabelas completas de XP por nível; cada waypoint/colisor do mapa; arte final; contratos de transporte após benchmark; preços e marcas de provedores; ranking/temporadas; design de skins comerciais; tolerâncias por hardware. São entregáveis técnicos de Fases 0–5, não omissões acidentais: exigir medidas ou licenças antes de fixá-los reduz retrabalho.

# 30. Apêndices de implementação e exemplos

## 30.1 Esquema ilustrativo de transformação (pseudodado)

```yaml
champion_variant_id: GKU_SUPER
forms:
  - id: BASE
    unlock_level: 1
    model: characters/goku_super/base
    portrait: ui/portraits/gku_super_base
    ki_regen_multiplier: 1.0
  - id: GOD
    unlock_level: 6
    model: characters/goku_super/god
    portrait: ui/portraits/gku_super_god
    ki_regen_multiplier: 1.10
  - id: BLUE
    unlock_level: 10
    model: characters/goku_super/blue
    portrait: ui/portraits/gku_super_blue
    ki_regen_multiplier: 1.05
  - id: BLUE_KAIOKEN
    unlock_level: 13
    model: characters/goku_super/blue_kaioken
    temporary_amplifier: true
  - id: UI_SIGN
    unlock_level: 16
    model: characters/goku_super/ui_sign
    dodge_charge_limit: 1
  - id: UI_MASTERED
    unlock_level: 18
    model: characters/goku_super/ui_mastered
    dodge_charge_limit: 2
```

Os atributos numéricos serão preenchidos sem permitir crescimento multiplicativo descontrolado; `unlock_level` não implica invulnerabilidade no mesmo tick. O campo `temporary_amplifier` significa que a forma pode conter modo ativável (Kaioken), em vez de estado permanentemente superior.

## 30.2 Exemplo de comando, evento e atualização (contrato)

```text
ClientCommand {
  sessionId, matchId, playerSlot, clientSequence, predictedTick,
  command: CastAbility { slot: 'Q', aimDirection: (x,y) }
}
Server validates session ownership, alive, cooldown, Ki, CC, cast range.
ServerEvent AbilityCastStarted { castId, entityId, slot, authoritativeTick }
ServerEvent AbilityHitResolved { castId, targetId, damage, shield, cc }
SnapshotDelta { tick, entityStatesVisibleToTeam, updatedCooldowns, eventRefs }
```

O client nunca envia `damage=99999`, `gold=...` ou `newForm=...` como estado aceito. Replays são baseados em versões de catálogo e eventos/snapshots reproduzíveis; versões incompatíveis exigem conversão ou recusa explícita.

## 30.3 Diagrama simples de estados de partida

```text
CREATED --> ALLOCATING --> WAITING_FOR_CLIENTS --> PRE_GAME --> IN_PROGRESS
                                                |             |
                                                |             +--> PAUSED [modo autorizado]
                                                |                       |
                                                |                       +--> IN_PROGRESS
                                                +--> CANCELLED
IN_PROGRESS --> ENDING --> RESULT_RECORDED --> ARCHIVED
                      \--> RESULT_RETRY_PENDING --> RESULT_RECORDED
```

Invariantes: não comprar antes de `PRE_GAME` autorizado; não aplicar dano após `ENDING`; não gravar dois resultados para `match_id`; não criar duas entidades fusionadas com os mesmos pais; não alterar forma sem evento de estado registrado; não gerar XP de treino com C inativo.

## 30.4 Sequência de entregas para desenvolver sem contradições

1. Fixar domínio de regras e nomenclatura, mapas/ids e `GameData`.
2. Validar movimento e rede com dois usuários antes de integrar 15 personagens.
3. Implementar Ki, XP e transformação em três personagens testáveis.
4. Construir partida completa contra bots sem depender de login remoto.
5. Adicionar backend social, draft, loading e pós-jogo.
6. Estender roster, objetivos e economia com testes de regressão.
7. Auditar arte/licenças, implementar reconnect e telemetria antes de distribuição.
8. Tratar fusão e ranking apenas quando núcleo competitivo estiver estável.

# 31. Fontes e referências verificadas (15/10/2026)

1. **Unity 6 — LTS e suporte:** https://unity.com/releases/unity-6/support — Unity 6.3 LTS e janela de suporte até dez/2027.
2. **MOBA CSharp Unity (yasgamesdev):** https://github.com/yasgamesdev/MOBA_CSharp_Unity — framework MIT; servidor C# desacoplado do Unity; componentes de visão, bush, skills, itens, dependências declaradas e Unity 2018.
3. **Unity 6.3 LTS release:** https://unity.com/blog/unity-6-3-lts-is-now-available — versão de LTS e suporte.
4. **Política de suporte .NET:** https://dotnet.microsoft.com/pt-br/platform/support/policy — .NET 10 LTS até 14/11/2028.
5. **Política de propriedade intelectual da Riot Games:** https://www.riotgames.com/pt-br/juridico — restrições de uso de IP em jogos/apps e de IP de terceiros.
6. **UNION OpenSource MOBA:** https://github.com/tammukul/UNION-OpenSource-MOBA — Apache 2.0, demo Android, Photon PUN/PlayFab, suporte experimental para Unity Editor.
7. **Unity Netcode for GameObjects:** https://github.com/Unity-Technologies/com.unity.netcode.gameobjects — alternativa oficial ao transporte/customização do framework legado.
8. **Vivox SDK (comunicação):** https://docs.unity.com/en-us/vivox-unity — opção a avaliar para voz/texto e ferramentas de moderação.

**Nota sobre fontes:** fontes de repositórios e fornecedores descrevem suas capacidades, mas não certificam que o jogo já compile no hardware atual ou que cada dependência esteja livre de restrições. Todas precisam de verificação local no spike da Fase 0. As regras de personagens, preços e transformações deste documento são propostas originais de produto, não fatos atribuídos às fontes.

# 32. Histórico, governança e próximos passos

**Versão 1.0 — 15/10/2026.** Consolidado briefing de MOBA 5v5 com universo de Dragon Ball, versões por saga, 15 personagens, Ki com treino de XP, 3 rotas, mapa, selva, itens, escola de treino, fusão cooperativa, assets transformáveis, jornada do client e arquitetura C#/Unity. Status de lançamento: **pré-produção / proposta técnica**, nenhum código auditado ou protótipo compilado por esta especificação.

**Próximo gate de decisão:** abrir `ADR-001` e `AUD-001`, executar o projeto open source escolhido, medir custo de portar cliente/servidor e validar legalidade de assets. Em paralelo, transformar seções 11–17 em schemas de dados e tarefas de testes. Só depois definir cronograma e investimento confiáveis.

**Critério para atualizar o documento:** mudanças de regra exigem identificação de dependências, versão, responsável, causa, resultado de testes e registro no changelog. Uma modificação de Ki que altere XP afeta missão de função, animação, HUD, itens, bots, telemetria e balanceamento; revisão dessas áreas é mandatória. O mesmo vale para transformações, fusão e mobilidade aérea.


---

# 33. Operação Work + VS Code + Codex — economia de contexto e início da implementação

**Adicionado na versão 1.1 — 15/10/2026.** Esta seção complementa o planejamento do capítulo 28, convertendo a especificação em um método de execução com baixo retrabalho e consumo controlado do plano ChatGPT Plus. Não significa que o jogo já compile.

## 33.1 Ferramentas e responsabilidades

- **VS Code + Codex (principal):** editar código local, criar testes, rodar compilação do módulo e entregar cada tarefa pequena com evidência.
- **ChatGPT Work (opcional):** auditoria delimitada, estudo de alternativas e análise de arquivos, nunca concorrente na mesma árvore de trabalho que o Codex. A cota de Work e Codex pode ser compartilhada no Plus; confirmar em Configurações > Uso, não presumir tokens disponíveis.
- **Git/GitHub (persistência):** repositório privado, commit por unidade de trabalho, backlog e relatório curto em Markdown. Evita reexplicar a história em novas sessões.
- **Unity Editor:** criação do cliente 3D após escolha da versão/estratégia, não obrigatório na primeira auditoria.

## 33.2 Unidade ótima de trabalho

Uma tarefa possui ID, dependências, objetivo observável, arquivos envolvidos, limites, testes e critério de aceite. **Uma tarefa não corresponde necessariamente a um arquivo:** movimentação e seu teste podem exigir mais de um arquivo. Usualmente, pequenas mudanças de 1–5 arquivos são mais fáceis de revisar; não dividir artificialmente uma funcionalidade que precisa passar testes em conjunto.

A cada execução: (1) selecionar o ticket; (2) ler AGENTS + tarefa ativa + status; (3) consultar apenas seção relevante do GDD e arquivos-alvo; (4) implementar; (5) rodar teste focal; (6) revisar diff; (7) registrar handoff; (8) encerrar. Nunca ordenar 'implemente tudo'.

## 33.3 Estrutura do starter de desenvolvimento

```text
AGENTS.md                  # política curta auto-lida pelo Codex quando aplicável
README.md                  # como começar
.github/workflows/         # validação barata de documentação
docs/GDD_MASTER.md         # este documento completo
docs/START_HERE.md          # passo a passo Windows
docs/CODEX_WORKFLOW.md      # orientações de consumo de contexto
docs/ARCHITECTURE_GUIDE.md # arquitetura e gates
docs/PROJECT_STATUS.md     # apenas último estado verificado
docs/ADR/                  # decisões técnicas justificadas
docs/audits/               # relatórios de compatibilidade
prompts/                   # comandos curtos e reproduzíveis
tasks/BACKLOG.md           # dependências e aceite
tasks/ACTIVE_TASK.md       # ÚNICA tarefa ativa
legal/ASSET_REGISTER.csv   # licença/origem de código, 3D, áudio e efeitos
scripts/verify_workspace.py
src/                       # nasce após auditoria, não fingir build
external/review/           # clone de terceiros ignorado pelo Git
content/                   # placeholders e assets aprovados
```

## 33.4 Preparação prática

No Windows, instalar Git e VS Code, conectar a extensão oficial Codex à conta ChatGPT e abrir **a pasta raiz** do pacote. Para AUD-001, Unity Hub e .NET SDK são opcionais até confirmar o ambiente e a necessidade. Fazer `git init`, `git add .` e commit inicial é recomendado após revisão. Verificar o pacote com `python scripts/verify_workspace.py` se houver Python.

Usar `prompts/01_FIRST_TASK.md`: auditoria de `https://github.com/yasgamesdev/MOBA_CSharp_Unity`, com clone isolado e registro de SHA/licença, builds tentados e relatório de viabilidade. **Não** migrar código no mesmo ticket e **não** instalar Unity legado sem aprovação.

## 33.5 Decisão A/B sobre o código aberto

Opção A: portar subsistemas do framework MIT após evidência de compilação e compatibilidade. Opção B: iniciar vertical slice própria para movimento/combate/Ki, reutilizando somente componentes viáveis e autorizados. Decisão deve considerar custo de manutenção, não apenas velocidade de primeiro demo. Escolha documentada em `ADR-001`, após `AUD-001`.

## 33.6 Política de modelos, skins e transformações

Todo arquivo incorporado requer proveniência, autor, direitos de redistribuição e observação se contém IP de terceiros. Mesmo skins gratuitas de mod não autorizam automaticamente usar personagens de Dragon Ball nem recursos do LoL. Para o primeiro teste, criar placeholder próprio com variação de forma, cor e aura. Evolução deve usar identificadores distintos de forma, estados replicados do servidor e ligação ao asset autorizado.

## 33.7 Checklist de entrega de ticket

- O comportamento foi testado ou existe bloqueio específico com reprodução.
- O diff não modifica módulos fora do escopo e não contém segredos.
- Os testes são determinísticos quando aplicáveis; dizer expressamente quando não executados.
- Backlog/status refletem o resultado real e próximo passo; nenhum relato de 'finalizado' sem evidência.
- Mudanças de arquitetura recebem uma ADR; dependências têm versões e licenças rastreadas.

## 33.8 Recomendação de início

Começar pela auditoria do repositório MOBA existente, não pelo launcher ou pelas 15 fichas completas. Após decidir a base, entregar um loop local mínimo e testável: entrada de movimento, um personagem placeholder, Ki carregando com C, habilidade, oponente estático e transformação visual por dados. Somente depois construir torre, tropas, loja e sistemas de rede/conta conforme dependências.

## 33.9 Riscos e expectativas de uso

Redução de contexto diminui consultas redundantes, mas não elimina limites do plano nem garante custo fixo. Nem Work nem Codex devem ser considerados processos de desenvolvimento autônomos contínuos sem gatilho real, tarefa e comprovação de execução. Cotas, modelos e regras de uso variam e precisam ser consultados no painel vigente. O histórico real permanece no Git, não na memória da conversa.

**Arquivos operacionais incluídos:** `README.md`, `AGENTS.md`, `docs/CODEX_WORKFLOW.md`, `docs/ARCHITECTURE_GUIDE.md`, `docs/PROJECT_STATUS.md`, `tasks/BACKLOG.md`, `tasks/ACTIVE_TASK.md`, `prompts/01_FIRST_TASK.md`, `prompts/02_RESUME_TASK.md` e `legal/ASSET_REGISTER.csv`. Estes arquivos fazem parte da entrega v1.1 e são a fonte de verdade executável para o Codex.
