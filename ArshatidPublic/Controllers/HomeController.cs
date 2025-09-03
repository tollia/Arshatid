using ArshatidModels.Dtos;
using ArshatidModels.Models.EF;
using ArshatidPublic.Classes;
using ArshatidPublic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace ArshatidPublic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, IMemoryCache cache, IConfiguration configuration)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _cache = cache;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [HttpGet]
        [ManualJwtSignIn]
        public async Task<IActionResult> Skraning(string jwt)
        {
            ViewBag.Name = User.Identity?.Name ?? "Óþekktur aðili";

            HttpClient client = _clientFactory.CreateClient("ArshatidApi");
            HttpResponseMessage response = await client.GetAsync("registration");
            HttpResponseMessage centersResponse = await client.GetAsync("costcenters");
            List<ArshatidCostCenter> costCenters = centersResponse.IsSuccessStatusCode
                ? await centersResponse.Content.ReadFromJsonAsync<List<ArshatidCostCenter>>()
                : new List<ArshatidCostCenter>();
            ViewBag.CostCenters = costCenters
                .OrderBy(c => c.CostCenterName)
                .Select(c => new SelectListItem(c.OrgUnitName + " - " + c.CostCenterName, c.Pk.ToString()))
                .ToList();

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.NotInvitedMessage = "Þú ert ekki á boðslista Árshátíðar, hafðu samband við arshatid@kopavogur.is ef þetta eru mistök.";
                return View(new RegistrationViewModel());
            }

            if (!response.IsSuccessStatusCode)
            {
                return View(new RegistrationViewModel());
            }

            RegistrationDto? registration = null;
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                registration = await response.Content.ReadFromJsonAsync<RegistrationDto>();
            }

            var model = new RegistrationViewModel();
            ViewBag.HasRegistration = registration != null;
            if (registration != null)
            {
                model.Plus = registration.Plus == 1;
                model.Vegan = registration.Vegan;
                model.Alergies = registration.Alergies;
                model.ArshatidCostCenterFk = registration.ArshatidCostCenterFk;
            }
            model.JwtToken = HttpContext.Items["jwt_token"].ToString();
            return View(model);
        }

        [HttpPost]
        [HttpGet]
        //[ValidateAntiForgeryToken]
        [ManualJwtSignIn]
        public async Task<IActionResult> SkraningUpsert([FromForm] string jwt, RegistrationViewModel model)
        {
            var client = _clientFactory.CreateClient("ArshatidApi");
            var request = new UpsertRegistrationRequest
            {
                Plus = model.Plus,
                Alergies = model.Alergies,
                Vegan = model.Vegan,
                ArshatidCostCenterFk = model.ArshatidCostCenterFk,
                JwtToken = model.JwtToken
            };
            await client.PutAsJsonAsync("registration", request);
            //return RedirectToAction(
            //    nameof(Skraning),
            //    new { jwt = model.JwtToken }
            //);
            return RedirectToAction(nameof(Super));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ManualJwtSignIn]
        public async Task<IActionResult> KemstEkki(string jwt)
        {
            var client = _clientFactory.CreateClient("ArshatidApi");
            await client.DeleteAsync("registration");
            return RedirectToAction(nameof(Sorry));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Super()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Sorry()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Image(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return NotFound();
            }

            var cacheKey = $"image_{imageName}";
            if (!_cache.TryGetValue(cacheKey, out CachedImage cached))
            {
                var client = _clientFactory.CreateClient("ArshatidApi");
                var eventId = _configuration.GetValue<int>("ArshatidApi:EventId");
                var response = await client.GetAsync($"events/{eventId}/images/{imageName}");
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var data = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                cached = new CachedImage(data, contentType);
                var minutes = _configuration.GetValue<int>("ImageCacheMinutes", 60);
                _cache.Set(cacheKey, cached, TimeSpan.FromMinutes(minutes));
            }

            return File(cached.Data, cached.ContentType);
        }

        private record CachedImage(byte[] Data, string ContentType);

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
