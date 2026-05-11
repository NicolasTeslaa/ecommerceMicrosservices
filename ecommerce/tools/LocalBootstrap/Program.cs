using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MySqlConnector;

var repoRoot = FindRepositoryRoot(AppContext.BaseDirectory);
var forceCatalogSeed = IsTrue(Environment.GetEnvironmentVariable("FORCE_CATALOG_SEED"));

var services = new[]
{
    new BootstrapTarget("AuthService", @"ecommerce\services\AuthService\Auth.API\Auth.API.csproj", @"ecommerce\services\AuthService\Auth.Infrastructure\Auth.Infrastructure.csproj", "AuthDbContext", @"ecommerce\services\AuthService\Auth.API\appsettings.json", "AuthDb"),
    new BootstrapTarget("CartService", @"ecommerce\services\CartService\Cart.API\Cart.API.csproj", @"ecommerce\services\CartService\Cart.Infrastructure\Cart.Infrastructure.csproj", "CartDbContext", @"ecommerce\services\CartService\Cart.API\appsettings.json", "CartDb"),
    new BootstrapTarget("CatalogService Write", @"ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj", @"ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj", "CatalogWriteDbContext", @"ecommerce\services\CatalogService\Catalog.API.Write\appsettings.json", "CatalogWriteDb"),
    new BootstrapTarget("CatalogService Read", @"ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj", @"ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj", "CatalogReadDbContext", @"ecommerce\services\CatalogService\Catalog.API.Read\appsettings.json", "CatalogReadDb"),
    new BootstrapTarget("CustomerService", @"ecommerce\services\CustomerService\Customer.API\Customer.API.csproj", @"ecommerce\services\CustomerService\Customer.Infrastructure\Customer.Infrastructure.csproj", "CustomerDbContext", @"ecommerce\services\CustomerService\Customer.API\appsettings.json", "CustomerDb"),
    new BootstrapTarget("ExpeditionService", @"ecommerce\services\ExpeditionService\Expedition.API\Expedition.API.csproj", @"ecommerce\services\ExpeditionService\Expedition.Infrastructure\Expedition.Infrastructure.csproj", "ExpeditionDbContext", @"ecommerce\services\ExpeditionService\Expedition.API\appsettings.json", "ExpeditionDb"),
    new BootstrapTarget("OrderService Write", @"ecommerce\services\OrderService\Order.API.Write\Order.API.Write.csproj", @"ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj", "OrderWriteDbContext", @"ecommerce\services\OrderService\Order.API.Write\appsettings.json", "OrderWriteDb"),
    new BootstrapTarget("OrderService Read", @"ecommerce\services\OrderService\Order.API.Read\Order.API.Read.csproj", @"ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj", "OrderReadDbContext", @"ecommerce\services\OrderService\Order.API.Read\appsettings.json", "OrderReadDb"),
    new BootstrapTarget("PaymentService", @"ecommerce\services\PaymentService\Payment.API\Payment.API.csproj", @"ecommerce\services\PaymentService\Payment.Infrastructure\Payment.Infrastructure.csproj", "PaymentDbContext", @"ecommerce\services\PaymentService\Payment.API\appsettings.json", "PaymentDb"),
    new BootstrapTarget("InventoryService", @"ecommerce\services\InventoryService\Inventory.API\Inventory.API.csproj", @"ecommerce\services\InventoryService\Inventory.Infrastructure\Inventory.Infrastructure.csproj", "InventoryDbContext", @"ecommerce\services\InventoryService\Inventory.API\appsettings.json", "InventoryDb"),
    new BootstrapTarget("NotaFiscalService", @"ecommerce\services\NotaFiscalService\NotaFiscal.API\NotaFiscal.API.csproj", @"ecommerce\services\NotaFiscalService\NotaFiscal.Infrastructure\NotaFiscal.Infrastructure.csproj", "NotaFiscalDbContext", @"ecommerce\services\NotaFiscalService\NotaFiscal.API\appsettings.json", "NotaFiscalDb"),
    new BootstrapTarget("NotificationService", @"ecommerce\services\NotificationService\Notification.API\Notification.API.csproj", @"ecommerce\services\NotificationService\Notification.Infrastructure\Notification.Infrastructure.csproj", "NotificationDbContext", @"ecommerce\services\NotificationService\Notification.API\appsettings.json", "NotificationDb")
};

Console.WriteLine($"[{Timestamp()}] Local bootstrap started.");
Console.WriteLine($"[{Timestamp()}] Repository root: {repoRoot}");

await EnsureKafkaTopicsExistAsync(repoRoot);
await EnsureDatabasesExistAsync(services, repoRoot);
await RestoreSolutionAsync(repoRoot);
await RunMigrationsAsync(services, repoRoot);
await SeedCatalogAsync(services, repoRoot, forceCatalogSeed);

Console.WriteLine($"[{Timestamp()}] Local bootstrap finished successfully.");

static async Task EnsureDatabasesExistAsync(IEnumerable<BootstrapTarget> services, string repoRoot)
{
    foreach (var service in services)
    {
        var connectionString = ResolveConnectionString(service, repoRoot);
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            Console.Error.WriteLine($"[{Timestamp()}] Connection string '{service.ConnectionStringName}' for {service.Name} does not define a database. Skipping.");
            continue;
        }

        builder.Database = string.Empty;
        builder.AllowUserVariables = true;

        Console.WriteLine($"[{Timestamp()}] Ensuring database '{databaseName}' exists for {service.Name}...");

        await using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`;";
        await command.ExecuteNonQueryAsync();
    }
}

static async Task EnsureKafkaTopicsExistAsync(string repoRoot)
{
    var bootstrapServers = ResolveKafkaBootstrapServers(repoRoot);
    var topics = ResolveKafkaTopics();

    Console.WriteLine($"[{Timestamp()}] Ensuring Kafka topics exist on '{bootstrapServers}'...");

    using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();

    try
    {
        await adminClient.CreateTopicsAsync([.. topics.Select(topic => new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 })]);
    }
    catch (CreateTopicsException exception)
    {
        var unexpectedErrors = exception.Results.Where(result => result.Error.Code != ErrorCode.TopicAlreadyExists).ToArray();
        if (unexpectedErrors.Length > 0)
        {
            var details = string.Join(Environment.NewLine, unexpectedErrors.Select(result => $"- {result.Topic}: {result.Error.Reason}"));
            Console.Error.WriteLine($"[{Timestamp()}] Failed to ensure Kafka topics on '{bootstrapServers}'.{Environment.NewLine}{details}");
        }
    }

    Console.WriteLine($"[{Timestamp()}] Kafka topics ready: {string.Join(", ", topics)}");
}

static async Task RestoreSolutionAsync(string repoRoot)
{
    var solutionPath = Path.Combine(repoRoot, "ecommerce", "ecommerce-platform.slnx");
    Console.WriteLine($"[{Timestamp()}] Restoring solution...");
    await RunProcessAsync("dotnet", ["restore", solutionPath, "--verbosity", "minimal"], repoRoot);
}

static async Task RunMigrationsAsync(IEnumerable<BootstrapTarget> services, string repoRoot)
{
    foreach (var service in services)
    {
        Console.WriteLine($"[{Timestamp()}] Applying migrations for {service.Name}...");
        await RunProcessAsync("dotnet", ["ef", "database", "update", "--project", Path.Combine(repoRoot, service.ProjectRelativePath), "--startup-project", Path.Combine(repoRoot, service.StartupProjectRelativePath), "--context", service.ContextName], repoRoot);
    }
}

static async Task SeedCatalogAsync(IEnumerable<BootstrapTarget> services, string repoRoot, bool forceCatalogSeed)
{
    var catalogWrite = services.Single(service => service.ContextName == "CatalogWriteDbContext");
    var inventory = services.Single(service => service.ContextName == "InventoryDbContext");

    var catalogConnectionString = ResolveConnectionString(catalogWrite, repoRoot);
    var inventoryConnectionString = ResolveConnectionString(inventory, repoRoot);

    await using var countConnection = new MySqlConnection(BuildConnectionStringWithUserVariables(catalogConnectionString));
    await countConnection.OpenAsync();

    await using var countCommand = countConnection.CreateCommand();
    countCommand.CommandText = "SELECT COUNT(*) FROM products;";

    var currentCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
    if (!forceCatalogSeed && currentCount > 0)
    {
        Console.WriteLine($"[{Timestamp()}] Catalog seed skipped because products already exist. Set FORCE_CATALOG_SEED=true to reload it.");
        return;
    }

    Console.WriteLine($"[{Timestamp()}] Loading catalog seed from seed-catalog-50k.sql...");

    var scriptPath = Path.Combine(repoRoot, "seed-catalog-50k.sql");
    var script = await File.ReadAllTextAsync(scriptPath);
    script = PrepareMySqlScript(script, catalogConnectionString, inventoryConnectionString);

    await using var seedConnection = new MySqlConnection(BuildConnectionStringWithUserVariables(catalogConnectionString));
    await seedConnection.OpenAsync();

    await using var command = seedConnection.CreateCommand();
    command.CommandTimeout = 0;
    command.CommandText = script;
    await command.ExecuteNonQueryAsync();

    Console.WriteLine($"[{Timestamp()}] Catalog seed finished.");
}

static string ResolveConnectionString(BootstrapTarget service, string repoRoot)
{
    var environmentValue = Environment.GetEnvironmentVariable($"ConnectionStrings__{service.ConnectionStringName}");
    if (!string.IsNullOrWhiteSpace(environmentValue))
        return environmentValue;

    var appSettingsPath = Path.Combine(repoRoot, service.AppSettingsRelativePath);
    using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

    if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) && connectionStrings.TryGetProperty(service.ConnectionStringName, out var connectionString))
        return connectionString.GetString() ?? string.Empty;

    Console.Error.WriteLine($"[{Timestamp()}] Connection string '{service.ConnectionStringName}' was not found in '{appSettingsPath}'. Using empty value.");
    return string.Empty;
}

static string ResolveKafkaBootstrapServers(string repoRoot)
{
    var environmentValue = Environment.GetEnvironmentVariable("Kafka__BootstrapServers");
    if (!string.IsNullOrWhiteSpace(environmentValue))
        return environmentValue;

    var appSettingsPath = Path.Combine(repoRoot, "ecommerce", "services", "AuthService", "Auth.API", "appsettings.json");
    using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

    if (document.RootElement.TryGetProperty("Kafka", out var kafkaSection) && kafkaSection.TryGetProperty("BootstrapServers", out var bootstrapServers))
        return bootstrapServers.GetString() ?? "localhost:9094";

    Console.Error.WriteLine($"[{Timestamp()}] Kafka:BootstrapServers was not found in '{appSettingsPath}'. Using localhost:9094.");
    return "localhost:9094";
}

static string[] ResolveKafkaTopics() => ["auth.user-registered", "catalog.product-created", "expedition.awaiting-carrier-pickup", "expedition.delivery-failed", "expedition.delivered", "expedition.in-transit", "expedition.picked-up-by-carrier", "inventory.reservation-rejected", "invoice.issued", "order.confirmed", "order.pending-payment", "order.processing.requested", "order.rejected", "payment.approved", "payment.failed"];

static string BuildConnectionStringWithUserVariables(string connectionString)
{
    var builder = new MySqlConnectionStringBuilder(connectionString) { AllowUserVariables = true };
    return builder.ConnectionString;
}

static string PrepareMySqlScript(string script, string catalogConnectionString, string inventoryConnectionString)
{
    var catalogDatabase = new MySqlConnectionStringBuilder(catalogConnectionString).Database;
    var inventoryDatabase = new MySqlConnectionStringBuilder(inventoryConnectionString).Database;

    return script
        .Replace("USE `ecommerce-plataform-catalog-write`;", $"USE `{catalogDatabase}`;", StringComparison.Ordinal)
        .Replace("`ecommerce-platform-inventory`", $"`{inventoryDatabase}`", StringComparison.Ordinal)
        .Replace("DELIMITER $$", string.Empty, StringComparison.Ordinal)
        .Replace("END$$", "END;", StringComparison.Ordinal)
        .Replace("DELIMITER ;", string.Empty, StringComparison.Ordinal);
}

static async Task RunProcessAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
{
    var output = new StringBuilder();
    var error = new StringBuilder();

    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };

    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = new Process { StartInfo = startInfo };

    process.OutputDataReceived += (_, args) =>
    {
        if (args.Data is not null)
        {
            Console.WriteLine(args.Data);
            output.AppendLine(args.Data);
        }
    };

    process.ErrorDataReceived += (_, args) =>
    {
        if (args.Data is not null)
        {
            Console.Error.WriteLine(args.Data);
            error.AppendLine(args.Data);
        }
    };

    if (!process.Start())
    {
        Console.Error.WriteLine($"[{Timestamp()}] Failed to start process '{fileName}'.");
        return;
    }

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
    {
        Console.Error.WriteLine($"[{Timestamp()}] Command '{fileName} {string.Join(' ', arguments)}' failed with exit code {process.ExitCode}.{Environment.NewLine}{output}{error}");
    }
}

static string FindRepositoryRoot(string startingPath)
{
    var directory = new DirectoryInfo(startingPath);

    while (directory is not null)
    {
        var solutionPath = Path.Combine(directory.FullName, "ecommerce", "ecommerce-platform.slnx");
        if (File.Exists(solutionPath))
            return directory.FullName;

        directory = directory.Parent;
    }

    Console.Error.WriteLine($"[{Timestamp()}] Could not find the repository root from the current execution directory. Using current directory.");
    return Directory.GetCurrentDirectory();
}

static bool IsTrue(string? value) => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
static string Timestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

internal sealed record BootstrapTarget(string Name, string StartupProjectRelativePath, string ProjectRelativePath, string ContextName, string AppSettingsRelativePath, string ConnectionStringName);
