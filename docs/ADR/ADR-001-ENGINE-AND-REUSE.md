# ADR-001 — Motor, reuso de MOBA e fronteiras de arquitetura

**Status:** PROPOSTO; condicionado à auditoria AUD-001. **Data:** 2026-10-15.

## Contexto

Desejamos MOBA isométrico 3D com 15 versões jogáveis, formas, Ki e fusões; orçamento de uso de IA limitado; reutilização de um projeto open source pode acelerar, mas também introduzir dívida técnica.

## Proposta

- Preferir Unity 6.3 LTS + C# para cliente e UI, com domínio C# puro e servidor dedicado autoritativo.
- Examinar primeiro <https://github.com/yasgamesdev/MOBA_CSharp_Unity>, projeto MIT publicado com Unity 2018.3.4f1 para o cliente e servidor C# separado. Não assumir compatibilidade moderna.
- Comparar portar módulos seletivos versus criar vertical slice própria mínima.
- Não reutilizar assets de Dragon Ball, League of Legends ou mods sem licença individual válida e autorização dos titulares aplicável.

## Alternativas

A. Portar grande parte do projeto original; risco de dependências antigas.
B. Extrair somente algoritmos de domínio/servidor permissíveis, construir Unity cliente novo.
C. Construir tudo novo com bibliotecas atuais, usando o legado como referência.

## Critério de decisão

Priorizar: segurança, licença, compilabilidade, esforço de migração, coesão de arquitetura e adequação ao servidor autoritativo. Ganho visual inicial isolado não justifica arquitetura inviável.

## Resultado e evidência pendentes

A preencher após `AUD-001`: SHA auditado, comandos e logs, versões reais, matriz de reuso, decisão e responsabilidade da migração.
