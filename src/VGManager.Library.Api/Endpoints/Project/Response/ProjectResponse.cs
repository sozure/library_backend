using System.ComponentModel.DataAnnotations;

namespace VGManager.Library.Api.Endpoints.Project.Response;

public class ProjectResponse
{
    [Required]
    public string Name { get; set; } = null!;
    public IEnumerable<string> SubscriptionIds { get; set; } = null!;
}
