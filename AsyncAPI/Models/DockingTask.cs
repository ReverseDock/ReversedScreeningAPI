namespace AsyncAPI.Models;

public record DockingTask
{
    public string submissionId { get; init; } = null!;

    public string receptorId { get; init; } = null!;

    public string ligandPath { get; init; } = null!;

    public string receptorPath { get; init; } = null!;

    public string configPath { get; init; } = null!;
}
