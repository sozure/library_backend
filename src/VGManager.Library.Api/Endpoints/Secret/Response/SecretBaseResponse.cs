using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.Secret.Response;

public abstract class SecretBaseResponse
{
    [Required]
    public string KeyVault { get; set; } = null!;

    [Required]
    public string SecretName { get; set; } = null!;

    [Required]
    public string SecretValue { get; set; } = null!;
}
