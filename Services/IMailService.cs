using HttpAPI.Models;

namespace Services;

public interface IMailService
{
    public Task PublishConfirmationMail(Submission submission);
    public Task PublishConfirmedMail(Submission submission);
    public Task PublishFinishedMail(Submission submission);
}