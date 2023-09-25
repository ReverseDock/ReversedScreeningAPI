using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class AlphaFoldReceptor 
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public string UniProtID { get; set; } = null!;
    public string FASTA { get; set; } = null!;
    public FileDescriptor? file { get; set; }
}