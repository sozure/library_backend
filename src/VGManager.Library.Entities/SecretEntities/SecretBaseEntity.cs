namespace VGManager.Library.Entities.SecretEntities;

public abstract record SecretBaseEntity
{
    public string Id { get; set; } = null!;
    public string User { get; set; } = null!;
    public DateTime Date { get; set; }
}
