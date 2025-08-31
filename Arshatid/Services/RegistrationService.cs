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

    public ArshatidRegistration? GetByInvitee(ArshatidInvitee invitee)
    {
        return _dbContext.ArshatidRegistrations
            .FirstOrDefault(r => r.ArshatidInviteeFk == invitee.Pk);
    }

    public ArshatidRegistration Upsert(
        ArshatidInvitee invitee, 
        int plus, 
        string alergies
    )
    {
        ArshatidRegistration? registration = GetByInvitee(invitee);
        if (registration == null)
        {
            if (invitee == null || invitee.Pk == 0)
            {
                throw new InvalidOperationException("Invitee not found");
            }

            registration = new ArshatidRegistration
            {
                ArshatidInviteeFk = invitee.Pk,
                Plus = plus,
                Alergies = alergies
            };
            _dbContext.ArshatidRegistrations.Add(registration);
        }
        else
        {
            registration.Plus = plus;
            registration.Alergies = alergies;
            _dbContext.ArshatidRegistrations.Update(registration);
        }

        _dbContext.SaveChanges();
        return registration;
    }

    public void Delete(ArshatidInvitee invitee)
    {
        ArshatidRegistration? registration = GetByInvitee(invitee);
        if (registration != null)
        {
            _dbContext.ArshatidRegistrations.Remove(registration);
            _dbContext.SaveChanges();
        }
    }
}
