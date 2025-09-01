namespace ArshatidModels.Dtos;

public sealed class RegistrationDto
{
    public int RegistrationId { get; set; }
    public int Plus { get; set; }
    public InviteeDto Invitee { get; set; } = new InviteeDto();
}
