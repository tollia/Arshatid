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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ArshatidInvitee>().ToTable("ArshatidInvitee", "dbo");
            modelBuilder.Entity<ArshatidRegistration>().ToTable("ArshatidRegistration", "dbo");
            modelBuilder.Entity<ArshatidModels.Models.EF.Arshatid>().ToTable("Arshatid", "dbo");
            modelBuilder.Entity<ArshatidImage>().ToTable("ArshatidImage", "dbo");
            modelBuilder.Entity<ArshatidImageType>().ToTable("ArshatidImageType", "dbo");

            modelBuilder.Entity<ArshatidRegistration>()
                .HasOne(r => r.Invitee)
                .WithMany(i => i.Registrations)   // ensure this nav exists on ArshatidInvitee
                .HasForeignKey(r => r.ArshatidInviteeFk)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArshatidInvitee>()
                .HasOne(i => i.Arshatid)
                .WithMany(e => e.Invitees)
                .HasForeignKey(i => i.ArshatidFk)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArshatidInvitee>().ToTable(t =>
            {
                // Exactly 10 characters
                t.HasCheckConstraint("ck_ArshatidInvitee_SsnLen", "LEN([Ssn]) = 10");

                // Digits only (no non-digits anywhere)
                t.HasCheckConstraint("ck_ArshatidInvitee_SsnDigits", "[Ssn] NOT LIKE '%[^0-9]%'");
            });

            // Invitee uniqueness per event + SSN
            modelBuilder.Entity<ArshatidInvitee>()
                .HasIndex(i => new { i.ArshatidFk, i.Ssn })
                .IsUnique()
                .HasDatabaseName("unq_ArshatidInvitee_Event_Ssn");

            // Registration uniqueness per invitee
            modelBuilder.Entity<ArshatidRegistration>()
                .HasIndex(r => r.ArshatidInviteeFk)
                .IsUnique()
                .HasDatabaseName("unq_ArshatidRegistration_Invitee");

            // Relationship clarity
            modelBuilder.Entity<ArshatidRegistration>()
                .HasOne(r => r.Invitee)
                .WithMany(i => i.Registrations)
                .HasForeignKey(r => r.ArshatidInviteeFk)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
