using MongoDB.Driver;
using Models;
using Microsoft.Extensions.Configuration;

namespace DataAccess.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly IMongoCollection<Submission> _submissionCollection;

    public SubmissionRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["SubmissionsCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _submissionCollection = mongoDatabase.GetCollection<Submission>(collectionName);
    }

    public async Task<List<Submission>> GetAsync()
    {
        return await _submissionCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Submission?> GetAsync(string id)
    {
        return await _submissionCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Submission submission) 
    {
        await _submissionCollection.InsertOneAsync(submission);
    }

    public async Task UpdateAsync(string id, Submission updatedSubmission)
    {
        await _submissionCollection.ReplaceOneAsync(x => x.id == id, updatedSubmission);
    }
    
    public async Task RemoveAsync(string id)
    {
        await _submissionCollection.DeleteOneAsync(x => x.id == id);
    }
}