namespace AsyncAPI.Models;

public record DockingResult
{
    public string submission { get; init; } = null!;
    public string receptor { get; init; } = null!;
    public float affinity { get; init; }
    public string fullOutputPath { get; init; } = null!;
};