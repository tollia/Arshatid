using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArshatidModels.Models.EF;

[Index(nameof(ArshatidFk), nameof(Ssn), IsUnique = true, Name = "unq_ArshatidRegistrations")]
[Table("ArshatidRegistration", Schema = "dbo")]
public class ArshatidRegistration
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Pk { get; set; }

    [Required]
    [StringLength(10)]
    public string Ssn { get; set; } = string.Empty;

    public int Plus { get; set; }

    public int ArshatidFk { get; set; }

    [StringLength(255)]
    public string? Alergies { get; set; }

    public int ArshatidInviteeFk { get; set; }

    [ForeignKey(nameof(ArshatidFk))]
    public virtual ArshatidEvent ArshatidEvent { get; set; } = null!;

    [ForeignKey(nameof(ArshatidInviteeFk))]
    public virtual ArshatidInvitee ArshatidInvitee { get; set; } = null!;
}
