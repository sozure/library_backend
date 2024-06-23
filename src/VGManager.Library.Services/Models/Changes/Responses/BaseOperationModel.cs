namespace VGManager.Library.Services.Models.Changes.Responses;

public abstract record BaseOperationModel
{
    public string Id { get; set; } = null!;
    public string User { get; set; } = null!;
    public DateTime Date { get; set; }
}
