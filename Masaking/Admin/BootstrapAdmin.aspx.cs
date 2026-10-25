using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class BootstrapAdmin : System.Web.UI.Page
    {
        protected TextBox txtKey, txtPassword, txtAdminName, txtAdminUsername, txtAdminEmail, txtAdminCountry, txtNewPassword, txtConfirm;
        protected Label lblMessage;
        protected Panel pnlCreateFirst, pnlPromoteExisting;
        protected Button btnCreateFirst;

        protected void Page_Load(object sender, EventArgs e)
        {
            int users, admins;
            GetAccountCounts(out users, out admins);
            if (admins > 0)
            {
                pnlCreateFirst.Visible = pnlPromoteExisting.Visible = false;
                Show("An active administrator already exists. Initial setup is closed.", false);
                return;
            }
            if (users == 0) { pnlCreateFirst.Visible = true; pnlPromoteExisting.Visible = false; return; }
            if (!Database.RequireUser(Response)) return;
            pnlCreateFirst.Visible = false; pnlPromoteExisting.Visible = true;
        }

        private static void GetAccountCounts(out int users, out int admins)
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) users,(SELECT COUNT(*) FROM users WHERE role='Admin' AND isActive=1) admins FROM users", cn))
            using (SqlDataReader rd = cmd.ExecuteReader()) { rd.Read(); users = rd.GetInt32(0); admins = rd.GetInt32(1); }
        }

        protected void btnCreateFirst_Click(object sender, EventArgs e)
        {
            string configuredKey = ConfigurationManager.AppSettings["AdminBootstrapKey"];
            if (String.IsNullOrWhiteSpace(configuredKey) || !FixedEquals(configuredKey, txtKey.Text)) { Show("Set AdminBootstrapKey in Web.config and enter the matching one-time setup key.", false); return; }
            string name = txtAdminName.Text.Trim(), username = txtAdminUsername.Text.Trim(), email = txtAdminEmail.Text.Trim(), country = txtAdminCountry.Text.Trim();
            if (name.Length == 0 || name.Length > 100 || country.Length == 0 || country.Length > 100 || !Regex.IsMatch(username, "^[A-Za-z0-9_.-]{3,40}$")) { Show("Enter a display name and country, and a username with 3–40 letters, numbers, dots, dashes or underscores.", false); return; }
            try { if (email.Length > 255) throw new FormatException(); new MailAddress(email); } catch { Show("Enter a valid email address.", false); return; }
            if (txtNewPassword.Text.Length < 8 || txtNewPassword.Text.Length > 64) { Show("Choose a password between 8 and 64 characters.", false); return; }
            if (txtNewPassword.Text != txtConfirm.Text) { Show("The passwords do not match.", false); return; }

            using (SqlConnection cn = Database.Open()) using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    using (SqlCommand count = new SqlCommand("SELECT COUNT(*) FROM users WITH (UPDLOCK,HOLDLOCK)", cn, tx))
                        if (Convert.ToInt32(count.ExecuteScalar()) != 0) { tx.Rollback(); Show("A user account now exists. Sign in with a member account to promote it instead.", false); return; }
                    int userId;
                    using (SqlCommand create = new SqlCommand("INSERT INTO users(username,email,password,displayName,role,country,image,createdAt,isActive) VALUES(@username,@email,@password,@name,'Admin',@country,@image,@createdAt,1); SELECT CAST(SCOPE_IDENTITY() AS int)", cn, tx))
                    {
                        create.Parameters.AddWithValue("@username", username); create.Parameters.AddWithValue("@email", email); create.Parameters.AddWithValue("@password", Database.HashPassword(txtNewPassword.Text));
                        create.Parameters.AddWithValue("@name", name); create.Parameters.AddWithValue("@country", country); create.Parameters.AddWithValue("@image", Database.DefaultAvatar); create.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                        userId = Convert.ToInt32(create.ExecuteScalar());
                    }
                    tx.Commit(); Session["UserId"] = userId; Session["UserRole"] = "Admin"; Session["DisplayName"] = name; Session["UserImage"] = Database.DefaultAvatar;
                    Response.Redirect("../Default.aspx", false); Context.ApplicationInstance.CompleteRequest();
                }
                catch { try { tx.Rollback(); } catch { } Show("The first administrator could not be created. Check that the email and username are unused, then try again.", false); }
            }
        }

        protected void btnPromote_Click(object sender, EventArgs e)
        {
            string configuredKey = ConfigurationManager.AppSettings["AdminBootstrapKey"];
            if (String.IsNullOrWhiteSpace(configuredKey) || !FixedEquals(configuredKey, txtKey.Text)) { Show("Initial administrator setup is not enabled, or the setup key is incorrect.", false); return; }
            using (SqlConnection cn = Database.Open()) using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    using (SqlCommand count = new SqlCommand("SELECT COUNT(*) FROM users WITH (UPDLOCK,HOLDLOCK) WHERE role='Admin' AND isActive=1", cn, tx))
                        if (Convert.ToInt32(count.ExecuteScalar()) > 0) { tx.Rollback(); Show("An active administrator already exists. This one-time setup is closed.", false); return; }
                    string stored;
                    using (SqlCommand get = new SqlCommand("SELECT password FROM users WHERE userID=@id AND isActive=1", cn, tx))
                    { get.Parameters.AddWithValue("@id", Database.UserId.Value); stored = Convert.ToString(get.ExecuteScalar()); }
                    if (!Database.VerifyPassword(txtPassword.Text, stored)) { tx.Rollback(); Show("Your current password is incorrect.", false); return; }
                    using (SqlCommand update = new SqlCommand("UPDATE users SET role='Admin' WHERE userID=@id AND isActive=1", cn, tx))
                    { update.Parameters.AddWithValue("@id", Database.UserId.Value); if (update.ExecuteNonQuery() != 1) { tx.Rollback(); Show("Your member account is no longer active.", false); return; } }
                    tx.Commit(); Session["UserRole"] = "Admin"; Show("Your account is now an administrator. Clear the setup key from Web.config after this first setup.", true);
                }
                catch { if (tx.Connection != null) tx.Rollback(); Show("Administrator setup could not be completed. Try again.", false); }
            }
        }

        private static bool FixedEquals(string left, string right)
        {
            byte[] a = Encoding.UTF8.GetBytes(left ?? ""); byte[] b = Encoding.UTF8.GetBytes(right ?? ""); int diff = a.Length ^ b.Length;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
        private void Show(string text, bool success) { lblMessage.Text = Server.HtmlEncode(text); lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block"; }
    }
}
