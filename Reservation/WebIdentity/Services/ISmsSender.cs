using System.Threading.Tasks;

namespace WebIdentity.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
