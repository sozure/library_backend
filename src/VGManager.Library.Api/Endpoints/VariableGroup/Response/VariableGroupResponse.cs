using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.VariableGroup.Response;

public record VariableGroupResponse
{
    [Required]
    public string Project { get; set; } = null!;

    [Required]
    public string VariableGroupName { get; set; } = null!;

    [Required]
    public string VariableGroupType { get; set; } = null!;
}
