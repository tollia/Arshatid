using Arshatid.Databases;
using ArshatidModels.Models.EF;
using Ganss.Excel;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Arshatid.Controllers;

[Route("Events/{eventId:int}/Invitees")]
public class InviteesController : Controller
{
    private readonly ArshatidDbContext _dbContext;
    private readonly GeneralDbContext _generalDbContext;

    public InviteesController(ArshatidDbContext dbContext, GeneralDbContext generalDbContext)
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
        int count = _dbContext.ArshatidInvitees.Count((ArshatidInvitee i) => i.ArshatidFk == eventId);
        ViewBag.Event = evnt;
        ViewBag.Count = count;
        return View();
    }

    [HttpPost("append")]
    public IActionResult Append(int eventId, IFormFile file)
    {
        if (file == null)
        {
            return RedirectToAction("Index", new { eventId = eventId });
        }
        List<ArshatidInvitee> newInvitees = ParseFile(eventId, file);
        List<string> ssns = newInvitees.Select((ArshatidInvitee i) => i.Ssn).ToList();
        List<ArshatidInvitee> existing = _dbContext.ArshatidInvitees
            .Where((ArshatidInvitee i) => i.ArshatidFk == eventId && ssns.Contains(i.Ssn))
            .ToList();
        foreach (ArshatidInvitee ex in existing)
        {
            newInvitees.RemoveAll((ArshatidInvitee i) => i.Ssn == ex.Ssn);
        }
        _dbContext.ArshatidInvitees.AddRange(newInvitees);
        _dbContext.SaveChanges();
        return RedirectToAction("Index", new { eventId = eventId });
    }

    [HttpPost("replace")]
    public IActionResult Replace(int eventId, IFormFile file, [FromQuery] bool confirm = false)
    {
        if (file == null)
        {
            return RedirectToAction("Index", new { eventId = eventId });
        }
        List<ArshatidInvitee> newInvitees = ParseFile(eventId, file);
        List<string> newSsns = newInvitees.Select((ArshatidInvitee i) => i.Ssn).ToList();
        List<ArshatidInvitee> existing = _dbContext.ArshatidInvitees
            .Where((ArshatidInvitee i) => i.ArshatidFk == eventId)
            .ToList();
        List<ArshatidInvitee> toRemove = existing
            .Where((ArshatidInvitee i) => !newSsns.Contains(i.Ssn))
            .ToList();
        if (!confirm)
        {
            List<int> removeIds = toRemove.Select((ArshatidInvitee i) => i.Pk).ToList();
            int conflictCount = _dbContext.ArshatidRegistrations
                .Count((ArshatidRegistration r) => r.Invitee.ArshatidFk == eventId && removeIds.Contains(r.ArshatidInviteeFk));
            if (conflictCount > 0)
            {
                ViewBag.Count = conflictCount;
                ViewBag.EventId = eventId;
                return View("ReplaceConfirm");
            }
        }
        foreach (ArshatidInvitee invitee in toRemove)
        {
            List<ArshatidRegistration> regs = _dbContext.ArshatidRegistrations
                .Where((ArshatidRegistration r) => r.ArshatidInviteeFk == invitee.Pk && r.Invitee.ArshatidFk == eventId)
                .ToList();
            _dbContext.ArshatidRegistrations.RemoveRange(regs);
        }
        _dbContext.ArshatidInvitees.RemoveRange(toRemove);
        List<string> existingSsns = existing.Select((ArshatidInvitee i) => i.Ssn).ToList();
        List<ArshatidInvitee> toAdd = newInvitees
            .Where((ArshatidInvitee i) => !existingSsns.Contains(i.Ssn))
            .ToList();
        _dbContext.ArshatidInvitees.AddRange(toAdd);
        _dbContext.SaveChanges();
        return RedirectToAction("Index", new { eventId = eventId });
    }

    private List<ArshatidInvitee> ParseFile(int eventId, IFormFile file)
    {
        List<ArshatidInvitee> invitees = new List<ArshatidInvitee>();
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension == ".xlsx" || extension == ".xls")
        {
            using Stream stream = file.OpenReadStream();
            ExcelMapper mapper = new ExcelMapper(stream);
            IEnumerable<InviteeRow> rows = mapper.Fetch<InviteeRow>();
            foreach (InviteeRow row in rows)
            {
                string? ssn = row.KT ?? row.Kennitala;
                if (string.IsNullOrWhiteSpace(ssn))
                {
                    continue;
                }
                string name = !string.IsNullOrWhiteSpace(row.Nafn) ? row.Nafn : ssn;
                ArshatidInvitee invitee = new ArshatidInvitee
                {
                    ArshatidFk = eventId,
                    Ssn = ssn,
                    FullName = name
                };
                invitees.Add(invitee);
            }
        }
        else
        {
            using StreamReader reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                string[] parts = line.Split(new[] { ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string ssn = parts[0].Trim();
                string name = parts.Length > 1 ? parts[1].Trim() : ssn;
                ArshatidInvitee invitee = new ArshatidInvitee
                {
                    ArshatidFk = eventId,
                    Ssn = ssn,
                    FullName = name
                };
                invitees.Add(invitee);
            }
        }
        return invitees;
    }

    private class InviteeRow
    {
        public string? KT { get; set; }
        public string? Kennitala { get; set; }
        public string? Nafn { get; set; }
    }
}
