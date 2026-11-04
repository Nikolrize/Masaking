<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="Masaking.Account.ResetPassword" %>

<!doctype html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Choose a new password | Masaking</title>
    <link href="../Content/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/auth.css" rel="stylesheet" />
</head>
<body class="auth-background">
    <form id="form1" runat="server">
        <main class="container min-vh-100 d-flex align-items-center justify-content-center py-4 py-md-5">
            <section class="card auth-card w-100 rounded-4 shadow-sm p-4 p-md-5" aria-labelledby="authHeading">
                <h1 id="authHeading" class="h2 fw-bold text-center mb-3">Choose a new password</h1>
                
                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-3">
                    <asp:Label runat="server" ID="lblPassword" class="form-label" AssociatedControlID="txtPassword">New password</asp:Label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPassword" runat="server" class="form-control" TextMode="Password" MaxLength="64" required="required" />
                        <button runat="server" id="btnShowPassword" class="btn btn-outline-secondary d-inline-flex align-items-center justify-content-center px-3" type="button" data-toggle-password="txtPassword" aria-label="Show password">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                    <div class="form-text">At least 8 characters.</div>
                </div>

                <div class="mb-4">
                    <asp:Label runat="server" ID="lblConfirm" class="form-label" AssociatedControlID="txtConfirm">Confirm new password</asp:Label>
                    <div class="input-group">
                        <asp:TextBox ID="txtConfirm" runat="server" class="form-control" TextMode="Password" MaxLength="64" required="required" />
                        <button runat="server" id="btnShowConfirmation" class="btn btn-outline-secondary d-inline-flex align-items-center justify-content-center px-3" type="button" data-toggle-password="txtConfirm" aria-label="Show confirmation">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                </div>

                <asp:Button ID="btnReset" runat="server" Text="Reset password" class="btn btn-primary w-100" OnClick="btnReset_Click" />

                <p class="mt-4 mb-0 text-center small">
                    <asp:HyperLink runat="server" ID="lnkBackToSignIn" NavigateUrl="Login.aspx">Back to sign in</asp:HyperLink>
                </p>
            </section>
        </main>
    </form>
    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/form-helpers.js?v=20261024-39"></script>
</body>
</html>