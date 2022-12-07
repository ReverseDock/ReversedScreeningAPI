using HttpAPI.Models;

using MongoDB.Driver;

namespace DataAccess.Repositories;

public class ReceptorRepository : IReceptorRepository
{
    private readonly IMongoCollection<Receptor> _ReceptorCollection;

    public ReceptorRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["ReceptorsCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _ReceptorCollection = mongoDatabase.GetCollection<Receptor>(collectionName);
    }

    public async Task<List<Receptor>> GetAsync()
    {
        return await _ReceptorCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Receptor?> GetAsync(string id)
    {
        return await _ReceptorCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Receptor>> GetAsync(IEnumerable<string> uniProtIds)
    {
        return await _ReceptorCollection.Find(x => uniProtIds.Contains(x.UniProtID)).ToListAsync();
    }

    public async Task<Receptor?> GetByUniProtIdAsync(string uniProtId)
    {
        return await _ReceptorCollection.Find(x => x.UniProtID == uniProtId).FirstOrDefaultAsync();
    }

    public async Task<Receptor> CreateAsync(Receptor Receptor) 
    {
        Receptor.createdAt = new DateTime();
        Receptor.updatedAt = new DateTime();
        await _ReceptorCollection.InsertOneAsync(Receptor);
        return Receptor;
    }

    public async Task UpdateAsync(string id, Receptor updatedReceptor)
    {
        updatedReceptor.updatedAt = new DateTime();
        await _ReceptorCollection.ReplaceOneAsync(x => x.id == id, updatedReceptor);
    }
    
    public async Task RemoveAsync(string id)
    {
        await _ReceptorCollection.DeleteOneAsync(x => x.id == id);
    }
}