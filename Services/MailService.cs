using System.Net.Http.Headers;
using AsyncAPI.Publishers;
using DataAccess.Repositories;

using HttpAPI.Models;

using AsyncAPI.Models;

namespace Services;

class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMailTaskPublisher _mailTaskPublisher;
    
    public MailService(ILogger<MailService> logger, IConfiguration configuration, IMailTaskPublisher mailTaskPublisher)
    {
        _logger = logger;
        _configuration = configuration;
        _mailTaskPublisher = mailTaskPublisher;
    }

    public async Task PublishConfirmationMail(Submission submission)
    {
        var mailTask = new MailTask
        {
            recipient = submission.emailAddress,
            subject = "Confirm Reversed Screening Submission",
            bodyHTML = $@"
            <html>
                <body>
                    <h1>Please confirm your submission</h1>
                    <p> Dear Reversed Screening user, </p>
                    <p> please confirm your submission by clicking on the following link: <a href='http://localhost:3000/confirm?id={submission.guid}&confirmationId={submission.confirmationGuid}'>Confirm</a>
                </body>
            </html>
            ",
            bodyRaw = $@"
             Dear Reversed Screening user,
             please confirm your submission by opening the following link: http://localhost:3000/confirm?id={submission.guid}&confirmationId={submission.confirmationGuid}
            "
        };

        await _mailTaskPublisher.PublishMailTask(mailTask);
    }

    public async Task PublishConfirmedMail(Submission submission)
    {
        var mailTask = new MailTask
        {
            recipient = submission.emailAddress,
            subject = "Reversed Screening Submission Confirmed",
            bodyHTML = $@"
            <html>
                <body>
                    <h1>Submission Confirmed</h1>
                    <p> Dear Reversed Screening user, </p>
                    <p> Thank you for confirming your submission. You can check your results at: <a href='http://localhost:3000/info?id={submission.guid}'>Results</a>
                </body>
            </html>
            ",
            bodyRaw = $@"
             Dear Reversed Screening user,
             thank you for confirming your submission. You can check your results at the following link: http://localhost:3000/info?id={submission.guid}
            "
        };

        await _mailTaskPublisher.PublishMailTask(mailTask);
    }

    public async Task PublishFinishedMail(Submission submission)
    {
        var mailTask = new MailTask
        {
            recipient = submission.emailAddress,
            subject = "Reversed Screening Submission Finished",
            bodyHTML = $@"
            <html>
                <body>
                    <h1>Submission Finished</h1>
                    <p> Dear Reversed Screening user, </p>
                    <p> Your submission has finished. You can check your results at: <a href='http://localhost:3000/info?id={submission.guid}'>Results</a>
                </body>
            </html>
            ",
            bodyRaw = $@"
             Dear Reversed Screening user,
             Your submission has finished. You can check your results at the following link: http://localhost:3000/info?id={submission.guid}
            "
        };

        await _mailTaskPublisher.PublishMailTask(mailTask);
    }
}