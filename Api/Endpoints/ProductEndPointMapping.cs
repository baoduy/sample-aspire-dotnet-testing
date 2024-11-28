using Api.Endpoints.Products;

namespace Api.Endpoints;

public static class ProductEndPointMapping
{
    public static WebApplication  MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/products")
            .WithOpenApi()
            .WithTags("ProductApis");

         group.MapPost("", async (IMediator mediator, CreateProductCommand command) =>
         {
             var id = await mediator.Send(command);
             return Results.Created($"/products/{id}", id);
         });

         group.MapGet("{id:int}", async (IMediator mediator, int id) =>
         {
             var product = await mediator.Send(new GetProductQuery { Id = id });
             return product is not null ? Results.Ok(product) : Results.NotFound();
         });

         group.MapPut("{id:int}", async (IMediator mediator, int id, UpdateProductCommand command) =>
         {
             if (id != command.Id) return Results.BadRequest();
             await mediator.Send(command);
             return Results.NoContent();
         });

         group.MapDelete("{id:int}", async (IMediator mediator, int id) =>
         {
             await mediator.Send(new DeleteProductCommand { Id = id });
             return Results.NoContent();
         });

        return app;
    }
}