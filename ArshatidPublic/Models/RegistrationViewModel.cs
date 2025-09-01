namespace ArshatidPublic.Models;

using System.ComponentModel.DataAnnotations;

public class RegistrationViewModel
{
    [Display(Name = "Plús einn")]
    public bool Plus { get; set; }
    [Display(Name = "Fæðuóþol")]
    public string? Alergies { get; set; }
    [Display(Name = "Kostnaðarstaður")]
    public int? ArshatidCostCenterFk { get; set; }
    [Display(Name = "Vegan")]
    public bool Vegan { get; set; }
}
