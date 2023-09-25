using HttpAPI.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataAccess.Repositories;

public class AlphaFoldReceptorRepository : IAlphaFoldReceptorRepository
{
    private readonly IMongoCollection<AlphaFoldReceptor> _collection;

    public AlphaFoldReceptorRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["AlphaFoldReceptorsCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _collection = mongoDatabase.GetCollection<AlphaFoldReceptor>(collectionName);
    }

    public async Task<AlphaFoldReceptor?> GetAsync(string id)
    {
        return await _collection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<AlphaFoldReceptor?> GetByUnitProtID(string uniProtId)
    {
        return await _collection.Find(x => x.UniProtID == uniProtId).FirstOrDefaultAsync();
    }

    public async Task<List<AlphaFoldReceptor>> GetAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<AlphaFoldReceptor> CreateAsync(AlphaFoldReceptor receptor)
    {
        await _collection.InsertOneAsync(receptor);
        return receptor;
    }

    public async Task UpdateAsync(string id, AlphaFoldReceptor updatedReceptor)
    {
        await _collection.ReplaceOneAsync(x => x.id == id, updatedReceptor);
    }

    public async Task RemoveAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.id == id);
    }
}