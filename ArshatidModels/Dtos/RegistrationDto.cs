namespace ArshatidModels.Dtos;

public sealed class RegistrationDto
{
    public int RegistrationId { get; set; }
    public int Plus { get; set; }
    public bool Vegan { get; set; }
    public string Alergies { get; set; } = string.Empty;
    public int? ArshatidCostCenterFk { get; set; }
    public InviteeDto Invitee { get; set; } = new InviteeDto();
}
