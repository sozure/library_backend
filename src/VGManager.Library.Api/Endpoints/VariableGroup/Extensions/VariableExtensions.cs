using VGManager.Adapter.Models.Requests.VG;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Services.Models.VariableGroups.Results;

namespace VGManager.Library.Api.Endpoints.VariableGroup.Extensions;

public static class VariableExtensions
{
    public static VariableGroupUpdateModel ToModel(this VariableUpdateRequest req)
        => new()
        {
            Organization = req.Organization,
            PAT = req.PAT,
            Project = req.Project,
            UserName = req.UserName,
            VariableGroupFilter = req.VariableGroupFilter,
            ContainsSecrets = req.ContainsSecrets,
            Exceptions = req.Exceptions?.Select(x => x.ToModel()).ToArray(),
            NewValue = req.NewValue,
            KeyFilter = req.KeyFilter,
            KeyIsRegex = req.KeyIsRegex,
            ValueFilter = req.ValueFilter
        };

    public static VariableGroupAddModel ToModel(this VariableAddRequest req)
        => new()
        {
            Organization = req.Organization,
            PAT = req.PAT,
            Project = req.Project,
            UserName = req.UserName,
            VariableGroupFilter = req.VariableGroupFilter,
            ContainsSecrets = req.ContainsSecrets,
            Exceptions = req.Exceptions?.Select(x => x.ToModel()).ToArray(),
            KeyFilter = req.KeyFilter,
            KeyIsRegex = req.KeyIsRegex,
            ValueFilter = req.ValueFilter,
            Key = req.Key,
            Value = req.Value
        };

    public static VariableGroupChangeModel ToModel(this VariableChangeRequest req)
        => new()
        {
            Organization = req.Organization,
            PAT = req.PAT,
            Project = req.Project,
            UserName = req.UserName,
            VariableGroupFilter = req.VariableGroupFilter,
            ContainsSecrets = req.ContainsSecrets,
            Exceptions = req.Exceptions?.Select(x => x.ToModel()).ToArray(),
            KeyFilter = req.KeyFilter,
            KeyIsRegex = req.KeyIsRegex,
            ValueFilter = req.ValueFilter
        };

    public static ExceptionModel ToModel(this ExceptionRequest req)
        => new() { VariableGroupName = req.VariableGroupName, VariableKey = req.VariableKey };

    public static VariableResponse ToResponse(this VariableResult result)
        => new()
        {
            KeyVaultName = result.KeyVaultName,
            Project = result.Project,
            SecretVariableGroup = result.SecretVariableGroup,
            VariableGroupKey = result.VariableGroupKey,
            VariableGroupName = result.VariableGroupName,
            VariableGroupValue = result.VariableGroupValue
        };

    public static VariableGroupModel ToModel(this VariableRequest request)
        => new()
        {
            ContainsSecrets = request.ContainsSecrets,
            KeyFilter = request.KeyFilter,
            KeyIsRegex = request.KeyIsRegex,
            PAT = request.PAT,
            Organization = request.Organization,
            Project = request.Project,
            UserName = request.UserName,
            ValueFilter = request.ValueFilter,
            VariableGroupFilter = request.VariableGroupFilter
        };
}
