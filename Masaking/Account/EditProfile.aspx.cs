using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public partial class EditProfile : ProfilePage
    {
        protected TextBox txtUsername, txtName, txtEmail, txtCountry, txtCurrentPassword, txtNewPassword, txtConfirmPassword;
        protected FileUpload uploadAvatar;
        protected Image imgAvatar;
        protected Button btnResetAvatar, btnSaveProfile, btnChangePassword;
        protected Label lblMessage;
        protected Repeater rptActivity;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            Form.Enctype = "multipart/form-data";
            try
            {
                LoadAccount();
                imgAvatar.ImageUrl = AvatarDisplayUrl;
                btnResetAvatar.Visible = !String.Equals(ProfileImage, Database.DefaultAvatar, StringComparison.OrdinalIgnoreCase);
                if (!IsPostBack)
                {
                    txtUsername.Text = ProfileUsername; txtName.Text = ProfileName; txtEmail.Text = ProfileEmail; txtCountry.Text = ProfileCountry;
                    rptActivity.DataSource = UserActivity(); rptActivity.DataBind();
                }
            }
            catch (SqlException ex) { Trace.Warn("Profile", "Unable to load profile information.", ex); Show("Your profile is temporarily unavailable. Please try again later.", false); btnSaveProfile.Enabled = btnChangePassword.Enabled = btnResetAvatar.Enabled = false; }
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (!btnSaveProfile.Enabled) return;
            string username = txtUsername.Text.Trim(), name = txtName.Text.Trim(), email = txtEmail.Text.Trim(), country = txtCountry.Text.Trim();
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]{3,50}$")) { Show("Use 3–50 letters, numbers, dots, underscores or hyphens for your username.", false); return; }
            if (name.Length == 0 || name.Length > 100 || country.Length == 0 || country.Length > 100) { Show("Enter a display name and country of at most 100 characters.", false); return; }
            try { if (String.IsNullOrWhiteSpace(email) || email.Length > 255) throw new FormatException(); var address = new MailAddress(email); if (address.Address != email) throw new FormatException(); }
            catch (FormatException) { Show("Enter a valid email address.", false); return; }
            string oldImage = ProfileImage, newImage = oldImage;
            try
            {
                using (SqlConnection cn = Database.Open())
                {
                    using (SqlCommand check = new SqlCommand("SELECT COUNT(*) FROM users WHERE userID<>@id AND (email=@email OR username=@username)", cn))
                    {
                        check.Parameters.Add("@id", SqlDbType.Int).Value = Database.UserId.Value; check.Parameters.Add("@email", SqlDbType.NVarChar, 255).Value = email; check.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;
                        if (Convert.ToInt32(check.ExecuteScalar()) > 0) { Show("That username or email is already used by another account.", false); return; }
                    }
                    if (uploadAvatar.HasFile) newImage = ImageUpload.Save(uploadAvatar.PostedFile, true);
                    using (SqlCommand cmd = new SqlCommand("UPDATE users SET username=@username,displayName=@name,email=@email,country=@country,image=@image WHERE userID=@id AND isActive=1", cn))
                    {
                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username; cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name; cmd.Parameters.Add("@email", SqlDbType.NVarChar, 255).Value = email; cmd.Parameters.Add("@country", SqlDbType.NVarChar, 100).Value = country; cmd.Parameters.Add("@image", SqlDbType.NVarChar, 500).Value = newImage; cmd.Parameters.Add("@id", SqlDbType.Int).Value = Database.UserId.Value;
                        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Profile could not be saved.");
                    }
                }
            }
            catch (InvalidDataException ex) { Show(ex.Message, false); return; }
            catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException || ex is IOException)
            {
                if (newImage != oldImage) DeleteOldImage(newImage);
                Trace.Warn("Profile", "Unable to save profile.", ex); Show("Your profile could not be saved. Please try again.", false); return;
            }
            Session["DisplayName"] = name; Session["UserImage"] = newImage;
            ProfileName = name; ProfileUsername = username; ProfileEmail = email; ProfileCountry = country; ProfileImage = newImage; imgAvatar.ImageUrl = AvatarDisplayUrl;
            btnResetAvatar.Visible = !String.Equals(newImage, Database.DefaultAvatar, StringComparison.OrdinalIgnoreCase);
            if (newImage != oldImage) DeleteOldImage(oldImage);
            Show("Your profile has been saved.", true);
        }

        protected void btnResetAvatar_Click(object sender, EventArgs e)
        {
            if (!btnResetAvatar.Enabled) return;
            try
            {
                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand("UPDATE users SET image=@image WHERE userID=@id AND isActive=1", cn))
                {
                    cmd.Parameters.Add("@image", SqlDbType.NVarChar, 500).Value = Database.DefaultAvatar; cmd.Parameters.Add("@id", SqlDbType.Int).Value = Database.UserId.Value;
                    if (cmd.ExecuteNonQuery() != 1) { Show("Your photo could not be reset.", false); return; }
                }
            }
            catch (SqlException ex) { Trace.Warn("Profile", "Unable to reset avatar.", ex); Show("Your photo could not be reset. Please try again.", false); return; }
            string oldImage = ProfileImage; Session["UserImage"] = ProfileImage = Database.DefaultAvatar; imgAvatar.ImageUrl = AvatarDisplayUrl; btnResetAvatar.Visible = false; DeleteOldImage(oldImage); Show("Your default photo has been restored.", true);
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (!btnChangePassword.Enabled) return;
            if (txtNewPassword.Text.Length < 8 || txtNewPassword.Text.Length > 64) { Show("Choose a new password between 8 and 64 characters.", false); return; }
            if (!String.Equals(txtNewPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal)) { Show("Your new password and confirmation do not match.", false); return; }
            try
            {
                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand("SELECT password FROM users WHERE userID=@id AND isActive=1", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = Database.UserId.Value;
                    string stored = Convert.ToString(cmd.ExecuteScalar());
                    if (!Database.VerifyPassword(txtCurrentPassword.Text, stored)) { Show("Your current password is incorrect.", false); return; }
                    using (SqlCommand update = new SqlCommand("UPDATE users SET password=@password WHERE userID=@id AND password=@previous AND isActive=1", cn))
                    {
                        update.Parameters.AddWithValue("@password", Database.HashPassword(txtNewPassword.Text)); update.Parameters.Add("@id", SqlDbType.Int).Value = Database.UserId.Value; update.Parameters.AddWithValue("@previous", stored);
                        if (update.ExecuteNonQuery() != 1) { Show("Your password changed during this request. Please try again.", false); return; }
                    }
                }
            }
            catch (SqlException ex) { Trace.Warn("Profile", "Unable to update password.", ex); Show("Your password could not be updated. Please try again.", false); return; }
            txtCurrentPassword.Text = txtNewPassword.Text = txtConfirmPassword.Text = ""; Show("Your password has been updated.", true);
        }

        private void DeleteOldImage(string path) { try { ImageUpload.DeleteUploaded(path, "Profiles"); } catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { Trace.Warn("Profile", "Unable to remove previous photo.", ex); } }
        private void Show(string text, bool success) { lblMessage.CssClass = success ? "alert alert-success d-block profile-message" : "alert alert-danger d-block profile-message"; lblMessage.Text = Server.HtmlEncode(text); }
    }
}
