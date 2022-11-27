using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class Submission
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    public string path { get; set; } = null!;

    public string? fileId = null;

    public string emailAddress = null!;

    public string IP = null!;

    public bool confirmed = false;

    public DateTime? updatedAt = null;

    public DateTime? createdAt = null;
}