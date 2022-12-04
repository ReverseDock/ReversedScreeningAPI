namespace AsyncAPI.Models;

public record DockingPrepTask
{
    public Guid id { get; init; } = Guid.Empty;
    public string fullPath { get; init; } = null!;
    public EDockingPrepPeptideType type { get; init; }
};
