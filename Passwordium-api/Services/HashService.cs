using Konscious.Security.Cryptography;
using Passwordium_api.Model.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Passwordium_api.Services {
    public class HashService {
        private const int SaltSize = 16;
        private const int HashSize = 32;

        private const int Iterations = 3;
        private const int MemorySize = 65536;
        private const int DegreeOfParallelism = 1;
        public User HashPassword(User user) {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = HashPassword(
                user.Password,
                salt
            );

            user.Password =
                Convert.ToBase64String(salt)
                + "."
                + Convert.ToBase64String(hash);

            return user;
        }

        public bool VerifyHashedPassword(User user, string password) {
            try {
                string[] parts = user.Password.Split('.');

                if (parts.Length != 2) {
                    return false;
                }

                byte[] salt =
                    Convert.FromBase64String(parts[0]);

                byte[] storedHash =
                    Convert.FromBase64String(parts[1]);

                byte[] calculatedHash =
                    HashPassword(password, salt);

                return CryptographicOperations.FixedTimeEquals(
                    storedHash,
                    calculatedHash
                );
            } catch (FormatException) {
                return false;
            }
        }

        private byte[] HashPassword(
            string password,
            byte[] salt) {
            using var argon2 =
                new Argon2id(
                    Encoding.UTF8.GetBytes(password)
                );

            argon2.Salt = salt;
            argon2.DegreeOfParallelism =
                DegreeOfParallelism;

            argon2.Iterations = Iterations;
            argon2.MemorySize = MemorySize;

            return argon2.GetBytes(HashSize);
        }
    }
}
