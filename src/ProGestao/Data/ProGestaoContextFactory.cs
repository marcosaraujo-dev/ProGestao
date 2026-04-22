using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProGestao.Data
{
    /// <summary>
    /// Factory para criação do DbContext em design-time (migrations).
    /// Evita que o EF Core precise inicializar toda a aplicação para gerar migrations.
    /// </summary>
    public class ProGestaoContextFactory : IDesignTimeDbContextFactory<ProGestaoContext>
    {
        public ProGestaoContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ProGestaoContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            return new ProGestaoContext(optionsBuilder.Options);
        }
    }
}
