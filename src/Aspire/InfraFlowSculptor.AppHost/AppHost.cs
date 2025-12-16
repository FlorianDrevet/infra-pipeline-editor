using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDbGate(option => option.WithHostPort(51622))
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("infraDb");

var apiBicep = builder.AddProject<BicepGenerator_Api>("bicep-generator-api")
    .WithExternalHttpEndpoints();

builder.AddProject<InfraFlowSculptor_Api>("infraflowsculptor-api")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitFor(database)
    .WithReference(apiBicep);

await builder.Build().RunAsync();
