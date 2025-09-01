using ArshatidModels.Dtos;
using ArshatidModels.Models.EF;
using ArshatidPublic.Classes;
using ArshatidPublic.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArshatidPublic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [HttpPost]
        [ManualJwtSignIn]
        public async Task<IActionResult> Skraning(string? jwt)
        {
            ViewBag.Name = User.Identity?.Name ?? "Óþekktur aðili";

            HttpClient client = _clientFactory.CreateClient("ArshatidApi");
            HttpResponseMessage response = await client.GetAsync("registration");
            HttpResponseMessage centersResponse = await client.GetAsync("costcenters");
            List<ArshatidCostCenter> costCenters = centersResponse.IsSuccessStatusCode
                ? await centersResponse.Content.ReadFromJsonAsync<List<ArshatidCostCenter>>()
                : new List<ArshatidCostCenter>();
            ViewBag.CostCentersJson = JsonSerializer.Serialize(costCenters);
            ViewBag.OrgUnits = costCenters
                .Where(c => c.IsDivision)
                .Select(c => c.OrgUnitName)
                .Distinct()
                .OrderBy(n => n)
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
                model.ArshatidCostCenterFk = registration.ArshatidCostCenterFk;
                if (model.ArshatidCostCenterFk != null)
                {
                    var selected = costCenters.FirstOrDefault(c => c.Pk == model.ArshatidCostCenterFk);
                    model.OrgUnitName = selected?.OrgUnitName;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ManualJwtSignIn]
        public async Task<IActionResult> Skraning(RegistrationViewModel model)
        {
            var client = _clientFactory.CreateClient("ArshatidApi");
            var request = new UpsertRegistrationRequest
            {
                Plus = model.Plus,
                Alergies = model.Alergies,
                Vegan = model.Vegan,
                ArshatidCostCenterFk = model.ArshatidCostCenterFk
            };
            await client.PutAsJsonAsync("registration", request);
            return RedirectToAction(nameof(Skraning));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ManualJwtSignIn]
        public async Task<IActionResult> KemstEkki()
        {
            var client = _clientFactory.CreateClient("ArshatidApi");
            await client.DeleteAsync("registration");
            return RedirectToAction(nameof(Skraning));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
