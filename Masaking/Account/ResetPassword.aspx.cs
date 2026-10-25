using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        protected TextBox txtPassword, txtConfirm;
        protected Label lblMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && GetUserId() == 0) Show("Enter your registered email on the forgot password page to begin.", false);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            int userId = GetUserId();
            if (userId == 0) { Show("Enter your registered email on the forgot password page to begin.", false); return; }
            if (txtPassword.Text.Length < 8 || txtPassword.Text.Length > 64 || txtPassword.Text != txtConfirm.Text) { Show("Enter matching passwords between 8 and 64 characters.", false); return; }
            using (SqlConnection cn = Database.Open()) using (SqlCommand cmd = new SqlCommand("UPDATE users SET password=@password WHERE userID=@id AND isActive=1", cn))
            { cmd.Parameters.AddWithValue("@password", Database.HashPassword(txtPassword.Text)); cmd.Parameters.AddWithValue("@id", userId); if (cmd.ExecuteNonQuery() != 1) { Show("This account is no longer available.", false); return; } }
            Session.Remove("PasswordResetUserId");
            Show("Your password has been reset. You can now sign in.", true);
        }

        private int GetUserId()
        {
            object id = Session["PasswordResetUserId"];
            return id == null ? 0 : Convert.ToInt32(id);
        }

        private void Show(string text, bool success) { lblMessage.Text = Server.HtmlEncode(text); lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block"; }
    }
}
