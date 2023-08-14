namespace AsyncAPI.Models;

public record DockingResult
{
    public string submission { get; init; } = null!;
    public Guid receptor { get; init; }
    public float affinity { get; init; }
    public string outputPath { get; init; } = null!;
    public int secondsToCompletion { get; init; }
    public bool success { get; init; }
};
