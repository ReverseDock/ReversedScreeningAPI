namespace AsyncAPI.Models;

public record Result
{
    public string submission { get; init; } = null!;
    public string receptor { get; init; } = null!;
    public float affinity { get; init; }
    public string fullOutputPath { get; init; } = null!;
};
