using Arshatid.Databases;
using ArshatidModels.Models.EF;

namespace Arshatid.Services;

public class InviteeService
{
    private readonly ArshatidDbContext _dbContext;

    public InviteeService(ArshatidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ArshatidInvitee? GetInvitee(int eventId, string ssn)
    {
        return _dbContext.ArshatidInvitees
            .FirstOrDefault(i => i.ArshatidFk == eventId && i.Ssn == ssn);
    }
}
