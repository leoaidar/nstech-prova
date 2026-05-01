# Registro de Decisões Arquiteturais

Documento que centraliza as principais Observações/decisões técnicas durante o desenvolvimento do Order Service NSTech.

## 1. Estrutura e Clean Architecture
**Data:** 01/05/2026
**Decisão:** A solução foi estruturada em 4 camadas distintas: `Domain`, `Application`, `Infrastructure` e `Api`, seguindo os princípios de Clean Architecture.
**Justificativa:** Aplicação orientada ao Domínio DDD, garantir que o Domain seja completamente agnóstico externamente. O fluxo aponta sempre para o centro (Domain <- Application <- Infrastructure/Api).

## 2. Injeção de Dependência e IoC
**Data:** 01/05/2026
**Decisão:** Em vez de utilizar um projeto legado de `CrossCutting` para registrar as dependências como era feito antigamente, achei melhor pelo uso de *Extension Methods* (`IServiceCollection`) localizados dentro de cada camada.
**Justificativa:** Fica mais alinhado com as práticas mais novas do .NET 8. A camada de `Api` atua como ponto de entrada único, chamando os métodos de extensão do DependencyInjection da `Application` e `Infrastructure`. Evita criar projetos "vazios" apenas para IoC e mantém alta coesão: cada projeto sabe como registrar suas próprias dependências. Marquei as classes concretas da Infraestrutura como `internal` pra garantir que a API dependa apenas de abstrações.