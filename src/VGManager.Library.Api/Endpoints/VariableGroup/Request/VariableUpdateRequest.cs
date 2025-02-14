using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public record VariableUpdateRequest : VariableChangeRequest
{
    [Required]
    public string NewValue { get; set; } = null!;
}
