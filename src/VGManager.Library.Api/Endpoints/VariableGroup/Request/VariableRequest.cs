using System.ComponentModel.DataAnnotations;
using VGManager.Library.Api.Common;

namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public record VariableRequest : ExtendedBasicRequest
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string VariableGroupFilter { get; set; } = null!;

    [Required]
    public string KeyFilter { get; set; } = null!;

    [Required]
    public bool ContainsSecrets { get; set; }

    public bool? KeyIsRegex { get; set; }

    public string? ValueFilter { get; set; }
}
