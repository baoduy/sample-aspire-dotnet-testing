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