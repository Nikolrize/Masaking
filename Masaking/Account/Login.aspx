<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Masaking.Account.Login" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Sign in | Masaking</title>

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
            <div class="masaking-card p-4 p-md-5">
                <p class="text-brand fw-semibold mb-2">WELCOME BACK</p>
                <h1 class="page-heading h2">Sign in to your kitchen</h1>
                <p class="text-secondary mb-4">Your next favourite Malaysian dish is waiting.</p>

                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-3">
                    <label class="form-label" for="txtEmail">Email address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                        TextMode="Email" required="required" />
                </div>

                <div class="mb-3">
                    <label class="form-label" for="txtPassword">Password</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control"
                            TextMode="Password" required="required" />
                        <button class="btn btn-outline-secondary password-toggle" type="button"
                            data-toggle-password="txtPassword" aria-label="Show password">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                </div>

                <div class="text-end mb-4">
                    <a class="text-brand" href="ForgotPassword.aspx">Forgot password?</a>
                </div>

                <asp:Button ID="btnLogin" runat="server" Text="Sign in"
                    CssClass="btn btn-primary w-100" OnClick="btnLogin_Click" />

                <p class="mt-4 mb-0 text-center">
                    New to Masaking? <a class="text-brand" href="Register.aspx">Create an account</a>
                </p>
            </div>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/form-helpers.js?v=20261024-39"></script>
</body>
</html>
