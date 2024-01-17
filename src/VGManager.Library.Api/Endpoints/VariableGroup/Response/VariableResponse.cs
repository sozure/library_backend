
using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.VariableGroup.Response;

public class VariableResponse
{
    [Required]
    public string Project { get; set; } = null!;

    [Required]
    public bool SecretVariableGroup { get; set; }

    [Required]
    public string VariableGroupName { get; set; } = null!;

    [Required]
    public string VariableGroupKey { get; set; } = null!;

    public string? VariableGroupValue { get; set; }

    public string? KeyVaultName { get; set; }
}
