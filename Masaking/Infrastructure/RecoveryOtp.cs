using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Masaking.Infrastructure
{
    public static class RecoveryOtp
    {
        public const int LifetimeMinutes = 10;
        public const int MaximumAttempts = 5;

        public static string NewChallenge()
        {
            byte[] bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static string NewCode()
        {
            // Rejection sampling avoids modulo bias, and D6 preserves leading zeroes.
            const uint range = 1000000;
            uint limit = uint.MaxValue - (uint.MaxValue % range);
            byte[] bytes = new byte[4];
            uint value;

            using (var rng = RandomNumberGenerator.Create())
            {
                do
                {
                    rng.GetBytes(bytes);
                    value = BitConverter.ToUInt32(bytes, 0);
                }
                while (value >= limit);
            }

            return (value % range).ToString("D6", CultureInfo.InvariantCulture);
        }

        public static bool HasValidChallenge(string challenge)
        {
            if (challenge == null || challenge.Length != 43)
            {
                return false;
            }

            foreach (char c in challenge)
            {
                bool isLetterOrDigit = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
                if (!isLetterOrDigit && c != '-' && c != '_')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasValidCode(string code)
        {
            if (code == null || code.Length != 6)
            {
                return false;
            }

            foreach (char c in code)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }

        public static byte[] HashChallenge(string challenge)
        {
            return Hash("Masaking.OtpChallenge.v1:" + challenge);
        }

        public static byte[] HashCode(string challenge, string code)
        {
            if (!HasValidChallenge(challenge) || !HasValidCode(code))
            {
                throw new ArgumentException("Invalid OTP input.");
            }

            return Hash("Masaking.OtpCode.v1:" + challenge + ":" + code);
        }

        public static bool Matches(string challenge, string code, byte[] expected)
        {
            if (!HasValidChallenge(challenge) || !HasValidCode(code) || expected == null || expected.Length != 32)
            {
                return false;
            }

            byte[] actual = HashCode(challenge, code);
            int difference = 0;

            for (int i = 0; i < actual.Length; i++)
            {
                difference |= actual[i] ^ expected[i];
            }

            return difference == 0;
        }

        private static byte[] Hash(string value)
        {
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }
    }
}