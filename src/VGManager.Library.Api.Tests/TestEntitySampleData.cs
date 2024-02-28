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
}
