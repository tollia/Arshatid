using Microsoft.EntityFrameworkCore;

namespace Arshatid.Databases
{
    public class ArshatidDbContext : BaseDbContext<ArshatidDbContext>
    {
        public ArshatidDbContext(DbContextOptions<ArshatidDbContext> options, IConfiguration configuration)
            : base(options, configuration)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
