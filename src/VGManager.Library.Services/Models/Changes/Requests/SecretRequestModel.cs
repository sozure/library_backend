namespace VGManager.Library.Services.Models.Changes.Requests;

public record SecretRequestModel : BaseRequestModel
{
    public string KeyVaultName { get; set; } = null!;
}
