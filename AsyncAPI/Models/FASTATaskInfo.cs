namespace AsyncAPI.Models;

public enum FASTATaskType
{
    Receptor,
    Ligand
}

public record FASTATaskInfo
{
    public FASTATaskType type { get; init; }
    public string? receptorId { get; init; }
    public string? submissionId { get; init; }
}