namespace AsyncAPI.Models;

public record DockingPrepTask
{
    public Guid id { get; init; } = Guid.Empty;
    public string path { get; init; } = null!;
    public EDockingPrepPeptideType type { get; init; }
};
