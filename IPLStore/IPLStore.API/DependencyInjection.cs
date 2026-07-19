using IPLStore.Application.Interfaces;
using IPLStore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IPLStore.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<IPLStoreDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IPLStoreRepository>();
            services.AddScoped<IProductQueries>(x => x.GetRequiredService<IPLStoreRepository>());

            return services;
        }
    }
}
