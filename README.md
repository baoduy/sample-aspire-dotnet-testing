# Sample using Aspire .NET For Integration Tests Automation

This repository demonstrates how to set up a .NET project with MediatR, Entity Framework Core, PostgreSQL, and xUnit for unit testing. Follow the steps below to get started.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Visual Studio Code](https://code.visualstudio.com/) or any other C# IDE

## Project Structure

The project is structured as follows:

- `Api`: Contains the main API project.
- `Aspire.Host`: Contains the hosting configuration for the API.
- `Aspire.ServiceDefaults`: Contains shared services and configurations.
- `Aspire.Tests`: Contains the unit tests for the API.

## Dependency Graph

![DependencyGraph](/ProjectDependency.png)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/sample-aspire-dotnet-unittests.git
cd sample-aspire-dotnet-unittests
```

### 2. Configure the Database

Ensure PostgreSQL is installed and running on your machine. Update the connection string in `appsettings.Development.json` to match your PostgreSQL configuration.

```1:10:Api/Api/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Db": "Host=localhost;Database=ProductDb;Username=yourusername;Password=yourpassword"
  }
```

### 3. Install Dependencies

Restore the NuGet packages for all projects.

```bash
dotnet restore
```

### 4. Update `Program.cs`

Ensure the `Program.cs` file is configured to use PostgreSQL and MediatR.

```1:43:Api/Api/Program.cs
using System.Reflection;
using Api.Configs;
using Api.Data;
using Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Db")));

builder.Services.AddMediatR(op => op.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.CustomSchemaIds(t =>
    t.GetCustomAttributes<SwaggerSchemaIdAttribute>().SingleOrDefault()?.SchemaId ??
    SwashbuckleHelpers.DefaultSchemaIdSelector(t)));
//Aspire Support
builder.AddServiceDefaults();

var app = builder.Build();
if (app.Environment.IsDevelopment())
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapDefaultEndpoints();
//Aspire Support
app.MapDefaultEndpoints();
app.MapProductEndpoints();
app.UseHttpsRedirection();
app.MapProductEndpoints();
await app.RunAsync();

//This Startup endpoint for Unit Tests
namespace Api
{
    public class Program
    {
    }
}
```

### 5. Define the Entity

Create the `Product` entity in the `Entities` folder.

```1:7:Api/Api/Data/Entities/Product.cs
namespace Api.Data;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
```

### 6. Create the DbContext

Define the `AppDbContext` class to manage the entity.

```1:8:Api/Api/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
```

### 7. Create MediatR Handlers

Implement MediatR handlers for CRUD operations.

#### Create Product

```1:27:Api/Api/Endpoints/Products/CreateProduct.cs
using Api.Configs;
using Api.Data;
using MediatR;

namespace Api.Endpoints.Products;

public class CreateProduct
{
    [SwaggerSchemaId($"{nameof(CreateProduct)}{nameof(Command)}")]
    public class Command : IRequest<int>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = new Product { Name = request.Name, Price = request.Price };
            context.Products.Add(product);

            await context.SaveChangesAsync(cancellationToken);
            return product.Id;
        }
    }
}
```

#### Read Product

```1:19:Api/Api/Endpoints/Products/GetProduct.cs
using Api.Configs;
using Api.Data;
using MediatR;

namespace Api.Endpoints.Products;

public class GetProduct
{
    [SwaggerSchemaId($"{nameof(GetProduct)}{nameof(Query)}")]
    public class Query : IRequest<Product?>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, Product?>
    {
        public async Task<Product?> Handle(Query request, CancellationToken cancellationToken)
            => await context.Products.FindAsync([request.Id], cancellationToken:cancellationToken);
    }
```

#### Update Product

```1:30:Api/Api/Endpoints/Products/UpdateProduct.cs
using Api.Configs;
using Api.Data;
using MediatR;

namespace Api.Endpoints.Products;

public class UpdateProduct
{
    [SwaggerSchemaId($"{nameof(UpdateProduct)}{nameof(Command)}")]
    public class Command : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([request.Id], cancellationToken);
            if (product == null) throw new Exception("Product not found");

            product.Name = request.Name;
            product.Price = request.Price;

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

#### Delete Product

```1:26:Api/Api/Endpoints/Products/DeleteProduct.cs
using Api.Configs;
using Api.Data;
using MediatR;

namespace Api.Endpoints.Products;

public class DeleteProduct
{
    [SwaggerSchemaId($"{nameof(DeleteProduct)}{nameof(Command)}")]
    public class Command : IRequest
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([request.Id],cancellationToken );
            if (product == null) throw new Exception("Product not found");

            context.Products.Remove(product);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### 8. Map Endpoints

Map the endpoints in `ProductEndPointMapping.cs`.

```1:43:Api/Api/Endpoints/ProductEndPointMapping.cs
using Api.Endpoints.Products;
using MediatR;

namespace Api.Endpoints;

public static class ProductEndPointMapping
{
    public static WebApplication  MapProductEndpoints(this WebApplication app)
    {
         var group = app.MapGroup("/products")
             .WithOpenApi()
             .WithTags("ProductApis")
             .WithDescription($"The endpoints of Products")
             .WithSummary($"The endpoints of Products");

         group.MapPost("", async (IMediator mediator, CreateProduct.Command command) =>
         {
             var id = await mediator.Send(command);
             return Results.Created($"/products/{id}", id);
         });

         group.MapGet("{id:int}", async (IMediator mediator, int id) =>
         {
             var product = await mediator.Send(new GetProduct.Query { Id = id });
             return product is not null ? Results.Ok(product) : Results.NotFound();
         });

         group.MapPut("{id:int}", async (IMediator mediator, int id, UpdateProduct.Command command) =>
         {
             if (id != command.Id) return Results.BadRequest();
             await mediator.Send(command);
             return Results.NoContent();
         });

         group.MapDelete("{id:int}", async (IMediator mediator, int id) =>
         {
             await mediator.Send(new DeleteProduct.Command { Id = id });
             return Results.NoContent();
         });

        return app;
    }
}
```

### 9. Configure Hosting

Ensure the hosting configuration is set up correctly in `Aspire.Host`.

```1:11:Api/Aspire.Host/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

//Database
var postgres = builder.AddPostgres("postgres").PublishAsConnectionString();
var db = postgres.AddDatabase("Db");

//Internal API
builder.AddProject<Projects.Api>("api")
    .WithReference(db);
    //.WithHttpEndpoint(5105);

```

### 10. Create Unit Tests

Create a test class to test the CRUD operations.

#### Add xUnit and Related Packages

Ensure the `Aspire.Tests.csproj` file includes the necessary packages.

```1:33:Api/Aspire.Tests/Aspire.Tests.csproj
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Aspire.Hosting.Testing" Version="8.2.0"/>
        <PackageReference Include="coverlet.collector" Version="6.0.2"/>
        <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.8" />
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0"/>
        <PackageReference Include="xunit" Version="2.9.0"/>
        <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2"/>
    </ItemGroup>

    <ItemGroup>
        <Using Include="System.Net"/>
        <Using Include="Microsoft.Extensions.DependencyInjection"/>
        <Using Include="Aspire.Hosting.ApplicationModel"/>
        <Using Include="Aspire.Hosting.Testing"/>
        <Using Include="Xunit"/>
    </ItemGroup>

    <ItemGroup>
      <ProjectReference Include="..\Api\Api.csproj" />
      <ProjectReference Include="..\Aspire.Host\Aspire.Host.csproj" />
    </ItemGroup>

</Project>
```

#### Create Test Fixture

Create a test fixture to set up the test environment.

```1:63:Api/Aspire.Tests/Fixtures/ApiFixture.cs
using Aspire.Hosting;
using Aspire.Tests.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Aspire.Tests.Fixtures;

public sealed class ApiFixture : WebApplicationFactory<Api.Program>, IAsyncLifetime
{
    private readonly IHost _app;

    private readonly IResourceBuilder<PostgresServerResource> _postgres;
    private string? _postgresConnectionString;

    public ApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(ApiFixture).Assembly.FullName,
            DisableDashboard = true
        };
        var builder = DistributedApplication.CreateBuilder(options);

        _postgres = builder.AddPostgres("postgres").PublishAsConnectionString();
        _app = builder.Build();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>

                { "ConnectionStrings:Db", _postgresConnectionString },
            }!);
        });

        var host = base.CreateHost(builder);
        host.EnsureDbCreated().GetAwaiter().GetResult();
        return host;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _app.Dispose();
        }
    }

    public async Task InitializeAsync()
    {
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();


```

#### Create Test Class

Implement the test class to test the CRUD operations.

```1:79:Api/Aspire.Tests/ProductEndpointsTests.cs
using System.Net.Http.Json;
using Api.Data;
using Api.Endpoints.Products;
using Aspire.Tests.Fixtures;
using Xunit.Abstractions;

namespace Aspire.Tests;

public class ProductEndpointsTests(ApiFixture fixture, ITestOutputHelper output) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

     [Fact]
    public async Task CreateProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var command = new CreateProduct.Command { Name = "Test Product", Price = 10.99m };
        // Act
        var response = await _client.PostAsJsonAsync("/products", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var productId = await response.Content.ReadFromJsonAsync<int>();
        Assert.True(productId > 0);
    }

    [Fact]
    public async Task GetProduct_ReturnsProduct()
    {
        // Arrange
        var command = new CreateProduct.Command { Name = "Test Product", Price = 10.99m };
        var createResponse = await _client.PostAsJsonAsync("/products", command);
        var productId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act
        var response = await _client.GetAsync($"/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(10.99m, product.Price);
    }

    [Fact]
    public async Task UpdateProduct_ReturnsNoContent()
    {
        // Arrange
        var command = new CreateProduct.Command { Name = "Test Product", Price = 10.99m };
        var createResponse = await _client.PostAsJsonAsync("/products", command);
        var productId = await createResponse.Content.ReadFromJsonAsync<int>();

        var updateCommand = new UpdateProduct.Command { Id = productId, Name = "Updated Product", Price = 20.99m };

        // Act
        var response = await _client.PutAsJsonAsync($"/products/{productId}", updateCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNoContent()
    {
        // Arrange
        var command = new CreateProduct.Command { Name = "Test Product", Price = 10.99m };
        var createResponse = await _client.PostAsJsonAsync("/products", command);
        var productId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act
        var response = await _client.DeleteAsync($"/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
```

### 11. Run the Tests

Run the tests using the .NET CLI or your preferred IDE.

```bash
dotnet test
```

## Conclusion

This setup provides a basic CRUD implementation using MediatR, Entity Framework Core, PostgreSQL, and xUnit for unit testing. You can extend this setup to include more features and tests as needed.
