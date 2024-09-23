var builder = DistributedApplication.CreateBuilder(args);

//Database
var postgres = builder.AddPostgres("postgres").PublishAsConnectionString();
var db = postgres.AddDatabase("Db");

//Internal API
builder.AddProject<Projects.Api>("api")
    .WithReference(db);

builder.Build().Run();