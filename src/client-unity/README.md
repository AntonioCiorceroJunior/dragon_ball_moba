# Entrada atual — DBM-000

Use Unity 6000.6.6f1 e abra Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity. Pressione Play; o mapa é gerado em execução. Built-in e Input Manager legado preservados.

[Guia atual de abertura, build e controles](../../README.md). [Estado e tarefa ativa](../../docs/coordination/PROJECT_STATUS.md). Os registros abaixo são históricos e não substituem a coordenação vigente.

---

# Atualização de arte — 16/10/2026

Abra Assets/_DragonMOBA/Scenes/ArtValidation/ArenaArt.unity e pressione Play.
Há 12 formas humanoides CC0, cenário, auras, HUD e áudio integrados.
Compilação, domínio, Play Mode e build Windows separados passaram.
Consulte [progresso](docs/art/STATUS.md), [impedimentos](docs/art/PENDENCIAS.md)
e o [guia atualizado](../../README.md).

O conteúdo abaixo documenta a configuração anterior de 15/10/2026.
A afirmação histórica de seis scripts intocados aplica-se somente àquela etapa:
PrototypeGame agora recebeu a integração visual; Domain e a cena original continuam preservados.

---
# Protótipo Unity — Dragon MOBA v0.1

**Estado (15/10/2026):** protótipo **offline** importado e compilado no Unity **6000.6.6f1**. Testes de domínio e smoke test real em Play Mode com os três personagens passaram; build Windows gerada. Partida interativa completa ainda não testada. [Guia de abertura, VS Code e evidências](../../README.md).

## Executar no Windows

1. Use o Unity Hub e o Editor **6000.6.6f1**, já instalado nesta máquina. VS Code e as extensões Unity/C# também já estão instalados.
2. No Unity Hub, **Add project from disk** e selecione a pasta `src/client-unity/` deste repositório.
3. Aguarde importar os scripts; confirme que o Console não possui erros.
4. **Input Manager (Old)** já está configurado. O protótipo utiliza `UnityEngine.Input` intencionalmente.
5. Abra [Assets/Scenes/ArenaPrototype.unity](Assets/Scenes/ArenaPrototype.unity), já gerada e incluída no Build. O menu **Dragon MOBA → Preparar e validar projeto** recria a cena somente se ela estiver ausente.
6. Pressione **Play**, escolha **Goku Criança**, **Goku Super** ou **Cell Z**.
7. Execute **Dragon MOBA → Executar testes de dominio** e verifique mensagem PASSOU no Console.

## Comandos e loop

| Entrada | Efeito |
|---|---|
| Clique direito no chão | Ir ao ponto |
| Clique direito em inimigo | Perseguir e atacar |
| Q | Rajada de Ki na área apontada |
| W | Impulso curto na direção do cursor |
| E | Barreira temporária (5 segundos) |
| R | Explosão na posição do cursor |
| C (segurar) | Carrega Ki; cheio, concede XP fora da base |
| P | Abre/fecha loja |
| H | Ajuda na tela |
| Enter (após terminar) | Reinicia partida |

Há três rotas, tropas de ambas as equipes, uma torre por rota por lado, núcleos, XP, ouro, compras limitadas à fonte, bots básicos, nível máximo 18 e formas automáticas. O núcleo só recebe dano após a destruição de todas as torres de sua equipe **neste protótipo**, uma simplificação do design final. A câmera é isométrica fixa no herói.

## Estrutura

- `Assets/Scripts/Domain/`: código C# puro (sem UnityEngine), `GameWorld`, comandos e regras; futuro servidor dedicado poderá aproveitar este domínio após validação determinística.
- `Assets/Scripts/Runtime/`: apresentação por primitivas 3D, câmera, interação e HUD provisório `OnGUI`.
- `Assets/Editor/`: gerador da cena, smoke tests sem dependências externas.

## Limites conhecidos (não confundir com jogo pronto)

- Simulação **local**, não rede/servidor dedicado; bots enxergam unidades sem névoa de guerra; não existe multiplayer 5v5.
- Três versões provisórias de personagens com quatro habilidades genéricas, não os kits oficiais finais.
- Personagens representados por primitivas geométricas; nenhuma skin/modelo de Dragon Ball ou de LoL foi incluído.
- Falta pathfinding com obstáculos, alcance de visão, loja avançada, histórico, anti-cheat, dano contínuo e animações de produção.
- Valores numéricos são de teste; faltam testes reais de equilíbrio, estabilidade e desempenho.
- Não usar o protótipo como servidor em rede sem mover a autoridade para servidor dedicado e revisar segurança/reprodução.

## Próximo ticket recomendado

`AUD-001`: auditar os projetos MOBA de código aberto já identificados para decidir portar partes deles ou manter o protótipo novo. Se reaproveitar código, verificar licença, versão Unity, dependências, conflitos de domínio e direitos de redistribuição.
