namespace VGManager.Library.Entities.VGEntities;

public record VGUpdateEntity : VGEntity
{
    public string NewValue { get; set; } = null!;
}
