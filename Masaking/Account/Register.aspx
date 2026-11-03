<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Masaking.Account.Register" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Create account | Masaking</title>

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
                <p class="text-brand fw-semibold mb-2 form-icon">
                    <i class="bi bi-person-plus" aria-hidden="true"></i>
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
                            TextMode="Password" MaxLength="64" required="required" />
                        <button class="btn btn-outline-secondary password-toggle" type="button"
                            data-toggle-password="txtPassword" aria-label="Show password">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                    <div class="form-text">Use 8 to 64 characters.</div>
                </div>

                <div class="mb-3">
                    <label class="form-label" for="txtConfirm">Confirm password</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control"
                            TextMode="Password" MaxLength="64" required="required" />
                        <button class="btn btn-outline-secondary password-toggle" type="button"
                            data-toggle-password="txtConfirm" aria-label="Show confirmation">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                </div>

                <div class="form-check register-terms mb-4">
                    <input id="chkTerms" runat="server" ClientIDMode="Static" type="checkbox" class="form-check-input" />
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
    <script src="../Scripts/form-helpers.js?v=20261024-39"></script>
</body>
</html>
