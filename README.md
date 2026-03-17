# E-commerce Microservices

Projeto de estudo/evolução de uma plataforma de e-commerce baseada em microserviços com foco atual no `CatalogService`.

## Estrutura

O repositório está organizado assim:

```text
ecommerce/
  gateway/
    ApiGateway/
  services/
    CatalogService/
      Catalog.API.Common/
      Catalog.API.Read/
      Catalog.API.Write/
      Catalog.Application/
      Catalog.Domain/
      Catalog.Infrastructure/
      Catalog.Tests/
  shared/
    ECommerce.Shared/
```

## Estado atual

Hoje o serviço mais evoluído do repositório é o `CatalogService`.

Ele já está seguindo uma abordagem de `CQRS`, com separação entre leitura e escrita:

- `Catalog.API.Read`: expõe endpoints de consulta
- `Catalog.API.Write`: expõe endpoints de comando
- `Catalog.Application`: handlers, DTOs, queries, commands e contratos
- `Catalog.Domain`: entidades e regras de domínio
- `Catalog.Infrastructure`: persistência, repositories, db contexts e projeções
- `Catalog.API.Common`: middleware e composição compartilhada entre os hosts da API
- `ECommerce.Shared`: contratos compartilhados entre microserviços

## CatalogService

### APIs

O `CatalogService` foi separado em duas APIs para permitir escalabilidade independente:

- leitura: `Catalog.API.Read`
- escrita: `Catalog.API.Write`

Essa separação permite, por exemplo:

- subir mais instâncias de leitura quando houver muito tráfego de consulta
- manter o lado de escrita mais controlado
- evoluir o modelo de leitura sem acoplar ao modelo de domínio

### Bancos

O catálogo trabalha com dois bancos lógicos:

- `catalog-write`: banco de escrita, fonte da verdade
- `catalog-read`: banco de leitura, usado pelos endpoints de consulta

No momento, `Products` e `Categories` já seguem essa estratégia.

### Fluxo de escrita e leitura

Para `Product` e `Category`, o fluxo atual é:

1. a API de escrita recebe o comando
2. o handler grava no banco de escrita
3. uma projeção atualiza o banco de leitura
4. a API de leitura consulta apenas o banco de leitura

Observação:
hoje a projeção para o banco de leitura é síncrona no fluxo da aplicação. O próximo passo arquitetural recomendado é evoluir isso para `Outbox + Background Worker`.

### Regra de negócio implementada

Ao criar ou atualizar um produto, o `CategoryId` precisa existir.

Ou seja:

- não é permitido criar produto com `CategoryId` vazio
- não é permitido criar ou atualizar produto apontando para uma categoria inexistente

## Resposta HTTP compartilhada

Foi criada uma class library compartilhada em `ecommerce/shared/ECommerce.Shared`.

Ela centraliza:

- `ApiResponse<T>`
- `ApiError`
- `PaginationRequest`
- `PaginationMetadata`
- `PagedResult<T>`

Isso permite padronizar os contratos HTTP entre os microserviços.

## Paginação

Os endpoints `GetAll` de produtos e categorias já suportam paginação.

Parâmetros:

- `PageNumber`
- `PageSize`

Os metadados de paginação retornam:

- `PageNumber`
- `PageSize`
- `TotalItems`
- `TotalPages`
- `HasPreviousPage`
- `HasNextPage`

Os endpoints `GetById` também retornam `ApiResponse` com metadado de paginação de item único para manter consistência de contrato.

## Gateway

O `ApiGateway` está configurado para rotear:

- requisições de leitura para `Catalog.API.Read`
- requisições de escrita para `Catalog.API.Write`

## Migrations

Como o `CatalogService` possui dois `DbContext`, as migrations devem ser geradas e aplicadas separadamente.

### Criar migration do banco de escrita

```powershell
dotnet ef migrations add InitialCatalogWrite `
  --context CatalogWriteDbContext `
  --project .\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj `
  --startup-project .\ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj `
  --output-dir Persistence\Migrations\Write
```

### Criar migration do banco de leitura

```powershell
dotnet ef migrations add InitialCatalogRead `
  --context CatalogReadDbContext `
  --project .\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj `
  --startup-project .\ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj `
  --output-dir Persistence\Migrations\Read
```

### Aplicar migration no banco de escrita

```powershell
dotnet ef database update `
  --context CatalogWriteDbContext `
  --project .\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj `
  --startup-project .\ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj
```

### Aplicar migration no banco de leitura

```powershell
dotnet ef database update `
  --context CatalogReadDbContext `
  --project .\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj `
  --startup-project .\ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj
```

## Testes

O repositório possui testes no `Catalog.Tests`, cobrindo principalmente:

- handlers de commands e queries
- controllers de leitura e escrita
- contratos compartilhados de paginação
- regras de negócio de categoria em produto

## Próximos passos recomendados

- implementar `Outbox` no `CatalogService`
- adicionar worker para projeção assíncrona do banco de leitura
- aplicar `ECommerce.Shared` aos outros microserviços
- evoluir observabilidade e health checks
- documentar como subir os serviços localmente via Docker ou compose, se esse for o caminho adotado
