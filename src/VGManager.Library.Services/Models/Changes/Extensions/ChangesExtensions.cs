using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Services.Models.Changes.Responses;

namespace VGManager.Library.Services.Models.Changes.Extensions;
public static class ChangesExtensions
{
    public static VGOperationModel ToModel(this VGEntity entity)
        => new()
        {
            User = entity.User,
            Date = entity.Date,
            Id = entity.Id,
            Key = entity.Key,
            Organization = entity.Organization,
            Project = entity.Project,
            Type = ChangeType.None.ToString(),
            VariableGroupFilter = entity.VariableGroupFilter
        };

    public static VGOperationModel ToModel(this VGUpdateEntity entity)
        => new()
        {
            User = entity.User,
            Date = entity.Date,
            Id = entity.Id,
            Key = entity.Key,
            Organization = entity.Organization,
            Project = entity.Project,
            Type = ChangeType.Update.ToString(),
            VariableGroupFilter = entity.VariableGroupFilter
        };

    public static VGOperationModel ToModel(this VGAddEntity entity)
        => new()
        {
            User = entity.User,
            Date = entity.Date,
            Id = entity.Id,
            Key = entity.Key,
            Organization = entity.Organization,
            Project = entity.Project,
            Type = ChangeType.Add.ToString(),
            VariableGroupFilter = entity.VariableGroupFilter
        };

    public static VGOperationModel ToModel(this VGDeleteEntity entity)
        => new()
        {
            User = entity.User,
            Date = entity.Date,
            Id = entity.Id,
            Key = entity.Key,
            Organization = entity.Organization,
            Project = entity.Project,
            Type = ChangeType.Delete.ToString(),
            VariableGroupFilter = entity.VariableGroupFilter
        };

    public static SecretOperationModel ToModel(this SecretChangeEntity entity)
        => new()
        {
            ChangeType = entity.ChangeType,
            Date = entity.Date,
            Id = entity.Id,
            KeyVaultName = entity.KeyVaultName,
            SecretNameRegex = entity.SecretNameRegex,
            User = entity.User
        };

    public static KVOperationModel ToModel(this KeyVaultCopyEntity entity)
        => new()
        {
            Id = entity.Id,
            User = entity.User,
            Date = entity.Date,
            OriginalKeyVault = entity.OriginalKeyVault,
            DestinationKeyVault = entity.DestinationKeyVault
        };
}
