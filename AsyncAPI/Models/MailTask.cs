namespace AsyncAPI.Models;

public record MailTask
{
    public string recipient { get; init; } = null!;
    public string subject { get; init; } = null!;
    public string bodyRaw { get; init; } = null!;
    public string bodyHTML { get; init; } = null!;
};

