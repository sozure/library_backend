using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.Library.Api.Endpoints.VariableGroup;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Api.MapperProfiles;
using VGManager.Library.AzureAdapter.Interfaces;
using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Models.Models;
using VGManager.Library.Models.StatusEnums;
using VGManager.Library.Repositories.Interfaces.VGRepositories;
using VGManager.Library.Services;
using VGManager.Library.Services.Settings;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class VariableGroupControllerTests
{
    private VariableGroupController _controller;
    private Mock<IVariableGroupAdapter> _variableGroupAdapter;
    private Mock<IProjectAdapter> _projectAdapter;
    private Mock<IVGAddColdRepository> _additionColdRepository;
    private Mock<IVGDeleteColdRepository> _deletionColdRepository;
    private Mock<IVGUpdateColdRepository> _editionColdRepository;

    [SetUp]
    public void Setup()
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(VariableGroupProfile));
        });

        var serviceMapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(Library.Services.MapperProfiles.ProjectProfile));
        });

        var mapper = mapperConfiguration.CreateMapper();
        var serviceMapper = serviceMapperConfiguration.CreateMapper();

        _variableGroupAdapter = new(MockBehavior.Strict);
        _projectAdapter = new(MockBehavior.Strict);
        _additionColdRepository = new(MockBehavior.Strict);
        _deletionColdRepository = new(MockBehavior.Strict);
        _editionColdRepository = new(MockBehavior.Strict);

        var variableServiceLoggerMock = new Mock<ILogger<VariableService>>();
        var variableGroupServiceLoggerMock = new Mock<ILogger<VariableGroupService>>();
        var variableFilterLoggerMock = new Mock<ILogger<VariableFilterService>>();

        var settings = Options.Create(new OrganizationSettings
        {
            Organizations = new string[] { "Organization1" }
        });

        var variableFilterService = new VariableFilterService(variableFilterLoggerMock.Object);

        var variableService = new VariableService(
            _variableGroupAdapter.Object,
            _additionColdRepository.Object,
            _deletionColdRepository.Object,
            _editionColdRepository.Object,
            variableFilterService,
            settings,
            variableServiceLoggerMock.Object
            );

        var vgService = new VariableGroupService(
            variableFilterService,
            _variableGroupAdapter.Object,
            variableGroupServiceLoggerMock.Object
            );

        var projectService = new ProjectService(_projectAdapter.Object, serviceMapper);

        _controller = new(variableService, vgService, projectService, mapper);
    }

    [Test]
    public async Task GetAsync_Works_well_1()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        var valueFilter = "value";

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, "key", valueFilter);

        var variableGroupEntity = TestSampleData.GetVariableGroupEntity();
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _variableGroupAdapter.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity);

        // Act
        var result = await _controller.GetAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, project, pat), Times.Once);
    }

    [Test]
    public async Task GetAsync_Works_well_2()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project);

        var variableGroupEntity = TestSampleData.GetVariableGroupEntity();
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _variableGroupAdapter.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity);

        // Act
        var result = await _controller.GetAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, project, pat), Times.Once);
    }

    [Test]
    public async Task GetAsync_All_works_well_1()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var valueFilter = "value";
        var firstProjectName = "Project1";
        var secondProjectName = "Project2";
        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, "key", valueFilter);

        var projectEntity = TestSampleData.GetProjectEntity(firstProjectName, secondProjectName);

        var variableGroupEntity = TestSampleData.GetVariableGroupEntity();
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(firstProjectName);
        var variableGroupResponse2 = TestSampleData.GetVariableGroupGetResponses(secondProjectName);

        variableGroupResponse.Data.AddRange(variableGroupResponse2.Data);

        _projectAdapter.Setup(x => x.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectEntity);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity);

        // Act
        var result = await _controller.GetAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.Setup(organization, firstProjectName, pat), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, secondProjectName, pat), Times.Once);
        _projectAdapter.Verify(x => x.GetProjectsAsync($"https://dev.azure.com/{organization}", pat, default), Times.Once);
    }

    [Test]
    public async Task GetAsync_All_works_well_2()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var valueFilter = "value";
        var firstProjectName = "Project1";
        var secondProjectName = "Project2";
        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, "key", valueFilter);

        var projectEntity = TestSampleData.GetProjectEntity(firstProjectName, secondProjectName);

        var variableGroupEntity = TestSampleData.GetVariableGroupEntity(AdapterStatus.Unknown);

        _projectAdapter.Setup(x => x.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectEntity);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity);

        // Act
        var result = await _controller.GetAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Status.Should().Be(AdapterStatus.Unknown);
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Data.ToList().Count.Should().Be(0);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.Setup(organization, firstProjectName, pat), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, secondProjectName, pat), Times.Once);
        _projectAdapter.Verify(x => x.GetProjectsAsync($"https://dev.azure.com/{organization}", pat, default), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        string valueFilter = null!;
        var newValue = "newValue";
        var statusResult = AdapterStatus.Success;

        var variableRequest = TestSampleData.GetVariableUpdateRequest("neptun", organization, pat, project, valueFilter, newValue);

        var variableGroupEntity1 = TestSampleData.GetVariableGroupEntity();
        var variableGroupEntity2 = TestSampleData.GetVariableGroupEntity(newValue);
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project, newValue);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity1)
            .ReturnsAsync(variableGroupEntity2);

        _editionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGUpdateEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Exactly(1));
        _variableGroupAdapter.Verify(x => x.Setup(organization, project, pat), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_All_works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        string valueFilter = null!;
        var newValue = "newValue";
        var statusResult = AdapterStatus.Success;
        var firstProjectName = "Project1";
        var secondProjectName = "Project2";

        var variableRequest = TestSampleData.GetVariableUpdateRequest("neptun", organization, pat, project, valueFilter, newValue);
        var projectEntity = TestSampleData.GetProjectEntity(firstProjectName, secondProjectName);

        var variableGroupEntity1 = TestSampleData.GetVariableGroupEntity();
        var variableGroupEntity2 = TestSampleData.GetVariableGroupEntity(newValue);
        var variableGroupEntity3 = TestSampleData.GetVariableGroupEntity();
        var variableGroupEntity4 = TestSampleData.GetVariableGroupEntity(newValue);

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(firstProjectName, newValue);
        var variableGroupResponse2 = TestSampleData.GetVariableGroupGetResponses(secondProjectName, newValue);

        variableGroupResponse.Data.AddRange(variableGroupResponse2.Data);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _projectAdapter.Setup(x => x.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectEntity);

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity1)
            .ReturnsAsync(variableGroupEntity2)
            .ReturnsAsync(variableGroupEntity3)
            .ReturnsAsync(variableGroupEntity4);

        _editionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGUpdateEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(4));
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.Setup(organization, firstProjectName, pat), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, secondProjectName, pat), Times.Once);
        _projectAdapter.Verify(x => x.GetProjectsAsync($"https://dev.azure.com/{organization}", pat, default), Times.Once);
    }

    [Test]
    public async Task UpdateInlineAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        string valueFilter = null!;
        var newValue = "newValue";
        var statusResult = AdapterStatus.Success;

        var variableRequest = TestSampleData.GetVariableUpdateRequest("neptunadapter", organization, pat, project, valueFilter, newValue);
        var variableGroupEntity = TestSampleData.GetVariableGroupEntity();

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity);

        _editionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGUpdateEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateInlineAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(statusResult);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Once);
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Exactly(1));
        _variableGroupAdapter.Verify(x => x.Setup(organization, project, pat), Times.Once);
    }

    [Test]
    public async Task AddAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        string valueFilter = null!;
        var newKey = "Test1";
        var newValue = "Test1";
        var statusResult = AdapterStatus.Success;

        var variableRequest = TestSampleData.GetVariableAddRequest(organization, pat, project, valueFilter, newKey, newValue);

        var variableGroupEntity1 = TestSampleData.GetVariableGroupEntity();
        var variableGroupEntity2 = TestSampleData.GetVariableGroupEntity(newKey, newValue);
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project, newKey, newValue);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity1)
            .ReturnsAsync(variableGroupEntity2);

        _additionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGAddEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.Setup(organization, project, pat), Times.Once);
        _additionColdRepository.Verify(x => x.AddEntityAsync(It.IsAny<VGAddEntity>(), default), Times.Once);
    }

    [Test]
    public async Task AddAsync_All_works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        string valueFilter = null!;
        var newKey = "Test1";
        var newValue = "Test1";
        var statusResult = AdapterStatus.Success;
        var firstProjectName = "Project1";
        var secondProjectName = "Project2";

        var variableRequest = TestSampleData.GetVariableAddRequest(organization, pat, project, valueFilter, newKey, newValue);
        var projectEntity = TestSampleData.GetProjectEntity(firstProjectName, secondProjectName);

        var variableGroupEntity1 = TestSampleData.GetVariableGroupEntity();
        var variableGroupEntity2 = TestSampleData.GetVariableGroupEntity(newKey, newValue);
        var variableGroupEntity3 = TestSampleData.GetVariableGroupEntity();
        var variableGroupEntity4 = TestSampleData.GetVariableGroupEntity(newKey, newValue);
        var variableGroupResponse1 = TestSampleData.GetVariableGroupGetResponses(firstProjectName, newKey, newValue);
        var variableGroupResponse2 = TestSampleData.GetVariableGroupGetResponses(secondProjectName, newKey, newValue);

        variableGroupResponse1.Data.AddRange(variableGroupResponse2.Data);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _projectAdapter.Setup(x => x.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectEntity);

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity1)
            .ReturnsAsync(variableGroupEntity2)
            .ReturnsAsync(variableGroupEntity3)
            .ReturnsAsync(variableGroupEntity4);

        _additionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGAddEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse1);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(4));
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Exactly(4));
        _variableGroupAdapter.Verify(x => x.Setup(organization, firstProjectName, pat), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, secondProjectName, pat), Times.Once);
        _projectAdapter.Verify(x => x.GetProjectsAsync($"https://dev.azure.com/{organization}", pat, default), Times.Once);
        _additionColdRepository.Verify(x => x.AddEntityAsync(It.IsAny<VGAddEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task DeleteAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        var keyFilter = "Test1";
        var entityValue = "Value1";
        var statusResult = AdapterStatus.Success;

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, keyFilter, null!);

        var variableGroupEntity1 = TestSampleData.GetVariableGroupEntity(keyFilter, entityValue);
        var variableGroupEntity2 = TestSampleData.GetVariableGroupEntityAfterDelete();
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponsesAfterDelete();

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity1)
            .ReturnsAsync(variableGroupEntity2);

        _deletionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGDeleteEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Exactly(2));
        _variableGroupAdapter.Verify(x => x.Setup(organization, project, pat), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_All_works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var keyFilter = "Test1";
        var entityValue = "Value1";
        var statusResult = AdapterStatus.Success;
        var firstProjectName = "Project1";
        var secondProjectName = "Project2";

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, keyFilter, null!);
        var projectEntity = TestSampleData.GetProjectEntity(firstProjectName, secondProjectName);
        var variableGroupEntity1 = TestSampleData.GetVariableGroupEntity(keyFilter, entityValue);
        var variableGroupEntity2 = TestSampleData.GetVariableGroupEntityAfterDelete();
        var variableGroupEntity3 = TestSampleData.GetVariableGroupEntity(keyFilter, entityValue);
        var variableGroupEntity4 = TestSampleData.GetVariableGroupEntityAfterDelete();
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponsesAfterDelete();

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _projectAdapter.Setup(x => x.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectEntity);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity1)
            .ReturnsAsync(variableGroupEntity2)
            .ReturnsAsync(variableGroupEntity3)
            .ReturnsAsync(variableGroupEntity4);

        _deletionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGDeleteEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Exactly(4));
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Exactly(4));
        _variableGroupAdapter.Verify(x => x.Setup(organization, firstProjectName, pat), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, secondProjectName, pat), Times.Once);
        _projectAdapter.Verify(x => x.GetProjectsAsync($"https://dev.azure.com/{organization}", pat, default), Times.Once);
    }

    [Test]
    public async Task DeleteInlineAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        var keyFilter = "Test1";
        var entityValue = "Value1";
        var statusResult = AdapterStatus.Success;

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, keyFilter, null!);

        variableRequest.VariableGroupFilter = "NeptunAdapter";

        var variableGroupEntity1 = TestSampleData.GetVariableGroupEntity(keyFilter, entityValue);

        _variableGroupAdapter.Setup(x => x.Setup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        _variableGroupAdapter.Setup(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResult);

        _variableGroupAdapter.SetupSequence(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(variableGroupEntity1);

        _deletionColdRepository.Setup(x => x.AddEntityAsync(It.IsAny<VGDeleteEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteInlineAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(statusResult);

        _variableGroupAdapter.Verify(x => x.GetAllAsync(default), Times.Once);
        _variableGroupAdapter.Verify(x => x.UpdateAsync(It.IsAny<VariableGroupParameters>(), It.IsAny<int>(), default), Times.Once);
        _variableGroupAdapter.Verify(x => x.Setup(organization, project, pat), Times.Once);
    }
}
