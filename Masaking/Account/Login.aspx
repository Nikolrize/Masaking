<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Masaking.Account.Login" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Sign in | Masaking</title>
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
</head>
<body class="masaking-page auth-background">
    <form id="form1" runat="server">

        <nav class="navbar navbar-expand-lg masaking-nav">
            <div class="container">
                <a class="navbar-brand" href="../Default.aspx">
                    <img class="brand-mark" src="../Content/images/Masaking%20Logo.png" alt="Masaking" />
                </a>
                <div class="navbar-nav ms-auto">
                    <a class="nav-link" href="../Recipes/BrowseRecipes.aspx">Recipes</a>
                    <a class="nav-link" href="../Community/Community.aspx">Community</a>
                    <a class="nav-link" href="Register.aspx">Create account</a>
                </div>
            </div>
        </nav>

        <main class="container auth-wrap">
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
                            <svg viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z" />
                                <circle cx="12" cy="12" r="3" />
                            </svg>
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
    <script src="../Scripts/masaking.js"></script>
</body>
</html>
