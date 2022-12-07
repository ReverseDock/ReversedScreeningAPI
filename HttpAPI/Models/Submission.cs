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
    [BsonRepresentation(BsonType.ObjectId)]
    public string? fileId = null;
    public string emailAddress = null!;
    public string IP = null!;
    [BsonRepresentation(BsonType.ObjectId)]
    public string? receptorListFileId = null;
    [BsonRepresentation(BsonType.ObjectId)]
    public string? fixedFileId = null;
    public string fixedJSONResult { get; set; } = null!;
    [BsonRepresentation(BsonType.ObjectId)]
    public string? pdbqtFileId = null;
    public string FASTA { get; set; } = null!;
    public SubmissionStatus status { get; set; }
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
}

public enum SubmissionStatus
{
    ConfirmationPending,
    Confirmed,
    PreparationFailed,
    PreparationComplete,
    InProgress,
    Finished,
    Failed
}