namespace AsyncAPI.Models;

public record FASTATaskInfo
{
    public FASTATaskType type { get; init; }
    public string submissionId { get; init; }
    public Guid receptorGuid { get; init; }
    public string UnitProtID { get; init; }
}

public enum FASTATaskType
{
    AlphaFold,
    UserPDB
}