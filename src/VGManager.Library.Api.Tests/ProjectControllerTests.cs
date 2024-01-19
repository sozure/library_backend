using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VGManager.Library.Api.Endpoints.Project;
using VGManager.Library.Api.Endpoints.Project.Response;
using VGManager.Library.Api.MapperProfiles;
using VGManager.Library.AzureAdapter.Interfaces;
using VGManager.Library.Models.Models;
using VGManager.Library.Models.StatusEnums;
using VGManager.Library.Services;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class ProjectControllerTests
{
    private ProjectController _controller;
    private Mock<IProjectAdapter> _adapter;

    [SetUp]
    public void Setup()
    {
        var apiMapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(ProjectProfile));
        });

        var serviceMapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(Library.Services.MapperProfiles.ProjectProfile));
        });

        var apiMapper = apiMapperConfiguration.CreateMapper();
        var serviceMapper = serviceMapperConfiguration.CreateMapper();

        _adapter = new Mock<IProjectAdapter>(MockBehavior.Strict);
        var projectService = new ProjectService(_adapter.Object, serviceMapper);

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

        var request = new ProjectRequest
        {
            Organization = organization,
            PAT = pat
        };

        var projectEntity = TestSampleData.GetProjectEntity(firstProjectName, secondProjectName);

        var projectsResponse = new AdapterResponseModel<IEnumerable<ProjectResponse>>
        {
            Status = AdapterStatus.Success,
            Data = new List<ProjectResponse>()
            {
                new ProjectResponse()
                {
                    Name = "Project1",
                    SubscriptionIds = Enumerable.Empty<string>()
                },
                new ProjectResponse()
                {
                    Name = "Project2",
                    SubscriptionIds = Enumerable.Empty<string>()
                }
            }
        };

        _adapter.Setup(x => x.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(projectEntity);

        // Act
        var result = await _controller.GetAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<ProjectResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(projectsResponse);

        _adapter.Verify(x => x.GetProjectsAsync(url, pat, default), Times.Once);
    }
}
