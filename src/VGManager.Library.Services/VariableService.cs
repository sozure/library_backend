using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.Library.AzureAdapter.Interfaces;
using VGManager.Library.Repositories.Interfaces.VGRepositories;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.VariableGroups.Requests;
using VGManager.Library.Services.Settings;

namespace VGManager.Library.Services;

public partial class VariableService : IVariableService
{
    private readonly IVariableGroupAdapter _variableGroupConnectionRepository;
    private readonly IVGAddColdRepository _additionColdRepository;
    private readonly IVGDeleteColdRepository _deletionColdRepository;
    private readonly IVGUpdateColdRepository _editionColdRepository;
    private readonly IVariableFilterService _variableFilterService;
    private readonly OrganizationSettings _organizationSettings;
    private string _project = null!;
    private readonly ILogger _logger;

    private readonly string SecretVGType = "AzureKeyVault";

    public VariableService(
        IVariableGroupAdapter variableGroupConnectionRepository,
        IVGAddColdRepository additionColdRepository,
        IVGDeleteColdRepository deletedColdRepository,
        IVGUpdateColdRepository editionColdRepository,
        IVariableFilterService variableFilterService,
        IOptions<OrganizationSettings> organizationSettings,
        ILogger<VariableService> logger
        )
    {
        _variableGroupConnectionRepository = variableGroupConnectionRepository;
        _additionColdRepository = additionColdRepository;
        _deletionColdRepository = deletedColdRepository;
        _editionColdRepository = editionColdRepository;
        _variableFilterService = variableFilterService;
        _organizationSettings = organizationSettings.Value;
        _logger = logger;
    }

    public void SetupConnectionRepository(VariableGroupModel variableGroupModel)
    {
        var project = variableGroupModel.Project;
        _variableGroupConnectionRepository.Setup(
            variableGroupModel.Organization,
            project,
            variableGroupModel.PAT
            );
        _project = project;
    }

    private static VariableGroupParameters GetVariableGroupParameters(VariableGroup filteredVariableGroup, string variableGroupName)
    {
        return new()
        {
            Name = variableGroupName,
            Variables = filteredVariableGroup.Variables,
            Description = filteredVariableGroup.Description,
            Type = filteredVariableGroup.Type,
        };
    }
}
