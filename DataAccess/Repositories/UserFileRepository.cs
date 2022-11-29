using HttpAPI.Models;

using MongoDB.Driver;

namespace DataAccess.Repositories;

public class UserFileRepository : IUserFileRepository
{
    private readonly IMongoCollection<UserFile> _UserFileCollection;

    public UserFileRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["UserFilesCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _UserFileCollection = mongoDatabase.GetCollection<UserFile>(collectionName);
    }

    public async Task<UserFile?> GetByGuid(Guid guid)
    {
        return await _UserFileCollection.Find(x => x.guid == guid).FirstOrDefaultAsync();
    }
    
    public async Task<List<UserFile>> GetAsync()
    {
        return await _UserFileCollection.Find(_ => true).ToListAsync();
    }

    public async Task<UserFile?> GetAsync(string id)
    {
        return await _UserFileCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<UserFile> CreateAsync(UserFile UserFile) 
    {
        UserFile.createdAt = new DateTime();
        UserFile.updatedAt = new DateTime();
        await _UserFileCollection.InsertOneAsync(UserFile);
        return UserFile;
    }

    public async Task UpdateAsync(string id, UserFile updatedUserFile)
    {
        updatedUserFile.updatedAt = new DateTime();
        await _UserFileCollection.ReplaceOneAsync(x => x.id == id, updatedUserFile);
    }
    
    public async Task RemoveAsync(string id)
    {
        await _UserFileCollection.DeleteOneAsync(x => x.id == id);
    }
}