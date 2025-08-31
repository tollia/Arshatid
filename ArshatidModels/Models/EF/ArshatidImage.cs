using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArshatidModels.Models.EF;

[Table("ArshatidImage", Schema = "dbo")]
public class ArshatidImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Pk { get; set; }

    [Column("ArshatifFk")]
    public int ArshatidFk { get; set; }

    [Required]
    [StringLength(255)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] ImageData { get; set; } = [];

    public int ImageTypeFk { get; set; }

    [ForeignKey(nameof(ArshatidFk))]
    public virtual Arshatid Event { get; set; } = null!;

    [ForeignKey(nameof(ImageTypeFk))]
    public virtual ArshatidImageType ImageType { get; set; } = null!;
}
