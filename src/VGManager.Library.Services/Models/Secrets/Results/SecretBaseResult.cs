namespace VGManager.Library.Services.Models.Secrets.Results;
public class SecretBaseResult
{
    public string KeyVault { get; set; } = null!;
    public string SecretName { get; set; } = null!;
    public string SecretValue { get; set; } = null!;
}
