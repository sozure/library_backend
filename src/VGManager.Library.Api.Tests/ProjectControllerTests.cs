using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Text.Json;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Common;
using VGManager.Library.Api.Endpoints.Project;
using VGManager.Library.Api.Endpoints.Project.Response;
using VGManager.Library.Api.MapperProfiles;
using VGManager.Library.Services;

using AdapterProjectRequest = VGManager.Adapter.Models.Requests.ProjectRequest;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class ProjectControllerTests
{
    private ProjectController _controller;
    private Mock<IVGManagerAdapterClientService> _clientService;

    [SetUp]
    public void Setup()
    {
        var apiMapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(ProjectProfile));
        });

        var apiMapper = apiMapperConfiguration.CreateMapper();

        _clientService = new(MockBehavior.Strict);

        var adapterCommunicator = new AdapterCommunicator(_clientService.Object);
        var projectService = new ProjectService(adapterCommunicator);

        _controller = new(projectService, apiMapper);
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
            Data = new List<ProjectResponse>()
            {
                new()
                {
                    Name = firstProjectName,
                    SubscriptionIds = Enumerable.Empty<string>()
                },
                new()
                {
                    Name = secondProjectName,
                    SubscriptionIds = Enumerable.Empty<string>()
                }
            }
        };

        var projectRes = GetProjectResponse(firstProjectName, secondProjectName);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        // Act
        var result = await _controller.GetAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<ProjectResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(projectsResponse);

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
                Data = new List<AdapterProjectRequest>()
                {
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
                },
                Status = AdapterStatus.Success
            }
        };
    }
}
