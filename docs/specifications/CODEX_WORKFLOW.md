# Workflow Codex + Work com orçamento de uso reduzido

**Versão 1.1 — 15/10/2026**

## Regra-mãe

Otimize pelo tamanho do **problema verificável**, não pelo número exato de arquivos. Um arquivo isolado não prova que um recurso funciona; um ticket deve entregar um comportamento e seu teste. Usualmente 1–5 arquivos é conveniente, mas não é obrigação.

## Papéis

| Ferramenta | Prioridade | Não usar para |
|---|---|---|
| VS Code + Codex | Implementação local, testes, refatoração pequena e correção | Pedidos vagos: “crie o jogo todo” |
| ChatGPT Work | Auditoria arquitetural/research pontual, comparar alternativas e preparar relatório | Editar os mesmos arquivos em paralelo com Codex |
| Git/GitHub | Fonte única da verdade para código, tickets, decisões | Guardar senhas, modelos de origem desconhecida |
| Unity Editor | Cenas, prefabs, importação, playtests | Executar produção online sem servidor e QA |

Work e Codex podem consumir cotas de uso compartilhadas no Plus. A configuração exata e o saldo precisam ser consultados no painel da conta. Não existe garantia de economia específica de tokens.

## Checklist de cada sessão

1. **Escolher somente um ticket.** Atualize `tasks/ACTIVE_TASK.md` com ID, propósito, escopo, critérios de aceite e testes.
2. **Abrir contexto seletivo.** Codex lê `AGENTS.md` automaticamente quando aplicável; peça leitura de `ACTIVE_TASK`, `PROJECT_STATUS`, trechos numerados do GDD e dos arquivos que serão alterados.
3. **Definir limite de ação:** arquivos permitidos, comandos permitidos, resultado mínimo. Não começar “melhorias adicionais” sem autorização.
4. **Implementar e verificar:** compilar módulo relevante, executar testes focados, registrar resultados (pass/fail/não executado).
5. **Salvar:** pequena revisão humana, `git diff`, commit descritivo quando confirmado.
6. **Handoff curto:** atualize `PROJECT_STATUS.md` com o que mudou, evidências, bloqueios e próximo ticket. Só mantenha referências, não cole logs enormes.
7. **Nova sessão = novo contexto compacto:** use `prompts/02_RESUME_TASK.md`, não reenvie os 120 KB do GDD.

## Contexto: mínimo, suficiente e sob demanda

- Contexto básico recorrente: `AGENTS.md` + `tasks/ACTIVE_TASK.md` + `docs/PROJECT_STATUS.md`.
- Arquitetura: capítulo 20–23 do GDD **somente** em alterações de arquitetura.
- Game mechanics: capítulo 11–17 relevante (apenas subseções atingidas).
- Frontend/game client: capítulo 2–7 relevante.
- Licenças/arte: capítulo 24–25 e `legal/ASSET_REGISTER.csv`.
- Network/server: capítulo 20–22 e contratos alterados.
- Não anexar screenshots/logs enormes quando bastam trechos de erro e comandos para reproduzir.

## Prompt econômico recomendado

```text
Execute apenas [ID] descrita em tasks/ACTIVE_TASK.md.
Leia AGENTS.md, docs/PROJECT_STATUS.md, as seções [X] do GDD e arquivos relacionados.
Limite alterações ao escopo da tarefa. Não leia ou reescreva todo o repositório.
Implemente o menor comportamento completo, adicione/ajuste testes e execute-os.
Ao final: resumo curto; arquivos alterados; evidências; bloqueios; próximo passo.
Atualize PROJECT_STATUS.md, sem afirmar que testes não executados passaram.
```

## Quebras de sessão e contexto

- Não peça para o agente recordar conversas antigas; o contexto confiável está no Git.
- Se exceder a cota ou encerrar sessão, o próximo agente deve ler **somente** status/ticket e diff recente.
- Em caso de falha, registrar causa específica e comando de reprodução.
- Não repetir auditorias inteiras se houver evidências válidas e commit fixado.
- Revisar `git status` antes e depois; nunca sobrescrever mudanças não verificadas de outro agente.

## Quando usar modelo mais capaz

Escolha ferramentas de maior capacidade para arquitetura de rede, sincronização, migração complicada e bugs concorrentes. Para renomeações, testes simples, documentação curta e pequenos ajustes de UI, prefira modelos ou configurações menos custosos **se disponíveis no painel do seu Codex**. Confirmar opções atuais; nomes e preços mudam.

## Critérios de pronto do ticket

- Comportamento especificado está implementado.
- Testes correspondentes passam ou o motivo para não executar está documentado.
- Não há mudanças fora do escopo e nem segredos/arte restrita adicionados.
- Contratos e dados são compatíveis ou há migração prevista.
- Status e evidências atualizados; diff revisável.

## Anti-padrões

- “Crie um MOBA completo com 15 personagens.”
- “Leia todos os arquivos e melhore tudo.”
- Um único mega-prompt para login + 5v5 + marketplace + launcher.
- Refatorar todos os arquivos ao corrigir um bug de Ki.
- Pedir múltiplos agentes simultâneos em arquivos compartilhados.
- Migrar para Unity novo antes de comprovar que o legado compila.
- Anexar o documento mestre inteiro em todas as sessões.
- Desenvolver com mods e skins de terceiros sem origem/licença verificadas.

## Métricas simples por ticket

Registrar opcionalmente: arquivos alterados; linhas de código; comandos/testes; tentativas/retrabalho; motivo de bloqueio; tempo manual percebido; custo/cota se visível no produto. Não inferir tokens gastos se não há contador verificável.
