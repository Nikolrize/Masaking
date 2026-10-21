using System;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public partial class Profile : System.Web.UI.Page
    {
        protected TextBox txtName, txtEmail, txtCountry, txtCurrentPassword, txtNewPassword;
        protected FileUpload uploadAvatar;
        protected Image imgAvatar;
        protected Button btnResetAvatar;
        protected Label lblMessage;
        protected Literal litStats, litProfileUsername;
        protected Panel pnlAdminProfileNote, pnlMemberProfileNote, pnlMemberRail;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            if (!IsPostBack) LoadProfile();
            ConfigureRoleDisplay();
        }

        private void ConfigureRoleDisplay()
        {
            bool isAdmin = String.Equals(Database.UserRole, "Admin", StringComparison.OrdinalIgnoreCase);
            pnlAdminProfileNote.Visible = isAdmin;
            pnlMemberProfileNote.Visible = !isAdmin;
            pnlMemberRail.Visible = !isAdmin;
        }

        private void LoadProfile()
        {
            using (SqlConnection cn = Database.Open())
            {
                using (SqlCommand cmd = new SqlCommand("SELECT username,displayName,email,country,image FROM users WHERE userID=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", Database.UserId.Value);
                    using (SqlDataReader rd = cmd.ExecuteReader()) if (rd.Read())
                    {
                        string username = Convert.ToString(rd["username"]), name = Convert.ToString(rd["displayName"]), email = Convert.ToString(rd["email"]), country = Convert.ToString(rd["country"]);
                        txtName.Text = name; txtEmail.Text = email; txtCountry.Text = country;
                        litProfileUsername.Text = Server.HtmlEncode(username);
                        imgAvatar.ImageUrl = Database.AvatarUrl(rd["image"]); Session["UserImage"] = imgAvatar.ImageUrl; btnResetAvatar.Visible = !String.Equals(imgAvatar.ImageUrl, Database.DefaultAvatar, StringComparison.OrdinalIgnoreCase);
                    }
                }
                bool isAdmin = String.Equals(Database.UserRole, "Admin", StringComparison.OrdinalIgnoreCase);
                if (!isAdmin) using (SqlCommand cmd = new SqlCommand("SELECT (SELECT COUNT(*) FROM recipes WHERE userID=@id) AS recipes,(SELECT COUNT(*) FROM reviews WHERE userID=@id) AS reviews,(SELECT COUNT(*) FROM favourites WHERE userID=@id) AS saves", cn))
                {
                    cmd.Parameters.AddWithValue("@id", Database.UserId.Value);
                    using (SqlDataReader rd = cmd.ExecuteReader()) if (rd.Read())
                    {
                        litStats.Text = "<div class='row g-2'><div class='col-12'><div class='profile-stat'><span class='small text-secondary'>Recipes shared</span><strong>" + rd.GetInt32(0) + "</strong><span class='small text-secondary'>Your dishes added</span></div></div><div class='col-6'><div class='profile-stat h-100'><span class='small text-secondary'>Reviews written</span><strong>" + rd.GetInt32(1) + "</strong></div></div><div class='col-6'><div class='profile-stat h-100'><span class='small text-secondary'>Recipes saved</span><strong>" + rd.GetInt32(2) + "</strong></div></div></div>";
                    }
                }
            }
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (String.IsNullOrWhiteSpace(txtName.Text) || txtName.Text.Trim().Length > 100 || String.IsNullOrWhiteSpace(txtCountry.Text) || txtCountry.Text.Trim().Length > 100) { Show("Enter a display name and country of at most 100 characters.", false); return; }
            try { if (email.Length > 255) throw new FormatException(); new MailAddress(email); } catch { Show("Enter a valid email address.", false); return; }
            using (SqlConnection cn = Database.Open()) using (SqlCommand check = new SqlCommand("SELECT COUNT(*) FROM users WHERE email=@email AND userID<>@id", cn))
            { check.Parameters.AddWithValue("@email", email); check.Parameters.AddWithValue("@id", Database.UserId.Value); if (Convert.ToInt32(check.ExecuteScalar()) > 0) { Show("That email address is already used by another account.", false); return; } }
            string oldImage = imgAvatar.ImageUrl, newImage = oldImage;
            try { if (uploadAvatar.HasFile) newImage = ImageUpload.Save(uploadAvatar.PostedFile, true); }
            catch (InvalidDataException ex) { Show(ex.Message, false); return; }
            try
            {
                using (SqlConnection cn = Database.Open()) using (SqlCommand cmd = new SqlCommand("UPDATE users SET displayName=@name,email=@email,country=@country,image=@image WHERE userID=@id", cn))
                { cmd.Parameters.AddWithValue("@name", txtName.Text.Trim()); cmd.Parameters.AddWithValue("@email", email); cmd.Parameters.AddWithValue("@country", txtCountry.Text.Trim()); cmd.Parameters.AddWithValue("@image", newImage); cmd.Parameters.AddWithValue("@id", Database.UserId.Value); if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Profile could not be saved."); }
            }
            catch { if (!String.Equals(newImage, oldImage, StringComparison.Ordinal)) ImageUpload.DeleteUploaded(newImage, "Profiles"); Show("Your profile could not be saved. Please try again.", false); return; }
            Session["DisplayName"] = txtName.Text.Trim(); Session["UserImage"] = newImage; imgAvatar.ImageUrl = Database.AvatarUrl(newImage); btnResetAvatar.Visible = !String.Equals(newImage, Database.DefaultAvatar, StringComparison.OrdinalIgnoreCase);
            if (!String.Equals(newImage, oldImage, StringComparison.Ordinal)) ImageUpload.DeleteUploaded(oldImage, "Profiles");
            Show("Your profile has been saved.", true);
        }

        protected void btnResetAvatar_Click(object sender, EventArgs e)
        {
            string oldImage = imgAvatar.ImageUrl;
            using (SqlConnection cn = Database.Open()) using (SqlCommand cmd = new SqlCommand("UPDATE users SET image=@image WHERE userID=@id", cn))
            { cmd.Parameters.AddWithValue("@image", Database.DefaultAvatar); cmd.Parameters.AddWithValue("@id", Database.UserId.Value); cmd.ExecuteNonQuery(); }
            Session["UserImage"] = Database.DefaultAvatar; imgAvatar.ImageUrl = Database.DefaultAvatar; btnResetAvatar.Visible = false;
            ImageUpload.DeleteUploaded(oldImage, "Profiles"); Show("Your default avatar has been restored.", true);
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (txtNewPassword.Text.Length < 8 || txtNewPassword.Text.Length > 256) { Show("Choose a new password between 8 and 256 characters.", false); return; }
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT password FROM users WHERE userID=@id", cn))
            {
                cmd.Parameters.AddWithValue("@id", Database.UserId.Value); string stored = Convert.ToString(cmd.ExecuteScalar());
                if (!Database.VerifyPassword(txtCurrentPassword.Text, stored)) { Show("Your current password is incorrect.", false); return; }
                using (SqlCommand update = new SqlCommand("UPDATE users SET password=@password WHERE userID=@id", cn))
                { update.Parameters.AddWithValue("@password", Database.HashPassword(txtNewPassword.Text)); update.Parameters.AddWithValue("@id", Database.UserId.Value); update.ExecuteNonQuery(); }
            }
            txtCurrentPassword.Text = txtNewPassword.Text = ""; Show("Your password has been updated.", true);
        }

        protected void lnkSignOut_Click(object sender, EventArgs e) { Session.Clear(); Session.Abandon(); Response.Redirect("Login.aspx"); }
        private void Show(string text, bool success) { lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block"; lblMessage.Text = Server.HtmlEncode(text); }
    }
}
