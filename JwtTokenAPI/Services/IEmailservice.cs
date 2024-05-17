using JwtTokenAPI.Models;

namespace JwtTokenAPI.Services
{
    public interface IEmailservice
    {
        void SendEmail(Message message);
    }
}
