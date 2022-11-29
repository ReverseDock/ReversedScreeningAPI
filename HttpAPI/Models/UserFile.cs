using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class UserFile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public Guid guid { get; set; }
    public string fullPath { get; set; } = null!;
    public string FASTA { get; set; } = null!;
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
}