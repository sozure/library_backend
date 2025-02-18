namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public sealed record ExceptionRequest
{
    public required string VariableGroupName { get; set; }
    public string? VariableKey { get; set; }
}
