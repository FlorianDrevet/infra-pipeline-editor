using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImageTag("latest")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("infraDb");

builder.AddProject<InfraFlowSculptor_Api>("api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
