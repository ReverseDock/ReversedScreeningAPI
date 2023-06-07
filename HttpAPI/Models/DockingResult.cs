using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class DockingResult
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public Guid guid { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string submissionId { get; set; } = null!;
    [BsonRepresentation(BsonType.ObjectId)]
    public string receptorId { get; set; } = null!;
    public float affinity { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string outputFileId { get; set; } = null!;
    public int secondsToCompletion { get; set; }
    public bool success { get; set; }
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
};