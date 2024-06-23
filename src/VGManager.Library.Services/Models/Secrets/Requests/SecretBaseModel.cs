namespace VGManager.Library.Services.Models.Secrets.Requests;
public record SecretBaseModel
{
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string UserName { get; set; } = null!;
}
