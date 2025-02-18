using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.VariableGroup;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Api.Tests;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.VGRepositories;
using VGManager.Library.Services;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Settings;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class VariableGroupHandlerTests
{
    private IProjectService _projectService;
    private IVariableService _variableService;
    private IVariableGroupService _variableGroupService;
    private Mock<IVGManagerAdapterClientService> _clientService;
    private OperationsDbContext _operationsDbContext = null!;

    [SetUp]
    public void Setup()
    {
        var variableServiceLoggerMock = new Mock<ILogger<VariableService>>();
        var variableGroupServiceLoggerMock = new Mock<ILogger<VariableGroupService>>();

        var settings = Options.Create(new OrganizationSettings
        {
            Organizations = ["Organization1"]
        });

        _operationsDbContext = DbContextTestBase.CreateDatabaseContext();

        _clientService = new(MockBehavior.Strict);
        var adapterCommunicator = new AdapterCommunicator(_clientService.Object);
        var additionColdRepository = new VGAddColdRepository(_operationsDbContext);
        var deletionColdRepository = new VGDeleteColdRepository(_operationsDbContext);
        var editionColdRepository = new VGUpdateColdRepository(_operationsDbContext);

        var variableFilterService = new VariableFilterService();
        _variableService = new VariableService(
            adapterCommunicator,
            additionColdRepository,
            deletionColdRepository,
            editionColdRepository,
            variableFilterService,
            settings,
            variableServiceLoggerMock.Object
            );

        _variableGroupService = new VariableGroupService(
            adapterCommunicator,
            variableGroupServiceLoggerMock.Object
            );

        _projectService = new ProjectService(adapterCommunicator);
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
        var vgResponse = GetVgResponse("Value123", "Value456", "Value789");
        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await VariableGroupHandler.GetAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetAsync_Works_well_2()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var foundProject = "Project1";
        var valueFilter = "value";
        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, "key", valueFilter);
        var projectRes = GetProjectResponse(foundProject);
        var vgResponse = GetVgResponse("Value123", "Value456", "Value789");

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(foundProject);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await VariableGroupHandler.GetAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetAsync_Works_well_3()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var foundProject = "Project1";
        var valueFilter = "value";

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, "Key", valueFilter);
        variableRequest.KeyIsRegex = false;

        var projectRes = GetProjectResponse(foundProject);
        var vgResponse = GetVgResponse("Value123", "Value456", "Value789");

        var variableGroupResponse = new AdapterResponseModel<List<VariableResponse>>()
        {
            Data = [],
            Status = AdapterStatus.Success
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await VariableGroupHandler.GetAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetAsync_Works_well_4()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var foundProject = "Project1";
        string valueFilter = null!;

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, "key", valueFilter);
        var projectRes = GetProjectResponse(foundProject);
        var vgResponse = GetVgResponse("Value123", "Value456", "Value789");

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(foundProject);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await VariableGroupHandler.GetAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetVariableGroupsAsync_works_well_1()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var valueFilter = "Value";
        var foundProject = "Project1";

        var variableRequest = new VariableGroupRequest
        {
            ContainsKey = true,
            VariableGroupFilter = "NeptunAdapter",
            Organization = organization,
            PAT = pat,
            Project = project,
            ValueFilter = valueFilter,
            ContainsSecrets = false,
            KeyFilter = "Key123",
            KeyIsRegex = true,
            UserName = "user"
        };

        var projectRes = GetProjectResponse(foundProject);

        var vgResponse = GetVgResponse();

        var response = new AdapterResponseModel<List<VariableGroupResponse>>()
        {
            Data =
            [
                new()
                {
                    Project = foundProject,
                    VariableGroupName = "NeptunAdapter",
                    VariableGroupType = "VariableGroup",
                }
            ],
            Status = AdapterStatus.Success
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await VariableGroupHandler.GetVariableGroupsAsync(variableRequest, _projectService, _variableGroupService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetVariableGroupsAsync_works_well_2()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        var valueFilter = "Value";

        var variableRequest = new VariableGroupRequest
        {
            ContainsKey = true,
            VariableGroupFilter = "NeptunAdapter",
            Organization = organization,
            PAT = pat,
            Project = project,
            ValueFilter = valueFilter,
            ContainsSecrets = false,
            KeyFilter = "Key123",
            KeyIsRegex = true,
            UserName = "user"
        };
        var vgResponse = GetVgResponse();

        var response = new AdapterResponseModel<List<VariableGroupResponse>>()
        {
            Data =
            [
                new()
                {
                    Project = project,
                    VariableGroupName = "NeptunAdapter",
                    VariableGroupType = "VariableGroup",
                }
            ],
            Status = AdapterStatus.Success
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await VariableGroupHandler.GetVariableGroupsAsync(variableRequest, _projectService, _variableGroupService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
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
        var variableRequest = TestSampleData.GetVariableUpdateRequest("Neptun", organization, pat, project, valueFilter, newValue);
        var vgResponse = GetVgResponse(newValue, newValue, newValue);

        var updateResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project, newValue);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("UpdateVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(updateResponse)));

        // Act
        var result = await VariableGroupHandler.UpdateAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("UpdateVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_All_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var foundProject = "Project1";
        string valueFilter = null!;
        var newValue = "newValue";
        var variableRequest = TestSampleData.GetVariableUpdateRequest("Neptun", organization, pat, project, valueFilter, newValue);

        var projectRes = GetProjectResponse(foundProject);
        var vgResponse = GetVgResponse(newValue, newValue, newValue);

        var updateResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(foundProject, newValue);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("UpdateVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(updateResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        // Act
        var result = await VariableGroupHandler.UpdateAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("UpdateVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), default), Times.Once);
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
        var variableRequest = TestSampleData.GetVariableUpdateRequest("Neptun", organization, pat, project, valueFilter, newValue);

        var updateResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var variableGroupResponse = AdapterStatus.Success;

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("UpdateVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(updateResponse)));

        // Act
        var result = await VariableGroupHandler.UpdateInlineAsync(variableRequest, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("UpdateVGRequest", It.IsAny<string>(), default), Times.Once);
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

        var variableRequest = TestSampleData.GetVariableAddRequest(organization, pat, project, valueFilter, newKey, newValue);

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project, newKey, newValue);

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data =
                [
                    new()
                    {
                        Name = "NeptunAdapter",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Test1"] = newValue,
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Test1"] = newValue
                        }
                    }
                ],
                Status = AdapterStatus.Success
            }
        };

        var addResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("AddVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(addResponse)));

        // Act
        var result = await VariableGroupHandler.AddAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("AddVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task AddInlineAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        string valueFilter = null!;
        var newValue = "newValue";
        var variableRequest = TestSampleData.GetVariableAddRequest(organization, pat, project, valueFilter, newValue, newValue);

        var updateResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var variableGroupResponse = AdapterStatus.Success;

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("AddVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(updateResponse)));

        // Act
        var result = await VariableGroupHandler.AddInlineAsync(variableRequest, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("AddVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        var keyFilter = "Test1";

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, keyFilter, null!);

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponsesAfterDelete();

        var deleteResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data = [],
                Status = AdapterStatus.Success
            }
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(deleteResponse)));

        // Act
        var result = await VariableGroupHandler.DeleteAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_All_works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "All";
        var foundProject = "Project1";
        var keyFilter = "Test1";

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, keyFilter, null!);

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponsesAfterDelete();

        var deleteResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data = [],
                Status = AdapterStatus.Success
            }
        };

        var projectRes = GetProjectResponse(foundProject);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(deleteResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        // Act
        var result = await VariableGroupHandler.DeleteAsync(variableRequest, _projectService, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task DeleteInlineAsync_Works_well()
    {
        // Arrange
        var organization = "Organization1";
        var pat = "WtxMFit1uz1k64u527mB";
        var project = "Project1";
        var keyFilter = "Test1";

        var variableRequest = TestSampleData.GetVariableRequest(organization, pat, project, keyFilter, null!);

        var deleteResponse = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(deleteResponse)));

        // Act
        var result = await VariableGroupHandler.DeleteInlineAsync(variableRequest, _variableService, default);

        // Assert
        result.Should().NotBeNull();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [TearDown]
    public async Task TearDown()
    {
        DbContextTestBase.TearDownDatabaseContext(_operationsDbContext);
        await _operationsDbContext.DisposeAsync();
    }

    private static BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>> GetVgResponse(
        string value,
        string value2,
        string value3
        ) => new()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data =
                [
                    new()
                    {
                        Name = "NeptunAdapter",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key123"] = value,
                            ["Key456"] = value2
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key789"] = value3
                        }
                    }
                ],
                Status = AdapterStatus.Success
            }
        };

    private static BaseResponse<AdapterResponseModel<List<SimplifiedVGResponse<string>>>> GetVgResponse() => new()
    {
        Data = new AdapterResponseModel<List<SimplifiedVGResponse<string>>>()
        {
            Data =
                [
                    new()
                    {
                        Name = "NeptunAdapter",
                        Type = "VariableGroup",
                        Variables = new Dictionary<string, string>
                        {
                            ["Key123"] = "Value123"
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Type = "VariableGroup",
                        Variables = new Dictionary<string, string>
                        {
                            ["Key789"] = "Value789"
                        }
                    }
                ],
            Status = AdapterStatus.Success
        }
    };

    private static BaseResponse<AdapterResponseModel<IEnumerable<ProjectRequest>>> GetProjectResponse(string foundProject) => new()
    {
        Data = new AdapterResponseModel<IEnumerable<ProjectRequest>>()
        {
            Data =
                [
                    new()
                    {
                        Project = new TeamProjectReference()
                        {
                            Name = foundProject
                        }
                    }
                ],
            Status = AdapterStatus.Success
        }
    };
}
