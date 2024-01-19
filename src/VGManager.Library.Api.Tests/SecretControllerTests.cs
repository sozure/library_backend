using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.Secret;
using VGManager.Library.Api.Endpoints.Secret.Response;
using VGManager.Library.Api.MapperProfiles;
using VGManager.Library.AzureAdapter.Interfaces;
using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;
using VGManager.Library.Services;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class SecretControllerTests
{
    private SecretController _controller;
    private IKeyVaultService _keyVaultService;
    private Mock<IKeyVaultAdapter> _adapter;
    private Mock<ISecretChangeColdRepository> _secretRepository;
    private Mock<IKeyVaultCopyColdRepository> _keyVaultRepository;

    [SetUp]
    public void Setup()
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(SecretProfile));
        });

        var mapper = mapperConfiguration.CreateMapper();

        _adapter = new(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<KeyVaultService>>();

        _keyVaultRepository = new(MockBehavior.Strict);
        _secretRepository = new(MockBehavior.Strict);

        _keyVaultService = new KeyVaultService(_adapter.Object, _secretRepository.Object, _keyVaultRepository.Object, loggerMock.Object);
        _controller = new(_keyVaultService, mapper);
    }

    [Test]
    public async Task GetAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "SecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsEntity = TestSampleData.GetSecretsEntity();
        var secretsGetResponse = TestSampleData.GetSecretsGetResponse();

        _adapter.Setup(x => x.GetSecretsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(secretsEntity);
        _adapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        // Act
        var result = await _controller.GetAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _adapter.Verify(x => x.Setup(keyVaultName, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.GetSecretsAsync(default), Times.Once);
    }

    [Test]
    public void GetDeleted_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsEntity = TestSampleData.GetEmptyDeletedSecretsEntity();
        var secretsGetResponse = TestSampleData.GetEmptySecretsGetResponse1();

        _adapter.Setup(x => x.GetDeletedSecrets(It.IsAny<CancellationToken>())).Returns(secretsEntity);
        _adapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        // Act
        var result = _controller.GetDeleted(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<DeletedSecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _adapter.Verify(x => x.Setup(keyVaultName, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.GetDeletedSecrets(default), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "SecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsEntity1 = TestSampleData.GetSecretsEntity();
        var secretsEntity2 = TestSampleData.GetEmptySecretsEntity();
        var secretsGetResponse = TestSampleData.GetEmptySecretsGetResponse();

        _adapter.SetupSequence(x => x.GetSecretsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(secretsEntity1)
            .ReturnsAsync(secretsEntity2);

        _adapter.Setup(x => x.DeleteSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(AdapterStatus.Success);
        _adapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _secretRepository.Setup(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _adapter.Verify(x => x.Setup(keyVaultName, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.GetSecretsAsync(default), Times.Exactly(2));
        _adapter.Verify(x => x.DeleteSecretAsync(It.IsAny<string>(), default), Times.Exactly(3));
        _secretRepository.Verify(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), default), Times.Once);
    }

    [Test]
    public async Task DeleteInlineAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "SecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);
        var status = AdapterStatus.Success;

        var secretsEntity1 = TestSampleData.GetSecretsEntity();
        var secretsEntity2 = TestSampleData.GetEmptySecretsEntity();

        _adapter.SetupSequence(x => x.GetSecretsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(secretsEntity1)
            .ReturnsAsync(secretsEntity2);

        _adapter.Setup(x => x.DeleteSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(AdapterStatus.Success);
        _adapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _secretRepository.Setup(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteInlineAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(status);

        _adapter.Verify(x => x.Setup(keyVaultName, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.GetSecretsAsync(default), Times.Once);
        _adapter.Verify(x => x.DeleteSecretAsync(It.IsAny<string>(), default), Times.Exactly(3));
        _secretRepository.Verify(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), default), Times.Once);
    }

    [Test]
    public void RecoverAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsEntity = TestSampleData.GetEmptyDeletedSecretsEntity();
        var secretsGetResponse = TestSampleData.GetEmptySecretsGetResponse1();

        _adapter.SetupSequence(x => x.GetDeletedSecrets(It.IsAny<CancellationToken>()))
            .Returns(secretsEntity)
            .Returns(secretsEntity);
        _adapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _secretRepository.Setup(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = _controller.RecoverAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<DeletedSecretResponse>>)((OkObjectResult)result.Result!.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _adapter.Verify(x => x.Setup(keyVaultName, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.GetDeletedSecrets(default), Times.Exactly(2));
        _secretRepository.Verify(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), default), Times.Once);
    }

    [Test]
    public void RecoverInlineAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);
        var secretsEntity = TestSampleData.GetEmptyDeletedSecretsEntity();

        _adapter.SetupSequence(x => x.GetDeletedSecrets(It.IsAny<CancellationToken>()))
            .Returns(secretsEntity);
        _adapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _secretRepository.Setup(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = _controller.RecoverInlineAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!.Result!).Value!).Should().Be(AdapterStatus.Success);

        _adapter.Verify(x => x.Setup(keyVaultName, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.GetDeletedSecrets(default), Times.Once);
        _secretRepository.Verify(x => x.AddEntityAsync(It.IsAny<SecretChangeEntity>(), default), Times.Once);
    }

    [Test]
    public async Task CopyAsync_Works_well()
    {
        // Arrange
        var fromKeyVault = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var ToKeyVault = "ToKeyVault";
        var request = TestSampleData.GetRequest(fromKeyVault, ToKeyVault, tenantId, clientId, clientSecret, true);

        _adapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _adapter.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<KeyVaultSecret>);

        _keyVaultRepository.Setup(
            x => x.AddEntityAsync(It.IsAny<KeyVaultCopyEntity>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CopyAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(AdapterStatus.Success);

        _keyVaultRepository.Verify(x => x.AddEntityAsync(It.IsAny<KeyVaultCopyEntity>(), default), Times.Once);
        _adapter.Verify(x => x.Setup(fromKeyVault, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.Setup(ToKeyVault, tenantId, clientId, clientSecret), Times.Once);
        _adapter.Verify(x => x.GetAllAsync(default), Times.Exactly(2));

    }
}
