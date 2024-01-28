//using Azure.Identity;
//using Microsoft.Extensions.Logging;

//namespace VGManager.Library.AzureAdapter.Tests;

//[TestFixture]
//public class KeyVaultAdapterTests
//{
//    private KeyVaultAdapter _vaultAdapter = null!;

//    [SetUp]
//    public void Setup()
//    {
//        var mockLogger = new Mock<ILogger<KeyVaultAdapter>>();
//        _vaultAdapter = new(mockLogger.Object);
//    }

//    [Test]
//    public async Task GetSecretAsync_Unknown_error()
//    {
//        // Arrange
//        var keyVaultName = "KeyVault-BKV";
//        var tenantId = "Ipsum";
//        var clientId = "Novum";
//        var clientSecret = "Lirum";
//        _vaultAdapter.Setup(keyVaultName, tenantId, clientId, clientSecret);

//        // Act
//        var call = () => _vaultAdapter.GetAllAsync(default);

//        // Assert
//        await call.Should().ThrowAsync<AuthenticationFailedException>();
//    }
//}
