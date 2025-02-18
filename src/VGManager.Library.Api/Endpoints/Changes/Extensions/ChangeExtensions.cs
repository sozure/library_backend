using VGManager.Library.Api.Endpoints.Changes.Request;
using VGManager.Library.Services.Models.Changes.Requests;

namespace VGManager.Library.Api.Endpoints.Changes.Extensions;

public static class ChangeExtensions
{
    public static VGRequestModel ToModel(this VGChangesRequest request)
        => new()
        {
            ChangeTypes = request.ChangeTypes,
            From = request.From,
            Limit = request.Limit,
            Organization = request.Organization,
            Project = request.Project,
            To = request.To,
            User = request.User
        };

    public static SecretRequestModel ToModel(this SecretChangesRequest request)
        => new()
        {
            From = request.From,
            Limit = request.Limit,
            To = request.To,
            User = request.User,
            KeyVaultName = request.KeyVaultName
        };

    public static KVRequestModel ToModel(this KVChangesRequest request)
        => new()
        {
            From = request.From,
            Limit = request.Limit,
            To = request.To,
            User = request.User
        };
}
