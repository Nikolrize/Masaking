<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="Masaking.Account.ForgotPassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Reset password | Masaking</title>

    <link href="<%= ResolveUrl("~/Content/bootstrap-icons.min.css?v=1") %>" rel="stylesheet" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/auth.css?v=4" rel="stylesheet" />
    <link href="../Content/css/mobile-artwork.css?v=2" rel="stylesheet" />
</head>
<body class="masaking-page auth-background">
    <form id="form1" runat="server">

        <main class="container auth-wrap">
            <a class="auth-brand" href="../Default.aspx" aria-label="Masaking home">
                <img src="../Content/images/Masaking%20Logo.png" alt="Masaking" width="170" height="41" />
            </a>
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
