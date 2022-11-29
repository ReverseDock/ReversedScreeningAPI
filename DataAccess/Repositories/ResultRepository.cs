using HttpAPI.Models;
using HttpAPI.Models.DTO;

using MongoDB.Driver;
using MongoDB.Bson;

namespace DataAccess.Repositories;

public class ResultRepository : IResultRepository
{
    private readonly IMongoCollection<Result> _ResultCollection;
    private readonly IMongoCollection<UserFile> _UserFileCollection;
    private readonly IMongoCollection<ReceptorFile> _ReceptorFileCollection;
    private readonly IMongoCollection<Submission> _SubmissionCollection;

    public ResultRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["ResultsCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _ResultCollection = mongoDatabase.GetCollection<Result>(collectionName);

        var userFileCollectionName = configuration.GetSection("MongoDB")["UserFilesCollectionName"];
        _UserFileCollection = mongoDatabase.GetCollection<UserFile>(userFileCollectionName);

        var receptorFileCollectionName = configuration.GetSection("MongoDB")["ReceptorFilesCollectionName"];
        _ReceptorFileCollection = mongoDatabase.GetCollection<ReceptorFile>(receptorFileCollectionName);

        var submissionCollectionName = configuration.GetSection("MongoDB")["SubmissionsCollectionName"];
        _SubmissionCollection = mongoDatabase.GetCollection<Submission>(submissionCollectionName);
    }

    public async Task<List<Result>> GetAsync()
    {
        return await _ResultCollection.Find(_ => true).ToListAsync();
    }

    public async Task<List<ResultDTO>> GetDTOAsync(string submissionId)
    {
        var results = await _ResultCollection.Aggregate()
            .Match<Result>(x => x.submissionId == submissionId)
            .Project<Result, ResultReceptorAffinityProjection>(x =>
                new ResultReceptorAffinityProjection { receptorId = x.receptorId, affinity = x.affinity })
            .Lookup<ResultReceptorAffinityProjection, ReceptorFile, ResultReceptorAffinityReceptorsProjection>(
                _ReceptorFileCollection, x => x.receptorId, y => y.id, z => z.receptors)
            .Project<ResultReceptorAffinityReceptorsProjection, ResultDTO>(x =>
                new ResultDTO { receptorFASTA = x.receptors.First().FASTA, affinity = x.affinity })
            .ToListAsync<ResultDTO>();
        return results;
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

    private class ResultReceptorAffinityProjection
    {
        public string receptorId { get; init; } = null!;
        public float affinity { get; init; }
    };

    private class ResultReceptorAffinityReceptorsProjection
    {
        public string receptorId { get; init; } = null!;
        public float affinity { get; init; }
        public IEnumerable<ReceptorFile> receptors;
    };
}