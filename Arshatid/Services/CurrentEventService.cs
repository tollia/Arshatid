using Arshatid.Databases;
using ArshatidModels.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace Arshatid.Services;

public class CurrentEventService
{
    private readonly ArshatidDbContext _dbContext;

    public CurrentEventService(ArshatidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ArshatidModels.Models.EF.Arshatid? GetCurrentEvent(DateTime nowLocal)
    {
        return _dbContext.ArshatidEvents
            .Where(e => e.RegistrationStartTime <= nowLocal && nowLocal <= e.RegistrationEndTime)
            .OrderByDescending(e => e.RegistrationStartTime)
            .FirstOrDefault();
    }
}
