using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Interfaces
{
    public interface ISendGridEmailService
    {
        Task SendEmailAsync(string to, string cc, string title, string content);
        Task SendEmailAsync(string to, string title, string content);
        Task SendEmailAsync(SendGridMessage email);
        Task<bool> SentAsync(string mailTos, string subject, string body);
        Task<bool> SentAsync(string mailTos, string subject, string body, string attachFileName, string attachFileContent);
        string ReadEmailTemplate(string template);
    }
}
