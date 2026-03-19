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

var bicepApi = builder.AddProject<BicepGenerator_Api>("bicep-generator-api")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitFor(database)
    .WithReference(blobs)
    .WaitFor(blobs);

var infraApi = builder.AddProject<InfraFlowSculptor_Api>("infraflowsculptor-api")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitFor(database)
    .WithReference(blobs)
    .WaitFor(blobs);

builder.AddJavaScriptApp("angular-frontend", "../../Front", "start:aspire")
    .WithNpm()
    .WithReference(infraApi)
    .WaitFor(infraApi)
    .WithReference(bicepApi)
    .WaitFor(bicepApi)
    .WithHttpEndpoint(port: 4200, targetPort: 4200, env: "NG_PORT");

await builder.Build().RunAsync();
