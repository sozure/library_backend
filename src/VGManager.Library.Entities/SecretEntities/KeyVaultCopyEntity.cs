namespace VGManager.Library.Entities.SecretEntities;

public class KeyVaultCopyEntity : SecretBaseEntity
{
    public string OriginalKeyVault { get; set; } = null!;
    public string DestinationKeyVault { get; set; } = null!;
}
