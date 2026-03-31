# E-commerce Microservices

Projeto de estudo e evolucao de uma plataforma de e-commerce orientada a microservicos, com foco em separacao de responsabilidades, contratos compartilhados e comunicacao entre servicos.

O repositorio ja cobre o fluxo principal de compra de ponta a ponta: autenticacao, catalogo, carrinho, enderecos do cliente, cotacao de frete, criacao de pedido, consulta de pedidos e aplicacao web consumindo tudo por um gateway unico.

## O que o projeto abrange

O ecossistema esta organizado em quatro frentes:

- `gateway/ApiGateway`: ponto unico de entrada para as APIs.
- `services/*`: microservicos independentes por dominio de negocio.
- `shared/ECommerce.Shared`: contratos HTTP, eventos de integracao e artefatos compartilhados.
- `spa/`: storefront web em React para navegacao, autenticacao e checkout.

## Dominios implementados

### Catalogo

O `CatalogService` e o servico mais maduro da solucao e segue uma abordagem `CQRS`, com separacao entre leitura e escrita:

- `Catalog.API.Read`: consultas de produtos e categorias.
- `Catalog.API.Write`: comandos de criacao, alteracao e exclusao.
- `Catalog.Application`: handlers, DTOs, commands, queries e contratos.
- `Catalog.Domain`: entidades e regras de negocio.
- `Catalog.Infrastructure`: persistencia, repositorios e projecoes.
- `Catalog.API.Common`: middleware e composicao compartilhada entre os hosts do servico.

Funcionalidades atuais do catalogo:

- cadastro, atualizacao e remocao de produtos;
- cadastro, atualizacao e remocao de categorias;
- listagem paginada de produtos e categorias;
- consulta por identificador;
- filtros de busca no catalogo;
- modelo de leitura separado do modelo transacional;
- validacoes de negocio para garantir integridade entre produto e categoria;
- atributos logisticos no produto, como peso, largura, altura, cubagem e CEP de origem, usados pelo checkout e pelo frete.

### Autenticacao e identidade

O `AuthService` centraliza o ciclo de autenticacao da plataforma:

- registro de usuario;
- login;
- endpoint de perfil autenticado (`me`);
- emissao e validacao de `JWT`;
- publicacao de evento de usuario registrado;
- uso de outbox para comunicacao assincrona.

Esse servico ja participa do fluxo distribuido da plataforma e fornece os dados de identidade que o frontend usa para sessao e checkout autenticado.

### Clientes e enderecos

O `CustomerService` concentra os dados cadastrais do cliente e a gestao de enderecos:

- consulta de cliente por id;
- listagem de enderecos;
- consulta de endereco especifico;
- cadastro e atualizacao de endereco;
- remocao de endereco;
- definicao de endereco padrao.

O servico tambem expoe um endpoint `gRPC` de validacao de endereco, consumido pelo fluxo de pedidos para confirmar que o endereco informado pertence ao cliente antes da persistencia final.

### Carrinho

O `CartService` cobre o carrinho de compras e suporta operacoes de:

- consulta do carrinho;
- inclusao de item;
- alteracao de quantidade;
- remocao de item;
- limpeza total do carrinho.

O modelo atual considera dono do carrinho por tipo e identificador, abrindo espaco para cenarios de usuario autenticado e outros perfis de posse.

### Frete

O `ShippingService` oferece a cotacao de frete a partir de dados fisicos e logisticos do pedido:

- peso;
- largura;
- cubagem;
- CEP de origem;
- CEP de destino;
- provedor de frete.

Esse servico ja esta integrado ao checkout da SPA e participa da composicao do valor final da compra.

### Pedidos

O `OrderService` foi estruturado com separacao entre leitura, escrita e processamento:

- `Order.API.Write`: recebe a solicitacao de criacao do pedido;
- `Order.API.Read`: consulta pedido por id e lista pedidos por cliente;
- `Order.API.Processor`: hospeda o processamento interno do pipeline assincrono;
- `Order.API.Common`, `Order.Application`, `Order.Domain` e `Order.Infrastructure`: camadas de aplicacao, dominio e infraestrutura.

Capacidades ja presentes:

- recebimento de pedido com resposta `Accepted`;
- processamento assincrono com background service;
- validacao do endereco do cliente via `gRPC`;
- projecao para modelo de leitura;
- publicacao de evento de pedido criado;
- outbox para orquestrar o processamento interno.

## Comunicacao entre servicos

O projeto ja contempla mais de um estilo de integracao distribuida:

- `HTTP` para APIs expostas ao gateway e ao frontend;
- `gRPC` para validacao interna de endereco entre pedidos e clientes;
- eventos de integracao compartilhados em `ECommerce.Shared/Messaging`;
- uso de `Kafka` e outbox em partes do fluxo assincrono.

Entre os contratos compartilhados atuais estao:

- `ApiResponse<T>` e `ApiError`;
- estruturas de paginacao;
- `UserRegisteredIntegrationEvent`;
- `OrderCreatedIntegrationEvent`;
- contrato `protobuf` para validacao de endereco do cliente.

## Gateway e composicao da plataforma

O `ApiGateway` concentra as rotas publicas da solucao e encaminha chamadas para:

- catalogo de leitura e escrita;
- autenticacao;
- carrinho;
- clientes;
- frete;
- pedidos de leitura e escrita;
- rota preparada para pagamentos.

Essa composicao permite que a SPA consuma a plataforma como uma experiencia unica, mesmo com os dominios separados em servicos independentes.

## SPA e experiencia do usuario

A aplicacao web em `ecommerce/spa` funciona como vitrine e camada de orquestracao da jornada do usuario. Hoje ela abrange:

- home e navegacao principal;
- catalogo de produtos;
- navegacao por categorias;
- detalhe de produto;
- autenticacao com login e cadastro;
- carrinho com drawer e pagina dedicada;
- checkout autenticado;
- consulta de CEP via ViaCEP para pre-preenchimento de endereco;
- calculo de frete durante a finalizacao;
- criacao de pedido;
- pagina de confirmacao.

No frontend, o estado do usuario, do carrinho e do ultimo pedido e mantido em stores dedicadas, integradas ao backend por uma camada unica de servicos.

## Qualidade e organizacao

O repositorio tambem contempla preocupacoes de engenharia importantes para uma arquitetura distribuida:

- padronizacao de resposta HTTP entre servicos;
- separacao por camadas (`API`, `Application`, `Domain`, `Infrastructure`);
- testes automatizados em servicos ja mais evoluidos, como catalogo e carrinho;
- estrutura preparada para expansao de novos dominios.

## Bootstrap local no Visual Studio

Se voce quiser deixar apenas o `Kafka` no Docker e subir o restante pelo Visual Studio, agora existe o projeto `ecommerce/tools/LocalBootstrap`.

Ele replica o papel do container `bootstrap`:

- garante que os bancos existam;
- roda `dotnet ef database update` para todos os `DbContexts`;
- aplica o seed do catalogo quando a tabela `products` estiver vazia.

Fluxo sugerido no Visual Studio:

1. suba o `Kafka` no Docker;
2. configure `LocalBootstrap` como o primeiro startup project;
3. configure as APIs que voce quer depurar como startup projects logo depois.

O `LocalBootstrap` termina sozinho depois de preparar o banco, entao ele funciona bem como a primeira etapa do F5. Se voce quiser forcar a recarga do seed do catalogo, defina a variavel de ambiente `FORCE_CATALOG_SEED=true` nesse projeto.

Se voce tambem quiser testar webhook da Stripe no mesmo F5, use o projeto `ecommerce/tools/StripeWebhookListener`. O perfil `Local + Kafka Docker` ja sobe esse listener e usa o profile `Payment.API (Stripe Local)`, que deixa `Stripe__WebhookSecret` vazio no ambiente local para aceitar os eventos encaminhados pela Stripe CLI.

## Escopo em expansao

Ja existem pastas reservadas para a evolucao de outros dominios do e-commerce, como:

- `PaymentService`;
- `InventoryService`;
- `NotificationService`;
- `NotaFiscalService`;
- `ExpeditionService`.

Esses modulos sinalizam a intencao do projeto de cobrir a cadeia completa de operacao de um e-commerce moderno, indo alem da vitrine e do checkout.
