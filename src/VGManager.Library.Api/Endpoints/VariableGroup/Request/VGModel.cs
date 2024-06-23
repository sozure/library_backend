namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public record VGModel
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
}
