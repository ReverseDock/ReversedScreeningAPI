using HttpAPI.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataAccess.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly ILogger<SubmissionRepository> _logger;
    private readonly IMongoCollection<Submission> _submissionCollection;

    public SubmissionRepository(
        ILogger<SubmissionRepository> logger,
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["SubmissionsCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _logger = logger;
        _submissionCollection = mongoDatabase.GetCollection<Submission>(collectionName);
    }

    public async Task<Submission?> GetByGuid(Guid guid)
    {
        return await _submissionCollection.Find(x => x.guid == guid).FirstOrDefaultAsync();
    }

    public async Task<List<Submission>> GetAsync()
    {
        return await _submissionCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Submission?> GetAsync(string id)
    {
        return await _submissionCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<Submission> CreateAsync(Submission submission) 
    {
        submission.createdAt = DateTime.Now;
        submission.updatedAt = DateTime.Now;
        await _submissionCollection.InsertOneAsync(submission);
        return submission;
    }

    public async Task UpdateAsync(string id, Submission updatedSubmission)
    {
        updatedSubmission.updatedAt = DateTime.Now;
        await _submissionCollection.ReplaceOneAsync(x => x.id == id, updatedSubmission);
    }

    public async Task UpdateReceptor(Submission submission, Receptor receptor)
    {
        _logger.LogInformation($"Updating receptor {receptor.guid} of submission {submission.id}");
        
        receptor.updatedAt = DateTime.Now;
        var filter = Builders<Submission>.Filter.Eq(x => x.id, submission.id);
        filter &= Builders<Submission>.Filter.ElemMatch(x => x.receptors, x => x.guid == receptor.guid);
        var update = Builders<Submission>.Update.Set(x => x.receptors[-1], receptor);

        await _submissionCollection.UpdateOneAsync(filter, update);
    }
    
    public async Task<Receptor?> GetReceptor(Submission submission, Guid guid)
    {
        return await _submissionCollection.AsQueryable()
                        .Where(x => x.id == submission.id)
                        .SelectMany(x => x.receptors)
                        .Where(x => x.guid == guid)
                        .FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(string id)
    {
        await _submissionCollection.DeleteOneAsync(x => x.id == id);
    }
}