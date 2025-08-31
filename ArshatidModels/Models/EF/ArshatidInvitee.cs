using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArshatidModels.Models.EF;

[Table("Invitee", Schema = "dbo")]
public class ArshatidInvitee
{
    [Key]
    [Column("pk")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Pk { get; set; }

    [Required]
    [StringLength(10)]
    public string Ssn { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string FullName { get; set; } = string.Empty;

    public int ArshatidFk { get; set; }

    [ForeignKey(nameof(ArshatidFk))]
    public virtual ArshatidEvent Event { get; set; } = null!;

    public virtual ICollection<ArshatidRegistration> Registrations { get; set; } = new List<ArshatidRegistration>();
}
