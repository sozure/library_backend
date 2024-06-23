using System.ComponentModel.DataAnnotations;
using VGManager.Library.Services.Models.Changes;

namespace VGManager.Library.Api.Endpoints.Changes.Request;

public record SecretChangesRequest : BaseRequest
{
    [Required]
    public string KeyVaultName { get; set; } = null!;
    [Required]
    public IEnumerable<ChangeType> ChangeTypes { get; set; } = Array.Empty<ChangeType>();
}
