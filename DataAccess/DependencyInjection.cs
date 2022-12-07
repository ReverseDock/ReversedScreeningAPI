using DataAccess.Repositories;

using HttpAPI.Models;

using MongoDB.Driver;

using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataAccessServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDB(
            this IServiceCollection services,
            IConfiguration configuration,
            bool IsDevelopment)
        {
            var connectionString = configuration.GetSection("MongoDB")["ConnectionString"];

            services.AddSingleton<IMongoClient, MongoClient>(sp => 
            {
                return new MongoClient(connectionString);
            });

            services.AddSingleton<ISubmissionRepository, SubmissionRepository>();
            services.AddSingleton<IUserFileRepository, UserFileRepository>();
            services.AddSingleton<IReceptorFileRepository, ReceptorFileRepository>();
            services.AddSingleton<IDockingResultRepository, DockingResultRepository>();

            return services;
        }

        public static IServiceCollection AddRedis(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var host = configuration.GetSection("Redis")["Host"];
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(host);
            });
            return services;
        }
    }
}