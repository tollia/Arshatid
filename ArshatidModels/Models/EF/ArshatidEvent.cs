using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArshatidModels.Models.EF;

[Table("Arshatid", Schema = "dbo")]
public class ArshatidEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Pk { get; set; }

    [Required]
    public DateTime EventTime { get; set; }

    [Required]
    [StringLength(255)]
    public string Heading { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? SendDescription { get; set; }

    [Required]
    public DateTime RegistrationStartTime { get; set; }

    [Required]
    public DateTime RegistrationEndTime { get; set; }

    public virtual ICollection<ArshatidImage> Images { get; set; } = new List<ArshatidImage>();
    public virtual ICollection<ArshatidInvitee> Invitees { get; set; } = new List<ArshatidInvitee>();
    public virtual ICollection<ArshatidRegistration> Registrations { get; set; } = new List<ArshatidRegistration>();
}
