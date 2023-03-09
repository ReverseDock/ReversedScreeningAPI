using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HttpAPI.Models;

public class Receptor
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string? fileId { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string? pdbqtFileId { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string? configFileId { get; set; }
    public string FASTA { get; set; } = null!;
    public string UniProtID { get; set; } = null!;
    public string name { get; set; } = null!;
    public ReceptorFileStatus status { get; set; }
    public DateTime? updatedAt = null;
    public DateTime? createdAt = null;
}

public enum ReceptorFileStatus
{
    TooBig,
    Unprocessed,
    PDBQTError,
    Ready
}