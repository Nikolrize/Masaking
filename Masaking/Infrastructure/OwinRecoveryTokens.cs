using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;


namespace Masaking.Infrastructure
{
    // Account snapshot used to bind recovery proofs to the current credentials.
    public sealed class RecoveryTokenUser : IUser<int>
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int AuthVersion { get; set; }
        public string PasswordHash { get; set; }

        public string SecurityStamp
        {
            get
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    {
                        writer.Write(Id);
                        writer.Write(AuthVersion);
                        writer.Write(UserName ?? "");
                        writer.Write(PasswordHash ?? "");
                    }
                    using (var sha = SHA256.Create())
                        return Convert.ToBase64String(sha.ComputeHash(stream.ToArray()));
                }
            }
        }
    }

    public static class OwinRecoveryTokens
    {
        private static volatile IDataProtectionProvider protectionProvider;
        private const string ProtectionPurpose = "Masaking.PasswordRecovery.Otp.v1";

        public static void Configure(IDataProtectionProvider provider)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            protectionProvider = provider;
        }

        private static UserManager<RecoveryTokenUser, int> CreateManager(RecoveryTokenUser user)
        {
            var provider = protectionProvider;
            if (provider == null)
                throw new InvalidOperationException("OWIN password recovery has not been initialized. Check OWIN startup discovery.");
            return new UserManager<RecoveryTokenUser, int>(new RecoveryTokenStore(user))
            {
                UserTokenProvider = new DataProtectorTokenProvider<RecoveryTokenUser, int>(provider.Create(ProtectionPurpose))
                {
                    TokenLifespan = TimeSpan.FromMinutes(RecoveryOtp.LifetimeMinutes)
                }
            };
        }

        public static string Generate(RecoveryTokenUser user)
        {
            using (var manager = CreateManager(user))
            {
                // The snapshot store completes synchronously and performs no asynchronous I/O.
                string token = manager.GeneratePasswordResetTokenAsync(user.Id).GetAwaiter().GetResult();
                return Encode(Convert.FromBase64String(token));
            }
        }

        public static bool Validate(RecoveryTokenUser user, string token)
        {
            byte[] protectedBytes;
            if (user == null || !TryDecode(token, out protectedBytes)) return false;
            using (var manager = CreateManager(user))
                return manager.VerifyUserTokenAsync(user.Id, "ResetPassword", Convert.ToBase64String(protectedBytes))
                    .GetAwaiter().GetResult();
        }

        private static bool TryDecode(string token, out byte[] bytes)
        {
            bytes = null;
            // Reject malformed or oversized input before decoding.
            if (String.IsNullOrEmpty(token) || token.Length < 80 || token.Length > 4096 || token.Length % 4 == 1)
                return false;
            foreach (char c in token)
                if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z')
                    && !(c >= '0' && c <= '9') && c != '-' && c != '_') return false;
            try
            {
                string padded = token.Replace('-', '+').Replace('_', '/');
                padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
                bytes = Convert.FromBase64String(padded);
                return String.Equals(Encode(bytes), token, StringComparison.Ordinal);
            }
            catch (FormatException) { return false; }
        }

        private static string Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }

    // Password writes and code consumption share a SQL transaction in PasswordRecovery.
    internal sealed class RecoveryTokenStore : IUserStore<RecoveryTokenUser, int>, IUserSecurityStampStore<RecoveryTokenUser, int>
    {
        private readonly RecoveryTokenUser snapshot;
        public RecoveryTokenStore(RecoveryTokenUser user) { snapshot = user; }
        public Task<RecoveryTokenUser> FindByIdAsync(int id)
        {
            return Task.FromResult(snapshot != null && snapshot.Id == id ? snapshot : null);
        }
        public Task<RecoveryTokenUser> FindByNameAsync(string name)
        {
            return Task.FromResult(snapshot != null && String.Equals(snapshot.UserName, name, StringComparison.OrdinalIgnoreCase) ? snapshot : null);
        }
        public Task<string> GetSecurityStampAsync(RecoveryTokenUser user) { return Task.FromResult(user.SecurityStamp); }
        public Task SetSecurityStampAsync(RecoveryTokenUser user, string stamp) { throw new NotSupportedException("Token-only store."); }
        public Task CreateAsync(RecoveryTokenUser user) { throw new NotSupportedException("Token-only store."); }
        public Task UpdateAsync(RecoveryTokenUser user) { throw new NotSupportedException("Token-only store."); }
        public Task DeleteAsync(RecoveryTokenUser user) { throw new NotSupportedException("Token-only store."); }
        public void Dispose() { }
    }
}
