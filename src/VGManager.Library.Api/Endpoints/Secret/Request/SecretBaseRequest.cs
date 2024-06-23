using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.Secret.Request;

public record SecretBaseRequest
{
    [Required]
    public string TenantId { get; set; } = null!;

    [Required]
    public string ClientId { get; set; } = null!;

    [Required]
    public string ClientSecret { get; set; } = null!;

    [Required]
    public string UserName { get; set; } = null!;
}
