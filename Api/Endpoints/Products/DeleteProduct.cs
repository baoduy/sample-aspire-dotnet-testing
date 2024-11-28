namespace Api.Endpoints.Products;

public class DeleteProductCommand : IRequest
{
    public int Id { get; set; }
}

internal class DeleteProductHandler(AppDbContext context) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([request.Id], cancellationToken);
        if (product == null) throw new Exception("Product not found");

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}