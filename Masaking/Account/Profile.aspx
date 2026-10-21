<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="Masaking.Account.Profile" %>
<%@ Import Namespace="Masaking.Infrastructure" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>My profile | Masaking</title>
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server" enctype="multipart/form-data">

        <nav class="navbar navbar-expand-lg masaking-nav">
            <div class="container">
                <a class="navbar-brand" href="../Default.aspx">
                    <img class="brand-mark" src="../Content/images/Masaking%20Logo.png" alt="Masaking" />
                </a>
                <div class="navbar-nav ms-auto">
                    <a class="nav-link" href="../Recipes/BrowseRecipes.aspx">Browse recipes</a>
                    <a class="nav-link" href="../Recipes/MyRecipes.aspx">My recipes</a>
                    <a class="nav-link" href="../Community/Community.aspx">Community</a>
                    <a class="nav-link d-flex align-items-center gap-2" href="Profile.aspx">
                        <img class="avatar-small" src="<%: Database.AvatarUrl(Session["UserImage"]) %>" alt="" /><%: Server.HtmlEncode(Convert.ToString(Session["DisplayName"])) %>
                    </a>
                    <asp:LinkButton ID="lnkSignOut" runat="server" CssClass="nav-link" OnClick="lnkSignOut_Click">Sign out</asp:LinkButton>
                </div>
            </div>
        </nav>

        <main class="container py-5">

            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <%-- Editable profile banner --%>
            <section class="profile-editor-card p-4 p-lg-5 mb-4">
                <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
                    <div><p class="text-brand fw-semibold text-uppercase small mb-2">Your Masaking account</p><h1 class="page-heading mb-1">Profile details</h1><asp:Panel ID="pnlMemberProfileNote" runat="server"><p class="text-secondary mb-0">Update your account information and profile photo.</p></asp:Panel><asp:Panel ID="pnlAdminProfileNote" runat="server" Visible="false" CssClass="mt-2"><span class="badge rounded-pill text-bg-success">Administrator</span><span class="text-secondary ms-2">Manage the Masaking community from your dashboard.</span></asp:Panel></div>
                    <span class="profile-username-chip"><span class="profile-summary-label">Username</span><strong><asp:Literal ID="litProfileUsername" runat="server" /></strong></span>
                </div>
                <div class="row g-4 align-items-start">
                    <div class="col-lg-4">
                        <div class="profile-photo-panel text-center p-4 h-100">
                            <asp:Image ID="imgAvatar" runat="server" CssClass="avatar-profile profile-editor-avatar mb-3" AlternateText="Your profile picture" />
                            <strong class="d-block mb-1">Your profile picture</strong><span class="small text-secondary">JPG, PNG or GIF, up to 5 MB.</span>
                            <label class="form-label text-start d-block mt-4" for="uploadAvatar">Upload a new picture</label>
                            <asp:FileUpload ID="uploadAvatar" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif" data-image-preview="profileAvatarPreview" />
                            <div id="profileAvatarError" class="small text-danger mt-1 text-start"></div>
                            <img id="profileAvatarPreview" class="image-preview avatar-preview mt-3" alt="Preview of your new profile picture" />
                        </div>
                    </div>
                    <div class="col-lg-8">
                        <div class="mb-3"><label class="form-label" for="txtName">Display name</label><asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" /></div>
                        <div class="mb-3"><label class="form-label" for="txtEmail">Email address</label><asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="255" required="required" /></div>
                        <div class="mb-4"><label class="form-label" for="txtCountry">Country</label><asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" MaxLength="100" /></div>
                        <div class="d-flex flex-wrap gap-2"><asp:Button ID="btnSaveProfile" runat="server" Text="Save profile" CssClass="btn btn-primary px-4" OnClick="btnSaveProfile_Click" /><asp:Button ID="btnResetAvatar" runat="server" Text="Use default avatar" CssClass="btn btn-outline-secondary" OnClick="btnResetAvatar_Click" /></div>
                    </div>
                </div>
            </section>

            <div class="row g-4">

                <%-- Left column: details and password --%>
                <div class="col-lg-7">

                    <section class="masaking-card p-4 p-lg-5">
                        <h2 class="h5 page-heading profile-panel-title mb-4">
                            <span class="title-mark form-icon">
                                <svg viewBox="0 0 24 24" aria-hidden="true">
                                    <circle cx="8" cy="15" r="4" />
                                    <path d="m11 12 9-9 2 2-2 2 2 2-3 3-2-2-3 3" />
                                </svg>
                            </span>
                            Change password
                        </h2>

                        <div class="mb-3">
                            <label class="form-label" for="txtCurrentPassword">Current password</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" TextMode="Password" />
                                <button class="btn btn-outline-secondary password-toggle" type="button"
                                    data-toggle-password="txtCurrentPassword" aria-label="Show current password">
                                    <svg viewBox="0 0 24 24" aria-hidden="true">
                                        <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z" />
                                        <circle cx="12" cy="12" r="3" />
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <div class="mb-3">
                            <label class="form-label" for="txtNewPassword">New password</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" />
                                <button class="btn btn-outline-secondary password-toggle" type="button"
                                    data-toggle-password="txtNewPassword" aria-label="Show new password">
                                    <svg viewBox="0 0 24 24" aria-hidden="true">
                                        <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z" />
                                        <circle cx="12" cy="12" r="3" />
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <asp:Button ID="btnChangePassword" runat="server" Text="Update password"
                            CssClass="btn btn-outline-primary" OnClick="btnChangePassword_Click" />
                    </section>
                </div>

                <%-- Right column: activity and shortcuts --%>
                <asp:Panel ID="pnlMemberRail" runat="server" CssClass="col-lg-5">
                    <section class="masaking-card p-4 p-lg-5">
                        <h2 class="h5 page-heading profile-panel-title mb-4">
                            <span class="title-mark">✦</span> Your cooking activity
                        </h2>
                        <asp:Literal ID="litStats" runat="server" />
                    </section>

                    <div class="d-grid gap-2 mt-4">
                        <a class="btn btn-primary" href="../Recipes/SubmitRecipe.aspx">Share a recipe</a>
                        <a class="btn btn-outline-success" href="../Recipes/MyRecipes.aspx">Manage my recipes</a>
                        <a class="btn btn-outline-success" href="../Community/Community.aspx">Visit the community</a>
                    </div>
                </asp:Panel>

            </div>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/masaking.js"></script>
</body>
</html>
