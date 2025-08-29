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

        public DbSet<ArshatidEvent> ArshatidEvents { get; set; }
        public DbSet<ArshatidImage> ArshatidImages { get; set; }
        public DbSet<ArshatidImageType> ArshatidImageTypes { get; set; }
        public DbSet<ArshatidInvitee> ArshatidInvitees { get; set; }
        public DbSet<ArshatidRegistration> ArshatidRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ArshatidInvitee>()
                .ToTable(t => t.HasCheckConstraint("ArshatidSsnLen", "len([Ssn])=(10)"));

            modelBuilder.Entity<ArshatidRegistration>()
                .HasIndex(e => new { e.ArshatidFk, e.Ssn })
                .IsUnique()
                .HasDatabaseName("unq_ArshatidRegistrations");
        }
    }
}
