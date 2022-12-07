namespace AsyncAPI.Models;

public record PDBFixTask
{
    public Guid id { get; init; } = Guid.Empty;
    public string path { get; init; } = null!;
}