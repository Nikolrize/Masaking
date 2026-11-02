using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;
using System.Web;

namespace Masaking.Infrastructure
{
    public static class Database
    {
        // Transparent fallback: profile and navbar components display initials, never a user icon.
        public const string DefaultAvatar = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";

        public static string AvatarUrl(object value)
        {
            string path = Convert.ToString(value ?? "");
            if (path.StartsWith("~/", StringComparison.Ordinal)) path = VirtualPathUtility.ToAbsolute(path);
            const string legacyPrefix = "/Content/Uploads/Profiles/";
            const string profilePrefix = "/Content/images/Uploads/Profiles/";
            if (path.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase)) path = profilePrefix + path.Substring(legacyPrefix.Length);
            if (path.StartsWith(profilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                string filename = path.Substring(profilePrefix.Length);
                Guid imageId;
                if (Path.GetFileName(filename) == filename && filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && Guid.TryParseExact(Path.GetFileNameWithoutExtension(filename), "N", out imageId)) return path;
            }
            return DefaultAvatar;
        }

        public static void EnsureDefaultAdmin()
        {
            using (SqlConnection cn = Open())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                using (SqlCommand check = new SqlCommand("SELECT username,email,role FROM users WITH (UPDLOCK,HOLDLOCK) WHERE username=@username OR email=@email", cn, tx))
                {
                    check.Parameters.AddWithValue("@username", "admin_default");
                    check.Parameters.AddWithValue("@email", "admin@gmail.com");
                    using (SqlDataReader reader = check.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool matches = String.Equals(Convert.ToString(reader["username"]), "admin_default", StringComparison.OrdinalIgnoreCase)
                                && String.Equals(Convert.ToString(reader["email"]), "admin@gmail.com", StringComparison.OrdinalIgnoreCase)
                                && String.Equals(Convert.ToString(reader["role"]), "Admin", StringComparison.OrdinalIgnoreCase);
                            if (!matches || reader.Read())
                                throw new InvalidOperationException("The default administrator email or username is already used by another account. Resolve this account conflict before starting the application.");
                            reader.Close();
                            tx.Commit();
                            return;
                        }
                    }
                }
                using (SqlCommand create = new SqlCommand("INSERT INTO users(username,email,password,displayName,role,country,image,createdAt,isActive) VALUES(@username,@email,@password,@name,'Admin',@country,@image,@createdAt,1)", cn, tx))
                {
                    create.Parameters.AddWithValue("@username", "admin_default");
                    create.Parameters.AddWithValue("@email", "admin@gmail.com");
                    create.Parameters.AddWithValue("@password", HashPassword("admin@1234"));
                    create.Parameters.AddWithValue("@name", "Admin");
                    create.Parameters.AddWithValue("@country", "Malaysia");
                    create.Parameters.AddWithValue("@image", DefaultAvatar);
                    create.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                    create.ExecuteNonQuery();
                }
                tx.Commit();
            }
        }

        public static SqlConnection Open()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MasakingDB"].ConnectionString);
            connection.Open();
            return connection;
        }

        public static int? UserId
        {
            get { return HttpContext.Current.Session["UserId"] as int?; }
        }

        public static string UserRole
        {
            get { return Convert.ToString(HttpContext.Current.Session["UserRole"] ?? ""); }
        }

        public static bool RequireUser(HttpResponse response)
        {
            if (!String.IsNullOrEmpty(ValidatedRole())) return true;
            response.Redirect("~/Account/Login.aspx", true);
            return false;
        }

        public static bool RequireAdmin(HttpResponse response)
        {
            string role = ValidatedRole();
            if (String.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)) return true;
            if (!String.IsNullOrEmpty(role)) { response.StatusCode = 403; response.SuppressContent = true; HttpContext.Current.ApplicationInstance.CompleteRequest(); }
            else response.Redirect("~/Account/Login.aspx", true);
            return false;
        }

        private static string ValidatedRole()
        {
            int? id = UserId;
            if (!id.HasValue) return null;
            using (SqlConnection cn = Open())
            using (SqlCommand cmd = new SqlCommand("SELECT role,isActive FROM users WHERE userID=@id", cn))
            {
                cmd.Parameters.AddWithValue("@id", id.Value);
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read() || !rd.GetBoolean(1)) { HttpContext.Current.Session.Clear(); return null; }
                    string role = rd.GetString(0); HttpContext.Current.Session["UserRole"] = role; return role;
                }
            }
        }

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            using (var derive = new Rfc2898DeriveBytes(password, salt, 120000))
                return "PBKDF2$120000$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(derive.GetBytes(32));
        }

        public static bool VerifyPassword(string password, string stored)
        {
            if (String.IsNullOrEmpty(stored)) return false;
            if (!stored.StartsWith("PBKDF2$", StringComparison.Ordinal)) return String.Equals(password, stored, StringComparison.Ordinal);
            try
            {
                string[] parts = stored.Split('$');
                int iterations = Int32.Parse(parts[1]);
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                using (var derive = new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    byte[] actual = derive.GetBytes(expected.Length);
                    int diff = actual.Length ^ expected.Length;
                    for (int i = 0; i < actual.Length && i < expected.Length; i++) diff |= actual[i] ^ expected[i];
                    return diff == 0;
                }
            }
            catch { return false; }
        }

        public static string Html(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value));
        }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}
