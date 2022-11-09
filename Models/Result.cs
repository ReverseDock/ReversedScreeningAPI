namespace Models;

public record Result
{
    public int submission { get; init; }
    public int ligand { get; init; }
    public float affinity { get; init; }
};
