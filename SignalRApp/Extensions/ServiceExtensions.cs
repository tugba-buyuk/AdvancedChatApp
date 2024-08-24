using Microsoft.EntityFrameworkCore;
using Repositories;

namespace SignalRApp.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureSqlContext(this IServiceCollection services,
            IConfiguration configuration) =>
                services.AddDbContext<RepositoryContext>(options =>
                options.UseSqlServer(
                 configuration.GetConnectionString("sqlConnection"),
                  b => b.MigrationsAssembly("SignalRApp")));
    }
}
