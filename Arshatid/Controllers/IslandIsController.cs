using Arshatid.Databases;
using Arshatid.Helpers;
using Arshatid.Services;
using ArshatidModels.Dtos;
using ArshatidModels.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arshatid.Controllers;

[Route("islandapi")]
[Authorize(AuthenticationSchemes = "IslandIs")]
public class IslandIsController : Controller
{
    private readonly CurrentEventService _currentEventService;
    private readonly ClaimsHelper _claimsHelper;
    private readonly InviteeService _inviteeService;
    private readonly RegistrationService _registrationService;
    private readonly GeneralDbContext _generalDbContext;
    private readonly ArshatidDbContext _dbContext;

    public IslandIsController(CurrentEventService currentEventService,
        ClaimsHelper claimsHelper,
        InviteeService inviteeService,
        RegistrationService registrationService,
        GeneralDbContext generalDbContext,
        ArshatidDbContext dbContext)
    {
        _currentEventService = currentEventService;
        _claimsHelper = claimsHelper;
        _inviteeService = inviteeService;
        _registrationService = registrationService;
        _generalDbContext = generalDbContext;
        _dbContext = dbContext;
    }

    private static DateTime GetNowLocal()
    {
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Atlantic/Reykjavik");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    [HttpGet("registration")]
    public ActionResult<RegistrationDto?> GetRegistration()
    {
        DateTime nowLocal = GetNowLocal();
        ArshatidModels.Models.EF.Arshatid? currentEvent = _currentEventService.GetCurrentEvent(nowLocal);
        if (currentEvent == null)
        {
            return Ok(null);
        }

        string ssn = _claimsHelper.GetSsn(User);
        ArshatidInvitee? invitee = _inviteeService.GetInvitee(currentEvent.Pk, ssn);
        if (invitee == null)
        {
            return BadRequest();
        }

        ArshatidRegistration? registration = _registrationService.GetByInvitee(invitee);
        if (registration == null)
        {
            return Ok(null);
        }

        RegistrationDto dto = MapToDto(currentEvent, invitee, registration);
        return Ok(dto);
    }

    [HttpPut("registration")]
    public ActionResult<RegistrationDto> UpsertRegistration([FromBody] UpsertRegistrationRequest request)
    {
        DateTime nowLocal = GetNowLocal();
        ArshatidModels.Models.EF.Arshatid? currentEvent = _currentEventService.GetCurrentEvent(nowLocal);
        if (currentEvent == null)
        {
            return BadRequest();
        }

        string ssn = _claimsHelper.GetSsn(User);
        ArshatidInvitee? invitee = _inviteeService.GetInvitee(currentEvent.Pk, ssn);
        if (invitee == null)
        {
            return BadRequest();
        }

        int plus = request.Plus ? 1 : 0;
        invitee.ArshatidCostCenterFk = request.ArshatidCostCenterFk;
        _dbContext.ArshatidInvitees.Update(invitee);
        _dbContext.SaveChanges();
        ArshatidRegistration registration = _registrationService.Upsert(invitee, plus, request.Alergies ?? string.Empty, request.Vegan);
        RegistrationDto dto = MapToDto(currentEvent, invitee, registration);
        return Ok(dto);
    }

    [HttpDelete("registration")]
    public IActionResult DeleteRegistration()
    {
        DateTime nowLocal = GetNowLocal();
        ArshatidModels.Models.EF.Arshatid? currentEvent = _currentEventService.GetCurrentEvent(nowLocal);
        if (currentEvent == null)
        {
            return BadRequest();
        }

        string ssn = _claimsHelper.GetSsn(User);
        ArshatidInvitee? invitee = _inviteeService.GetInvitee(currentEvent.Pk, ssn);
        if (invitee == null)
        {
            return BadRequest();
        }

        _registrationService.Delete(invitee);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("costcenters")]
    public ActionResult<IEnumerable<ArshatidCostCenter>> GetCostCenters()
    {
        var centers = _dbContext.ArshatidCostCenters
            .OrderBy(c => c.OrgUnitName)
            .ThenBy(c => c.CostCenterName)
            .ToList();
        return Ok(centers);
    }

    private RegistrationDto MapToDto(ArshatidModels.Models.EF.Arshatid currentEvent, ArshatidInvitee invitee, ArshatidRegistration registration)
    {
        string name = _generalDbContext.Person
            .FirstOrDefault(p => p.Ssn == invitee.Ssn)?.Name ?? invitee.Ssn;

        return new RegistrationDto
        {
            RegistrationId = registration.Pk,
            Plus = registration.Plus,
            Vegan = registration.Vegan,
            ArshatidCostCenterFk = invitee.ArshatidCostCenterFk,
            Invitee = new InviteeDto
            {
                EventId = invitee.ArshatidFk,
                InviteeId = invitee.Pk,
                Ssn = invitee.Ssn,
                Name = name
            }
        };
    }
}
