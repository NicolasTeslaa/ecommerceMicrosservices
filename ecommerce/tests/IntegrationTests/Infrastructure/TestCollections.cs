namespace IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class MicroservicesCollection : ICollectionFixture<MicroservicesTestEnvironment>
{
    public const string Name = "microservices";
}
