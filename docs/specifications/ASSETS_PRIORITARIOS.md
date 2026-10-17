# Checklist de downloads e destinos — Dragon Ball MOBA

Data da curadoria: 15/10/2026. Links apontam para páginas originais; podem exigir conta/aceite de licença. **Baixe no computador, não copie os ZIPs diretamente para `Assets/`.** A pasta de staging fica fora de `Assets`, no projeto Unity em `ExternalAssets/Staging/` (ou em outra pasta externa, se preferir). Não importe tudo de uma vez.

## Etapa A — ESSENCIAL (baixar agora)

| Ordem | Pacote — link oficial | O que oferece | Licença / compatibilidade | Destino de importação após triagem |
|---|---|---|---|---|
| A1 | [Quaternius Universal Base Characters](https://quaternius.com/packs/universalbasecharacters.html) | 6 personagens humanoides rigados + penteados; formas infantil/adulta | CC0; FBX/glTF; para Unity Humanoid | `Assets/_DragonMOBA/Art/Characters/Source/QuaterniusBase/` |
| A2 | [Quaternius Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html) | Movimentos 8 direções, corrida, luta, morte etc.; usar **versão Standard gratuita** | CC0; animações FBX, retargeting | `Assets/_DragonMOBA/Art/Animations/Source/Quaternius/` |
| A3 | [Quaternius Ultimate Stylized Nature](https://quaternius.com/packs/ultimatestylizednature.html) | Árvores, grama, rochas, árvores e vegetação para selva e rotas | CC0; FBX/OBJ/glTF | `Assets/_DragonMOBA/Art/Environment/Source/Nature/` |
| A4 | [Quaternius Ultimate Modular Ruins](https://quaternius.com/packs/ultimatemodularruins.html) | Paredes, ruínas, estruturas de torres e bases | CC0; FBX/OBJ | `Assets/_DragonMOBA/Art/Environment/Source/Ruins/` |
| A5 | [Kenney UI Pack – Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi) | Painéis, barras, botões para HUD de vida, Ki, menu/loja | CC0; PNG | `Assets/_DragonMOBA/Art/UI/Source/KenneySciFi/` |
| A6 | [Kenney Input Prompts](https://kenney.nl/assets/input-prompts) | Ícones de mouse, teclado, habilidades/atalhos | CC0 | `Assets/_DragonMOBA/Art/UI/Source/InputPrompts/` |
| A7 | [Kenney Sci-Fi Sounds](https://kenney.nl/assets/sci-fi-sounds) | Bases para feixes, energia e sistemas | CC0; áudio | `Assets/_DragonMOBA/Art/Audio/Source/SciFi/` |
| A8 | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | Impactos de golpe/torre/colisão | CC0; áudio | `Assets/_DragonMOBA/Art/Audio/Source/Impact/` |

A maioria dos downloads é fornecida por botão nas páginas, não por URL direta estável. **Não automatizar downloads que exijam login, aceitação de licença ou bypass de proteção**. O Codex pode processar ZIPs depois que você baixar legalmente.

## Etapa B — VFX e pós-processamento (após A1–A4 funcionando)

| Ordem | Recurso | Uso | Condição |
|---|---|---|---|
| B1 | [Magic Effects FREE — Hovl Studio](https://marketplace.unity.com/packages/vfx/particles/spells/magic-effects-free-247933) | Explosões, magia e projéteis que podem ser recoloridos como Ki | **Unity Asset Store EULA**; página declara compatibilidade URP; importar pelo Package Manager/My Assets, testar no Unity 6 |
| B2 | [Easy Impact Frames — URP](https://assetstore.unity.com/packages/vfx/shaders/easy-impact-frames-urp-355378) | Quadros estilizados de impacto, útil em técnicas especiais | Grátis, **Unity Asset Store EULA**; requer URP, validar compatibilidade Unity 6 |
| B3 | [Toon shader URP (MIT)](https://github.com/hatfullr/UnityURP-AnimeStyleCelShader) | Referência para materiais cel-shaded/outline | Licença MIT; **não importar o repositório inteiro sem auditoria**, revisar shaders e compatibilidade |
| B4 | [Poly Haven](https://polyhaven.com/) | HDRI e texturas de terreno, se necessário | CC0; preferir texturas 1K/2K e materiais consistentes |

**Atenção ao render pipeline:** se o projeto atual não for URP, não converter automaticamente. O Codex deve identificar primeiro no `ProjectSettings`/Quality/Graphics, criar branch e testar em cópia. Não usar pacotes marcados como somente Built-in em um projeto URP.

## Etapa C — opcionais após a primeira melhoria

| Recurso | Motivo / observação |
|---|---|
| [Quaternius Modular Character Outfits – Fantasy](https://quaternius.com/packs/modularcharacteroutfitsfantasy.html) | Roupas modulares compatíveis com Universal Base Characters; download gratuito Standard pode ser grande (~280MB) |
| [KayKit Adventurers](https://kaylousberg.itch.io/kaykit-adventurers) | Personagens extras rigados, CC0; estilo mais low-poly, útil para protótipos de NPCs |
| [KayKit Character Animations](https://kaylousberg.itch.io/kaykit-character-animations) | Complemento de animações específico para rig KayKit; evitar misturar rigs antes de validar retargeting |
| [Quaternius Bestiary Dungeon Monsters](https://quaternius.com/packs/bestiarydungeonmonsterskit.html) | Monstros base para criaturas de selva |
| [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds) | UI, seleção, loja e cliques |
| [Kenney Minimap Pack](https://kenney.nl/assets/minimap-pack) | Referência de componentes; tem aparência pixelada, pode não combinar com HUD final |
| [Adobe Mixamo](https://www.mixamo.com/) | Animações humanoides extras se faltar recarga de Ki, esquiva etc.; gratuito com Adobe ID, **não redistribuir arquivos brutos ao público** |

## Diretrizes técnicas de importação

1. Confirmar `Assets/` e `ProjectSettings/` no projeto **atual** (do usuário), não na pasta do ZIP antigo.
2. Registrar a versão de Unity, URP/Built-in, erros atuais do Console e FPS da cena antes de importar qualquer recurso.
3. Criar branch/backup: `art/first-visual-pass`; nenhuma mudança em código de gameplay sem justificativa/teste.
4. Importar primeiro A1 e A2. Configurar **Rig → Animation Type: Humanoid** onde aplicável, Avatar e Animator Controller próprios.
5. Usar **prefab visual como filho** do GameObject controlado pelos scripts existentes: trocar arte **não significa** trocar o controlador de Ki/combat/movement.
6. Criar uma forma funcional de um personagem (Idle, Run, Attack, Hit, Death) antes de 6 formas e antes dos 15 personagens.
7. Só então importar A3/A4 e montar cenário preservando colisão e caminhos existentes, verificando NavMesh/pathfinding.
8. Importar A5/A6 e criar HUD de Vida, Ki, XP, nível e ícones sem substituir listeners e lógica de gameplay.
9. Importar os VFX individualmente após confirmar URP; criar Ki Charge, Beam, Impact, Transform com partículas e materiais parametrizáveis.
10. Importar A7/A8; normalizar volumes, usar Audio Mixer, evitar som contínuo excessivo.
11. Validar a cada etapa: `Console = 0 errors`, gameplay funciona, fps aceitável, materiais não ficam rosa, cenas carregam, Build funciona quando disponível.

## Estrutura exata de destino

```text
<PASTA_DO_PROJETO_UNITY>/
├── Assets/
│   └── _DragonMOBA/
│       ├── Art/
│       │   ├── Characters/
│       │   │   ├── Source/QuaterniusBase/
│       │   │   ├── GokuChild/{Base,Nimbus,Oozaru}/
│       │   │   ├── GokuSuper/{Base,God,Blue,BlueKaioken,UISign,UIMastered}/
│       │   │   └── Cell/{Imperfect,SemiPerfect,Perfect}/
│       │   ├── Animations/{Source/Quaternius,Controllers,Overrides}/
│       │   ├── Environment/{Source/Nature,Source/Ruins,Materials,Prefabs}/
│       │   ├── VFX/{Ki,Beams,Impact,Transform,Source}/
│       │   ├── UI/{Source/KenneySciFi,Source/InputPrompts,HUD,Icons,Menus}/
│       │   └── Audio/{Source/SciFi,Source/Impact,SFX,Music,Mixers}/
│       ├── Prefabs/{Champions,Minions,Towers,Objectives}/
│       └── Scenes/ArtValidation/
├── ExternalAssets/
│   └── Staging/  # os downloads zip ficam aqui, FORA de Assets
└── ... (pastas pré-existentes do projeto)
```

**Para controle de versão:** não adicionar em Git os ZIPs grandes de staging nem assets cujo contrato proíba redistribuição de arquivos-fonte. O repositório privado ou público não concede por si só direito de publicar modelos de Dragon Ball. Guardar comprovantes/licenças e atribuições necessárias em `docs/art/`.

## Primeira entrega visual mensurável

- Um campeão com modelo 3D rigado e 5 estados de animação, andando e atacando sem quebrar física/controle.
- 3 skins técnicas trocadas por nível na mesma personagem (materiais/cabelo/aura, mesmo que ainda não sejam Goku oficial).
- Trecho de mapa com árvores, rochas e torre renderizada sem invadir a rota.
- Barra de Ki com feedback de carregamento + VFX visível e contraste correto.
- Screenshot antes/depois e log dos testes executados.
