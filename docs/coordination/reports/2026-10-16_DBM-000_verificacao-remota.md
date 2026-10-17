# DBM-000 — conferência após publicação

**Data/responsável:** 2026-10-16, Codex. **Status:** concluída. Este é um complemento da mesma tarefa DBM-000; o [relatório original](2026-10-16_DBM-000_publicacao-inicial.md) permanece imutável.

## Commits efetivamente publicados

- [`04e33a986c7c545c5ae855f7ba4ef1a246ec9a8f`](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/commit/04e33a986c7c545c5ae855f7ba4ef1a246ec9a8f): fontes Unity, assets licenciados, documentos mestres e evidências de importação/Play Mode/build.
- [`684b5e467a48211b2d88fda98e47daef0d38f519`](https://github.com/AntonioCiorceroJunior/dragon_ball_moba/commit/684b5e467a48211b2d88fda98e47daef0d38f519): handoff, relatório e preservação exata das licenças. Árvore: `4a969ce1f833b021f77768797f6b4875b6ffc643`.

Ambos foram publicados no GitHub pelo conector autenticado. `main` e a branch de preparação avançaram sem força e preservaram o commit-base remoto. A cópia local foi sincronizada por fetch/fast-forward e passou a usar `main`.

## Clone novo e verificações

Após publicar, foram executados:

```powershell
git clone --single-branch --branch main https://github.com/AntonioCiorceroJunior/dragon_ball_moba.git Temp/DBMRemoteVerification
powershell -NoProfile -ExecutionPolicy Bypass -File Temp/DBMRemoteVerification/scripts/verify_publication.ps1
```

O clone recebido do GitHub apontou para **`684b5e467a48211b2d88fda98e47daef0d38f519`**. A verificação passou com **450 arquivos**, **176 `.meta`**, **26 arquivos importados com SHA-256 correto**, **sete avisos CC0** e **três hashes de domínio intactos**. O total de bytes no checkout pode variar por conversão de linhas em arquivos de texto; o conteúdo Git é identificado pela árvore.

Uma comparação adicional com o protótipo original confirmou os **19 arquivos C# com o mesmo conteúdo após normalizar quebras de linha**. Desses, **14 são idênticos byte a byte**; cinco tiveram apenas CRLF convertido para LF conforme `.gitattributes`:

- `Assets/Editor/ArtSceneSetup.cs`
- `Assets/Editor/ArtSmoke.cs`
- `Assets/Scripts/Runtime/ArtPresentation.cs`
- `Assets/Scripts/Runtime/CharacterPresentation.cs`
- `Assets/Scripts/Runtime/PrototypeGame.cs`

Isso detalha a comparação de 19 arquivos feita antes do transporte no relatório original. Não houve alteração de lógica; os três arquivos de domínio continuam idênticos byte a byte.

Foi conferida a ausência de mudanças nos seis arquivos protegidos: `AGENTS.md`, README de coordenação, `TO_CODEX.md`, `TASK_QUEUE.md`, `DECISIONS.md` e `REPORT_TEMPLATE.md`. O status Git local estava limpo após sincronizar a publicação.

Provas: [RemoteVerification.txt](../evidence/DBM-000/RemoteVerification.txt) e [comparação C# recebida do remoto](../evidence/DBM-000/remote-code-comparison.json).

## Alcance e próximos passos

Esta conferência adicional verificou recebimento e integridade dos fontes; não repetiu Unity, partida completa ou controles físicos no segundo clone. A importação, os testes de Play Mode, a compilação e a abertura do executável já foram executados na cópia limpa da mesma entrega e estão no relatório principal. Após eles, as mudanças de fonte limitaram-se ao transporte de quebras de linha e à preservação dos bytes de avisos de licença; este commit adicional contém somente documentação/provas.

Nenhum bloqueio de publicação permanece. **DBM-001 — corrigir progressão de habilidades e autoataque** continua aguardando instrução do ChatGPT/usuário; não foi iniciada. O acesso Git HTTPS por terminal continua sem credencial local, mas a publicação foi concluída pelo conector e não exige tokens dentro do projeto.
