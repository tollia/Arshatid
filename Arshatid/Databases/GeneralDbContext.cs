using ArshatidModels.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace Arshatid.Databases
{
    public class GeneralDbContext : BaseDbContext<GeneralDbContext>
    {
        public GeneralDbContext(DbContextOptions<GeneralDbContext> options, IConfiguration configuration)
            : base(options, configuration)
        {
        }

        public DbSet<Person> Person { get; set; }
    }
}

