//using Microsoft.Extensions.Logging;
//using VGManager.Adapter.Models.StatusEnums;

//namespace VGManager.Library.AzureAdapter.Tests;

//[TestFixture]
//public class ProjectAdapterTests
//{
//    private ProjectAdapter _projectAdapter = null!;

//    [SetUp]
//    public void Setup()
//    {
//        var mockLogger = new Mock<ILogger<ProjectAdapter>>();
//        _projectAdapter = new(mockLogger.Object);
//    }

//    [Test]
//    public async Task GetProjectsAsync_Unknown_error()
//    {
//        // Arrange
//        var baseUrl = "baseUrl";
//        var pat = "pat";


//        // Act
//        var result = await _projectAdapter.GetProjectsAsync(baseUrl, pat, default);

//        // Assert
//        result.Status.Should().Be(AdapterStatus.Unknown);
//    }

//    [Test]
//    public async Task GetProjectsAsync_Unauthorized()
//    {
//        // Arrange
//        var baseUrl = "https://dev.azure.com/beviktor95";
//        var pat = "pat";

//        // Act
//        var result = await _projectAdapter.GetProjectsAsync(baseUrl, pat, default);

//        // Assert
//        result.Status.Should().Be(AdapterStatus.Unauthorized);
//    }
//}
