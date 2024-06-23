namespace VGManager.Library.Services.Models.Secrets.Requests;
public record SecretCopyModel : SecretBaseModel
{
    public string FromKeyVault { get; set; } = null!;

    public string ToKeyVault { get; set; } = null!;

    public bool OverrideSecret { get; set; }
}
