namespace VGManager.Library.Services.Models.Secrets.Requests;
public record SecretModel : SecretBaseModel
{
    public string KeyVaultName { get; set; } = null!;
    public string SecretFilter { get; set; } = null!;
}
