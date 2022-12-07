using HttpAPI.Models;

using MongoDB.Driver;

namespace DataAccess.Repositories;

public class ReceptorFileRepository : IReceptorFileRepository
{
    private readonly IMongoCollection<ReceptorFile> _ReceptorFileCollection;

    public ReceptorFileRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["ReceptorFilesCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _ReceptorFileCollection = mongoDatabase.GetCollection<ReceptorFile>(collectionName);
    }

    public async Task<List<ReceptorFile>> GetAsync()
    {
        return await _ReceptorFileCollection.Find(_ => true).ToListAsync();
    }

    public async Task<ReceptorFile?> GetAsync(string id)
    {
        return await _ReceptorFileCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<List<ReceptorFile>> GetAsync(IEnumerable<string> uniProtIds)
    {
        return await _ReceptorFileCollection.Find(x => uniProtIds.Contains(x.UniProtID)).ToListAsync();
    }

    public async Task<ReceptorFile> CreateAsync(ReceptorFile ReceptorFile) 
    {
        ReceptorFile.createdAt = new DateTime();
        ReceptorFile.updatedAt = new DateTime();
        await _ReceptorFileCollection.InsertOneAsync(ReceptorFile);
        return ReceptorFile;
    }

    public async Task UpdateAsync(string id, ReceptorFile updatedReceptorFile)
    {
        updatedReceptorFile.updatedAt = new DateTime();
        await _ReceptorFileCollection.ReplaceOneAsync(x => x.id == id, updatedReceptorFile);
    }
    
    public async Task RemoveAsync(string id)
    {
        await _ReceptorFileCollection.DeleteOneAsync(x => x.id == id);
    }
}