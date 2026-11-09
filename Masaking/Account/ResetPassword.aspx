<%@ Page Title="Enter reset code | Masaking" Language="C#" AutoEventWireup="true" EnableViewStateMac="true" EnableEventValidation="true" CodeBehind="ResetPassword.aspx.cs" Inherits="Masaking.Account.ResetPassword" %>

<!doctype html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Enter reset code | Masaking</title>
    <link href="../Content/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/auth.css" rel="stylesheet" />
</head>
<body class="auth-background">
    <form id="form1" runat="server">
        <main class="container min-vh-100 d-flex align-items-center justify-content-center py-4 py-md-5">
            <section class="card auth-card w-100 rounded-4 shadow-sm p-4 p-md-5" aria-labelledby="authHeading">
                <h1 id="authHeading" runat="server" ClientIDMode="Static" class="h2 fw-bold text-center mb-3">Reset your password</h1>
                <p id="resetInstructions" runat="server" class="text-secondary text-center mb-4">Enter the six-digit code from your email and choose a new password. Codes expire after 10 minutes. Keep using this browser.</p>
                
                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />
                <asp:Panel ID="pnlSuccess" runat="server" Visible="false" class="text-center" role="status" aria-live="polite">
                    <div class="d-inline-flex align-items-center justify-content-center rounded-circle bg-success bg-opacity-10 text-success p-3 mb-3" aria-hidden="true">
                        <i class="bi bi-check-circle fs-1 lh-1"></i>
                    </div>
                    <p class="fw-semibold mb-2">Your password has been updated.</p>
                    <p class="text-secondary mb-4">Previous sign-ins have been signed out. Sign in with your new password to continue.</p>
                    <asp:HyperLink ID="lnkSuccessSignIn" runat="server" NavigateUrl="Login.aspx" class="btn btn-primary text-white w-100">Back to sign in</asp:HyperLink>
                </asp:Panel>

                <asp:Panel ID="pnlResetForm" runat="server">
                <div class="mb-3">
                    <label class="form-label" for="txtOtp">Email verification code</label>
                    <asp:TextBox ID="txtOtp" runat="server" ClientIDMode="Static" class="form-control" MaxLength="6" inputmode="numeric" autocomplete="one-time-code" pattern="[0-9]{6}" required="required" />
                    <div class="form-text">You have up to five attempts per code.</div>
                </div>

                <div class="mb-3">
                    <asp:Label runat="server" ID="lblPassword" class="form-label" AssociatedControlID="txtPassword">New password</asp:Label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPassword" runat="server" class="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" required="required" />
                        <button runat="server" id="btnShowPassword" class="btn btn-outline-secondary d-inline-flex align-items-center justify-content-center px-3" type="button" data-toggle-password="txtPassword" aria-label="Show password">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                    <div class="form-text">At least 8 characters.</div>
                </div>

                <div class="mb-4">
                    <asp:Label runat="server" ID="lblConfirm" class="form-label" AssociatedControlID="txtConfirm">Confirm new password</asp:Label>
                    <div class="input-group">
                        <asp:TextBox ID="txtConfirm" runat="server" class="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" required="required" />
                        <button runat="server" id="btnShowConfirmation" class="btn btn-outline-secondary d-inline-flex align-items-center justify-content-center px-3" type="button" data-toggle-password="txtConfirm" aria-label="Show confirmation">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                </div>

                <asp:Button ID="btnReset" runat="server" Text="Reset password" class="btn btn-primary w-100" OnClick="btnReset_Click" />
                <asp:Button ID="btnResend" runat="server" Text="Resend code" class="btn btn-outline-secondary w-100 mt-3" CausesValidation="false" formnovalidate="formnovalidate" OnClick="btnResend_Click" />
                <p class="mt-2 mb-0 text-center small">Wait at least 60 seconds between requests. Up to five requests per hour.</p>
                <p class="mt-3 mb-0 text-center small"><a href="ForgotPassword.aspx">Use a different email</a></p>
                </asp:Panel>

                <asp:Panel ID="pnlRequestCode" runat="server" Visible="false" class="d-grid">
                    <asp:HyperLink ID="lnkRequestCode" runat="server" NavigateUrl="ForgotPassword.aspx" class="btn btn-primary text-white">Request a reset code</asp:HyperLink>
                </asp:Panel>

                <p id="signInFooter" runat="server" class="mt-4 mb-0 text-center small">
                    <asp:HyperLink runat="server" ID="lnkBackToSignIn" NavigateUrl="Login.aspx">Back to sign in</asp:HyperLink>
                </p>
            </section>
        </main>
    </form>
    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/form-helpers.js"></script>
</body>
</html>
