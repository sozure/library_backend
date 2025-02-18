namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public record VariableChangeRequest : VariableRequest
{
    public ExceptionRequest[]? Exceptions { get; set; } = null!;
}
