using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;

namespace Masaking.Infrastructure
{
    public static class PasswordRecovery
    {
        public const string RequestMessage = "If an active account exists with that email, we'll send a six-digit reset code. Please check your inbox and spam folder.";
        public const string InvalidCodeMessage = "The code is incorrect, expired or unavailable. Codes allow five attempts. You can request a new code.";

        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
            {
                return false;
            }

            try
            {
                return string.Equals(new MailAddress(email).Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string RequestReset(string email, string ipAddress, string previousChallenge = null)
        {
            string challenge = RecoveryOtp.NewChallenge();
            if (!IsEmailValid(email))
            {
                return challenge;
            }

            try
            {
                if (!AllowRequest(email, ipAddress))
                {
                    // A premature resend must not discard a code already in transit.
                    if (RecoveryOtp.HasValidChallenge(previousChallenge))
                    {
                        challenge = previousChallenge;
                    }

                    return challenge;
                }
            }
            catch (Exception error)
            {
                RecoveryDelivery.ReportFailure(error);
                return challenge;
            }

            // Queue all requests so SMTP timing does not reveal registered accounts.
            try
            {
                HostingEnvironment.QueueBackgroundWorkItem(cancel =>
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        IssueAndSend(email, challenge);
                    }
                    catch (Exception error)
                    {
                        RecoveryDelivery.ReportFailure(error);
                    }
                });
            }
            catch (Exception error)
            {
                RecoveryDelivery.ReportFailure(error);
            }

            return challenge;
        }

        private static bool AllowRequest(string email, string ipAddress)
        {
            return AllowAttempt(
                kind: "E",
                value: email.ToUpperInvariant(),
                limit: 5,
                minutes: 60,
                cooldown: 60,
                secondKind: "I",
                secondValue: ipAddress ?? "unknown",
                secondLimit: 20,
                secondMinutes: 60
            );
        }

        private static bool AllowCodeAttempt(string ipAddress)
        {
            return AllowAttempt(
                kind: "V",
                value: ipAddress ?? "unknown",
                limit: 30,
                minutes: 15,
                cooldown: 0,
                secondKind: null,
                secondValue: null,
                secondLimit: 0,
                secondMinutes: 0
            );
        }

        private static bool AllowAttempt(
            string kind,
            string value,
            int limit,
            int minutes,
            int cooldown,
            string secondKind,
            string secondValue,
            int secondLimit,
            int secondMinutes)
        {
            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                // Serialize the short check/insert across sessions and application instances.
                const string lockSql = @"
                    DECLARE @result int;
                    EXEC @result = sys.sp_getapplock 
                        @Resource = 'Masaking.PasswordRecovery.Throttle',
                        @LockMode = 'Exclusive', 
                        @LockOwner = 'Transaction', 
                        @LockTimeout = 5000;
                    SELECT @result;";

                using (SqlCommand gate = new SqlCommand(lockSql, cn, tx))
                {
                    if (Convert.ToInt32(gate.ExecuteScalar()) < 0)
                    {
                        return false;
                    }
                }

                if (!WithinLimit(cn, tx, kind, value, limit, minutes, cooldown)
                    || (secondKind != null && !WithinLimit(cn, tx, secondKind, secondValue, secondLimit, secondMinutes, 0)))
                {
                    return false;
                }

                RecordAttempt(cn, tx, kind, value);
                if (secondKind != null)
                {
                    RecordAttempt(cn, tx, secondKind, secondValue);
                }

                const string cleanupSql = "DELETE TOP (200) FROM dbo.passwordResetAttempts WHERE createdAtUtc < DATEADD(hour, -24, SYSUTCDATETIME())";
                using (SqlCommand cleanup = new SqlCommand(cleanupSql, cn, tx))
                {
                    cleanup.ExecuteNonQuery();
                }

                tx.Commit();
                return true;
            }
        }

        private static bool WithinLimit(
            SqlConnection cn,
            SqlTransaction tx,
            string kind,
            string value,
            int limit,
            int minutes,
            int cooldown)
        {
            const string sql = @"
                SELECT CASE 
                    WHEN COUNT(*) < @limit 
                     AND (@cooldown = 0 OR COUNT(CASE WHEN createdAtUtc > DATEADD(second, -@cooldown, SYSUTCDATETIME()) THEN 1 END) = 0)
                    THEN 1 
                    ELSE 0 
                END 
                FROM dbo.passwordResetAttempts
                WHERE scopeType = @kind 
                  AND scopeHash = @hash 
                  AND createdAtUtc > DATEADD(minute, -@minutes, SYSUTCDATETIME())";

            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@kind", SqlDbType.Char, 1).Value = kind;
                cmd.Parameters.Add("@hash", SqlDbType.Binary, 32).Value = Hash(value);
                cmd.Parameters.AddWithValue("@limit", limit);
                cmd.Parameters.AddWithValue("@minutes", minutes);
                cmd.Parameters.AddWithValue("@cooldown", cooldown);

                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        private static void RecordAttempt(SqlConnection cn, SqlTransaction tx, string kind, string value)
        {
            const string sql = "INSERT dbo.passwordResetAttempts (scopeType, scopeHash, createdAtUtc) VALUES (@kind, @hash, SYSUTCDATETIME())";

            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@kind", SqlDbType.Char, 1).Value = kind;
                cmd.Parameters.Add("@hash", SqlDbType.Binary, 32).Value = Hash(value);
                cmd.ExecuteNonQuery();
            }
        }

        private static void IssueAndSend(string email, string challenge)
        {
            RecoveryTokenUser user;

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT userID, email, password, authVersion FROM dbo.users WHERE email = @email AND isActive = 1", cn))
            {
                cmd.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    user = reader.Read() ? ReadUser(reader) : null;
                }
            }

            if (user == null)
            {
                return;
            }

            string protectedTicket = OwinRecoveryTokens.Generate(user);
            string code = RecoveryOtp.NewCode();

            const string insertSql = @"
                INSERT dbo.passwordResetTokens (userID, tokenHash, email, authVersion, createdAtUtc, expiresAtUtc, challengeHash, protectedTicket, failedAttempts)
                SELECT userID, @hash, email, authVersion, SYSUTCDATETIME(), DATEADD(minute, 10, SYSUTCDATETIME()), @challenge, @ticket, 0
                FROM dbo.users 
                WHERE userID = @id 
                  AND email = @email 
                  AND authVersion = @version 
                  AND password = @previous 
                  AND isActive = 1";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(insertSql, cn))
            {
                cmd.Parameters.Add("@hash", SqlDbType.Binary, 32).Value = RecoveryOtp.HashCode(challenge, code);
                cmd.Parameters.Add("@challenge", SqlDbType.Binary, 32).Value = RecoveryOtp.HashChallenge(challenge);
                cmd.Parameters.Add("@ticket", SqlDbType.NVarChar, -1).Value = protectedTicket;
                cmd.Parameters.AddWithValue("@id", user.Id);
                cmd.Parameters.AddWithValue("@email", user.UserName);
                cmd.Parameters.AddWithValue("@version", user.AuthVersion);
                cmd.Parameters.AddWithValue("@previous", user.PasswordHash);

                if (cmd.ExecuteNonQuery() != 1)
                {
                    return;
                }
            }

            try
            {
                RecoveryMail.SendOtp(user.UserName, code);
            }
            catch
            {
                const string updateSql = "UPDATE dbo.passwordResetTokens SET usedAtUtc = SYSUTCDATETIME() WHERE challengeHash = @challenge AND usedAtUtc IS NULL";

                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(updateSql, cn))
                {
                    cmd.Parameters.Add("@challenge", SqlDbType.Binary, 32).Value = RecoveryOtp.HashChallenge(challenge);
                    cmd.ExecuteNonQuery();
                }

                throw;
            }
        }

        public static bool ResetPassword(string challenge, string code, string password, string ipAddress)
        {
            if (!RecoveryOtp.HasValidChallenge(challenge)
                || !RecoveryOtp.HasValidCode(code)
                || password == null
                || password.Length < 8
                || password.Length > 64)
            {
                return false;
            }

            if (!AllowCodeAttempt(ipAddress))
            {
                return false;
            }

            string recipient;
            // Derive before locking so password hashing does not hold database locks.
            string passwordHash = Database.HashPassword(password);

            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                // Lock the account first, then the token. Concurrent completions for the
                // same account must re-read its updated stamp after the winner commits.
                object userId;
                using (SqlCommand lookup = new SqlCommand("SELECT userID FROM dbo.passwordResetTokens WHERE challengeHash = @challenge", cn, tx))
                {
                    lookup.Parameters.Add("@challenge", SqlDbType.Binary, 32).Value = RecoveryOtp.HashChallenge(challenge);
                    userId = lookup.ExecuteScalar();

                    if (userId == null)
                    {
                        return false;
                    }
                }

                // Release the lookup's shared token lock before waiting for the user lock.
                using (SqlCommand gate = new SqlCommand("SELECT authVersion FROM dbo.users WITH (UPDLOCK, HOLDLOCK) WHERE userID = @id AND isActive = 1", cn, tx))
                {
                    gate.Parameters.AddWithValue("@id", userId);

                    if (gate.ExecuteScalar() == null)
                    {
                        return false;
                    }
                }

                RecoveryTokenUser user;
                byte[] expectedHash;
                string protectedTicket;

                const string checkSql = @"
                    SELECT u.userID, u.email, u.password, u.authVersion, t.tokenHash, t.protectedTicket
                    FROM dbo.passwordResetTokens t WITH (UPDLOCK, HOLDLOCK)
                    JOIN dbo.users u ON u.userID = t.userID
                    WHERE t.challengeHash = @challenge 
                      AND t.usedAtUtc IS NULL 
                      AND t.expiresAtUtc > SYSUTCDATETIME()
                      AND t.failedAttempts < 5 
                      AND t.protectedTicket IS NOT NULL
                      AND u.isActive = 1 
                      AND u.authVersion = t.authVersion 
                      AND u.email = t.email";

                using (SqlCommand check = new SqlCommand(checkSql, cn, tx))
                {
                    check.Parameters.Add("@challenge", SqlDbType.Binary, 32).Value = RecoveryOtp.HashChallenge(challenge);
                    using (SqlDataReader reader = check.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        user = ReadUser(reader);
                        expectedHash = (byte[])reader[4];
                        protectedTicket = reader.GetString(5);
                    }
                }

                if (!RecoveryOtp.Matches(challenge, code, expectedHash) || !OwinRecoveryTokens.Validate(user, protectedTicket))
                {
                    const string failureSql = @"
                        UPDATE dbo.passwordResetTokens
                        SET failedAttempts = failedAttempts + 1,
                            usedAtUtc = CASE WHEN failedAttempts + 1 >= 5 THEN SYSUTCDATETIME() ELSE usedAtUtc END
                        WHERE challengeHash = @challenge";

                    using (SqlCommand failure = new SqlCommand(failureSql, cn, tx))
                    {
                        failure.Parameters.Add("@challenge", SqlDbType.Binary, 32).Value = RecoveryOtp.HashChallenge(challenge);
                        failure.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return false;
                }

                const string updateSql = @"
                    UPDATE dbo.users SET password = @password, authVersion = authVersion + 1 WHERE userID = @id;
                    UPDATE dbo.passwordResetTokens SET usedAtUtc = SYSUTCDATETIME() WHERE userID = @id AND usedAtUtc IS NULL;";

                using (SqlCommand cmd = new SqlCommand(updateSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.Parameters.AddWithValue("@password", passwordHash);
                    cmd.ExecuteNonQuery();
                }

                recipient = user.UserName;
                tx.Commit();
            }

            // Notification failure must not undo a committed password change.
            try
            {
                HostingEnvironment.QueueBackgroundWorkItem(cancel =>
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        RecoveryMail.SendConfirmation(recipient);
                    }
                    catch (Exception error)
                    {
                        Trace.TraceError("Password reset confirmation failed: {0}", RecoveryDelivery.FailureCategory(error));
                    }
                });
            }
            catch (Exception)
            {
                Trace.TraceError("Password reset confirmation could not be queued.");
            }

            return true;
        }

        private static RecoveryTokenUser ReadUser(SqlDataReader reader)
        {
            return new RecoveryTokenUser
            {
                Id = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                AuthVersion = reader.GetInt32(3)
            };
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