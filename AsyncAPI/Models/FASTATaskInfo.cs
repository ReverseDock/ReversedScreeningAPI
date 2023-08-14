namespace AsyncAPI.Models;

public record FASTATaskInfo
{
    public string submissionId { get; init; }
    public Guid receptorGuid { get; init; }
}