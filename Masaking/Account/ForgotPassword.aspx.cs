using System;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;
using PasswordRecovery = Masaking.Infrastructure.PasswordRecovery;

namespace Masaking.Account
{
    public partial class ForgotPassword : RecoveryPage
    {
        protected TextBox txtEmail;
        protected Label lblMessage;
        protected void btnSend_Click(object sender, EventArgs e)
        {
            Session.Remove("PasswordResetUserId");
            string email = txtEmail.Text.Trim();
            if (!PasswordRecovery.IsEmailValid(email))
            {
                lblMessage.Text = "Enter a valid email address.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }
            string challenge = PasswordRecovery.RequestReset(email, Request.UserHostAddress);
            Session["PasswordRecoveryChallenge"] = challenge;
            Session["PasswordRecoveryEmail"] = email;
            Response.Redirect("ResetPassword.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
