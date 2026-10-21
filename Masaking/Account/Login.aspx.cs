using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected TextBox txtEmail, txtPassword;
        protected Label lblMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Database.UserId.HasValue) Response.Redirect("Profile.aspx");
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT userID,password,displayName,role,isActive,image FROM users WHERE email=@email", cn))
            {
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read() || !rd.GetBoolean(rd.GetOrdinal("isActive")) || !Database.VerifyPassword(txtPassword.Text, rd.GetString(rd.GetOrdinal("password"))))
                    { Show("Email or password is incorrect, or this account is inactive."); return; }
                    Session.Clear(); Session["UserId"] = rd.GetInt32(rd.GetOrdinal("userID"));
                    Session["UserRole"] = rd.GetString(rd.GetOrdinal("role")); Session["DisplayName"] = rd.GetString(rd.GetOrdinal("displayName"));
                    Session["UserImage"] = Database.AvatarUrl(rd["image"]);
                    if (!rd.GetString(rd.GetOrdinal("password")).StartsWith("PBKDF2$", StringComparison.Ordinal))
                    {
                        string legacyPassword = rd.GetString(rd.GetOrdinal("password")); int userId = rd.GetInt32(rd.GetOrdinal("userID"));
                        rd.Close(); using (SqlCommand migrate = new SqlCommand("UPDATE users SET password=@password WHERE userID=@id", cn))
                        { migrate.Parameters.AddWithValue("@password", Database.HashPassword(legacyPassword)); migrate.Parameters.AddWithValue("@id", userId); migrate.ExecuteNonQuery(); }
                    }
                }
            }
            string next = Request.QueryString["returnUrl"];
            if (!String.IsNullOrEmpty(next) && next.StartsWith("/", StringComparison.Ordinal) && !next.StartsWith("//", StringComparison.Ordinal)) Response.Redirect(next, false);
            else Response.Redirect(String.Equals(Database.UserRole, "Admin", StringComparison.OrdinalIgnoreCase) ? "../Admin/ManageRecipes.aspx" : "Profile.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void Show(string message) { lblMessage.Text = Server.HtmlEncode(message); lblMessage.CssClass = "alert alert-danger d-block"; }
    }
}
