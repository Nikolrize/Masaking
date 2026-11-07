<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Masaking.Account.Register" %>

<!doctype html>
    <html lang="en">
    <head runat="server">
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title>Create account | Masaking</title>

        <link href="../Content/bootstrap-icons.min.css" rel="stylesheet" />
        <link href="../Content/bootstrap.min.css" rel="stylesheet" />
        <link href="../Content/Site.css" rel="stylesheet" />
        <link href="../Content/css/auth.css" rel="stylesheet" />
    </head>
    <body class="auth-background">
        <form id="form1" runat="server">

            <main class="container min-vh-100 d-flex align-items-center justify-content-center py-4 py-md-5">
                <section class="card auth-card w-100 rounded-4 shadow-sm p-4 p-md-5" aria-labelledby="authHeading">
                    <h1 id="authHeading" class="h2 fw-bold text-center mb-3">Create your account</h1>
                    <p class="text-secondary text-center mb-4">Save recipes, share what you cook, and learn at your own pace.</p>

                    <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                    <div class="mb-3">
                        <asp:Label runat="server" ID="lblName" class="form-label" AssociatedControlID="txtName">Display name</asp:Label>
                        <asp:TextBox ID="txtName" runat="server" class="form-control" MaxLength="80" required="required" />
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" ID="lblUsername" class="form-label" AssociatedControlID="txtUsername">Username</asp:Label>
                        <asp:TextBox ID="txtUsername" runat="server" class="form-control" MaxLength="40" required="required" />
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" ID="lblEmail" class="form-label" AssociatedControlID="txtEmail">Email address</asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" class="form-control" TextMode="Email" MaxLength="254" required="required" />
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" ID="lblCountry" class="form-label" AssociatedControlID="ddlCountry">Country</asp:Label>
                        <asp:DropDownList ID="ddlCountry" runat="server" ClientIDMode="Static" class="form-select" required="required" data-country-select="countryOther" />

                        <div id="countryOther" class="mt-3">
                            <asp:Label runat="server" ID="lblCountry2" class="form-label" AssociatedControlID="txtCountry">Your country (if you selected Other)</asp:Label>
                            <asp:TextBox ID="txtCountry" runat="server" ClientIDMode="Static" class="form-control" MaxLength="80" autocomplete="country-name" />
                        </div>
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" ID="lblPassword" class="form-label" AssociatedControlID="txtPassword">Password</asp:Label>
                        <div class="input-group">
                            <asp:TextBox ID="txtPassword" runat="server" class="form-control" TextMode="Password" MaxLength="64" required="required" />
                            <button runat="server" id="btnShowPassword" class="btn btn-outline-secondary d-inline-flex align-items-center justify-content-center px-3" type="button" data-toggle-password="txtPassword" aria-label="Show password">
                                <i class="bi bi-eye" aria-hidden="true"></i>
                            </button>
                        </div>
                        <div class="form-text">Use 8 to 64 characters.</div>
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" ID="lblConfirm" class="form-label" AssociatedControlID="txtConfirm">Confirm password</asp:Label>
                        <div class="input-group">
                            <asp:TextBox ID="txtConfirm" runat="server" class="form-control" TextMode="Password" MaxLength="64" required="required" />
                            <button runat="server" id="btnShowConfirmation" class="btn btn-outline-secondary d-inline-flex align-items-center justify-content-center px-3" type="button" data-toggle-password="txtConfirm" aria-label="Show confirmation">
                                <i class="bi bi-eye" aria-hidden="true"></i>
                            </button>
                        </div>
                    </div>

                    <div class="d-flex align-items-start gap-2 mb-4">
                        <input id="chkTerms" runat="server" clientidmode="Static" type="checkbox" class="form-check-input flex-shrink-0" />
                        <asp:Label runat="server" ID="lblChkTerms" class="form-check-label" AssociatedControlID="chkTerms">
                        I agree to share respectfully with the Masaking community.
                        </asp:Label>
                    </div>

                    <asp:Button ID="btnRegister" runat="server" Text="Create account" class="btn btn-primary w-100" OnClick="btnRegister_Click" />

                    <p class="mt-4 mb-0 text-center small">
                        Already a member? 
                    <asp:HyperLink runat="server" ID="lnkSignIn" class="fw-semibold" NavigateUrl="Login.aspx">Sign in</asp:HyperLink>
                    </p>
                </section>
            </main>

        </form>

        <script src="../Scripts/bootstrap.bundle.min.js"></script>
        <script src="../Scripts/form-helpers.js"></script>
    </body>
    </html>
