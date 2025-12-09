using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDbGate()
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("infraDb");

builder.AddProject<InfraFlowSculptor_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
