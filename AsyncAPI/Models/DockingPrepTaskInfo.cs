namespace AsyncAPI.Models;

public record DockingPrepTaskInfo
{
    public EDockingPrepPeptideType type { get; init; }
    public Guid? receptorGuid { get; init; }
    public string? submissionId { get; init; }
}