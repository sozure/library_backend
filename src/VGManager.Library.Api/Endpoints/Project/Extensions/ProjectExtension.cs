using VGManager.Adapter.Models.Requests;
using VGManager.Library.Api.Endpoints.Project.Response;

namespace VGManager.Library.Api.Endpoints.Project.Extensions;

public static class ProjectExtension
{
    public static ProjectResponse ToResponse(this ProjectRequest request)
        => new()
        {
            Name = request.Project.Name,
            SubscriptionIds = request.SubscriptionIds
        };
}
