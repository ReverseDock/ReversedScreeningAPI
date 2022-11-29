namespace AsyncAPI.Models;

public record FASTAResult
{
    public Guid id { get; init; } = Guid.Empty;
    public string FASTA { get; init; } = null!;
}
