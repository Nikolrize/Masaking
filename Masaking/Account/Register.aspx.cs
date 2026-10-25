using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public partial class Register : System.Web.UI.Page
    {
        protected TextBox txtName, txtUsername, txtEmail, txtCountry, txtPassword, txtConfirm;
        protected System.Web.UI.HtmlControls.HtmlInputCheckBox chkTerms;
        protected Label lblMessage;

        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string country = txtCountry.Text.Trim();
            if (name.Length > 80) { Message("Keep your display name within 80 characters.", false); return; }
            if (country.Length > 80) { Message("Keep your country within 80 characters.", false); return; }
            if (name.Length == 0 || username.Length < 3 || country.Length == 0 || !Regex.IsMatch(username, "^[A-Za-z0-9_.-]{3,40}$"))
            { Message("Enter a display name, country, and a username with 3–40 letters, numbers, dots, dashes or underscores.", false); return; }
            try { if (email.Length > 255) throw new FormatException(); new MailAddress(email); } catch { Message("Enter a valid email address.", false); return; }
            if (txtPassword.Text.Length < 8 || txtPassword.Text.Length > 64) { Message("Choose a password between 8 and 64 characters.", false); return; }
            if (txtPassword.Text != txtConfirm.Text) { Message("The passwords do not match.", false); return; }
            if (!chkTerms.Checked) { Message("Please confirm the community guidelines before creating an account.", false); return; }

            using (SqlConnection cn = Database.Open())
            {
                using (SqlCommand check = new SqlCommand("SELECT COUNT(*) FROM users WHERE username=@username OR email=@email", cn))
                {
                    check.Parameters.AddWithValue("@username", username); check.Parameters.AddWithValue("@email", email);
                    if (Convert.ToInt32(check.ExecuteScalar()) > 0) { Message("That username or email is already registered.", false); return; }
                }
                using (SqlCommand cmd = new SqlCommand("INSERT INTO users(username,email,password,displayName,role,country,image,createdAt,isActive) VALUES(@username,@email,@password,@name,'Member',@country,@image,@createdAt,1); SELECT CAST(SCOPE_IDENTITY() AS int)", cn))
                {
                    cmd.Parameters.AddWithValue("@username", username); cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", Database.HashPassword(txtPassword.Text)); cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@country", country); cmd.Parameters.AddWithValue("@image", Database.DefaultAvatar); cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                    int userId = Convert.ToInt32(cmd.ExecuteScalar());
                    Session["UserId"] = userId; Session["UserRole"] = "Member"; Session["DisplayName"] = name; Session["UserImage"] = Database.DefaultAvatar;
                    Response.Redirect("Profile.aspx", false); Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        private void Message(string text, bool success)
        {
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Text = Server.HtmlEncode(text);
        }
    }
}
