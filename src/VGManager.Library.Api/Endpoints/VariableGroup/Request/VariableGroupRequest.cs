using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.VariableGroup.Request;

public class VariableGroupRequest : VariableRequest
{
    [Required]
    public bool ContainsKey { get; set; }

    public VGModel[]? PotentialVariableGroups { get; set; }
}
