# Registro de Decisões Arquiteturais

Documento que centraliza as principais Observações/decisões técnicas durante o desenvolvimento do Order Service NSTech.

## 1. Estrutura e Clean Architecture
**Data:** 01/05/2026
**Decisão:** A solução foi estruturada em 4 camadas distintas: `Domain`, `Application`, `Infrastructure` e `Api`, seguindo os princípios de Clean Architecture.
**Justificativa:** Aplicação orientada ao Domínio DDD, garantir que o Domain seja completamente agnóstico externamente. O fluxo aponta sempre para o centro (Domain <- Application <- Infrastructure/Api).