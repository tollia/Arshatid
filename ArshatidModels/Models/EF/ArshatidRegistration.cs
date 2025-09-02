using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArshatidModels.Models.EF;

[Index(nameof(ArshatidInviteeFk), IsUnique = true, Name = "unq_ArshatidRegistration")]
[Table("ArshatidRegistration", Schema = "dbo")]
public class ArshatidRegistration
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Pk { get; set; }

    public int Plus { get; set; } = 0;

    [StringLength(255)]
    public string? Alergies { get; set; }

    public bool Vegan { get; set; } = false;

    public int? ArshatidCostCenterFk { get; set; }

    [Required]
    public int ArshatidInviteeFk { get; set; }

    [ForeignKey(nameof(ArshatidInviteeFk))]
    public virtual ArshatidInvitee Invitee { get; set; } = null!;

    [ForeignKey(nameof(ArshatidCostCenterFk))]
    public virtual ArshatidCostCenter? CostCenter { get; set; }
}
