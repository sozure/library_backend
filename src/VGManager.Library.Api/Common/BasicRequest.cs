using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Common;

public record BasicRequest
{
    [Required]
    public string Organization { get; set; } = null!;
    [Required]
    public string PAT { get; set; } = null!;
}
