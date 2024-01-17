namespace VGManager.Library.Entities.SecretEntities;

public abstract class SecretBaseEntity
{
    public string Id { get; set; } = null!;
    public string User { get; set; } = null!;
    public DateTime Date { get; set; }
}
