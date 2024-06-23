using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Common;

public record ExtendedBasicRequest : BasicRequest
{
    [Required]
    public string Project { get; set; } = null!;
}
