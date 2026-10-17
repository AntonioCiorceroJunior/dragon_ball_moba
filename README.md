# Dragon Ball MOBA

Protótipo offline de MOBA em Unity e C# com Ki, progressão e transformações. Os fontes do protótipo VisualTest foram importados na tarefa **DBM-000**, preservando o domínio e as apresentações existentes. Bugs e evolução de gameplay permanecem na fila de coordenação.

## Abrir e executar

1. Clone este repositório. No Unity Hub, adicione **`src/client-unity`** dentro da cópia clonada, não a raiz do repositório.
2. Use **Unity 6000.6.6f1**, Windows Build Support para compilar Windows, pipeline **Built-in** e **Input Manager legado**.
3. Abra `Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity` e pressione **Play**. O mapa é gerado em execução. Confira **VISUAL TEST 01 - Organic Arena**.

O início usa o campeão lógico GKU_SUPER com humanoide genérico CC0 animado. Esc retorna à seleção; botão direito move/ataca; Q/W/E/R usam técnicas; C carrega Ki; P abre a loja; H mostra ajuda. A **DBM-001** de pontos de habilidade e autoataques foi integrada à main e possui testes nativos registrados. O mapa, economia, visão, kits exclusivos e partida completa seguem em evolução; a **DBM-002** de torres/colisões aguarda validação em PR separado.

Na raiz do clone, com esse projeto fechado no Editor:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify_publication.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPrepare
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualPlay
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate_unity.ps1 -Mode VisualBuild
```

Se o Editor estiver em outro local, passe `-UnityEditor` com o caminho completo. A build local fica em `src/client-unity/Builds/VisualTest/DragonMobaVisualTest.exe`; depois de compilar, `JOGAR_VISUAL_TEST.cmd` abre essa versão. Builds e caches não são publicados. Copie a pasta de build inteira para transportar um executável.

Todos os assets necessários do subconjunto CC0 foram incluídos, com `.meta` e licenças. Não é necessário baixar os ZIPs originais para abrir a cena. O Unity resolve os pacotes de `Packages/manifest.json` e `packages-lock.json`; precisa de acesso ao registro e licença local do Editor.

## VS Code e documentação

Abra a raiz deste clone no VS Code. `.vscode/` contém tarefas VisualPrepare/VisualPlay/VisualBuild e configuração para anexar o depurador ao Unity. O Unity gera a solução em `src/client-unity`. `global.json` mantém a seleção local de SDK 8.0.318 usado anteriormente; outros ambientes podem ajustar a localização do SDK sem instalar dependências globais nesta tarefa.

- **[Comece aqui: central de documentação atual](docs/README.md)**.
- **[Estado atual e PRs](docs/STATUS_ATUAL.md)**.
- **[Plano mestre de arquitetura e implementação](docs/PLANO_MESTRE_ATUAL.md)**.
- [Documentos mestres e proveniência](docs/specifications/README.md).
- [Pesquisa de LoL, MOBAs e código aberto](docs/research/README.md).
- [Manifesto de assets e licenças](docs/ASSET_MANIFEST.md).
- [Galeria histórica real do protótipo](docs/evidence/Comparison.html), anterior à publicação DBM-000.
- [Relatório completo do trabalho anterior](docs/RELATORIO_COMPLETO_DO_TRABALHO.md), com caminhos pessoais substituídos por marcadores.
- [Estado técnico e verificações atuais](docs/coordination/PROJECT_STATUS.md).

Documentos anteriores conservam propostas e resultados históricos. O protocolo em `AGENTS.md` e `docs/coordination/` define a única tarefa ativa. As especificações não implicam que todos os recursos planejados estejam implementados.

## Coordenação

As instruções de trabalho e relatórios ficam em [`docs/coordination/`](docs/coordination/README.md).

- ChatGPT: define tarefas e critérios em `TO_CODEX.md`.
- Codex (VS Code): implementa localmente, testa, registra evidências e publica `FROM_CODEX.md` e relatórios por execução.
- Todas as mudanças relevantes são versionadas com Git; nenhuma etapa deve ser tratada como validada sem testes registrados.

**Licenças:** este repositório é público. Não publicar arquivos privados, segredos, modelos de franquias ou assets de terceiros sem autorização e licença de redistribuição adequada. O projeto não é afiliado à Riot Games nem aos titulares de Dragon Ball.
