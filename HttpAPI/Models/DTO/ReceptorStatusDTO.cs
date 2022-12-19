namespace HttpAPI.Models.DTO;

public class ReceptorStatusDTO
{
    public int exhaustiveness { get; init; }
    public IEnumerable<ReceptorDTO> receptors { get; init; } = null!;
};

public class ReceptorDTO
{
    public string UniProtId { get; init; } = null!;
    public string status { get; init; } = null!;
    public string name { get; init; } = null!;
};

