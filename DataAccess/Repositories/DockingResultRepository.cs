using HttpAPI.Models;
using HttpAPI.Models.DTO;

using MongoDB.Driver;
using MongoDB.Bson;

namespace DataAccess.Repositories;

public class DockingResultRepository : IDockingResultRepository
{
    private readonly IMongoCollection<DockingResult> _DockingResultCollection;
    private readonly IMongoCollection<UserFile> _UserFileCollection;
    private readonly IMongoCollection<ReceptorFile> _ReceptorFileCollection;
    private readonly IMongoCollection<Submission> _SubmissionCollection;

    public DockingResultRepository(
        IMongoClient mongoClient,
        IConfiguration configuration
    )
    {
        var databaseName = configuration.GetSection("MongoDB")["DatabaseName"];
        var collectionName = configuration.GetSection("MongoDB")["DockingResultsCollectionName"];
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _DockingResultCollection = mongoDatabase.GetCollection<DockingResult>(collectionName);

        var userFileCollectionName = configuration.GetSection("MongoDB")["UserFilesCollectionName"];
        _UserFileCollection = mongoDatabase.GetCollection<UserFile>(userFileCollectionName);

        var receptorFileCollectionName = configuration.GetSection("MongoDB")["ReceptorFilesCollectionName"];
        _ReceptorFileCollection = mongoDatabase.GetCollection<ReceptorFile>(receptorFileCollectionName);

        var submissionCollectionName = configuration.GetSection("MongoDB")["SubmissionsCollectionName"];
        _SubmissionCollection = mongoDatabase.GetCollection<Submission>(submissionCollectionName);
    }

    public async Task<List<DockingResult>> GetAsync()
    {
        return await _DockingResultCollection.Find(_ => true).ToListAsync();
    }

    public async Task<List<DockingResultDTO>> GetDTOAsync(string submissionId)
    {
        var DockingResults = await _DockingResultCollection.Aggregate()
            .Match<DockingResult>(x => x.submissionId == submissionId)
            .Project<DockingResult, DockingResultReceptorAffinityProjection>(x =>
                new DockingResultReceptorAffinityProjection { receptorId = x.receptorId, affinity = x.affinity })
            .Lookup<DockingResultReceptorAffinityProjection, ReceptorFile, DockingResultReceptorAffinityReceptorsProjection>(
                _ReceptorFileCollection, x => x.receptorId, y => y.id, z => z.receptors)
            .Project<DockingResultReceptorAffinityReceptorsProjection, DockingResultDTO>(x =>
                new DockingResultDTO { receptorFASTA = x.receptors.First().FASTA, affinity = x.affinity })
            .ToListAsync<DockingResultDTO>();
        return DockingResults;
    }

    public async Task<DockingResult?> GetAsync(string id)
    {
        return await _DockingResultCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task<List<DockingResult>> GetBySubmissionId(string submissionId)
    {
        return await _DockingResultCollection.Find(x => x.submissionId == submissionId).ToListAsync();
    }

    public async Task<DockingResult> CreateAsync(DockingResult DockingResult) 
    {
        DockingResult.createdAt = new DateTime();
        DockingResult.updatedAt = new DateTime();
        await _DockingResultCollection.InsertOneAsync(DockingResult);
        return DockingResult;
    }

    public async Task UpdateAsync(string id, DockingResult updatedDockingResult)
    {
        updatedDockingResult.updatedAt = new DateTime();
        await _DockingResultCollection.ReplaceOneAsync(x => x.id == id, updatedDockingResult);
    }
    
    public async Task RemoveAsync(string id)
    {
        await _DockingResultCollection.DeleteOneAsync(x => x.id == id);
    }

    private class DockingResultReceptorAffinityProjection
    {
        public string receptorId { get; init; } = null!;
        public float affinity { get; init; }
    };

    private class DockingResultReceptorAffinityReceptorsProjection
    {
        public string receptorId { get; init; } = null!;
        public float affinity { get; init; }
        public IEnumerable<ReceptorFile> receptors;
    };
}