namespace HttpAPI.Models.DTO;

public class SubmissionInfoDTO
{
    public string ligandFASTA { get; init; } = null!;
    public IEnumerable<DockingResultDTO> dockingResults { get; init; }
};
