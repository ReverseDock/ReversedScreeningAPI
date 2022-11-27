using AsyncAPI.Consumers;
using AsyncAPI.Publishers;

using MassTransit;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AsyncAPIServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncAPI(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var host = configuration.GetSection("RabbitMQ")["Host"];
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ResultConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(host, "/");
                    cfg.ConfigureEndpoints(ctx);
                });
            });
            services.AddTransient<IDockingPublisher, DockingPublisher>();
            
            return services;
        }
    }
}