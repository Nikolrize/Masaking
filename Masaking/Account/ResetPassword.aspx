<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="Masaking.Account.ResetPassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Choose a new password | Masaking</title>

    <link href="<%= ResolveUrl("~/Content/bootstrap-icons.min.css?v=1") %>" rel="stylesheet" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/auth.css" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server">

        <main class="container auth-wrap">
            <section class="masaking-card p-4 p-md-5">
                <p class="text-brand fw-semibold">ACCOUNT HELP</p>
                <h1 class="page-heading h2">Choose a new password</h1>

                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-3">
                    <label class="form-label" for="txtPassword">New password</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control"
                            TextMode="Password" MaxLength="64" required="required" />
                        <button class="btn btn-outline-secondary password-toggle" type="button"
                            data-toggle-password="txtPassword" aria-label="Show password">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                    <div class="form-text">At least 8 characters.</div>
                </div>

                <div class="mb-4">
                    <label class="form-label" for="txtConfirm">Confirm new password</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control"
                            TextMode="Password" MaxLength="64" required="required" />
                        <button class="btn btn-outline-secondary password-toggle" type="button"
                            data-toggle-password="txtConfirm" aria-label="Show confirmation">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                </div>

                <asp:Button ID="btnReset" runat="server" Text="Reset password"
                    CssClass="btn btn-primary w-100" OnClick="btnReset_Click" />
            </section>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/form-helpers.js?v=20261024-39"></script>
</body>
</html>