namespace VGManager.Library.Api.Endpoints.Secret.Response;

public record DeletedSecretResponse : SecretBaseResponse
{
    public DateTimeOffset? DeletedOn { get; set; }
}
