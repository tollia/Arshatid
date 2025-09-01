namespace ArshatidModels.Dtos;

public sealed class UpsertRegistrationRequest
{
    public bool Plus { get; set; } = false;
    public string? Alergies { get; set; } = null!;
    public bool Vegan { get; set; } = false;
    public int? ArshatidCostCenterFk { get; set; }
    public string JwtToken { get; set; }
}
