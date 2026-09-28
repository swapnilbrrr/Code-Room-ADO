using System;
using System.Security.Cryptography;

namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// PBKDF2 password hashing with a per-account random salt, using no external package.
    /// Stored format: {iterations}.{base64 salt}.{base64 hash}
    /// The parameters and format are identical to the source application, so hashes written by the
    /// MySQL/EF Core build still verify here and vice versa.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("A password is required.", nameof(password));
            }

            var salt = new byte[SaltSize];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            var hash = Derive(password, salt, Iterations, KeySize);

            return string.Join(".", Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Split(new[] { '.' }, 3);

            if (parts.Length != 3)
            {
                return false;
            }

            int iterations;

            if (!int.TryParse(parts[0], out iterations) || iterations <= 0)
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[1]);
                var expected = Convert.FromBase64String(parts[2]);
                var actual = Derive(password, salt, iterations, expected.Length);

                return FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] Derive(string password, byte[] salt, int iterations, int keyLength)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(keyLength);
            }
        }

        /// <summary>
        /// Comparison that does not stop at the first difference, so a failure cannot be timed into a
        /// guess about the stored hash.
        /// </summary>
        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;

            for (var i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }

            return difference == 0;
        }
    }
}
