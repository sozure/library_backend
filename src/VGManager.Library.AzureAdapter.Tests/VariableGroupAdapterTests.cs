//using Microsoft.Extensions.Logging;
//using Microsoft.TeamFoundation.DistributedTask.WebApi;
//using VGManager.Adapter.Models.StatusEnums;

//namespace VGManager.Library.AzureAdapter.Tests;

//public class VariableGroupAdapterTests
//{
//    private VariableGroupAdapter _variableGroupAdapter = null!;

//    [SetUp]
//    public void Setup()
//    {
//        var mockLogger = new Mock<ILogger<VariableGroupAdapter>>();
//        _variableGroupAdapter = new(mockLogger.Object);
//    }

//    [Test]
//    public async Task GetAllAsync_UnAuthorized()
//    {
//        // Arrange
//        var organization = "beviktor95";
//        var project = "bkv";
//        var pat = "pat";
//        _variableGroupAdapter.Setup(organization, project, pat);

//        // Act
//        var result = await _variableGroupAdapter.GetAllAsync(default);

//        // Assert
//        result.Status.Should().Be(AdapterStatus.Unauthorized);
//    }

//    [Test]
//    public async Task UpdateAsync_UnAuthorized()
//    {
//        // Arrange
//        var organization = "beviktor95";
//        var project = "bkv";
//        var pat = "pat";
//        _variableGroupAdapter.Setup(organization, project, pat);

//        var variableGroupParameters = new VariableGroupParameters
//        {
//            Name = "Name"
//        };

//        // Act
//        var result = await _variableGroupAdapter.UpdateAsync(variableGroupParameters, 0, default);

//        // Assert
//        result.Should().Be(AdapterStatus.Unauthorized);
//    }
//}
