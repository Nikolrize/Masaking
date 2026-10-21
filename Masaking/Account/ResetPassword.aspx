<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="Masaking.Account.ResetPassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Choose a new password | Masaking</title>
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server">

        <nav class="navbar masaking-nav">
            <div class="container">
                <a class="navbar-brand" href="../Default.aspx">
                    <img class="brand-mark" src="../Content/images/Masaking%20Logo.png" alt="Masaking" />
                </a>
                <a class="nav-link" href="Login.aspx">Sign in</a>
            </div>
        </nav>

        <main class="container auth-wrap">
            <section class="masaking-card p-4 p-md-5">
                <p class="text-brand fw-semibold">ACCOUNT HELP</p>
                <h1 class="page-heading h2">Choose a new password</h1>

                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-3">
                    <label class="form-label" for="txtPassword">New password</label>
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
                    <div class="form-text">At least 8 characters.</div>
                </div>

                <div class="mb-4">
                    <label class="form-label" for="txtConfirm">Confirm new password</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control"
                            TextMode="Password" required="required" />
                        <button class="btn btn-outline-secondary password-toggle" type="button"
                            data-toggle-password="txtConfirm" aria-label="Show confirmation">
                            <svg viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z" />
                                <circle cx="12" cy="12" r="3" />
                            </svg>
                        </button>
                    </div>
                </div>

                <asp:Button ID="btnReset" runat="server" Text="Reset password"
                    CssClass="btn btn-primary w-100" OnClick="btnReset_Click" />
            </section>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/masaking.js"></script>
</body>
</html>