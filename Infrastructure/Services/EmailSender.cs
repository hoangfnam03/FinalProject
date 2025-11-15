using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        public EmailSender(ILogger<EmailSender> logger) => _logger = logger;

        public Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            _logger.LogInformation("SEND EMAIL -> {To}\nSUBJECT: {Subj}\nBODY:\n{Body}", toEmail, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}
