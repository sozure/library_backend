using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Common;

public class ExtendedBasicRequest : BasicRequest
{
    [Required]
    public string Project { get; set; } = null!;
}
