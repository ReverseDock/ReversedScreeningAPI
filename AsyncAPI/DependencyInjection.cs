using AsyncAPI.Consumers;
using AsyncAPI.Publishers;
using MassTransit;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncAPI(
            this IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ResultConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/");
                    cfg.ConfigureEndpoints(ctx);
                });
            });
            services.AddScoped<IDockingPublisher, DockingPublisher>();
            
            return services;
        }
    }
}