namespace AsyncAPI.Models;

public record DockingTask
{
    public string submissionId { get; init; } = null!;

    public Guid receptorId { get; init; }

    public string ligandPath { get; init; } = null!;

    public string receptorPath { get; init; } = null!;

    public string configPath { get; init; } = null!;
    
    public int exhaustiveness { get; init; } = 0;
}
