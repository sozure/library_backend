using VGManager.Adapter.Models.Requests.VG;
using VGManager.Library.Api.Common;

namespace VGManager.Library.Api.Endpoints.Project.Extensions;

public static class BaseExtension
{
    public static BaseModel ToModel(this BasicRequest request)
        => new()
        {
            Organization = request.Organization,
            PAT = request.PAT
        };
}
