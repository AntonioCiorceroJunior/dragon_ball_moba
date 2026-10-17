# Arquitetura — guia de execução

Este documento orienta as decisões de implementação. O **GDD capítulos 20–23** é a especificação detalhada.

## Stack proposta, condicionada à auditoria

| Domínio | Preferência | Evidência necessária |
|---|---|---|
| Cliente | Unity 6.3 LTS + C# + URP | Compatibilidade de projeto e importação de assets |
| UI | UI Toolkit no client; uGUI no HUD se necessário | Protótipo e custo de renderização |
| Domínio | C# puro e testes determinísticos | Build local sem UnityEngine |
| Simulação | Servidor dedicado autoritativo, tick fixo | Duas conexões, correção, carga e determinismo |
| Transporte | ENet-CSharp ou alternativa auditada | Segurança, manutenção e perfis de loss/latência |
| Serialização | MessagePack ou Protobuf validado | Versionamento e tamanho dos pacotes |
| Backend | ASP.NET Core/.NET 10 LTS | Contratos auth, matchmaking, inventário, histórico |
| Dados | PostgreSQL; Redis apenas se necessário | Integridade transacional e necessidades reais |
| Operação | GitHub Actions + Docker Compose quando útil | Pipeline mínimo, secrets fora do repo |

## Módulos e dependências

```text
GameData ----> Domain <---- Simulation <---- Dedicated Server
                       ^         |
                       |         v
Client (Unity) ----> Protocol <---- Server transport
     |
     v
Presentation / UI / VFX
Backend APIs -> Contracts -> Postgres [Redis opcional]
```

`Domain` deve ser compilável/testável sem Unity, I/O nem rede; cliente nunca envia dano aplicado, XP concedida ou vitória. Envia intenção. O servidor valida, simula e replica estado. UI pode predizer feedback não autoritativo, mas sempre aceita correções.

## Gate A/B — base pronta ou nova base

- **A: MOBA_CSharp_Unity:** testar clonagem e SHA, versão Unity original (2018.3.4f1), soluções C#, deps, transporte, licença MIT, compilação do server/client, mapa, herói, skills, net, qualidade do código e riscos.
- **B: vertical slice própria mínima:** planejar movimento em 1 rota + 2 personagens placeholder + ataques validados pelo servidor sem login social. Produzir esforço relativo.
- **Critério:** escolher caminho que minimiza custo **futuro** de manutenção e adaptação, não quantidade de arquivos prontos. Não portar todo o legado só porque existe.
- **Restrição:** projetos Unity antigos podem conter APIs e pacotes obsoletos; separar auditoria do código de produção. Nunca inserir arquivos externos em `src` antes de aprovação de licença/segurança e ADR registrada.

## Sequência técnica mínima

1. Fixar upstream, auditar e decidir ADR.
2. Domínio C# independente e testes da regra de Ki.
3. Jogo de treino local com uma rota e dois placeholders, sem 5v5.
4. Simulação de servidor e comandos de movimento/ataque.
5. Transformações com dados e replicação, validação por teste.
6. Economia, tropas, torres, objetivos, 3 rotas.
7. Expansão dos personagens, lobby e multiplayer 5v5.

## Portas de qualidade

- **G1:** `Domain` testável sem Unity.
- **G2:** server único decisor do combate e Ki/XP.
- **G3:** gameplay reprodutível; snapshots e replays sem divergência relevante.
- **G4:** importação de forma reversível e registrável.
- **G5:** projeto pode ser compilado em máquina limpa seguindo README.
- **G6:** licença conhecida para cada asset incorporado.

## Organização futura após AUD-001

```text
src/
  client-unity/    (projeto Unity real, criado após decisão)
  server/          (host do servidor dedicado)
  domain/          (regras puras e testes)
  protocol/        (schemas e serialização)
  backend/         (conta/lobby/matchmaking)
  tools/           (validações e importadores)
content/
  characters/      (definições declarativas e skins licenciadas)
  maps/            (dados de navegação e cenários)
```

Não criar soluções falsas apenas para preencher diretórios. O `src/` deste starter contém somente orientações.
