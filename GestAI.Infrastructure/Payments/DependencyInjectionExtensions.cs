using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestAI.Infrastructure.Payments
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddPayPal(this IServiceCollection services, IConfiguration configuration)
        {
            var opts = new PayPalOptions();
            configuration.GetSection("PayPal").Bind(opts);
            services.AddSingleton(opts);
            services.AddHttpClient<PayPalClient>();
            return services;
        }
    }
}
