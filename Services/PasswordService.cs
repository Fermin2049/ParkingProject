using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace FinalMarzo.net.Services
{
    public class PasswordService
    {
        private readonly string _salt;

        public PasswordService(string salt)
        {
            _salt = salt ?? throw new ArgumentNullException(nameof(salt));
        }

        public string HashPassword(string password)
        {
            byte[] saltBytes = Encoding.ASCII.GetBytes(_salt);
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: saltBytes,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8
                )
            );
        }

        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }
    }
}
