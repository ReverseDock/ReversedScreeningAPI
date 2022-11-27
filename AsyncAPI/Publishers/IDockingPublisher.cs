using AsyncAPI.Models;

namespace AsyncAPI.Publishers;

public interface IDockingPublisher
{
    public Task PublishDocking(Docking docking);
}