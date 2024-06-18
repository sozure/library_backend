using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.Changes;
using VGManager.Library.Api.Endpoints.Changes.Request;
using VGManager.Library.Api.MapperProfiles;
using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.SecretRepositories;
using VGManager.Library.Repositories.VGRepositories;
using VGManager.Library.Services;
using VGManager.Library.Services.Models.Changes;
using VGManager.Library.Services.Models.Changes.Responses;
using ServiceChangesProfile = VGManager.Library.Services.MapperProfiles.ChangesProfile;

namespace VGManager.Library.Api.Tests;

[TestFixture]
public class ChangeControllerTests
{
    private ChangesController _controller;
    private OperationsDbContext _operationsDbContext = null!;

    [SetUp]
    public void Setup()
    {
        var apiMapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(ChangesProfile));
        });

        var apiMapper = apiMapperConfiguration.CreateMapper();

        var serviceMapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(ServiceChangesProfile));
        });

        var serviceMapper = serviceMapperConfiguration.CreateMapper();

        _operationsDbContext = DbContextTestBase.CreateDatabaseContext();

        var addRepo = new VGAddColdRepository(_operationsDbContext);
        var deleteRepo = new VGDeleteColdRepository(_operationsDbContext);
        var updateRepo = new VGUpdateColdRepository(_operationsDbContext);
        var keyVaultCopy = new KeyVaultCopyColdRepository(_operationsDbContext);
        var secretRepo = new SecretChangeColdRepository(_operationsDbContext);
        var changesService = new ChangeService(addRepo, updateRepo, deleteRepo, keyVaultCopy, secretRepo, serviceMapper);
        _controller = new(changesService, apiMapper);
    }

    [Test]
    public async Task GetVariableChangesAsync_Works_well()
    {
        // Arrange
        var request = new VGChangesRequest
        {
            ChangeTypes = [ChangeType.Add, ChangeType.Delete, ChangeType.Update],
            From = new DateTime(2024, 2, 27, 10, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc),
            User = "beviktor95@gmail.com",
            Limit = 10,
            Organization = "VGManager",
            Project = "VGManager.Library.Api"
        };

        var response = new RepositoryResponseModel<VGOperationModel>()
        {
            Data = GetVGOperations(),
            Status = RepositoryStatus.Success
        };

        // Act
        var result = await _controller.GetVariableChangesAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((RepositoryResponseModel<VGOperationModel>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task GetVariableChangesAsync_Works_well_2()
    {
        // Arrange
        var request = new VGChangesRequest
        {
            ChangeTypes = [ChangeType.Add, ChangeType.Delete, ChangeType.Update],
            From = new DateTime(2024, 2, 27, 10, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc),
            Limit = 10,
            Organization = "VGManager",
            Project = "VGManager.Library.Api"
        };

        var response = new RepositoryResponseModel<VGOperationModel>()
        {
            Data = GetVGOperations(),
            Status = RepositoryStatus.Success
        };

        // Act
        var result = await _controller.GetVariableChangesAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((RepositoryResponseModel<VGOperationModel>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task GetSecretChangesAsync_Works_well()
    {
        // Arrange
        var request = new SecretChangesRequest
        {
            From = new DateTime(2024, 2, 27, 10, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc),
            User = "TestUser",
            Limit = 10,
            KeyVaultName = "TestKeyVault"
        };

        var response = new RepositoryResponseModel<SecretOperationModel>()
        {
            Data = GetSecretOperations(),
            Status = RepositoryStatus.Success
        };

        // Act
        var result = await _controller.GetSecretChangesAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((RepositoryResponseModel<SecretOperationModel>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task GetSecretChangesAsync_Works_well_2()
    {
        // Arrange
        var request = new SecretChangesRequest
        {
            From = new DateTime(2024, 2, 27, 10, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc),
            Limit = 10,
            KeyVaultName = "TestKeyVault"
        };

        var response = new RepositoryResponseModel<SecretOperationModel>()
        {
            Data = GetSecretOperations(),
            Status = RepositoryStatus.Success
        };

        // Act
        var result = await _controller.GetSecretChangesAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((RepositoryResponseModel<SecretOperationModel>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task GetKVChangesAsync_Works_well()
    {
        // Arrange
        var request = new KVChangesRequest
        {
            From = new DateTime(2024, 2, 27, 10, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc),
            User = "TestUser",
            Limit = 10,
        };

        var response = new RepositoryResponseModel<KVOperationModel>()
        {
            Data = GetKVCopyOperations(),
            Status = RepositoryStatus.Success
        };

        // Act
        var result = await _controller.GetKVChangesAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((RepositoryResponseModel<KVOperationModel>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task GetKVChangesAsync_Works_well_2()
    {
        // Arrange
        var request = new KVChangesRequest
        {
            From = new DateTime(2024, 2, 27, 10, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc),
            Limit = 10,
        };

        var response = new RepositoryResponseModel<KVOperationModel>()
        {
            Data = GetKVCopyOperations(),
            Status = RepositoryStatus.Success
        };

        // Act
        var result = await _controller.GetKVChangesAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((RepositoryResponseModel<KVOperationModel>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);
    }

    [TearDown]
    public async Task TearDown()
    {
        DbContextTestBase.TearDownDatabaseContext(_operationsDbContext);
        await _operationsDbContext.DisposeAsync();
    }

    private static List<KVOperationModel> GetKVCopyOperations() => [
                new()
                {
                    Id = "xyz",
                    DestinationKeyVault = "TestKeyVault1",
                    OriginalKeyVault = "TestKeyVault2",
                    Date = new DateTime(2024, 2, 28, 14, 0, 0, DateTimeKind.Utc),
                    User = "TestUser"
                }
        ];

    private static List<SecretOperationModel> GetSecretOperations() => [
                new()
                {
                    Id = "xyz",
                    Date = new DateTime(2024, 2, 28, 13, 0, 0, DateTimeKind.Utc),
                    User = "TestUser",
                    ChangeType = SecretChangeType.Delete,
                    KeyVaultName = "TestKeyVault",
                    SecretNameRegex = "TestSecret"
                }
            ];

    private static List<VGOperationModel> GetVGOperations() => [
                new()
                {
                    Id = "xyz",
                    Key = "DatabaseProvider",
                    Organization = "VGManager",
                    Project = "VGManager.Library.Api",
                    Date = new DateTime(2024, 2, 28, 10, 0, 0, DateTimeKind.Utc),
                    User = "beviktor95@gmail.com",
                    VariableGroupFilter = "VGManager.Library.Api",
                    Type = "Add"
                },
                new()
                {
                    Id = "xyz",
                    Key = "DatabaseProvider",
                    Organization = "VGManager",
                    Project = "VGManager.Library.Api",
                    Date = new DateTime(2024, 2, 28, 11, 0, 0, DateTimeKind.Utc),
                    User = "beviktor95@gmail.com",
                    VariableGroupFilter = "VGManager.Library.Api",
                    Type = "Update"
                },
                new()
                {
                    Id = "xyz",
                    Key = "DatabaseProvider",
                    Organization = "VGManager",
                    Project = "VGManager.Library.Api",
                    Date = new DateTime(2024, 2, 28, 12, 0, 0, DateTimeKind.Utc),
                    User = "beviktor95@gmail.com",
                    VariableGroupFilter = "VGManager.Library.Api",
                    Type = "Delete"
                }
            ];
}
