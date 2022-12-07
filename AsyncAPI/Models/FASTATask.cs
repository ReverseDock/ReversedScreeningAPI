namespace AsyncAPI.Models;

public record FASTATask
{
    public Guid id { get; init; } = Guid.Empty;
    public string path { get; init; } = null!;
}
