namespace VGManager.Library.Services.Models.Changes.Requests;

public class VGRequestModel : BaseRequestModel
{
    public string Organization { get; set; } = null!;
    public string Project { get; set; } = null!;
    public IEnumerable<ChangeType> ChangeTypes { get; set; } = Array.Empty<ChangeType>();
}
