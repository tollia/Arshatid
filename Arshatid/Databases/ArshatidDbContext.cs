using ArshatidModels.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace Arshatid.Databases
{
    public class ArshatidDbContext : BaseDbContext<ArshatidDbContext>
    {
        public ArshatidDbContext(DbContextOptions<ArshatidDbContext> options, IConfiguration configuration)
            : base(options, configuration)
        {
        }

        public DbSet<ArshatidModels.Models.EF.Arshatid> ArshatidEvents { get; set; }
        public DbSet<ArshatidImage> ArshatidImages { get; set; }
        public DbSet<ArshatidImageType> ArshatidImageTypes { get; set; }
        public DbSet<ArshatidInvitee> ArshatidInvitees { get; set; }
        public DbSet<ArshatidRegistration> ArshatidRegistrations { get; set; }
        public DbSet<ArshatidCostCenter> ArshatidCostCenters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table names (keeps EF from guessing)
            modelBuilder.Entity<ArshatidModels.Models.EF.Arshatid>().ToTable("Arshatid", "dbo");
            modelBuilder.Entity<ArshatidInvitee>().ToTable("ArshatidInvitee", "dbo");
            modelBuilder.Entity<ArshatidRegistration>().ToTable("ArshatidRegistration", "dbo");
            modelBuilder.Entity<ArshatidImage>().ToTable("ArshatidImage", "dbo");
            modelBuilder.Entity<ArshatidImageType>().ToTable("ArshatidImageType", "dbo");
            modelBuilder.Entity<ArshatidCostCenter>().ToTable("ArshatidCostCenter", "dbo");

            // ArshatidEvent (principal) 1* ArshatidInvitee (dependent)
            modelBuilder.Entity<ArshatidInvitee>()
                .HasOne(i => i.Event)
                .WithMany(e => e.Invitees)
                .HasForeignKey(i => i.ArshatidFk)
                .OnDelete(DeleteBehavior.Cascade);

            // ArshatidInvitee (principal) 1* ArshatidRegistration (dependent)
            modelBuilder.Entity<ArshatidRegistration>()
                .HasOne(r => r.Invitee)
                .WithMany(i => i.Registrations)
                .HasForeignKey(r => r.ArshatidInviteeFk)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArshatidRegistration>()
                .HasOne(r => r.CostCenter)
                .WithMany(c => c.Registrations)
                .HasForeignKey(r => r.ArshatidCostCenterFk);

            // Uniqueness you wanted
            modelBuilder.Entity<ArshatidInvitee>()
                .HasIndex(i => new { i.ArshatidFk, i.Ssn })
                .IsUnique()
                .HasDatabaseName("unq_ArshatidInvitee_Event_Ssn");

            modelBuilder.Entity<ArshatidRegistration>()
                .HasIndex(r => r.ArshatidInviteeFk)
                .IsUnique()
                .HasDatabaseName("unq_ArshatidRegistration_Invitee");

            // SSN checks (length + digits)
            modelBuilder.Entity<ArshatidInvitee>().ToTable(t =>
            {
                t.HasCheckConstraint("ck_ArshatidInvitee_SsnLen", "LEN([Ssn]) = 10");
                t.HasCheckConstraint("ck_ArshatidInvitee_SsnDigits", "[Ssn] NOT LIKE '%[^0-9]%'");
            });
        }
    }
}
