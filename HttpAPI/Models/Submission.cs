using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class Submission
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public Guid guid { get; set; }
    public Guid confirmationGuid { get; set; }
    public string emailAddress { get; set; }
    public string IP { get; set; }
    public string fixedJSONResult { get; set; }
    public string FASTA { get; set; }
    public SubmissionStatus status { get; set; } = SubmissionStatus.Incomplete;
    public FileDescriptor ligand { get; set; }
    public FileDescriptor? fixedLigand { get; set; }
    public FileDescriptor? pdbqtLigand { get; set; }
    public List<Receptor> receptors { get; set; }
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
}

public enum SubmissionStatus
{
    Incomplete, // No receptors added
    ConfirmationPending,
    Confirmed,
    PreparationFailed, // PDBQT creation failed
    PreparationComplete, // PDBQT creation succeeded
    InProgress, // First result is returned
    Finished, // All results returned
    Failed
}