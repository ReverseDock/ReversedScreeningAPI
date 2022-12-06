using HttpAPI.Models;

namespace Services;

public interface IPDBFixService
{
    public Task PublishPDBFixTask(UserFile userFile);
    public Task<bool> CheckPDBFixStatus(UserFile userFile);
    public Task<FileStream?> GetFixedFile(UserFile userFile);
}