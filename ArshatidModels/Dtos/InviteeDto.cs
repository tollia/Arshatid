namespace ArshatidModels.Dtos;

public sealed class InviteeDto
{
    public int InviteeId { get; set; }
    public int EventId { get; set; }
    public int? CostCenterId { get; set; } = 0;
    public string Ssn { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
