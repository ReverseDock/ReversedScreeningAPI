using AsyncAPI.Models;

namespace AsyncAPI.Publishers;

public interface IDockingTaskPublisher
{
    public Task PublishDockingTask(DockingTask docking);
}