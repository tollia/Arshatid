namespace ArshatidModels.Dtos;

public sealed class UpsertRegistrationRequest
{
    public bool Plus { get; set; } = false;
    public string? Alergies { get; set; } = null!;
}
