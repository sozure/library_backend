using Microsoft.TeamFoundation.Core.WebApi;

namespace VGManager.Library.Services.Models.Projects;

public record ProjectResult
{
    public TeamProjectReference Project { get; set; } = null!;
    public IEnumerable<string> SubscriptionIds { get; set; } = null!;
}
