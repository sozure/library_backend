namespace VGManager.Library.Services.Models.Changes.Responses;

public record VGOperationModel : BaseOperationModel
{
    public string Type { get; set; } = null!;
    public string Organization { get; set; } = null!;
    public string Project { get; set; } = null!;
    public string VariableGroupFilter { get; set; } = null!;
    public string Key { get; set; } = null!;
}
