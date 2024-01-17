using VGManager.Library.Entities.SecretEntities;

namespace VGManager.Library.Services.Models.Changes.Responses;

public class SecretOperationModel : BaseOperationModel
{
    public string KeyVaultName { get; set; } = null!;
    public string SecretNameRegex { get; set; } = null!;
    public SecretChangeType ChangeType { get; set; }
}
