namespace AsyncAPI.Models;

public enum FASTATaskType
{
    Receptor,
    UserFile
}

public record FASTATaskInfo
{
    public FASTATaskType type { get; init; }
    public string? receptorId { get; init; }
    public string? userFileId { get; init; }
}