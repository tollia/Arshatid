using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace Arshatid.Databases
{
    public abstract class BaseDbContext<TContext> : DbContext where TContext : DbContext
    {
        private readonly IConfiguration _configuration;

        protected BaseDbContext(DbContextOptions<TContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public SqlConnection SqlConnection
        {
            get
            {
                DbConnection dbConnection = Database.GetDbConnection();
                if (dbConnection.State != ConnectionState.Open)
                {
                    dbConnection.Open();
                }
                return dbConnection as SqlConnection;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString(typeof(TContext).Name);
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException($"Connection string '{typeof(TContext).Name}' not found.");
                }
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}

