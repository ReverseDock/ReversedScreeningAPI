using HttpAPI.Models;

using MongoDB.Driver;

namespace DataAccess.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IMongoCollection<FileDescriptor> _FileDescriptorCollection;

    public FileRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["FilesCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _FileDescriptorCollection = mongoDatabase.GetCollection<FileDescriptor>(collectionName);
    }

    public async Task<FileDescriptor?> GetByGuid(Guid guid)
    {
        return await _FileDescriptorCollection.Find(x => x.guid == guid).FirstOrDefaultAsync();
    }
    
    public async Task<List<FileDescriptor>> GetAsync()
    {
        return await _FileDescriptorCollection.Find(_ => true).ToListAsync();
    }

    public async Task<FileDescriptor?> GetAsync(string id)
    {
        return await _FileDescriptorCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<FileDescriptor> CreateAsync(FileDescriptor FileDescriptor) 
    {
        FileDescriptor.createdAt = DateTime.Now;
        FileDescriptor.updatedAt = DateTime.Now;
        await _FileDescriptorCollection.InsertOneAsync(FileDescriptor);
        return FileDescriptor;
    }

    public async Task UpdateAsync(string id, FileDescriptor updatedFileDescriptor)
    {
        updatedFileDescriptor.updatedAt = DateTime.Now;
        await _FileDescriptorCollection.ReplaceOneAsync(x => x.id == id, updatedFileDescriptor);
    }
    
    public async Task RemoveAsync(string id)
    {
        await _FileDescriptorCollection.DeleteOneAsync(x => x.id == id);
    }
}