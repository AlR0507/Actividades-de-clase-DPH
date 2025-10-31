using Microsoft.EntityFrameworkCore;
using Protegiendo_un_sitio_web.Models;
using System.Security.Cryptography;
using Protegiendo_un_sitio_web.Data;


namespace Protegiendo_un_sitio_web.Services
{
    public interface ISessionService
    {
        Task<UserSession> CreateAsync(int userId);
        Task<User?> ValidateAsync(string sessionId);
        Task TouchAsync(string sessionId);
        Task InvalidateAsync(string sessionId);
    }
}