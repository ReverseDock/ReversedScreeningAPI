namespace HttpAPI.Models.DTO;

public class DockingResultDTO
{
    public Guid guid { get; init; }
    public string receptorFASTA { get; init; } = null!;
    public string receptorName {get; init; } = null!;
    public float affinity { get; init; }
    public bool success { get; init; }
    public HttpAPI.Models.ReceptorFileStatus status { get; init; }
};
