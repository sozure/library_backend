using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.Changes.Request;

public abstract class BaseRequest
{
    [Required]
    public DateTime From { get; set; }
    [Required]
    public DateTime To { get; set; }
    [Required]
    public int Limit { get; set; }
    public string? User { get; set; } = null!;
}
