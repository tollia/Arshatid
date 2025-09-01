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

    public int? ArshatidCostCenterFk { get; set; }

    [ForeignKey(nameof(ArshatidFk))]
    public virtual Arshatid Event { get; set; } = null!;

    [ForeignKey(nameof(ArshatidCostCenterFk))]
    public virtual ArshatidCostCenter? CostCenter { get; set; }

    public virtual ICollection<ArshatidRegistration> Registrations { get; set; } = new List<ArshatidRegistration>();
}
