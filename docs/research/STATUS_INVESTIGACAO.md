# Estado transparente da investigação MOBA

**Data:** 2026-10-16 · **Propriedade:** ChatGPT + verificações futuras Codex/Unity.  
**Objetivo:** impedir que uma bibliografia extensa ou um documento longamente escrito seja confundido com pesquisa exaustiva, código pronto ou jogo validado.

### Níveis de evidência

- **E1** — fonte oficial/artigo identificado e consultado (escopo e versão conhecidos).
- **E2** — fonte estudada com mecanismo/limites, comparada explicitamente com **código atual do Dragon Ball MOBA**.
- **E3** — teste determinístico especificado **e executado**, ou código externo compilado e testado. Não basta teste planejado.
- **E4** — mecanismo implementado e comprovado em **Unity/Windows** no projeto.
- **E5** — playtest humano + medição/balanceamento por cenários e avaliação de longo prazo.

**E3/E4 não estão associados à pesquisa recente.** DBM-001 possui testes publicados pelo Codex anteriores, mas novos sistemas seguem sem esses gates.

| Subtema | Profundidade comprovada nesta rodada | Fonte/entrega | Trabalho que falta |
|---|---|---|---|
| Filosofia de itens, mythical removal, gold/snowball | E2 | Dossiê 04, Riot /dev 2024 | quantificar ganho de poder por item no nosso jogo |
| Itens/receitas/ícones | E2 | Dossiê 04, Data Dragon + Riot shop 2020 | sistema próprio data-driven, E3/E4, licenças de ícones |
| HP, regen, Armor/KiResist | E2 | Dossiê 04 + GameWorld/EntityState | Implementar StatCalculator + testes |
| Dano físico/Ki/true, shield, crítico, life steal | E2 | Dossiê 04 + Riot 10.23/12.10 | implementar pipeline, testagem matemática e validação runtime |
| Rank de QWER e upgrades | E4 para DBM-001 anterior; E2 novo | PR #1 e Dossiê 04 | habilidade diferente por herói/forma, playtest |
| Treino Ki C pós-carga | E2 | Dossiê 04 + GameWorld 188–241 | simular XP/min real, UX e anti-abuso |
| Ataque básico, windup/recovery, orbwalking | E2 | Dossiê 06 + Attack.cs externo | implementar estados e testar frame/cancelamento |
| Colisão de estruturas, torre aggro | E2 + implementação não comprovada PR #2 | Dossiê 05, código PR #2 | Unity/Windows E3/E4, sticky target, escudo/aggro |
| A*/NavMesh/flow fields | E2 | Dossiê 05 + Riot Tech Swarm + open source adapter | código de pathfinder externo integral, benchmark com 10–180 unidades |
| Waves, aggro minion, ritmo do jogo | E2 | Dossiê 05, Riot 13.10/26.1 | WAVE-001 e XP/Zeni por minute medidos |
| Topologia 3 lanes e selva | E2 | Dossiê 05 + Riot Map Changes 2024 | blockout MapDefinition + simetria/fog/jungle |
| Objetivos, evolução e macro | E1–E2 | documentos iniciais Riot + Dossiê 05 | código de objetivos, bot test, playtest |
| Visão, brush, wards, fog | E1–E2 | Riot Tech + auditoria GameWorld | contratos matemáticos, serviço vision, teste de vazamento |
| Câmera MOBA e hitbox click | E2 | Dossiê 05 + ArenaCameraController | análise de jogo real 720/1080/21:9 e UX |
| Modelos, Goku pequeno, Oozaru, animação | E1–E2 | GDD + estado do código | auditoria de meshes/rigs/licenças, E4, foot sliding |
| Som/vozes/VFX/auras | E1 | GDD, Clarity, Riot Tech | design sonoro, fontes licenciadas, implementação e teste |
| Champion select, lobby, preload | E2 | PrototypeGame + pesquisa prévia | estado de sessão + seleção real E4 |
| Balanceamento longitudinal LoL | E2 (paper completo analisado) | Dossiê 07, He et al. | outros papers completos e reprodução de resultados |
| Postmortem 2025 / mudanças 2026 | E2 | Dossiê 07 / Riot dev | análises mais amplas por temporada |
| Código aberto MOBA_CSharp_Unity | E2 em 13 arquivos | Dossiê 06 com URLs por classe | fixar commit, auditar bibliotecas/visão/nav e compilar isolado |
| Multiplayer autoritativo, snapshots, replay, anti-cheat | E1 | documentação de arquitetura e Riot Tech | protocolo, testes headless e estudo profundo C#/NET |
| Design de partidas humanas e usabilidade | E1 | plano preliminar | coleta real de observações, entrevistas e telemetria |
| Direito de IP, modelos, sprites | E1 | licença MIT externo, Riot developer policy | direitos de Dragon Ball, licença por asset/voz e distribuição |

## Próxima sequência de investigação ainda **não concluída**

1. **Dossiê 08:** sistema de visão e fog; ler código de bush/sight/ward em framework aberto e provar acesso de bots à informação.
2. **Dossiê 09:** engenharia de servidor/snapshot/interpolação/replay/autoritatividade, comparação concreta com servidor C# aberto.
3. **Dossiê 10:** arte técnica/rig/root motion, navegação por forma, VFX e áudio, incluindo pipeline/licença.
4. **Dossiê 11:** mecanismos de suporte/selva/objetivos/late game, cronologia oficial com tabela de mudanças cruzadas.
5. **Dossiê 12:** pesquisa acadêmica adicional com textos integrais, metodologias e leitura crítica por subtema.
6. **Dossiê 13:** priorização final de arquitetura por custo técnico, testes de qualidade e prazo observado, sem cronogramas inventados.
7. **Experimentos práticos (dependem de Unity/Codex):** compilar PR #2, executar smoke e testar manualmente. Não mesclar código só por documentação.

**Restrições:** pesquisa em Github não é execução em background. Nova rodada começa quando o usuário solicita continuidade ou quando uma automação autorizada executar; relatórios devem atualizar este estado com links e critérios de evidência.
