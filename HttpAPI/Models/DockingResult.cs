using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class DockingResult
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string submissionId { get; init; } = null!;
    [BsonRepresentation(BsonType.ObjectId)]
    public string receptorId { get; init; } = null!;
    public float affinity { get; init; }
    public string fullOutputPath { get; init; } = null!;
    public int secondsToCompletion { get; init; }
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
};
