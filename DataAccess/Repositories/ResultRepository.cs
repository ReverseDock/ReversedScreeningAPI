using HttpAPI.Models;

using MongoDB.Driver;

namespace DataAccess.Repositories;

public class ResultRepository : IResultRepository
{
    private readonly IMongoCollection<Result> _ResultCollection;

    public ResultRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["ResultsCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _ResultCollection = mongoDatabase.GetCollection<Result>(collectionName);
    }

    public async Task<List<Result>> GetAsync()
    {
        return await _ResultCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Result?> GetAsync(string id)
    {
        return await _ResultCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Result>> GetBySubmissionId(string submissionId)
    {
        return await _ResultCollection.Find(x => x.submissionId == submissionId).ToListAsync();
    }

    public async Task CreateAsync(Result Result) 
    {
        Result.createdAt = new DateTime();
        Result.updatedAt = new DateTime();
        await _ResultCollection.InsertOneAsync(Result);
    }

    public async Task UpdateAsync(string id, Result updatedResult)
    {
        updatedResult.updatedAt = new DateTime();
        await _ResultCollection.ReplaceOneAsync(x => x.id == id, updatedResult);
    }
    
    public async Task RemoveAsync(string id)
    {
        await _ResultCollection.DeleteOneAsync(x => x.id == id);
    }
}