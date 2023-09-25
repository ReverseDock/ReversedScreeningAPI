namespace HttpAPI.Models;

public class Receptor
{
    public Guid guid { get; set; }
    public string name { get; set; } = null!;
    public string FASTA { get; set; } = null!;
    public ReceptorFileStatus status { get; set; }
    public float affinity { get; set; }
    public int secondsToCompletion { get; set; }
    public bool success { get; set; }
    public FileDescriptor? file { get; set; }
    public FileDescriptor? pdbqtFile { get; set; }
    public FileDescriptor? configFile { get; set; }
    public FileDescriptor? outputFile { get; set; }
    public bool alphaFold { get; set; } = false;
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