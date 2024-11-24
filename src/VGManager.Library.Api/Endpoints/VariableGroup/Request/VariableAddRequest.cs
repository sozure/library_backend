using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public record VariableAddRequest : VariableChangeRequest
{
    [Required]
    public string Key { get; set; } = null!;

    [Required]
    public string Value { get; set; } = null!;
}
