using Arshatid.Databases;
using ArshatidModels.Models.EF;
using Ganss.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Arshatid.Controllers;

[Route("Events/{eventId:int}/Registrations")]
public class RegistrationsController : Controller
{
    private readonly ArshatidDbContext _dbContext;
    private readonly GeneralDbContext _generalDbContext;

    public RegistrationsController(ArshatidDbContext dbContext, GeneralDbContext generalDbContext)
    {
        _dbContext = dbContext;
        _generalDbContext = generalDbContext;
    }

    [HttpGet]
    public IActionResult Index(int eventId)
    {
        ArshatidModels.Models.EF.Arshatid? evnt = _dbContext.ArshatidEvents.FirstOrDefault((ArshatidModels.Models.EF.Arshatid e) => e.Pk == eventId);
        if (evnt == null)
        {
            return NotFound();
        }
        List<ArshatidRegistration> registrations = _dbContext.ArshatidRegistrations
            .Where((ArshatidRegistration r) => r.Invitee.ArshatidFk == eventId)
            .ToList();
        int inviteeCount = _dbContext.ArshatidInvitees
            .Count((ArshatidInvitee i) => i.ArshatidFk == eventId);
        int registeredCount = registrations.Count;
        int plusCount = registrations.Sum((ArshatidRegistration r) => r.Plus);
        Dictionary<DateTime, int> histogram = registrations
            .GroupBy((ArshatidRegistration r) => DateTime.Today)
            .ToDictionary((IGrouping<DateTime, ArshatidRegistration> g) => g.Key,
                          (IGrouping<DateTime, ArshatidRegistration> g) => g.Count());
        ViewBag.Event = evnt;
        ViewBag.InviteeCount = inviteeCount;
        ViewBag.RegisteredCount = registeredCount;
        ViewBag.PlusCount = plusCount;
        ViewBag.Histogram = histogram;
        ViewBag.EventId = eventId;
        return View();
    }

    [HttpGet("export")]
    public IActionResult Export(int eventId, string format)
    {
        var registrations = _dbContext.ArshatidRegistrations
            .Include(r => r.Invitee)
            .Include(r => r.CostCenter)
            .Where(r => r.Invitee.ArshatidFk == eventId)
            .ToList();

        var rows = registrations.Select(reg => new RegistrationExport
        {
            Ssn = reg.Invitee.Ssn,
            FullName = reg.Invitee.FullName,
            Plus = reg.Plus,
            Alergies = reg.Alergies,
            Vegan = reg.Vegan,
            Phone = reg.Invitee.Phone,
            Email = reg.Invitee.Email,
            Gender = reg.Invitee.Gender,
            OrgUnitName = reg.CostCenter?.OrgUnitName,
            CostCenterName = reg.CostCenter?.CostCenterName,
            CostCenterCode = reg.CostCenter?.CostCenterCode
        }).ToList();

        if (format == "xlsx")
        {
            var mapper = new ExcelMapper();
            var stream = new MemoryStream();
            mapper.Save(stream, rows, "Registrations");
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "registrations.xlsx");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Kennitala,Nafn,Gestur,Fæðuóþol,Vegan,Sími,Tölvupóstur,Kyn,Skipulagseining,Vinnustaður,Kostnaðarlykill");
        foreach (var row in rows)
        {
            sb.AppendLine($"{row.Ssn},{row.FullName},{row.Plus},{row.Alergies},{row.Vegan},{row.Phone},{row.Email},{row.Gender},{row.OrgUnitName},{row.CostCenterName},{row.CostCenterCode}");
        }

        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "registrations.csv");
    }

    private class RegistrationExport
    {
        [Display(Name = "Kennitala")]
        public string Ssn { get; set; } = string.Empty;
        [Display(Name = "Nafn")]
        public string FullName { get; set; } = string.Empty;
        [Display(Name = "Gestur")]
        public int Plus { get; set; }
        [Display(Name = "Fæðuóþol")]
        public string? Alergies { get; set; }
        [Display(Name = "Vegan")]
        public bool Vegan { get; set; }
        [Display(Name = "Sími")]
        public string? Phone { get; set; }
        [Display(Name = "Tölvupóstur")]
        public string? Email { get; set; }
        [Display(Name = "Kyn")]
        public string? Gender { get; set; }
        [Display(Name = "Skipulagseining")]
        public string? OrgUnitName { get; set; }
        [Display(Name = "Vinnustaður")]
        public string? CostCenterName { get; set; }
        [Display(Name = "Kostnaðarlykill")]
        public int? CostCenterCode { get; set; }
    }
}
