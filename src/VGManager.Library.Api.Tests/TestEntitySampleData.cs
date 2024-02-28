using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Entities.VGEntities;

namespace VGManager.Library.Api.Tests;

public static class TestEntitySampleData
{
    public static VGAddEntity GetAddEntity()
    {
        return new()
        {
            Id = "xyz",
            Key = "DatabaseProvider",
            Value = "PostgreSql",
            Organization = "VGManager",
            Project = "VGManager.Library.Api",
            Date = DateTime.Now,
            User = "beviktor95@gmail.com",
            VariableGroupFilter = "VGManager.Library.Api"
        };
    }

    public static VGUpdateEntity GetUpdateEntity()
    {
        return new()
        {
            Id = "xyz",
            Key = "DatabaseProvider",
            Organization = "VGManager",
            Project = "VGManager.Library.Api",
            Date = DateTime.Now,
            User = "beviktor95@gmail.com",
            VariableGroupFilter = "VGManager.Library.Api",
            NewValue = "PostgreSql"
        };
    }

    public static VGDeleteEntity GetDeleteEntity()
    {
        return new()
        {
            Id = "xyz",
            Key = "DatabaseProvider",
            Organization = "VGManager",
            Project = "VGManager.Library.Api",
            Date = DateTime.Now,
            User = "beviktor95@gmail.com",
            VariableGroupFilter = "VGManager.Library.Api",
        };
    }

    public static SecretChangeEntity GetSecretChangeEntity()
    {
        return new()
        {
            Id = "xyz",
            Date = DateTime.Now,
            User = "TestUser",
            ChangeType = SecretChangeType.Delete,
            KeyVaultName = "TestKeyVault",
            SecretNameRegex = "TestSecret"
        };
    }

    public static KeyVaultCopyEntity GetKeyVaultCopyEntity()
    {
        return new()
        {
            Id = "xyz",
            DestinationKeyVault = "TestKeyVault1",
            OriginalKeyVault = "TestKeyVault2",
            Date = DateTime.Now,
            User = "TestUser"
        };
    }
}
