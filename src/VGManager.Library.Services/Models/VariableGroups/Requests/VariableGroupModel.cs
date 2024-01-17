using VGManager.Library.Services.Models.Common;

namespace VGManager.Library.Services.Models.VariableGroups.Requests;

public class VariableGroupModel : BaseModel
{
    public string UserName { get; set; } = null!;

    public string Project { get; set; } = null!;

    public string VariableGroupFilter { get; set; } = null!;

    public string KeyFilter { get; set; } = null!;

    public bool ContainsSecrets { get; set; }

    public bool? KeyIsRegex { get; set; }

    public string? ValueFilter { get; set; }
}
