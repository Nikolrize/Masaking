<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Masaking.Account.Login" %>

<!doctype html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Sign in | Masaking</title>
    <link href="../Content/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/auth.css" rel="stylesheet" />
</head>
<body class="auth-background">
    <form id="form1" runat="server">
        <main class="container min-vh-100 d-flex align-items-center justify-content-center py-4 py-md-5">
            <section class="card auth-card w-100 rounded-4 shadow-sm p-4 p-md-5" aria-labelledby="authHeading">
                <h1 id="authHeading" class="h2 fw-bold text-center mb-3">Welcome Back!</h1>
                <p class="text-secondary text-center mb-4">Sign in to continue your cooking journey.</p>
                
                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-3">
                    <asp:Label runat="server" ID="lblEmail" class="form-label" AssociatedControlID="txtEmail">Email address</asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" class="form-control" TextMode="Email" required="required" />
                </div>

                <div class="mb-3">
                    <asp:Label runat="server" ID="lblPassword" class="form-label" AssociatedControlID="txtPassword">Password</asp:Label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPassword" runat="server" class="form-control" TextMode="Password" required="required" />
                        <button runat="server" id="btnShowPassword" class="btn btn-outline-secondary d-inline-flex align-items-center justify-content-center px-3" type="button" data-toggle-password="txtPassword" aria-label="Show password">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                </div>

                <div class="text-end mt-3">
                    <asp:HyperLink runat="server" ID="lnkForgotPassword" NavigateUrl="ForgotPassword.aspx">Forgot password?</asp:HyperLink>
                </div>

                <asp:Button ID="btnLogin" runat="server" Text="Sign in" class="btn btn-primary w-100 mt-3" OnClick="btnLogin_Click" />

                <p class="mt-4 mb-0 text-center small">
                    New to Masaking? <asp:HyperLink runat="server" ID="lnkCreateAnAccount" class="fw-semibold" NavigateUrl="Register.aspx">Create an account</asp:HyperLink>
                </p>
            </section>
        </main>
    </form>
    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/form-helpers.js"></script>
</body>
</html>
