namespace VGManager.Library.Api.Endpoints.Secret.Response;

public class DeletedSecretResponse : SecretBaseResponse
{
    public DateTimeOffset? DeletedOn { get; set; }
}
