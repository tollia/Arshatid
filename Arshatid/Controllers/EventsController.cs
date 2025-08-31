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
        IQueryable<ArshatidEvent> query = _dbContext.ArshatidEvents;
        if (hidePast)
        {
            query = query.Where((ArshatidEvent e) => e.RegistrationEndTime >= nowLocal);
        }
        List<ArshatidEvent> events = query
            .OrderByDescending((ArshatidEvent e) => e.EventTime)
            .ToList();
        return View(events);
    }

    [HttpGet("Upsert/{id?}")]
    public IActionResult Upsert(int? id)
    {
        ArshatidEvent model;
        if (id.HasValue)
        {
            ArshatidEvent? existing = _dbContext.ArshatidEvents
                .FirstOrDefault((ArshatidEvent e) => e.Pk == id.Value);
            if (existing == null)
            {
                return NotFound();
            }
            model = existing;
        }
        else
        {
            model = new ArshatidEvent();
        }
        return View(model);
    }

    [HttpPost("Upsert")]
    public IActionResult Upsert(
        ArshatidEvent model, 
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
