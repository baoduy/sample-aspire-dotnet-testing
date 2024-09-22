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
}