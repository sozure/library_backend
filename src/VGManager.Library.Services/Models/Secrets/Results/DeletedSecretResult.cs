namespace VGManager.Library.Services.Models.Secrets.Results;

public class DeletedSecretResult : SecretBaseResult
{
    public DateTimeOffset? DeletedOn { get; set; }
}
