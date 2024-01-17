namespace VGManager.Library.Services.Models.VariableGroups.Requests;

public class VariableGroupUpdateModel : VariableGroupModel
{
    public string NewValue { get; set; } = null!;
}
