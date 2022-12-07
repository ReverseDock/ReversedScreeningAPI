namespace AsyncAPI.Models;

public record PDBFixResult
{
    public Guid id { get; init; } = Guid.Empty;
    public string path { get; init; } = null!;
    public string JSONResult { get; init; } = null!;
}