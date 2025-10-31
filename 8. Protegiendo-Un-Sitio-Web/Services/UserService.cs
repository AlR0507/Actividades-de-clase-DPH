using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Protegiendo_un_sitio_web.Data;
using Protegiendo_un_sitio_web.Models;

namespace Protegiendo_un_sitio_web.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) => _db = db;
        public Task<User?> FindByUsernameAsync(string username) => _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        public Task<User?> FindByIdAsync(int id) => _db.Users.FindAsync(id).AsTask();
        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}