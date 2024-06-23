namespace VGManager.Library.Services.Models.Changes.Responses;

public record KVOperationModel : BaseOperationModel
{
    public string OriginalKeyVault { get; set; } = null!;
    public string DestinationKeyVault { get; set; } = null!;
}
