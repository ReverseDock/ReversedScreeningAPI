using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class Result
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public string submissionId { get; init; } = null!;
    public string receptorId { get; init; } = null!;
    public float affinity { get; init; }
    public string fullOutputPath { get; init; } = null!;
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
};
