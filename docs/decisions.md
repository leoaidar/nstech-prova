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


## 3. Modelagem de Domínio Rico
**Data:** 01/05/2026

**Decisão:** As entidades do domínio (como `Order` e `Product`) foram projetadas como classes com comportamentos encapsulados. As propriedades utilizam `private set` e as coleções, como `OrderItems`, são expostas apenas como `IReadOnlyCollection`.

**Justificativa:** Evitar anti-pattern de "Modelos Anêmicos". A lógica de negócio e variações tipo `não adicionar item sem estoque ou cálculo de totais` ficam dentro da própria entidade, fácil de manter bom pra testes unitários e consistência dos dados antes de qualquer persistência.

