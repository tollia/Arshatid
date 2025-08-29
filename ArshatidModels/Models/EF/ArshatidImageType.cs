using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArshatidModels.Models.EF;

[Table("ArshatidImageType", Schema = "dbo")]
public class ArshatidImageType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Pk { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<ArshatidImage> ArshatidImages { get; set; } = new List<ArshatidImage>();
}
