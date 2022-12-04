namespace AsyncAPI.Models;

public record PDBFixTask
{
    public Guid id { get; init; } = Guid.Empty;
    public string fullPath { get; init; } = null!;
}