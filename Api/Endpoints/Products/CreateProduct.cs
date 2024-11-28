namespace Api.Endpoints.Products;

public class CreateProductCommand : IRequest<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

internal class CreateProductHandler(AppDbContext context) : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product { Name = request.Name, Price = request.Price };
        context.Products.Add(product);

        await context.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}