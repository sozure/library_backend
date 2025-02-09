namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public record VariableChangeRequest: VariableRequest
{
    public ExceptionModel[]? Exceptions { get; set; } = null!;
}

public record ExceptionModel
{
    public required string VariableGroupName { get; set; }
    public string? VariableKey { get; set; }
}
