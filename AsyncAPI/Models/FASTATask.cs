namespace AsyncAPI.Models;

public record FASTATask
{
    public Guid id { get; init; } = Guid.Empty;
    public string fullPath { get; init; } = null!;
}
