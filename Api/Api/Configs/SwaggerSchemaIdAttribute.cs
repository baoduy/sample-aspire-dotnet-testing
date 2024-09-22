namespace Api.Configs;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class SwaggerSchemaIdAttribute(string schemaId) : Attribute
{
    public string SchemaId { get; init; } = schemaId;
}