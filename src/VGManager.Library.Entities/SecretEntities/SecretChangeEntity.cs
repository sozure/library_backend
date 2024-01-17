namespace VGManager.Library.Entities.SecretEntities;

public class SecretChangeEntity : SecretBaseEntity
{
    public string KeyVaultName { get; set; } = null!;
    public string SecretNameRegex { get; set; } = null!;
    public SecretChangeType ChangeType { get; set; }
}
