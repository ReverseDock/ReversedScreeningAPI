using DataAccess.Repositories;

using HttpAPI.Models;

using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataAccessServiceCollectionExtensions
    {
         public static IServiceCollection AddDataAccess(
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
            services.AddSingleton<IRepository<ReceptorFile>, ReceptorFileRepository>();
            services.AddSingleton<IResultRepository, ResultRepository>();

            return services;
        }
    }
}