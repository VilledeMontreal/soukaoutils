using System.Threading.Tasks;

namespace WebIdentity.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
