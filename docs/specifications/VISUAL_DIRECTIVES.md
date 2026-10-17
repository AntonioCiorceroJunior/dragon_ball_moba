# DRAGON BALL MOBA — DIRETRIZES DE REESTRUTURAÇÃO VISUAL E TÉCNICA

## 1. Objetivo geral

Estamos desenvolvendo um MOBA 3D inspirado nas mecânicas estratégicas de League of Legends, mas ambientado no universo de Dragon Ball.

O jogo já possui um protótipo funcional em Unity/C#, com movimentação, combate, habilidades, Ki, evolução, torres, tropas e uma arena experimental.

O objetivo agora é melhorar significativamente a qualidade visual e estrutural do projeto, aproveitando os recursos já existentes e bibliotecas de código aberto quando forem tecnicamente adequadas.

**Não quero recomeçar o projeto do zero nem perder as funcionalidades que já desenvolvemos.**

A transformação visual deverá aproximar o jogo da qualidade e legibilidade dos MOBAs comerciais, preservando identidade própria e utilizando recursos com licenças apropriadas.

## 2. Antes de modificar qualquer coisa

1. Examine a arquitetura atual do projeto.
2. Identifique o funcionamento dos scripts existentes.
3. Verifique os assets que já foram importados.
4. Identifique a versão do Unity e o render pipeline utilizado.
5. Registre a situação atual do projeto.
6. Crie um checkpoint no Git antes das alterações.
7. Verifique erros de compilação e problemas evidentes de execução.

Faça uma auditoria dos seguintes projetos:

- https://github.com/yasgamesdev/MOBA_CSharp_Unity
- https://github.com/tammukul/UNION-OpenSource-MOBA

Compare os sistemas desses projetos com os nossos.

Verifique movimentação, câmera, IA, torres, tropas, habilidades, equipamentos, visão e arquitetura de rede.

Avalie código, dependências, licenças e compatibilidade com nosso Unity.

Não migre o projeto inteiro sem demonstrar que isso é vantajoso. Reutilize módulos somente quando houver benefício técnico comprovado.

## 3. Reconstrução visual do mapa

O mapa atual é muito geométrico, com cores chapadas, caminhos retangulares, rio representado por uma faixa azul e distribuição artificial da vegetação.

Quero um mapa isométrico 3D, semelhante à organização estratégica do Summoner's Rift.

### Elementos necessários

- Três rotas claramente identificadas.
- Selva com caminhos e entradas estratégicas.
- Rio com água visualmente convincente.
- Torres integradas ao ambiente.
- Bases visualmente distintas.
- Vegetação variada e distribuída naturalmente.
- Pedras, ruínas, desníveis e outros elementos ambientais.
- Arbustos com função de ocultação.
- Áreas específicas para monstros neutros.
- Caminhos legíveis para movimentação dos personagens.

Evite quadrados e retângulos artificiais quando não forem necessários.

Use os pacotes de vegetação e ruínas já baixados. Se forem insuficientes, procure alternativas gratuitas e com licença compatível.

**Preserve as posições e regras lógicas existentes enquanto substitui a representação visual.**

Não altere colisões, caminhos e navegação sem verificar o impacto na movimentação, nas tropas e nos bots.

## 4. Câmera isométrica

A câmera atual está muito vertical e deixa os personagens pequenos.

Quero uma câmera inspirada no comportamento do LoL:

- Projeção isométrica ou ortográfica inclinada.
- Ângulo que permita visualizar altura e profundidade.
- Distância adequada para visualizar personagem e área próxima.
- Acompanhamento suave do personagem.
- Zoom com limites configuráveis.
- Possibilidade de câmera livre.
- Tecla para centralizar no personagem.
- Limites para impedir que a câmera saia do mapa.

O tamanho do personagem deve permitir identificar claramente sua aparência, animações e transformação.

Configure e teste a interface em 1920×1080 e 1280×720.

## 5. Personagens 3D

Os modelos atuais são provisórios e precisam melhorar.

Comece pelos três personagens existentes:

1. Goku Criança.
2. Goku Super.
3. Cell.

Cada personagem deverá possuir:

- Modelo 3D humanoide com silhueta identificável.
- Rig e Animator configurados.
- Idle.
- Caminhada.
- Corrida.
- Ataque básico.
- Preparação de habilidade.
- Recebimento de dano.
- Morte.
- Carregamento de Ki.
- Transformação.
- Indicadores visuais de seleção.

Utilize os modelos e animações já disponíveis no projeto. Se não forem suficientes, pesquise alternativas reutilizáveis com licença adequada.

Não utilize modelos ou materiais extraídos de jogos comerciais sem autorização.

### Transformações visuais

Cada transformação deverá possuir aparência diferente.

Para Goku Super, preparar a arquitetura para:

- Base.
- Super Saiyajin God.
- Super Saiyajin Blue.
- Blue com Kaioken.
- Instinto Superior Incompleto.
- Instinto Superior Completo.

Cada forma deve poder alterar cabelo, materiais, aura, postura e efeitos de habilidade.

O sistema deve suportar diferentes modelos ou variantes visuais, sem exigir alterações no núcleo do combate.

As transformações continuarão sendo ativadas pelas regras de nível existentes.

## 6. Efeitos visuais e combate

Precisamos substituir os efeitos simples por efeitos de combate com identidade visual inspirada nas técnicas de Dragon Ball.

Prioridades:

- Aura de Ki ao carregar energia.
- Projéteis de energia.
- Kamehameha com preparação, disparo e impacto.
- Explosões de Ki.
- Barreiras de energia.
- Efeitos de deslocamento.
- Impactos de golpes físicos.
- Animações de transformação.
- Efeitos de eliminação.

Utilize partículas, shaders e efeitos compatíveis com o render pipeline do projeto.

Os efeitos precisam ser visualmente interessantes, mas sem prejudicar a leitura estratégica da partida nem a taxa de quadros.

O carregamento de Ki deve ficar claramente visível, mostrando que o personagem está parado e vulnerável.

## 7. Interface — reconstrução da HUD

A interface atual contém caixas grandes que ocupam excessivamente a tela.

Quero uma interface moderna de MOBA, compacta e organizada.

### Layout proposto

**Parte inferior central:**
- Retrato do personagem.
- Vida.
- Ki.
- Experiência.
- Habilidades Q, W, E, R.
- Tempos de recarga.
- Indicadores de transformação.
- Itens equipados.

**Canto inferior direito:**
- Minimapa.
- Torres e objetivos.
- Identificação de aliados e inimigos visíveis.

**Parte superior:**
- Placar.
- Tempo de partida.
- Eliminações.
- Objetivos importantes.

**Menus adicionais:**
- Loja de equipamentos.
- Placar completo.
- Informações detalhadas do personagem.
- Menu de configurações.
- Tela de ajuda.

Os controles de ajuda e textos de depuração não devem permanecer ocupando grandes áreas durante a partida. Coloque-os em um painel opcional.

Utilize os assets de interface já disponíveis ou crie componentes próprios.

A interface precisa atualizar corretamente quando o personagem recebe dano, recupera Ki, sobe de nível, transforma-se, compra itens ou morre.

## 8. Estruturas e torres

As torres atuais parecem objetos geométricos simples.

Quero torres de energia inspiradas na tecnologia do universo Dragon Ball, mantendo identidade visual original.

Cada torre deverá possuir:

- Modelo 3D detalhado.
- Base e corpo proporcionais.
- Identificação de equipe.
- Cristal ou núcleo energético.
- Animação de ataque.
- Projétil de energia.
- Efeito visual de dano.
- Animação de destruição.
- Barra de vida legível.

Preserve sua lógica de aquisição de alvos, dano, alcance e destruição.

## 9. Tropas e movimentação

Os minions precisam ser representados por pequenos robôs de combate.

Teremos unidades corpo a corpo, unidades à distância e unidades de cerco.

Crie animações adequadas para caminhada, ataque, dano e morte.

Verifique:

- Navegação pelas rotas.
- Colisões.
- Prioridade de alvos.
- Ataques contra torres.
- Alinhamento visual das tropas.
- Sincronização entre animação e dano.

Não substitua a lógica existente sem necessidade.

## 10. Correções funcionais

Na captura atual, o personagem Cell aparece com 0/736 HP.

Investigue se existe algum problema relacionado a morte, atualização da interface, respawn ou permanência do personagem na arena.

Verifique também:

- Barras de vida e Ki.
- Indicadores de experiência.
- Tempos de recarga.
- Transformações.
- Carregamento de Ki.
- Aquisição de alvos.
- Ataques básicos.
- Funcionamento da loja.
- Destruição de torres.

Corrija problemas reproduzíveis, preservando o comportamento esperado dos sistemas.

## 11. Organização da implementação

Trabalhe em etapas pequenas e verificáveis para economizar tokens.

**Etapa 1 — Auditoria e correções**
Analisar o projeto, possíveis bibliotecas MOBA e erros existentes.

**Etapa 2 — Câmera e escala**
Melhorar enquadramento, controles da câmera e proporções.

**Etapa 3 — Mapa**
Melhorar terreno, caminhos, rio, vegetação e estruturas.

**Etapa 4 — Personagens**
Integrar modelos e animações de Goku Criança, Goku Super e Cell.

**Etapa 5 — Transformações**
Adicionar variantes de aparência, auras e animações.

**Etapa 6 — Combate visual**
Adicionar partículas e efeitos de habilidades.

**Etapa 7 — HUD**
Substituir os painéis provisórios por interface moderna.

**Etapa 8 — Validação**
Compilar, executar e verificar se as funcionalidades originais continuam operando corretamente.

Não avance para uma etapa complexa se a anterior estiver quebrada.

## 12. Regras para economizar tokens do Codex

- Leia apenas os arquivos necessários para cada tarefa.
- Consulte o AGENTS.md e os documentos relevantes.
- Não releia toda a documentação a cada execução.
- Não reescreva arquivos inteiros quando uma alteração localizada resolver.
- Não explique novamente a arquitetura já documentada.
- Evite refatorações sem benefício demonstrável.
- Evite downloads repetidos.
- Reutilize assets existentes.
- Faça testes direcionados a cada alteração.
- Registre o progresso em PROJECT_STATUS.md.
- Registre problemas pendentes e próxima tarefa.
- Mantenha um histórico sucinto das decisões técnicas.

Ao concluir cada etapa, informe:

1. Arquivos modificados.
2. Funcionalidades implementadas.
3. Testes executados e resultados.
4. Problemas pendentes.
5. Próxima tarefa recomendada.

Não afirme que algo funciona sem realmente testar.

## 13. Resultado esperado

Quero que o protótipo evolua visualmente para um MOBA 3D com:

- Câmera isométrica apropriada.
- Mapa detalhado e legível.
- Personagens 3D animados.
- Transformações com aparências diferentes.
- Auras e habilidades de energia.
- Torres e tropas estilizadas.
- Interface organizada e profissional.
- Mecânicas existentes preservadas.

O objetivo desta fase não é desenvolver os 15 personagens nem implementar o multiplayer completo.

**O objetivo é construir uma primeira versão visualmente convincente e tecnicamente estável**, que sirva como referência de qualidade para todas as próximas etapas.

Comece pela auditoria, crie um checkpoint do projeto e avance pela primeira etapa executável. Não interrompa a execução para pedir autorização em mudanças pequenas e reversíveis dentro do projeto. Solicite aprovação antes de operações destrutivas, migrações amplas ou instalação de dependências com restrições relevantes.