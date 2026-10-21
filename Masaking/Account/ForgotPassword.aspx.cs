using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected TextBox txtEmail;
        protected Label lblMessage;

        protected void btnSend_Click(object sender, EventArgs e)
        {
            Session.Remove("PasswordResetUserId");
            int userId;
            using (SqlConnection cn = Database.Open()) using (SqlCommand cmd = new SqlCommand("SELECT userID FROM users WHERE email=@email AND isActive=1", cn))
            {
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                object found = cmd.ExecuteScalar();
                if (found == null) { lblMessage.Text = Server.HtmlEncode("No active account with that email was found."); lblMessage.CssClass = "alert alert-danger d-block"; return; }
                userId = Convert.ToInt32(found);
            }
            Session["PasswordResetUserId"] = userId;
            Response.Redirect("ResetPassword.aspx", false); Context.ApplicationInstance.CompleteRequest();
        }
    }
}
