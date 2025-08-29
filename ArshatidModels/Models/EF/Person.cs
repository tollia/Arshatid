using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArshatidModels.Models.EF;

[Table("main_person", Schema = "dbo")]
public class Person
{
    [Key]
    [Column("Ssn")]
    [StringLength(10)]
    public string Ssn { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(10)]
    public string? Postalcode { get; set; }

    [StringLength(255)]
    public string? Municipality { get; set; }
}
