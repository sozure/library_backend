using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Entities.VGEntities;

namespace VGManager.Library.Api.Tests;

public static class TestEntitySampleData
{
    public static VGAddEntity GetAddEntity() => new()
    {
        Id = "xyz",
        Key = "DatabaseProvider",
        Value = "PostgreSql",
        Organization = "VGManager",
        Project = "VGManager.Library.Api",
        Date = new DateTime(2024, 2, 28, 10, 0, 0, DateTimeKind.Utc),
        User = "beviktor95@gmail.com",
        VariableGroupFilter = "VGManager.Library.Api"
    };

    public static VGUpdateEntity GetUpdateEntity() => new()
    {
        Id = "xyz",
        Key = "DatabaseProvider",
        Organization = "VGManager",
        Project = "VGManager.Library.Api",
        Date = new DateTime(2024, 2, 28, 11, 0, 0, DateTimeKind.Utc),
        User = "beviktor95@gmail.com",
        VariableGroupFilter = "VGManager.Library.Api",
        NewValue = "PostgreSql"
    };

    public static VGDeleteEntity GetDeleteEntity() => new()
    {
        Id = "xyz",
        Key = "DatabaseProvider",
        Organization = "VGManager",
        Project = "VGManager.Library.Api",
        Date = new DateTime(2024, 2, 28, 12, 0, 0, DateTimeKind.Utc),
        User = "beviktor95@gmail.com",
        VariableGroupFilter = "VGManager.Library.Api",
    };

    public static SecretChangeEntity GetSecretChangeEntity() => new()
    {
        Id = "xyz",
        Date = new DateTime(2024, 2, 28, 13, 0, 0, DateTimeKind.Utc),
        User = "TestUser",
        ChangeType = SecretChangeType.Delete,
        KeyVaultName = "TestKeyVault",
        SecretNameRegex = "TestSecret"
    };

    public static KeyVaultCopyEntity GetKeyVaultCopyEntity() => new()
    {
        Id = "xyz",
        DestinationKeyVault = "TestKeyVault1",
        OriginalKeyVault = "TestKeyVault2",
        Date = new DateTime(2024, 2, 28, 14, 0, 0, DateTimeKind.Utc),
        User = "TestUser"
    };
}
