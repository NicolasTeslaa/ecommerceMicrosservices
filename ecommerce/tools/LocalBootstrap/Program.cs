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
    new BootstrapTarget(
        Name: "AuthService",
        StartupProjectRelativePath: @"ecommerce\services\AuthService\Auth.API\Auth.API.csproj",
        ProjectRelativePath: @"ecommerce\services\AuthService\Auth.Infrastructure\Auth.Infrastructure.csproj",
        ContextName: "AuthDbContext",
        AppSettingsRelativePath: @"ecommerce\services\AuthService\Auth.API\appsettings.json",
        ConnectionStringName: "AuthDb"),
    new BootstrapTarget(
        Name: "CartService",
        StartupProjectRelativePath: @"ecommerce\services\CartService\Cart.API\Cart.API.csproj",
        ProjectRelativePath: @"ecommerce\services\CartService\Cart.Infrastructure\Cart.Infrastructure.csproj",
        ContextName: "CartDbContext",
        AppSettingsRelativePath: @"ecommerce\services\CartService\Cart.API\appsettings.json",
        ConnectionStringName: "CartDb"),
    new BootstrapTarget(
        Name: "CatalogService Write",
        StartupProjectRelativePath: @"ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj",
        ProjectRelativePath: @"ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj",
        ContextName: "CatalogWriteDbContext",
        AppSettingsRelativePath: @"ecommerce\services\CatalogService\Catalog.API.Write\appsettings.json",
        ConnectionStringName: "CatalogWriteDb"),
    new BootstrapTarget(
        Name: "CatalogService Read",
        StartupProjectRelativePath: @"ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj",
        ProjectRelativePath: @"ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj",
        ContextName: "CatalogReadDbContext",
        AppSettingsRelativePath: @"ecommerce\services\CatalogService\Catalog.API.Read\appsettings.json",
        ConnectionStringName: "CatalogReadDb"),
    new BootstrapTarget(
        Name: "CustomerService",
        StartupProjectRelativePath: @"ecommerce\services\CustomerService\Customer.API\Customer.API.csproj",
        ProjectRelativePath: @"ecommerce\services\CustomerService\Customer.Infrastructure\Customer.Infrastructure.csproj",
        ContextName: "CustomerDbContext",
        AppSettingsRelativePath: @"ecommerce\services\CustomerService\Customer.API\appsettings.json",
        ConnectionStringName: "CustomerDb"),
    new BootstrapTarget(
        Name: "ExpeditionService",
        StartupProjectRelativePath: @"ecommerce\services\ExpeditionService\Expedition.API\Expedition.API.csproj",
        ProjectRelativePath: @"ecommerce\services\ExpeditionService\Expedition.Infrastructure\Expedition.Infrastructure.csproj",
        ContextName: "ExpeditionDbContext",
        AppSettingsRelativePath: @"ecommerce\services\ExpeditionService\Expedition.API\appsettings.json",
        ConnectionStringName: "ExpeditionDb"),
    new BootstrapTarget(
        Name: "OrderService Write",
        StartupProjectRelativePath: @"ecommerce\services\OrderService\Order.API.Write\Order.API.Write.csproj",
        ProjectRelativePath: @"ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj",
        ContextName: "OrderWriteDbContext",
        AppSettingsRelativePath: @"ecommerce\services\OrderService\Order.API.Write\appsettings.json",
        ConnectionStringName: "OrderWriteDb"),
    new BootstrapTarget(
        Name: "OrderService Read",
        StartupProjectRelativePath: @"ecommerce\services\OrderService\Order.API.Read\Order.API.Read.csproj",
        ProjectRelativePath: @"ecommerce\services\OrderService\Order.Infrastructure\Order.Infrastructure.csproj",
        ContextName: "OrderReadDbContext",
        AppSettingsRelativePath: @"ecommerce\services\OrderService\Order.API.Read\appsettings.json",
        ConnectionStringName: "OrderReadDb"),
    new BootstrapTarget(
        Name: "PaymentService",
        StartupProjectRelativePath: @"ecommerce\services\PaymentService\Payment.API\Payment.API.csproj",
        ProjectRelativePath: @"ecommerce\services\PaymentService\Payment.Infrastructure\Payment.Infrastructure.csproj",
        ContextName: "PaymentDbContext",
        AppSettingsRelativePath: @"ecommerce\services\PaymentService\Payment.API\appsettings.json",
        ConnectionStringName: "PaymentDb"),
    new BootstrapTarget(
        Name: "InventoryService",
        StartupProjectRelativePath: @"ecommerce\services\InventoryService\Inventory.API\Inventory.API.csproj",
        ProjectRelativePath: @"ecommerce\services\InventoryService\Inventory.Infrastructure\Inventory.Infrastructure.csproj",
        ContextName: "InventoryDbContext",
        AppSettingsRelativePath: @"ecommerce\services\InventoryService\Inventory.API\appsettings.json",
        ConnectionStringName: "InventoryDb"),
    new BootstrapTarget(
        Name: "NotaFiscalService",
        StartupProjectRelativePath: @"ecommerce\services\NotaFiscalService\NotaFiscal.API\NotaFiscal.API.csproj",
        ProjectRelativePath: @"ecommerce\services\NotaFiscalService\NotaFiscal.Infrastructure\NotaFiscal.Infrastructure.csproj",
        ContextName: "NotaFiscalDbContext",
        AppSettingsRelativePath: @"ecommerce\services\NotaFiscalService\NotaFiscal.API\appsettings.json",
        ConnectionStringName: "NotaFiscalDb"),
    new BootstrapTarget(
        Name: "NotificationService",
        StartupProjectRelativePath: @"ecommerce\services\NotificationService\Notification.API\Notification.API.csproj",
        ProjectRelativePath: @"ecommerce\services\NotificationService\Notification.Infrastructure\Notification.Infrastructure.csproj",
        ContextName: "NotificationDbContext",
        AppSettingsRelativePath: @"ecommerce\services\NotificationService\Notification.API\appsettings.json",
        ConnectionStringName: "NotificationDb")
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
            throw new InvalidOperationException($"Connection string '{service.ConnectionStringName}' for {service.Name} does not define a database.");
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

    using var adminClient = new AdminClientBuilder(new AdminClientConfig
    {
        BootstrapServers = bootstrapServers
    }).Build();

    try
    {
        await adminClient.CreateTopicsAsync(
        [
            .. topics.Select(topic => new TopicSpecification
            {
                Name = topic,
                NumPartitions = 1,
                ReplicationFactor = 1
            })
        ]);
    }
    catch (CreateTopicsException exception)
    {
        var unexpectedErrors = exception.Results
            .Where(result => result.Error.Code != ErrorCode.TopicAlreadyExists)
            .ToArray();

        if (unexpectedErrors.Length > 0)
        {
            var details = string.Join(
                Environment.NewLine,
                unexpectedErrors.Select(result => $"- {result.Topic}: {result.Error.Reason}"));

            throw new InvalidOperationException(
                $"Failed to ensure Kafka topics on '{bootstrapServers}'.{Environment.NewLine}{details}",
                exception);
        }
    }

    Console.WriteLine($"[{Timestamp()}] Kafka topics ready: {string.Join(", ", topics)}");
}

static async Task RestoreSolutionAsync(string repoRoot)
{
    var solutionPath = Path.Combine(repoRoot, "ecommerce", "ecommerce-platform.slnx");

    Console.WriteLine($"[{Timestamp()}] Restoring solution...");
    await RunProcessAsync(
        fileName: "dotnet",
        arguments: ["restore", solutionPath, "--verbosity", "minimal"],
        workingDirectory: repoRoot);
}

static async Task RunMigrationsAsync(IEnumerable<BootstrapTarget> services, string repoRoot)
{
    foreach (var service in services)
    {
        Console.WriteLine($"[{Timestamp()}] Applying migrations for {service.Name}...");

        await RunProcessAsync(
            fileName: "dotnet",
            arguments:
            [
                "ef",
                "database",
                "update",
                "--project",
                Path.Combine(repoRoot, service.ProjectRelativePath),
                "--startup-project",
                Path.Combine(repoRoot, service.StartupProjectRelativePath),
                "--context",
                service.ContextName
            ],
            workingDirectory: repoRoot);
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
    {
        return environmentValue;
    }

    var appSettingsPath = Path.Combine(repoRoot, service.AppSettingsRelativePath);
    using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

    if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
        connectionStrings.TryGetProperty(service.ConnectionStringName, out var connectionString))
    {
        return connectionString.GetString()
            ?? throw new InvalidOperationException($"Connection string '{service.ConnectionStringName}' in '{appSettingsPath}' is null.");
    }

    throw new InvalidOperationException($"Connection string '{service.ConnectionStringName}' was not found in '{appSettingsPath}'.");
}

static string ResolveKafkaBootstrapServers(string repoRoot)
{
    var environmentValue = Environment.GetEnvironmentVariable("Kafka__BootstrapServers");
    if (!string.IsNullOrWhiteSpace(environmentValue))
    {
        return environmentValue;
    }

    var appSettingsPath = Path.Combine(repoRoot, "ecommerce", "services", "AuthService", "Auth.API", "appsettings.json");
    using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

    if (document.RootElement.TryGetProperty("Kafka", out var kafkaSection) &&
        kafkaSection.TryGetProperty("BootstrapServers", out var bootstrapServers))
    {
        return bootstrapServers.GetString()
            ?? throw new InvalidOperationException($"Kafka:BootstrapServers in '{appSettingsPath}' is null.");
    }

    throw new InvalidOperationException($"Kafka:BootstrapServers was not found in '{appSettingsPath}'.");
}

static string[] ResolveKafkaTopics() =>
    [
        "auth.user-registered",
        "catalog.product-created",
        "expedition.awaiting-carrier-pickup",
        "expedition.delivery-failed",
        "expedition.delivered",
        "expedition.in-transit",
        "expedition.picked-up-by-carrier",
        "inventory.reservation-rejected",
        "invoice.issued",
        "order.confirmed",
        "order.pending-payment",
        "order.processing.requested",
        "order.rejected",
        "payment.approved",
        "payment.failed"
    ];

static string BuildConnectionStringWithUserVariables(string connectionString)
{
    var builder = new MySqlConnectionStringBuilder(connectionString)
    {
        AllowUserVariables = true
    };

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
    {
        startInfo.ArgumentList.Add(argument);
    }

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
        throw new InvalidOperationException($"Failed to start process '{fileName}'.");
    }

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            $"Command '{fileName} {string.Join(' ', arguments)}' failed with exit code {process.ExitCode}.{Environment.NewLine}{output}{error}");
    }
}

static string FindRepositoryRoot(string startingPath)
{
    var directory = new DirectoryInfo(startingPath);

    while (directory is not null)
    {
        var solutionPath = Path.Combine(directory.FullName, "ecommerce", "ecommerce-platform.slnx");
        if (File.Exists(solutionPath))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not find the repository root from the current execution directory.");
}

static bool IsTrue(string? value) =>
    string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);

static string Timestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

internal sealed record BootstrapTarget(
    string Name,
    string StartupProjectRelativePath,
    string ProjectRelativePath,
    string ContextName,
    string AppSettingsRelativePath,
    string ConnectionStringName);
