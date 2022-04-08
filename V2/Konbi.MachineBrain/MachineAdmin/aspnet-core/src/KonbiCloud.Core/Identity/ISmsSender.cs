using System.Threading.Tasks;

namespace KonbiCloud.Identity
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}