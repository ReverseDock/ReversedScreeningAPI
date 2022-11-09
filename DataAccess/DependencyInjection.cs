using DataAccess.Repositories;

using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataAccessServiceCollectionExtensions
    {
         public static IServiceCollection AddDataAccess(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("MongoDB")["ConnectionString"];

            services.AddSingleton<IMongoClient, MongoClient>(sp => 
            {
                return new MongoClient(connectionString);
            });

            services.AddSingleton<ISubmissionRepository, SubmissionRepository>();

            return services;
        }
    }
}