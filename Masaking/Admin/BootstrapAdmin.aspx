<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BootstrapAdmin.aspx.cs" Inherits="Masaking.Admin.BootstrapAdmin" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Initial admin setup | Masaking</title>

    <link href="<%= ResolveUrl("~/Content/bootstrap-icons.min.css?v=1") %>" rel="stylesheet" />
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/auth.css" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server">
        <nav class="navbar masaking-nav">
            <div class="container">
                <a class="navbar-brand" href="../Default.aspx">
                    <img class="brand-mark" src="../Content/images/Masaking%20Logo.png" alt="Masaking" />
                </a>
            </div>
        </nav>

        <main class="container auth-wrap">
            <section class="masaking-card p-4 p-md-5">
                <p class="text-brand fw-semibold">ONE-TIME SETUP</p>
                <h1 class="page-heading h2">Set up the first administrator</h1>
                <p class="text-secondary">When the user table is empty, create the first admin here. If member accounts already exist, sign in as the account to promote. Public registration remains member-only.</p>

                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

                <div class="mb-3">
                    <label class="form-label" for="txtKey">One-time setup key</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtKey" runat="server" CssClass="form-control" TextMode="Password" required="required" />
                        <button class="btn btn-outline-secondary password-toggle" type="button" data-toggle-password="txtKey" aria-label="Show setup key">
                            <i class="bi bi-eye" aria-hidden="true"></i>
                        </button>
                    </div>
                </div>

                <asp:Panel ID="pnlCreateFirst" runat="server" Visible="false">
                    <h2 class="h5 page-heading">Create administrator account</h2>

                    <div class="mb-3">
                        <label class="form-label" for="txtAdminName">Display name</label>
                        <asp:TextBox ID="txtAdminName" runat="server" CssClass="form-control" MaxLength="100" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label" for="txtAdminUsername">Username</label>
                        <asp:TextBox ID="txtAdminUsername" runat="server" CssClass="form-control" MaxLength="40" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label" for="txtAdminEmail">Email address</label>
                        <asp:TextBox ID="txtAdminEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="255" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label" for="txtAdminCountry">Country</label>
                        <asp:TextBox ID="txtAdminCountry" runat="server" CssClass="form-control" MaxLength="100" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label" for="txtNewPassword">Password</label>
                        <div class="input-group">
                            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" />
                            <button class="btn btn-outline-secondary password-toggle" type="button" data-toggle-password="txtNewPassword" aria-label="Show password">
                                <i class="bi bi-eye" aria-hidden="true"></i>
                            </button>
                        </div>
                    </div>

                    <div class="mb-4">
                        <label class="form-label" for="txtConfirm">Confirm password</label>
                        <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" />
                    </div>

                    <asp:Button ID="btnCreateFirst" runat="server" Text="Create first administrator" CssClass="btn btn-primary w-100" OnClick="btnCreateFirst_Click" />
                </asp:Panel>

                <asp:Panel ID="pnlPromoteExisting" runat="server" Visible="false">
                    <p class="small text-secondary">Signed in as an existing member? Verify your current password to promote that account.</p>

                    <div class="mb-4">
                        <label class="form-label" for="txtPassword">Your current password</label>
                        <div class="input-group">
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
                            <button class="btn btn-outline-secondary password-toggle" type="button" data-toggle-password="txtPassword" aria-label="Show password">
                                <i class="bi bi-eye" aria-hidden="true"></i>
                            </button>
                        </div>
                    </div>

                    <asp:Button ID="btnPromote" runat="server" Text="Promote my account" CssClass="btn btn-primary w-100" OnClick="btnPromote_Click" />
                </asp:Panel>
            </section>
        </main>
    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/form-helpers.js?v=20261024-39"></script>
</body>
</html>