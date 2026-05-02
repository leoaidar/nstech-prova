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


## 4. Persistência e Mapeamento (EF Core)
**Data:** 01/05/2026

**Decisão:** O mapeamento do banco de dados foi isolado na camada de `Infrastructure` utilizando a Fluent API do EF Core (classes `IEntityTypeConfiguration`). Os itens do pedido (`OrderItem`).

**Justificativa:** Manter o projeto `Domain` sem referência ao pacote do Entity Framework Core. O uso de (`OwnsMany`) reflete o conceito de DDD onde o `OrderItem` não possui ciclo de vida fora de sua raiz (`Order`).


## 5. Idempotência e CQRS
**Data:** 01/05/2026

**Decisão:** O padrão CQRS foi implementado utilizando `MediatR` para separar as leituras (Queries) e escritas (Commands). A idempotência exigida nos endpoints de Confirmação e Cancelamento foi resolvida via **Estado da Entidade**, retornando sucesso precoce, caso a transição de estado já foi efetuada anteriormente.

**Justificativa:** Tratar a idempotência verificando o estado atual da entidade (`Order.Status`) evita duplicação de processamento (como baixar o estoque duas vezes) sem a necessidade de introduzir complexidade de infra, como tabelas de chaves de idempotência ou cache distribuído.


## 6. Porta de Entrada Mundo Externo (API REST) e Autenticação (JWT)
**Data:** 01/05/2026

**Decisão:** Os Controllers foram estruturados sob o padrão "Thin Controllers" , delegando toda a orquestração de requisições para o MediatR. A segurança da API foi implementada utilizando autenticação stateless com JWT via package `Microsoft.AspNetCore.Authentication.JwtBearer`.

**Justificativa:** Manter Controllers sem regras de negócio garante que a camada de API seja responsável puramente por preocupações HTTP, S do SOLID, respeitando o Princípio da Responsabilidade Única (SRP). O JWT foi escolhido por ser o padrão da indústria para APIs RESTful, garantindo segurança sem a necessidade de manter estado no servidor.


## 7. Tratamento Global de Erros (Global Exception Handling)
**Data:** 01/05/2026

**Decisão:** Utilização da nova interface `IExceptionHandler` do .NET 8 junto com o `ProblemDetails` pra capturar exceções globalmente.

**Justificativa:** Centraliza o tratamento de erros, evita o vazamento de stack traces na API. Exceções de negócio (`DomainException`) são interceptadas e traduzidas para HTTP 400 (Bad Request), falhas não mapeadas retornam HTTP 500 (Internal Server Error) com mensagens padronizadas, melhorando segurança e experiência de consumo da API.

