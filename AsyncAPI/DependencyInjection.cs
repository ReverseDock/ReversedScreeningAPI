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
                x.AddConsumer<DockingResultConsumer>();
                x.AddConsumer<FASTAResultConsumer>();
                x.AddConsumer<DockingPrepResultConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.ConcurrentMessageLimit = 1;
                    cfg.Host(host, "/");
                    cfg.ConfigureEndpoints(ctx);
                    cfg.UseRawJsonSerializer();
                });
            });
            services.AddTransient<IDockingTaskPublisher, DockingTaskPublisher>();
            services.AddTransient<IFASTATaskPublisher, FASTATaskPublisher>();
            services.AddTransient<IDockingPrepTaskPublisher, DockingPrepTaskPublisher>();
            services.AddTransient<IMailTaskPublisher, MailTaskPublisher>();
            
            return services;
        }
    }
}