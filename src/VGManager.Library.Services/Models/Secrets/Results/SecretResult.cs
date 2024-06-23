namespace VGManager.Library.Services.Models.Secrets.Results;

public record SecretResult : SecretBaseResult
{
    public string CreatedBy { get; set; } = null!;
}
