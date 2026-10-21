<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="Masaking.Account.ForgotPassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Reset password | Masaking</title>
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
</head>
<body class="masaking-page auth-background">
    <form id="form1" runat="server">

        <nav class="navbar masaking-nav">
            <div class="container">
                <a class="navbar-brand" href="../Default.aspx">
                    <img class="brand-mark" src="../Content/images/Masaking%20Logo.png" alt="Masaking" />
                </a>
                <a class="nav-link" href="Login.aspx">Back to sign in</a>
            </div>
        </nav>

        <main class="container auth-wrap">
            <section class="masaking-card p-4 p-md-5">
                <p class="text-brand fw-semibold">ACCOUNT HELP</p>
                <h1 class="page-heading h2">Forgot your password?</h1>
                <p class="text-secondary">Enter the email linked to your account to continue directly to password reset.</p>

                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-4">
                    <label for="txtEmail" class="form-label">Email address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                        TextMode="Email" required="required" />
                </div>

                <asp:Button ID="btnSend" runat="server" Text="Continue to reset"
                    CssClass="btn btn-primary w-100" OnClick="btnSend_Click" />
            </section>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
</body>
</html>
