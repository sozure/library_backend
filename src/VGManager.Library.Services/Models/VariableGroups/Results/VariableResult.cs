namespace VGManager.Library.Services.Models.VariableGroups.Results;

public record VariableResult
{
    public string Project { get; set; } = null!;
    public bool SecretVariableGroup { get; set; }
    public string VariableGroupName { get; set; } = null!;
    public string VariableGroupKey { get; set; } = null!;
    public string? VariableGroupValue { get; set; }
    public string? KeyVaultName { get; set; }
}
