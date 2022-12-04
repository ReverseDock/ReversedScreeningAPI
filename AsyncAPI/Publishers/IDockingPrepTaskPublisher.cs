using AsyncAPI.Models;

namespace AsyncAPI.Publishers;

public interface IDockingPrepTaskPublisher
{
    public Task PublishDockingPrepTask(DockingPrepTask task);
}