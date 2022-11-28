namespace AsyncAPI.Models;

public record Docking
{
    public string submissionId { get; init; } = null!;

    public string receptorId { get; init; } = null!;

    public string fullLigandPath { get; init; } = null!;

    public string fullReceptorPath { get; init; } = null!;
}
