using Arshatid.Databases;
using ArshatidModels.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arshatid.Controllers;

[Route("[controller]")]
public class EventsController : Controller
{
    private readonly ArshatidDbContext _dbContext;

    public EventsController(ArshatidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static DateTime GetNowLocal()
    {
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Atlantic/Reykjavik");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    [HttpGet]
    public IActionResult Index([FromQuery] bool hidePast = false)
    {
        DateTime nowLocal = GetNowLocal();
        IQueryable<ArshatidModels.Models.EF.Arshatid> query = _dbContext.ArshatidEvents;
        if (hidePast)
        {
            query = query.Where((ArshatidModels.Models.EF.Arshatid e) => e.RegistrationEndTime >= nowLocal);
        }
        List<ArshatidModels.Models.EF.Arshatid> events = query
            .OrderByDescending((ArshatidModels.Models.EF.Arshatid e) => e.EventTime)
            .ToList();
        return View(events);
    }

    [HttpGet("Upsert/{id?}")]
    public IActionResult Upsert(int? id)
    {
        ArshatidModels.Models.EF.Arshatid model;
        if (id.HasValue)
        {
            ArshatidModels.Models.EF.Arshatid? existing = _dbContext.ArshatidEvents
                .FirstOrDefault((ArshatidModels.Models.EF.Arshatid e) => e.Pk == id.Value);
            if (existing == null)
            {
                return NotFound();
            }
            model = existing;
        }
        else
        {
            model = new ArshatidModels.Models.EF.Arshatid();
        }
        return View(model);
    }

    [HttpPost("Upsert")]
    public IActionResult Upsert(
        ArshatidModels.Models.EF.Arshatid model, 
        [FromQuery] bool back = false, 
        [FromQuery] bool hidePast = false
    )
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Pk == 0)
        {
            _dbContext.ArshatidEvents.Add(model);
        }
        else
        {
            _dbContext.ArshatidEvents.Update(model);
        }
        _dbContext.SaveChanges();

        return RedirectToAction("Index", new { hidePast = hidePast });
    }
}
