using Arshatid.Databases;
using ArshatidModels.Models.EF;

namespace Arshatid.Services;

public class RegistrationService
{
    private readonly ArshatidDbContext _dbContext;

    public RegistrationService(ArshatidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ArshatidRegistration? GetByInvitee(int eventId, int inviteeId)
    {
        return _dbContext.ArshatidRegistrations
            .FirstOrDefault(r => r.ArshatidFk == eventId && r.ArshatidInviteeFk == inviteeId);
    }

    public ArshatidRegistration Upsert(int eventId, int inviteeId, int plus)
    {
        ArshatidRegistration? registration = GetByInvitee(eventId, inviteeId);
        if (registration == null)
        {
            ArshatidInvitee? invitee = _dbContext.ArshatidInvitees
                .FirstOrDefault(i => i.Pk == inviteeId);
            if (invitee == null)
            {
                throw new InvalidOperationException("Invitee not found");
            }

            registration = new ArshatidRegistration
            {
                ArshatidFk = eventId,
                ArshatidInviteeFk = inviteeId,
                Ssn = invitee.Ssn,
                Plus = plus
            };
            _dbContext.ArshatidRegistrations.Add(registration);
        }
        else
        {
            registration.Plus = plus;
            _dbContext.ArshatidRegistrations.Update(registration);
        }

        _dbContext.SaveChanges();
        return registration;
    }

    public void Delete(int eventId, int inviteeId)
    {
        ArshatidRegistration? registration = GetByInvitee(eventId, inviteeId);
        if (registration != null)
        {
            _dbContext.ArshatidRegistrations.Remove(registration);
            _dbContext.SaveChanges();
        }
    }
}
