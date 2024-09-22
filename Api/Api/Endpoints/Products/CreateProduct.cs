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