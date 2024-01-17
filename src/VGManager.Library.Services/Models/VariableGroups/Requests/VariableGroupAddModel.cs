namespace VGManager.Library.Services.Models.VariableGroups.Requests;

public class VariableGroupAddModel : VariableGroupModel
{
    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;
}
