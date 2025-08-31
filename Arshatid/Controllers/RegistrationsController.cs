using Arshatid.Databases;
using ArshatidModels.Models.EF;
using Ganss.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
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
        int inviteeCount = registrations.Count;
        int plusCount = registrations.Sum((ArshatidRegistration r) => r.Plus);
        Dictionary<DateTime, int> histogram = registrations
            .GroupBy((ArshatidRegistration r) => DateTime.Today)
            .ToDictionary((IGrouping<DateTime, ArshatidRegistration> g) => g.Key,
                          (IGrouping<DateTime, ArshatidRegistration> g) => g.Count());
        ViewBag.Event = evnt;
        ViewBag.InviteeCount = inviteeCount;
        ViewBag.PlusCount = plusCount;
        ViewBag.Histogram = histogram;
        return View();
    }

    [HttpGet("export")]
    public IActionResult Export(int eventId, string format)
    {
        List<ArshatidRegistration> registrations = _dbContext.ArshatidRegistrations
            .Where((ArshatidRegistration r) => r.Invitee.ArshatidFk == eventId)
            .ToList();
        List<RegistrationExport> rows = new List<RegistrationExport>();
        foreach (ArshatidRegistration reg in registrations)
        {
            string name = _generalDbContext.Person
                .FirstOrDefault((Person p) => p.Ssn == reg.Invitee.Ssn)?.Name ?? reg.Invitee.Ssn;
            RegistrationExport row = new RegistrationExport
            {
                Ssn = reg.Invitee.Ssn,
                Name = name,
                Plus = reg.Plus,
                EventId = reg.Invitee.ArshatidFk
            };
            rows.Add(row);
        }
        if (format == "xlsx")
        {
            ExcelMapper mapper = new ExcelMapper();
            MemoryStream stream = new MemoryStream();
            mapper.Save(stream, rows, "Registrations");
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "registrations.xlsx");
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Ssn,Name,Plus,EventId");
        foreach (RegistrationExport row in rows)
        {
            sb.AppendLine($"{row.Ssn},{row.Name},{row.Plus},{row.EventId}");
        }
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "registrations.csv");
    }

    private class RegistrationExport
    {
        public string Ssn { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Plus { get; set; }
        public int EventId { get; set; }
    }
}
