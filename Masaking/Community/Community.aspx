<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Community.aspx.cs" Inherits="Masaking.Community.Community" %>
<%@ Import Namespace="Masaking.Infrastructure" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Community | Masaking</title>
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
                    <a class="nav-link" href="../Recipes/BrowseRecipes.aspx">Recipes</a>
                    <a class="nav-link" href="Reviews.aspx">Reviews</a>
                    <a class="nav-link d-flex align-items-center gap-2" href="../Account/Profile.aspx">
                        <img class="avatar-small" src="<%: Database.AvatarUrl(Session["UserImage"]) %>" alt="" />My profile
                    </a>
                </div>
            </div>
        </nav>

        <main class="container py-5">

            <%-- Page header --%>
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3 mb-4">
                <div>
                    <p class="text-brand fw-semibold mb-1">FROM OUR COMMUNITY</p>
                    <h1 class="page-heading">Cooking is better together</h1>
                    <p class="text-secondary mb-0">Share a dish you have made and swap helpful cooking notes with other home cooks.</p>
                </div>
                <asp:Panel ID="pnlAddPost" runat="server">
                    <button class="btn btn-primary" type="button"
                        data-bs-toggle="collapse" data-bs-target="#pnlShare"
                        aria-expanded="false" aria-controls="pnlShare">
                        <span class="me-1" aria-hidden="true">＋</span> Add new post
                    </button>
                </asp:Panel>
            </div>

            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <%-- New post form --%>
            <asp:Panel ID="pnlShare" runat="server" ClientIDMode="Static" CssClass="collapse masaking-card p-4 mb-4">
                <h2 class="h5 page-heading">Share a cooking moment</h2>

                <div class="row g-3">
                    <div class="col-md-4">
                        <label class="form-label" for="ddlRecipe">Recipe</label>
                        <asp:DropDownList ID="ddlRecipe" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-8">
                        <label class="form-label" for="txtPost">Your note</label>
                        <asp:TextBox ID="txtPost" runat="server" CssClass="form-control"
                            MaxLength="2000" TextMode="MultiLine" Rows="2"
                            placeholder="What did you learn or change while cooking?" />
                    </div>
                </div>

                <div class="mt-3">
                    <label class="form-label" for="uploadPostImage">Add a photo (optional)</label>
                    <asp:FileUpload ID="uploadPostImage" runat="server" CssClass="form-control"
                        accept="image/jpeg,image/png,image/gif"
                        data-image-preview="communityImagePreview" />
                    <div id="communityImageError" class="small text-danger mt-1"></div>
                    <img id="communityImagePreview" class="image-preview mt-3" alt="Preview of your community photo" />
                </div>

                <asp:Button ID="btnPost" runat="server" Text="Share with the community"
                    CssClass="btn btn-primary mt-3" OnClick="btnPost_Click" />
            </asp:Panel>

            <%-- Guest prompt --%>
            <asp:Panel ID="pnlGuest" runat="server" Visible="false" CssClass="masaking-card p-4 mb-4">
                <a class="text-brand" href="../Account/Login.aspx">Sign in</a>
                to share cooking notes and join the conversation.
            </asp:Panel>

            <%-- Sort --%>
            <asp:Panel ID="pnlSort" runat="server" CssClass="d-flex justify-content-end align-items-center gap-2 mb-5">
                <label class="small text-secondary" for="ddlSort">Sort by</label>
                <asp:DropDownList ID="ddlSort" runat="server" CssClass="form-select form-select-sm"
                    Style="max-width: 190px" AutoPostBack="true"
                    OnSelectedIndexChanged="ddlSort_SelectedIndexChanged">
                    <asp:ListItem Value="newest">Newest first</asp:ListItem>
                    <asp:ListItem Value="oldest">Oldest first</asp:ListItem>
                    <asp:ListItem Value="popular">Most discussed</asp:ListItem>
                </asp:DropDownList>
            </asp:Panel>

            <%-- Single post thread --%>
            <asp:Panel ID="pnlThread" runat="server" Visible="false" CssClass="masaking-card p-4 mb-4">
                <a class="small text-brand" href="Community.aspx">← All community posts</a>
                <h2 class="h4 page-heading mt-2">
                    <asp:Literal ID="litThreadTitle" runat="server" />
                </h2>
                <asp:Image ID="imgThread" runat="server" Visible="false" CssClass="community-photo mb-3" AlternateText="Community post photo" />
                <p class="text-secondary">
                    <asp:Literal ID="litThreadPost" runat="server" />
                </p>

                <asp:Repeater ID="rptComments" runat="server">
                    <ItemTemplate>
                        <article class="border-top py-3">
                            <strong><%#: Eval("displayName") %></strong>
                            <span class="small text-secondary ms-2"><%#: Eval("createdAt", "{0:dd MMM yyyy h:mm tt}") %></span>
                            <p class="mb-0 mt-1"><%#: Eval("content") %></p>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Panel ID="pnlNoComments" runat="server" Visible="false" CssClass="text-secondary border-top pt-3">
                    No comments yet. Start the conversation.
                </asp:Panel>

                <asp:Panel ID="pnlCommentForm" runat="server" CssClass="border-top pt-3 mt-2">
                    <label class="form-label" for="txtComment">Add a cooking note</label>
                    <asp:TextBox ID="txtComment" runat="server" CssClass="form-control"
                        MaxLength="1000" TextMode="MultiLine" Rows="2" />
                    <asp:Button ID="btnComment" runat="server" Text="Post comment"
                        CssClass="btn btn-primary mt-2" OnClick="btnComment_Click" />
                </asp:Panel>

                <asp:HyperLink ID="lnkCommentLogin" runat="server" Visible="false"
                    NavigateUrl="../Account/Login.aspx" CssClass="text-brand">Sign in to comment</asp:HyperLink>
            </asp:Panel>

            <%-- Post list --%>
            <div class="row g-3">
                <asp:Repeater ID="rptPosts" runat="server">
                    <ItemTemplate>
                        <div class="col-lg-6">
                            <article class="masaking-card p-4 h-100">
                                <div class="d-flex justify-content-between gap-2">
                                    <div>
                                        <h2 class="h5 page-heading mb-1">
                                            <a class="text-reset text-decoration-none"
                                               href='<%# "../Recipes/RecipeDetails.aspx?id=" + Eval("recipeID") %>'><%#: Eval("title") %></a>
                                        </h2>
                                        <p class="small text-brand mb-3">Shared by <%#: Eval("displayName") %> · <%#: Eval("createdAt", "{0:dd MMM yyyy}") %></p>
                                    </div>
                                    <span class="auth-ornament fs-4">✿</span>
                                </div>

                                <asp:Panel runat="server" Visible='<%# !String.IsNullOrEmpty(Convert.ToString(Eval("image"))) %>'>
                                    <img class="community-photo mb-3" src='<%# CommunityImageUrl(Eval("image")) %>' alt="Photo shared with this post" />
                                </asp:Panel>

                                <p><%#: Eval("content") %></p>

                                <a class="btn btn-sm btn-outline-success"
                                   href='<%# "Community.aspx?post=" + Eval("postID") %>'>Join the conversation (<%#: Eval("commentCount") %>)</a>
                            </article>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="masaking-card p-5 text-center">
                <h2 class="h5 page-heading">A fresh page in the community cookbook</h2>
                <p class="text-secondary mb-0">Approved community posts will appear here.</p>
            </asp:Panel>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/masaking.js"></script>
</body>
</html>