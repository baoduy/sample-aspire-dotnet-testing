var builder = DistributedApplication.CreateBuilder(args);

//Database
var postgres = builder
    .AddPostgres("postgres")
    .AddDatabase("Db");

//Internal API
builder.AddProject<Projects.Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();