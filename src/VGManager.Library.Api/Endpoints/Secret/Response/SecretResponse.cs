using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.Secret.Response;

public record SecretResponse : SecretBaseResponse
{
    [Required]
    public string CreatedBy { get; set; } = null!;
}
