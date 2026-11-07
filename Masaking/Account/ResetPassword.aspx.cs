using System;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;
using PasswordRecovery = Masaking.Infrastructure.PasswordRecovery;

namespace Masaking.Account
{
    public partial class ResetPassword : RecoveryPage
    {
        protected TextBox txtPassword, txtConfirm, txtOtp;
        protected Label lblMessage;
        protected Panel pnlResetForm, pnlSuccess, pnlRequestCode;
        protected HyperLink lnkSuccessSignIn, lnkRequestCode;
        protected HtmlGenericControl authHeading, resetInstructions, signInFooter;
        protected Button btnResend;
        private bool canReset;
        private bool resetSucceeded;
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Remove("PasswordResetUserId");
            canReset = RecoveryOtp.HasValidChallenge(Session["PasswordRecoveryChallenge"] as string);
            if (!canReset) Show("Request a reset code first. Use the same browser to enter the code from your email.", false);
            else if (!IsPostBack)
            {
                lblMessage.Text = Server.HtmlEncode(PasswordRecovery.RequestMessage);
                lblMessage.Attributes["class"] = "alert alert-info d-block";
            }
            SetFormEnabled(canReset);
            btnResend.Enabled = PasswordRecovery.IsEmailValid(Session["PasswordRecoveryEmail"] as string);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (!canReset) return;

            string code = txtOtp.Text.Trim();
            if (!RecoveryOtp.HasValidCode(code))
            {
                Show("Enter the six-digit code from your email.", false);
                return;
            }

            if (txtPassword.Text.Length < 8 || txtPassword.Text.Length > 64 || txtPassword.Text != txtConfirm.Text)
            {
                Show("Enter matching passwords between 8 and 64 characters.", false);
                return;
            }

            try
            {
                if (!PasswordRecovery.ResetPassword(Session["PasswordRecoveryChallenge"] as string, code, txtPassword.Text, Request.UserHostAddress))
                {
                    Show(PasswordRecovery.InvalidCodeMessage, false);
                    return;
                }
                Session.Clear();
                Session.Abandon();
                txtOtp.Text = txtPassword.Text = txtConfirm.Text = "";
                SetFormEnabled(false);
                btnResend.Enabled = false;
                resetSucceeded = true;
            }
            catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
            {
                RecoveryDelivery.ReportFailure(ex);
                Show("Password reset is temporarily unavailable. Please try again later.", false);
            }
        }

        private void SetFormEnabled(bool enabled)
        {
            txtOtp.Enabled = txtPassword.Enabled = txtConfirm.Enabled = btnReset.Enabled = enabled;
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            string email = Session["PasswordRecoveryEmail"] as string;
            if (!PasswordRecovery.IsEmailValid(email))
            {
                Show("Return to Forgot Password to request a new code.", false);
                return;
            }
            Session["PasswordRecoveryChallenge"] = PasswordRecovery.RequestReset(email, Request.UserHostAddress,
                Session["PasswordRecoveryChallenge"] as string);
            Response.Redirect("ResetPassword.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected override void OnPreRender(EventArgs e)
        {
            pnlSuccess.Visible = resetSucceeded;
            pnlResetForm.Visible = canReset && !resetSucceeded;
            pnlRequestCode.Visible = !canReset && !resetSucceeded;
            resetInstructions.Visible = canReset && !resetSucceeded;
            signInFooter.Visible = !resetSucceeded;
            lblMessage.Visible = !resetSucceeded;
            if (resetSucceeded)
            {
                authHeading.InnerText = "Password reset successful";
                Title = "Password reset successful | Masaking";
            }
            base.OnPreRender(e);
        }

        private void Show(string text, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(text);
            lblMessage.Attributes["class"] = success ? "alert alert-success d-block" : "alert alert-danger d-block";
        }
    }
}
