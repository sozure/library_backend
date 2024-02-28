using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.Json;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.VariableGroup;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Api.MapperProfiles;
using VGManager.Library.Api.Tests;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.VGRepositories;
using VGManager.Library.Services;
using VGManager.Library.Services.Settings;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class VariableGroupControllerTests
{
    private VariableGroupController _controller;
    private AdapterCommunicator _adapterCommunicator;
    private Mock<IVGManagerAdapterClientService> _clientService;
    private VGAddColdRepository _additionColdRepository;
    private VGDeleteColdRepository _deletionColdRepository;
    private VGUpdateColdRepository _editionColdRepository;

    private OperationsDbContext _operationsDbContext = null!;

    [SetUp]
    public void Setup()
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(VariableGroupProfile));
        });

        var mapper = mapperConfiguration.CreateMapper();

        var variableServiceLoggerMock = new Mock<ILogger<VariableService>>();
        var variableGroupServiceLoggerMock = new Mock<ILogger<VariableGroupService>>();

        var settings = Options.Create(new OrganizationSettings
        {
            Organizations = ["Organization1"]
        });

        _operationsDbContext = DbContextTestBase.CreateDatabaseContext();

        _clientService = new(MockBehavior.Strict);
        _adapterCommunicator = new(_clientService.Object);
        _additionColdRepository = new(_operationsDbContext);
        _deletionColdRepository = new(_operationsDbContext);
        _editionColdRepository = new(_operationsDbContext);

        var variableFilterService = new VariableFilterService();
        var variableService = new VariableService(
            _adapterCommunicator,
            _additionColdRepository,
            _deletionColdRepository,
            _editionColdRepository,
            variableFilterService,
            settings,
            variableServiceLoggerMock.Object
            );

        var vgService = new VariableGroupService(
            _adapterCommunicator,
            variableGroupServiceLoggerMock.Object
            );

        var projectService = new ProjectService(_adapterCommunicator);
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

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data = new List<SimplifiedVGResponse<string>>()
                {
                    new()
                    {
                        Name = "NeptunAdapter",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key123"] = "Value123",
                            ["Key456"] = "Value456"
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key789"] = "Value789"
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(project);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await _controller.GetAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

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

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data = new List<SimplifiedVGResponse<string>>()
                {
                    new()
                    {
                        Name = "NeptunAdapter",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key123"] = "Value123",
                            ["Key456"] = "Value456"
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key789"] = "Value789"
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        var variableGroupResponse = TestSampleData.GetVariableGroupGetResponses(foundProject);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetProjectsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(projectRes)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        // Act
        var result = await _controller.GetAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

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

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<VariableGroup>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<VariableGroup>>()
            {
                Data = new List<VariableGroup>()
                {
                    new()
                    {
                        Name = "NeptunAdapter",
                        Type = "VariableGroup",
                        Variables = new Dictionary<string, VariableValue>
                        {
                            ["Key123"] = new() { Value = "Value123" }
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Type = "VariableGroup",
                        Variables = new Dictionary<string, VariableValue>
                        {
                            ["Key789"] = new() { Value = "Value789" }
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

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
        var result = await _controller.GetVariableGroupsAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableGroupResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);

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

        var vgResponse = new BaseResponse<AdapterResponseModel<List<VariableGroup>>>()
        {
            Data = new AdapterResponseModel<List<VariableGroup>>()
            {
                Data =
                [
                    new()
                    {
                        Name = "NeptunAdapter",
                        Type = "VariableGroup",
                        Variables = new Dictionary<string, VariableValue>
                        {
                            ["Key123"] = new() { Value = "Value123" }
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Type = "VariableGroup",
                        Variables = new Dictionary<string, VariableValue>
                        {
                            ["Key789"] = new() { Value = "Value789" }
                        }
                    }
                ],
                Status = AdapterStatus.Success
            }
        };

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
        var result = await _controller.GetVariableGroupsAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<VariableGroupResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(response);

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

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data = new List<SimplifiedVGResponse<string>>()
                {
                    new()
                    {
                        Name = "NeptunAdapter",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key123"] = newValue,
                            ["Key456"] = newValue
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key789"] = newValue
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

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
        var result = await _controller.UpdateAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

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

        var vgResponse = new BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>()
            {
                Data = new List<SimplifiedVGResponse<string>>()
                {
                    new()
                    {
                        Name = "NeptunAdapter",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key123"] = newValue,
                            ["Key456"] = newValue
                        }
                    },
                    new()
                    {
                        Name = "NeptunApi",
                        Variables = new Dictionary<string, string>()
                        {
                            ["Key789"] = newValue
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

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
        var result = await _controller.UpdateAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

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
        var result = await _controller.UpdateInlineAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(variableGroupResponse);

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
                Data = new List<SimplifiedVGResponse<string>>()
                {
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
                },
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
        var result = await _controller.AddAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

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
        var result = await _controller.AddInlineAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(variableGroupResponse);

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
                Data = Enumerable.Empty<SimplifiedVGResponse<string>>(),
                Status = AdapterStatus.Success
            }
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetAllVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(vgResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(deleteResponse)));

        // Act
        var result = await _controller.DeleteAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

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
                Data = Enumerable.Empty<SimplifiedVGResponse<string>>(),
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
        var result = await _controller.DeleteAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<List<VariableResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(variableGroupResponse);

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
        var result = await _controller.DeleteInlineAsync(variableRequest, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(AdapterStatus.Success);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteVGRequest", It.IsAny<string>(), default), Times.Once);
    }

    [TearDown]
    public async Task TearDown()
    {
        DbContextTestBase.TearDownDatabaseContext(_operationsDbContext);
        await _operationsDbContext.DisposeAsync();
    }

    private static BaseResponse<AdapterResponseModel<IEnumerable<ProjectRequest>>> GetProjectResponse(string foundProject)
    {
        return new BaseResponse<AdapterResponseModel<IEnumerable<ProjectRequest>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<ProjectRequest>>()
            {
                Data = new List<ProjectRequest>()
                {
                    new()
                    {
                        Project = new TeamProjectReference()
                        {
                            Name = foundProject
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };
    }
}
