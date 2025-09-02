using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArshatidModels.Models.EF;

[Table("ArshatidInvitee", Schema = "dbo")]
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

    [StringLength(255)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Gender { get; set; }

    [ForeignKey(nameof(ArshatidFk))]
    public virtual Arshatid Event { get; set; } = null!;

    public virtual ICollection<ArshatidRegistration> Registrations { get; set; } = new List<ArshatidRegistration>();
}
