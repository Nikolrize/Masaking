<%@ Page Language="C#" AutoEventWireup="true" EnableViewStateMac="true" EnableEventValidation="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="Masaking.Account.ForgotPassword" %>

<!doctype html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Reset password | Masaking</title>
    <link href="../Content/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/auth.css" rel="stylesheet" />
</head>
<body class="auth-background">
    <form id="form1" runat="server">
        <main class="container min-vh-100 d-flex align-items-center justify-content-center py-4 py-md-5">
            <section class="card auth-card w-100 rounded-4 shadow-sm p-4 p-md-5" aria-labelledby="authHeading">
                <h1 id="authHeading" class="h2 fw-bold text-center mb-3">Forgot your password?</h1>
                <p class="text-secondary text-center mb-4">Enter your account email and we'll send you a six-digit reset code. The code expires after 10 minutes.</p>
                
                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />
                
                <div class="mb-4">
                    <asp:Label runat="server" ID="lblEmail" AssociatedControlID="txtEmail" class="form-label">Email address</asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" class="form-control" TextMode="Email" MaxLength="254" autocomplete="email" required="required" />
                </div>
                
                <asp:Button ID="btnSend" runat="server" Text="Send reset code" class="btn btn-primary w-100" OnClick="btnSend_Click" />
                
                <p class="mt-4 mb-0 text-center small">
                    <asp:HyperLink runat="server" ID="lnkBackToSignIn" NavigateUrl="Login.aspx">Back to sign in</asp:HyperLink>
                </p>
            </section>
        </main>
    </form>
    <script src="../Scripts/bootstrap.bundle.min.js"></script>
</body>
</html>
