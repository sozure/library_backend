namespace VGManager.Library.Services.Models.Changes.Requests;

public class SecretRequestModel : BaseRequestModel
{
    public string KeyVaultName { get; set; } = null!;
}
