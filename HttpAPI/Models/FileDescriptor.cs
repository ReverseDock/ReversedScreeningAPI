using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class FileDescriptor
{
    public Guid guid { get; set; }
    public string path { get; set; } = null!;
    public bool isPublic { get; set; } = false;
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
}