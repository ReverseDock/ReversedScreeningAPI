namespace AsyncAPI.Models;

public record PDBFixResult
{
    public Guid id { get; init; } = Guid.Empty;
    public string fullPath { get; init; } = null!;
}