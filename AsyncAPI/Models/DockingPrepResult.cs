namespace AsyncAPI.Models;

public record DockingPrepResult
{
    public Guid id { get; init; } = Guid.Empty;
    public string fullPath { get; init; } = null!;
    public string fullConfigPath { get; init; } = null!;
};
