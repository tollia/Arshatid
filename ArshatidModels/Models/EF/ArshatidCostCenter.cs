using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArshatidModels.Models.EF;

[Table("ArshatidCostCenter", Schema = "dbo")]
public class ArshatidCostCenter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Pk { get; set; }

    [Required]
    [StringLength(255)]
    public string Corporation { get; set; } = string.Empty;

    [Required]
    public int OrgUnitId { get; set; }

    [Required]
    [StringLength(255)]
    public string OrgUnitName { get; set; } = string.Empty;

    [Required]
    public bool IsDivision { get; set; } = false;

    [Required]
    [StringLength(255)]
    public string CostCenterName { get; set; } = string.Empty;

    [Required]
    public int CostCenterCode { get; set; }

    public virtual ICollection<ArshatidRegistration> Registrations { get; set; } = new List<ArshatidRegistration>();
}

