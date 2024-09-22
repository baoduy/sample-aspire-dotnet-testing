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