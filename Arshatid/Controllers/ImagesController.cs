using Arshatid.Databases;
using ArshatidModels.Models.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Arshatid.Controllers;

[Route("Events/{eventId}/Images")]
public class ImagesController : Controller
{
    private readonly ArshatidDbContext _dbContext;

    public ImagesController(ArshatidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Index(int eventId)
    {
        List<ArshatidImage> images = _dbContext.ArshatidImages
            .Include((ArshatidImage i) => i.ImageType)
            .Where((ArshatidImage i) => i.ArshatidFk == eventId)
            .ToList();
        List<ArshatidImageType> types = _dbContext.ArshatidImageTypes.ToList();
        ViewBag.ImageTypes = new SelectList(types, "Pk", "Name");
        ViewBag.EventId = eventId;
        return View(images);
    }

    [HttpPost]
    public IActionResult Index(int eventId, int imageTypeFk, List<IFormFile> files)
    {
        foreach (IFormFile file in files)
        {
            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            ArshatidImage image = new ArshatidImage
            {
                ArshatidFk = eventId,
                ContentType = file.ContentType,
                ImageData = ms.ToArray(),
                ImageTypeFk = imageTypeFk
            };
            _dbContext.ArshatidImages.Add(image);
        }
        _dbContext.SaveChanges();
        return RedirectToAction("Index", new { eventId = eventId });
    }

    [HttpGet("{imageId}")]
    public IActionResult GetImage(int eventId, int imageId)
    {
        ArshatidImage? image = _dbContext.ArshatidImages
            .FirstOrDefault((ArshatidImage i) => i.Pk == imageId && i.ArshatidFk == eventId);
        if (image == null)
        {
            return NotFound();
        }
        return File(image.ImageData, image.ContentType);
    }

    [HttpDelete("{imageId}")]
    public IActionResult Delete(int eventId, int imageId)
    {
        ArshatidImage? image = _dbContext.ArshatidImages
            .FirstOrDefault((ArshatidImage i) => i.Pk == imageId && i.ArshatidFk == eventId);
        if (image == null)
        {
            return NotFound();
        }
        _dbContext.ArshatidImages.Remove(image);
        _dbContext.SaveChanges();
        return NoContent();
    }
}
