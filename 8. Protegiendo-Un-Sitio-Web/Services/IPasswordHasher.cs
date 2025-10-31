using System.Security.Cryptography;
using System.Text;


namespace Protegiendo_un_sitio_web.Services
{
    public interface IPasswordHasher
    {
        string Hash(string password, int iterations = 100_000);
        bool Verify(string password, string stored);
    }
}