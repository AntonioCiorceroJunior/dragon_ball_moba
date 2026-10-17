# Auditoria de prontidão documental — Dragon Ball MOBA

**Responsável:** ChatGPT · **Data:** 17/10/2026 · **Tipo:** gate documental e revisão de integração.  
**Baseline conferido:** main após PR #3, commit `b026a2648f41b88a6606660fe8ef6dac372103fa`.  
**Veredicto:** **DOCUMENTAÇÃO APTA PARA INICIAR VALIDAÇÃO DA DBM-002**, sem bloqueios estruturais de documentação. **NÃO** é aprovação do gameplay da DBM-002, que permanece em PR aberto e exige Unity/Windows.

## 1. Escopo efetivamente auditado

- Árvore GitHub `main` completa: consulta de árvore, **487 arquivos**; retorno não truncado.
- Hierarquia principal: `README.md`, `AGENTS.md`, `docs/README.md`, `START_HERE.md`, `STATUS_ATUAL.md`, `PLANO_MESTRE_ATUAL.md`, `DOCUMENTOS_FONTE_E_PRECEDENCIA.md`, `docs/coordination/TO_CODEX.md`, `TASK_QUEUE.md`, `README.md`, `FROM_CODEX.md`, `PROJECT_STATUS.md`, `DECISIONS.md`, `docs/research/README.md` e `docs/specifications/README.md`.
- **Verificação de links relativos** em **15 documentos selecionados**: oito documentos principais/coordenação (**51 links**) e sete documentos de pesquisa/especificações (**44 links**) — **95 links locais, nenhum destino quebrado na árvore principal**. Não equivale a ter executado crawler HTTP, conferido âncoras ou auditado todos os textos históricos.
- GDD mestre, os sete dossiês `docs/research/01_...`–`07_...`, o blueprint preliminar, os status e as evidências anteriores estão versionados e acessíveis.
- Presença de ferramentas e projeto: `scripts/validate_unity.ps1` e `scripts/verify_publication.ps1`; `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity`, `Assets/Editor/DomainSelfTests.cs`, `ProjectSettings/ProjectVersion.txt`. `ProjectVersion` contém **6000.6.6f1**.
- Na branch `chatgpt/DBM-002-towers-collisions` conferidos: relatório `2026-10-16_DBM-002_chatgpt-implementation.md`, baseline `evidence/DBM-002/DOMAIN_BASELINE.csv`, testes com identificadores `TR-01..05` e `COL-01..05`. Esses testes **existem em código, não foram executados nesta auditoria**. O `verify_publication.ps1` da própria branch prioriza baseline DBM-002.
- Estado GitHub: PR #3 de documentação **mesclado**; PR #2 **aberto/draft, não mesclado**. A tarefa ativa em `docs/coordination/TO_CODEX.md` é exatamente **DBM-002**, com instrução de validar, corrigir e reportar, sem merge automático.

## 2. Hierarquia considerada apta

| Pergunta | Documento / artefato de autoridade |
|---|---|
| Estado implementado | branch/commit e testes correspondentes; `docs/STATUS_ATUAL.md` apenas como snapshot |
| Produto futuro | `docs/specifications/GAME_DESIGN.md` |
| Arquitetura e ordem proposta | `docs/PLANO_MESTRE_ATUAL.md` |
| Por que a proposta faz sentido | `docs/research/` e limites de evidência |
| Uma única ação permitida ao Codex | `AGENTS.md` + `docs/coordination/TO_CODEX.md` |
| Resultados da última execução Codex | `docs/coordination/FROM_CODEX.md` + `PROJECT_STATUS.md` + `reports/` |
| Divergência entre documentos | `docs/DOCUMENTOS_FONTE_E_PRECEDENCIA.md` e `coordination/DECISIONS.md` |

## 3. Riscos restantes, sem bloqueio da DBM-002

1. **Handoffs da DBM-001 são históricos:** `FROM_CODEX.md` e `coordination/PROJECT_STATUS.md` indicam “sem merge” porque precedem a integração. **Não** devem sobrescrever a informação atual: PR #1 já foi mesclado. Ao iniciar, ler `TO_CODEX.md` de main e o estado GitHub.
2. **Especificação ≠ parâmetros definitivos:** GDD propõe Goku Criança com 660 HP; código da main tem 570. Blueprint sugere crescimento alternativo. A divergência está registrada para decisão futura em DBM-003A; **não impede testar torres/colisões na DBM-002**.
3. **Blueprint preliminar e dossiês sobrepõem conceitos:** índices explicam precedência, mas literatura não foi totalmente reproduzida/validada. Não é necessário consumir contexto Codex lendo tudo.
4. **Nova `main` após PR #3 e branch DBM-002:** antes de validar, sincronizar com segurança e checar diferenças de base sem sobrescrever o domínio/testes ou destruir estado local. Não exigir rebase se desnecessário; não `push --force`.
5. **Testes Unity e partidas completas ainda faltam:** nenhuma verificação remota de arquivos prova compilação, UI nativa ou funcionalidade. DBM-002 só avança após gates previstos no TO_CODEX.

## 4. Liberação e sequência

**Liberado:** enviar ao Codex a tarefa existente em `docs/coordination/TO_CODEX.md` para testar e corrigir DBM-002.  
**Ainda não liberado:** declarar DBM-002 concluída, mesclar PR #2 ou começar DBM-003A/itens sem avaliar as evidências do Codex.

**Próximo gate:** Codex executa `verify_publication.ps1` e `validate_unity.ps1` nos modos `VisualPrepare`, `VisualPlay`, `VisualBuild`, confirma testes TR/COL, valida mouse/teclado no executável, regressão DBM-001 e publica relatório no PR #2. ChatGPT então revisa o merge; depois agenda DBM-003A.

**Nota de escopo:** esta é auditoria **documental/estática feita via GitHub**; nenhum comando PowerShell, Unity Editor ou executável Windows foi executado neste relatório.
