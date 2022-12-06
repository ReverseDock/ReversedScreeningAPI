using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class Submission
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public Guid guid { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string? fileId = null;
    public string emailAddress = null!;
    public string IP = null!;
    public bool failed = false;
    public bool confirmed = false;
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
}