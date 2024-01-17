namespace VGManager.Library.Services.Models.Secrets.Requests;
public class SecretModel : SecretBaseModel
{
    public string KeyVaultName { get; set; } = null!;
    public string SecretFilter { get; set; } = null!;
}
