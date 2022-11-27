namespace AsyncAPI.Models;

public record Result
{
    public string submission { get; init; } = null!;
    public int ligand { get; init; }
    public float affinity { get; init; }
    public string fullOutputPath { get; init; } = null!;
};
