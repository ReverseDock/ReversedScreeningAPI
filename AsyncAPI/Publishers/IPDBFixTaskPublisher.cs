using AsyncAPI.Models;

namespace AsyncAPI.Publishers;

public interface IPDBFixTaskPublisher
{
    public Task PublishPDBFixTask(PDBFixTask task);
}