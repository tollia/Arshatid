namespace ArshatidPublic.Models;

using System.ComponentModel.DataAnnotations;

public class RegistrationViewModel
{
    [Display(Name = "Plús einn")]
    public bool Plus { get; set; }
    public string? Alergies { get; set; }
}
