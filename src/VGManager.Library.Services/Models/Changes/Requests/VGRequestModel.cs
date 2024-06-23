namespace VGManager.Library.Services.Models.Changes.Requests;

public record VGRequestModel : BaseRequestModel
{
    public string Organization { get; set; } = null!;
    public string Project { get; set; } = null!;
    public IEnumerable<ChangeType> ChangeTypes { get; set; } = [];
}
