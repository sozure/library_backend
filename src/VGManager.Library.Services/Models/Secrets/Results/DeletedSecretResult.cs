namespace VGManager.Library.Services.Models.Secrets.Results;

public record DeletedSecretResult : SecretBaseResult
{
    public DateTimeOffset? DeletedOn { get; set; }
}
