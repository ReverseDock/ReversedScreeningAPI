using Microsoft.Extensions.Configuration;
using AsyncAPI.Consumers;
using AsyncAPI;
using EasyNetQ;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncAPI(
            this IServiceCollection services)
        {
            services.AddScoped<ResultConsumer>();
            services.AddSingleton<ResultConsumerWrapper>();
            services.AddSingleton<IAdvancedBus>((sp) =>
            {
                var _bus = RabbitHutch.CreateBus("host=rabbitmq;prefetchcount=1").Advanced;
                var queue = _bus.QueueDeclare("q.dockings.results", true, false, false);
                var exchange = _bus.ExchangeDeclare("e.dockings", "direct");
                var binding = _bus.Bind(exchange, queue, "Result");

                var consumerWrapper = sp.GetService<ResultConsumerWrapper>()!;

                _bus.Consume<Models.Result>(queue, async (msg, info) => {
                    await consumerWrapper._resultConsumer.Consume(msg, info);
                });
                
                return _bus;
            });


            return services;
        }
    }
}