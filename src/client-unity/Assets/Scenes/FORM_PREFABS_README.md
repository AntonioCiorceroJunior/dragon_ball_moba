# Aparências por transformação — contratos de prefab

O protótipo tenta carregar automaticamente um prefab **diferente para cada forma**. Nenhum modelo com direitos de terceiros está incluído neste repositório.

Crie um prefab dentro de `Assets/Resources/CharacterForms/<CHAMPION_ID>/<FORM_ID>.prefab`.
Exemplos:

```text
Assets/Resources/CharacterForms/
  GKU_KID/
    kid_base.prefab
    kid_nimbus.prefab
    kid_mastery.prefab
  GKU_SUPER/
    super_base.prefab
    super_god.prefab
    super_blue.prefab
    super_blue_kk.prefab
    super_ui_sign.prefab
    super_ui.prefab
  CELL_Z/
    cell_imperfect.prefab
    cell_semi.prefab
    cell_perfect.prefab
```

Se o prefab existir, `PrototypeGame.CreateEntityVisual` instancia e substitui a aparência no estágio correspondente. Caso não exista, o jogo usa uma forma geométrica colorida diferente, sem travar a simulação. Inclua `Collider` no prefab para seleção por clique direito. O script adiciona um collider simples caso não encontre nenhum, o que pode ser insuficiente para formas grandes.

**Meta para produção:** substituir Resources por Addressables validados, usar rig compatível quando possível, reaproveitar animações, configurar camadas visuais/VFX/SFX separadas e pré-carregar os assets antes da transformação. `Resources.Load` é suficiente apenas neste protótipo offline. Todas as malhas, animações, texturas, retratos e sons deverão ter origem e licença verificadas em `legal/ASSET_REGISTER.csv`.
