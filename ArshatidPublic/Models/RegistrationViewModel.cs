namespace ArshatidPublic.Models;

using System.ComponentModel.DataAnnotations;

public class RegistrationViewModel
{
    [Display(Name = "Plús einn")]
    public bool Plus { get; set; }
    [Display(Name = "Fæðuóþol")]
    public string? Alergies { get; set; }
    [Display(Name = "Vinnustaður")]
    public int? ArshatidCostCenterFk { get; set; }
    [Display(Name = "Vegan")]
    public bool Vegan { get; set; }
    public string JwtToken { get; set; } = string.Empty;
}
