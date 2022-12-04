namespace AsyncAPI.Models;

public record DockingTask
{
    public string submissionId { get; init; } = null!;

    public string receptorId { get; init; } = null!;

    public string fullLigandPath { get; init; } = null!;

    public string fullReceptorPath { get; init; } = null!;

    public string fullConfigPath { get; init; } = null!;
}
