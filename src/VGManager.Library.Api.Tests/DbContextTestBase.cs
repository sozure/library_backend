using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VGManager.Library.Repositories.DbContexts;

namespace VGManager.Library.Api.Tests;

public static class DbContextTestBase
{
    public static OperationsDbContext CreateDatabaseContext()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var dbOptions = new DbContextOptionsBuilder<OperationsDbContext>()
            .EnableSensitiveDataLogging()
            .UseSqlite(connection)
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning))
            .Options;

        using var dbContext = new OperationsDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var addition = TestEntitySampleData.GetAddEntity();
        var deletion = TestEntitySampleData.GetDeleteEntity();
        var update = TestEntitySampleData.GetUpdateEntity();

        dbContext.VGAdditions.Add(addition);
        dbContext.VGDeletions.Add(deletion);
        dbContext.VGEditions.Add(update);
        dbContext.SaveChanges();

        return new OperationsDbContext(dbOptions);
    }

    public static void TearDownDatabaseContext(OperationsDbContext dbContext)
    {
        dbContext.VGAdditions.RemoveRange(dbContext.VGAdditions);
        dbContext.VGDeletions.RemoveRange(dbContext.VGDeletions);
        dbContext.VGEditions.RemoveRange(dbContext.VGEditions);
        dbContext.SaveChanges();
        dbContext.Database.CloseConnection();
        dbContext.Dispose();
    }
}
