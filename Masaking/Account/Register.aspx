<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Masaking.Account.Register" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Create account | Masaking</title>
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
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#debugNav">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div id="debugNav" class="collapse navbar-collapse">
                    <ul class="navbar-nav ms-auto">
                        <li class="nav-item"><a class="nav-link" href="../Recipes/BrowseRecipes.aspx">Recipes</a></li>
                        <li class="nav-item"><a class="nav-link" href="../Community/Community.aspx">Community</a></li>
                        <li class="nav-item"><a class="nav-link" href="Login.aspx">Login</a></li>
                    </ul>
                </div>
            </div>
        </nav>

        <main class="container auth-wrap">
            <div class="masaking-card p-4 p-md-5">
                <p class="text-brand fw-semibold mb-2 form-icon">
                    <svg viewBox="0 0 24 24" aria-hidden="true">
                        <circle cx="12" cy="8" r="3.5" />
                        <path d="M4.5 20c.6-4 3.2-6 7.5-6s6.9 2 7.5 6" />
                    </svg>
                    JOIN THE TABLE
                </p>
                <h1 class="page-heading h2">Create your account</h1>
                <p class="text-secondary mb-4">Save recipes, share what you cook, and learn at your own pace.</p>

                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-3">
                    <label class="form-label" for="txtName">Display name</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="80" required="required" />
                </div>

                <div class="mb-3">
                    <label class="form-label" for="txtUsername">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="40" required="required" />
                </div>

                <div class="mb-3">
                    <label class="form-label" for="txtEmail">Email address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                        TextMode="Email" MaxLength="254" required="required" />
                </div>

                <div class="mb-3">
                    <label class="form-label" for="txtCountry">Country</label>
                    <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control"
                        MaxLength="80" placeholder="e.g. Malaysia" required="required" />
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
                    <div class="form-text">Use at least 8 characters.</div>
                </div>

                <div class="mb-3">
                    <label class="form-label" for="txtConfirm">Confirm password</label>
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

                <div class="form-check register-terms mb-4">
                    <asp:CheckBox ID="chkTerms" runat="server" CssClass="form-check-input" />
                    <label class="form-check-label" for="chkTerms">I agree to share respectfully with the Masaking community.</label>
                </div>

                <asp:Button ID="btnRegister" runat="server" Text="Create account"
                    CssClass="btn btn-primary w-100" OnClick="btnRegister_Click" />

                <p class="mt-4 mb-0 text-center">
                    Already a member? <a class="text-brand" href="Login.aspx">Sign in</a>
                </p>
            </div>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/masaking.js"></script>
</body>
</html>
