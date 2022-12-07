namespace AsyncAPI.Models;

public record DockingPrepResult
{
    public Guid id { get; init; } = Guid.Empty;
    public string path { get; init; } = null!;
    public string configPath { get; init; } = null!;
};
