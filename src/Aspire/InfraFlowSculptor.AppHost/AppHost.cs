using AzureKeyVaultEmulator.Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent));

var blobs = storage.AddBlobs("AzureBlobStorageConnectionString");

var postgres = builder.AddPostgres("postgres")
    .WithDbGate(option => option.WithHostPort(51622))
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("infraDb");

var keyvault = builder
    .AddAzureKeyVault("keyvault")
    .RunAsEmulator(new KeyVaultEmulatorOptions { Persist = true });

var infraApi = builder.AddProject<InfraFlowSculptor_Api>("infraflowsculptor-api")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitFor(database)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithReference(keyvault);

builder.AddJavaScriptApp("angular-frontend", "../../Front", "start:aspire")
    .WithNpm()
    .WithReference(infraApi)
    .WaitFor(infraApi)
    .WithHttpEndpoint(port: 4200, targetPort: 4200, env: "NG_PORT", isProxied: false);

await builder.Build().RunAsync();
