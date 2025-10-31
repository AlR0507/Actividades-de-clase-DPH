using Microsoft.EntityFrameworkCore;
using Protegiendo_un_sitio_web.Models;
using System.Security.Cryptography;
using Protegiendo_un_sitio_web.Data;


namespace Protegiendo_un_sitio_web.Services
{
    public interface IUserService
    {
        Task<User?> FindByUsernameAsync(string username);
        Task<User?> FindByIdAsync(int id);
        Task<User> CreateAsync(User user);
    }
}
