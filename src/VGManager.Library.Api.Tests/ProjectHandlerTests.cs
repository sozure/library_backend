using System.Text.Json;
using Microsoft.TeamFoundation.Core.WebApi;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Common;
using VGManager.Library.Api.Endpoints.Project;
using VGManager.Library.Api.Endpoints.Project.Response;
using VGManager.Library.Services;
using VGManager.Library.Services.Interfaces;
using AdapterProjectRequest = VGManager.Adapter.Models.Requests.ProjectRequest;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class ProjectHandlerTests
{
    private IProjectService _projectService;
    private Mock<IVGManagerAdapterClientService> _clientService;

    [SetUp]
    public void Setup()
    {
        _clientService = new(MockBehavior.Strict);

        var adapterCommunicator = new AdapterCommunicator(_clientService.Object);
        _projectService = new ProjectService(adapterCommunicator);
    }

    [Test]
    public async Task GetAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var url = $"https://dev.azure.com/{organization}";
        var pat = "WtxMFit1uz1k64u527mB";
        var firstProjectName = "Project1";
        var secondProjectName = "Project2";

        var request = new BasicRequest
        {
            Organization = organization,
            PAT = pat
        };

        var projectsResponse = new AdapterResponseModel<IEnumerable<ProjectResponse>>
        {
            Status = AdapterStatus.Success,
            Data =
            [
                new()
                {
                    Name = firstProjectName,
                    SubscriptionIds = []
                },
                new()
                {
                    Name = secondProjectName,
                    SubscriptionIds = []
                }
            ]
        };

        var projectRes = GetProjectResponse(firstProjectName, secondProjectName);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        // Act
        var result = await ProjectHandler.GetAsync(request, _projectService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), default), Times.Once);
    }

    private static BaseResponse<AdapterResponseModel<IEnumerable<AdapterProjectRequest>>> GetProjectResponse(
        string foundProject1,
        string foundProject2
        )
    {
        return new BaseResponse<AdapterResponseModel<IEnumerable<AdapterProjectRequest>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<AdapterProjectRequest>>()
            {
                Data =
                [
                    new()
                    {
                        Project = new TeamProjectReference()
                        {
                            Name = foundProject1
                        }
                    },
                    new()
                    {
                        Project = new TeamProjectReference()
                        {
                            Name = foundProject2
                        }
                    }
                ],
                Status = AdapterStatus.Success
            }
        };
    }
}
