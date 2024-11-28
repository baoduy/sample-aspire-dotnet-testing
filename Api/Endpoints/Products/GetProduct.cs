namespace Api.Endpoints.Products;

public class GetProductQuery : IRequest<Product?>
{
    public int Id { get; set; }
}

internal class GetProductHandler(AppDbContext context) : IRequestHandler<GetProductQuery, Product?>
{
    public async Task<Product?> Handle(GetProductQuery request, CancellationToken cancellationToken)
        => await context.Products.FindAsync([request.Id], cancellationToken: cancellationToken);
}