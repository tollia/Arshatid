using Arshatid.Databases;
using ArshatidModels.Models.EF;
using Microsoft.AspNetCore.Mvc;

namespace Arshatid.Controllers;

[Route("[controller]")]
public class CostCenterController : Controller
{
    private readonly ArshatidDbContext _dbContext;

    public CostCenterController(ArshatidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Index()
    {
        List<ArshatidCostCenter> centers = _dbContext.ArshatidCostCenters
            .OrderBy(c => c.Pk)
            .ToList();
        return View(centers);
    }

    [HttpPost("Update")]
    public IActionResult Update([FromBody] ArshatidCostCenter model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _dbContext.ArshatidCostCenters.Update(model);
        _dbContext.SaveChanges();
        return Ok();
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] ArshatidCostCenter model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _dbContext.ArshatidCostCenters.Add(model);
        _dbContext.SaveChanges();
        return Json(new { id = model.Pk });
    }
}
