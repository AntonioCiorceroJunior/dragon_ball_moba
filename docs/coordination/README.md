# Protocolo de coordenação — ChatGPT ↔ Codex

## Objetivo
O GitHub é a fonte compartilhada de verdade para tarefas, decisões, progresso, código e evidências do Dragon Ball MOBA.

**Não é um chat em tempo real.** ChatGPT publica as instruções; o usuário aciona o Codex no VS Code; o Codex executa, testa, faz commit/push e publica o relatório; o ChatGPT revisa quando solicitado. Nenhum dos dois trabalha continuamente sem uma execução iniciada.

## Arquivos e responsáveis

| Arquivo | Responsável | Objetivo |
|---|---|---|
| `TO_CODEX.md` | ChatGPT | Única tarefa ativa, escopo, aceitação e restrições |
| `FROM_CODEX.md` | Codex | Resumo mais recente, resultado e impedimentos |
| `PROJECT_STATUS.md` | Codex | Estado técnico atual e próxima etapa |
| `TASK_QUEUE.md` | ChatGPT | Backlog e ordem de execução |
| `DECISIONS.md` | ChatGPT + Codex (propostas) | Decisões e justificativas |
| `REPORT_TEMPLATE.md` | ChatGPT | Contrato mínimo de cada relatório |
| `reports/YYYY-MM-DD_DBM-NNN_<tema>.md` | Codex | Relatório imutável por entrega |

## Ciclo de trabalho
1. ChatGPT revisa o último `FROM_CODEX.md`, `PROJECT_STATUS.md`, diffs/commits e relatório.
2. ChatGPT publica nova tarefa em `TO_CODEX.md`, referenciando um ID do backlog (DBM-NNN).
3. Usuário envia ao Codex no VS Code: "Sincronize o repositório, leia AGENTS.md e docs/coordination/TO_CODEX.md e execute somente a tarefa ativa".
4. Codex cria branch/checkpoint quando necessário, faz alterações mínimas, testa e documenta.
5. Codex faz commit/push, cria `reports/`, atualiza `FROM_CODEX.md` e `PROJECT_STATUS.md`; indica commit e resultados.
6. ChatGPT revisa a entrega, aprova ou retorna ajustes.

## Regras de evidência
- Distinguir: **implementado**, **compila**, **testado em Play Mode**, **testado no executável**, **não comprovado**.
- Cada entrega lista caminhos relativos, comandos utilizados, resultados, erros e comportamento esperado/observado.
- Se o GitHub não estiver acessível, salvar relatório local e declarar que falta publicar, sem afirmar sucesso remoto.
- Se surgir conflito entre arquivos, não sobrescrever a versão remota sem reconciliar.
- A raiz real do Unity e a versão do editor devem estar sempre registradas em `PROJECT_STATUS.md`.

## Segurança e licenças
O repositório é **público**. Não publicar tokens, perfis locais, `Library`, `Builds`, backups, pacotes ZIP, modelos extraídos de LoL/Dragon Ball ou assets sem licença de redistribuição. Git não é backup do projeto inteiro: assets grandes podem requerer Git LFS ou armazenamento autorizado externo, sem que isso signifique redistribuição livre.
