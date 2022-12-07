namespace AsyncAPI.Models;

public record DockingPrepTaskInfo
{
    public EDockingPrepPeptideType type { get; init; }
    public string? receptorId { get; init; }
    public string? submissionId { get; init; }
}