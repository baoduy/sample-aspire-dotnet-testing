namespace Api.Endpoints.Products;

public class UpdateProductCommand : IRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

internal class UpdateProductHandler(AppDbContext context) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([request.Id], cancellationToken);
        if (product == null) throw new Exception("Product not found");

        product.Name = request.Name;
        product.Price = request.Price;

        await context.SaveChangesAsync(cancellationToken);
    }
}