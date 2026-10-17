# Direção de arte e critério de aprovação

**Meta visual:** MOBA isométrico 3D legível, inspirado em combates de anime, sem misturar dezenas de estilos de assets.

## Direção prioritária
- Personagens: proporções humanoides estilizadas, silhueta evidente vista de cima, sombras cel-shaded e contorno sutil; sem modelos hiper-realistas misturados com low-poly.
- Terreno: verde moderado, rochas terrosas, rio azul discreto, trilhas diferenciadas e bases contrastantes por cor. Evitar excesso de partículas cobrindo tropas ou habilidades.
- Câmera: preservar posição, zoom e projeção atuais até realizar testes de alcance, tamanho de personagem e leitura das habilidades.
- VFX: Ki por partículas azul/branco e tonalidades por forma; preparar `MaterialPropertyBlock` ou material instance conforme necessidade. Bloom com intensidade moderada; objetivo é ler o combate.
- UI: manter os valores do gameplay como fonte de verdade e usar bindings/eventos existentes. Barra de vida e de Ki, ícones Q/W/E/R, nível, XP, transformações e minimapa.

## Formas visuais técnicas — primeiro lote

Os modelos genéricos CC0 NÃO são equivalentes aos personagens oficiais: nomes abaixo identificam papéis do protótipo e seus estados de aparência. **Não redistribuir assets fanmade de DB sem autorização**.

| ID | Estados a preparar | Gatilhos atuais/propostos | Exigências visuais |
|---|---|---|---|
| `goku_child` | Base, Nimbus, Oozaru | Base; nv. 11; habilidade temporária nv. 16 | Tamanho e silhueta distintos; Nimbus exige prefab; Oozaru exige rig próprio mais tarde |
| `goku_super` | Base, God, Blue, BlueKaioken, UISign, UIMastered | Base; nv. 6, 10, 13, 16, 18 (**comparar com código** e GDD) | Cabelo/material/aura; partículas e retrato por forma; evitar trocar controlador de gameplay |
| `cell` | Imperfect, SemiPerfect, Perfect | Níveis conforme implementação real | Mudança de malha no futuro; protótipo pode usar silhueta/material alternativo |

**Não pressupor que o código v0.1 já tenha todos esses estados nem adicionar lógica sem ticket.** Primeiro ler os IDs/níveis atuais e criar mapeamento por dados (ScriptableObject ou equivalente). Preferir `AnimatorOverrideController` em formas com rig compartilhado; forma gigante pode ter animator separado.

## Critérios básicos de qualidade
- Personagens reconhecíveis pelo contorno a 1080p na câmera real do jogo.
- Sem sprites borrados: conferir import settings e 9-slicing das barras.
- Sem pink materials; URP e materiais funcionando.
- Nenhum VFX sustenta mais de 2s de obstrução forte da câmera.
- Efeitos temporários com pooling e limpeza ao sair de cena.
- Animações compatíveis com movimento implementado: se locomotion controlada por script, iniciar **in-place** em vez de root motion.
- Medição de FPS antes/depois em uma máquina definida. Não estabelecer metas irreais sem hardware de referência.
- Testes de gameplay repetidos (caminhar, atacar, carregar KI, evoluir, comprar, morrer, reiniciar).

## Propriedade intelectual e distribuição
CC0 cobre os packs do Quaternius e Kenney explicitamente referenciados, mas não libera marcas, personagens, músicas e modelos protegidos de Dragon Ball. Um fan model publicado sob CC0 não apaga os direitos sobre personagens e design pré-existentes. Assets do Unity Asset Store seguem o EULA próprio. Mixamo: usar em jogo compilado, não publicar pacote bruto. Documentar procedência em `ASSET_REGISTER.csv`.
