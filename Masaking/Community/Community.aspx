<%@ Page Title="Community Kitchen | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Community.aspx.cs" Inherits="Masaking.Community.Community" %>
<%@ Import Namespace="Masaking.Infrastructure" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <link href="<%= ResolveUrl("~/Content/css/community.css?v=20261024-40") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/css/community-responsive.css?v=2") %>" rel="stylesheet" />
    <%-- Official Bootstrap Icons v1.13.1, stored locally with the MIT license. --%>

    <main class="community-view community-feed-view">
        <div class="community-page-decor" aria-hidden="true">
            <img class="community-botanical community-botanical-left" src="<%= ResolveUrl("~/Content/images/community/community-botanical.png") %>" alt="" width="1024" height="1536" decoding="async" />
            <img class="community-botanical community-botanical-right" src="<%= ResolveUrl("~/Content/images/community/community-botanical.png") %>" alt="" width="1024" height="1536" decoding="async" />
        </div>

        <%-- Community-feed banner and compact filters from the reference. --%>
        <header class="community-banner mb-2">
            <div class="community-banner-inner container position-relative">
                <img class="community-banner-art d-none d-sm-block" src="<%= ResolveUrl("~/Content/images/community/three_plates_food.png?v=20261024-34") %>" alt="" aria-hidden="true" width="1678" height="937" />
                
                <div class="community-banner-copy">
                    <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-3">
                        <div class="community-hero-copy">
                            <h1 class="fw-bold mb-2">Community Kitchen</h1>
                            <p class="fw-bold mb-0">Share what you're cooking, ask questions, and exchange kitchen tips.</p>
                        </div>
                    </div>

                    <div class="community-search-row mb-4">
                        <div class="community-search-wrap">
                            <label for="communitySearch" class="visually-hidden">Search community posts</label>
                            <div class="input-group community-search-group">
                                <span class="input-group-text bg-transparent rounded-start border-end-0">
                                    <i class="bi bi-search community-icon" aria-hidden="true"></i>
                                </span>
                                <input id="communitySearch" type="search" class="form-control border-start-0 rounded-end" placeholder="Search community posts..." />
                            </div>
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlSort" runat="server" CssClass="community-filter-row position-relative">
                    <div class="d-flex flex-wrap justify-content-between align-items-center gap-2">
                        <nav class="d-flex flex-wrap gap-2" aria-label="Filter community posts">
                            <button type="button" class="btn btn-sm btn-primary rounded-pill px-3" data-community-filter="all" aria-pressed="true">All Posts</button>
                            <button type="button" class="btn btn-sm btn-outline-secondary rounded-pill px-3" data-community-filter="recipes" aria-pressed="false">With Recipes</button>
                            <button type="button" class="btn btn-sm btn-outline-secondary rounded-pill px-3" data-community-filter="discussions" aria-pressed="false">Discussions</button>
                        </nav>
                        <div>
                            <asp:Label runat="server" AssociatedControlID="ddlSort" CssClass="visually-hidden" Text="Sort posts" />
                            <asp:DropDownList ID="ddlSort" data-community-sort="Sort posts" runat="server" CssClass="form-select form-select-sm rounded-pill" AutoPostBack="true" OnSelectedIndexChanged="ddlSort_SelectedIndexChanged">
                                <asp:ListItem Value="newest">Latest</asp:ListItem>
                                <asp:ListItem Value="popular">Popular</asp:ListItem>
                                <asp:ListItem Value="oldest">Oldest</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </header>

        <div class="community-body container">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" CssClass="d-block mb-3" />

            <%-- Guests Prompt --%>
            <asp:Panel ID="pnlGuest" runat="server" Visible="false" CssClass="card community-card rounded-3 mb-4">
                <div class="card-body d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 p-4">
                    <div>
                        <h2 class="page-heading h5 mb-1 fw-bold text-dark">Join the conversation</h2>
                        <p class="text-secondary mb-0">Sign in to share cooking notes, like posts, and comment.</p>
                    </div>
                    <a class="btn btn-primary rounded-pill px-4 py-2 flex-shrink-0 fw-semibold" href="../Account/Login.aspx">Sign in</a>
                </div>
            </asp:Panel>

            <%-- Individual, softly bordered post cards match the community-feed reference. --%>
            <asp:Repeater ID="rptPosts" runat="server" OnItemCommand="rptPosts_ItemCommand">
                <ItemTemplate>
                    <article class="card community-card rounded-3 mb-3 community-feed-post" data-has-recipe='<%# Eval("recipeID") != DBNull.Value ? "true" : "false" %>'>
                        <div class="card-body p-3 p-sm-4">
                            <div class="d-flex gap-3">
                                <%# CommunityAvatar(Eval("authorImage"), Eval("displayName"), "Post author") %>
                                <div class="community-content flex-grow-1">
                                    <header class="d-flex justify-content-between align-items-start gap-2 mb-2">
                                        <div class="community-author d-flex flex-wrap align-items-baseline gap-2">
                                            <strong class="small"><%#: Eval("displayName") %></strong>
                                            <span class="small text-secondary">@<%#: Eval("username") %></span>
                                            <span class="small text-secondary">· <%# ContentDate(Eval("updatedAt"), Eval("createdAt")) %></span>
                                        </div>
                                        <asp:PlaceHolder runat="server" Visible='<%# CanDeletePost(Eval("userID")) %>'>
                                            <details class="community-menu dropdown position-relative flex-shrink-0">
                                                <summary class="btn btn-sm btn-light bg-transparent border-0 rounded-2 p-1 text-secondary" aria-label="Post options">
                                                    <i class="bi bi-three-dots community-icon" aria-hidden="true"></i>
                                                </summary>
                                                <div class="dropdown-menu d-block end-0 rounded-2 shadow-sm p-2">
                                                    <asp:HyperLink ID="lnkEditPost" runat="server" Visible='<%# IsPostOwner(Eval("userID")) && CanEdit(Convert.ToDateTime(Eval("createdAt"))) %>' NavigateUrl='<%# "EditPost.aspx?id=" + Eval("postID") %>' CssClass="dropdown-item rounded-2 small d-flex align-items-center gap-2">
                                                        <i class="bi bi-pencil community-icon" aria-hidden="true"></i> Edit Post
                                                    </asp:HyperLink>
                                                    <asp:Panel ID="pnlDeletePost" runat="server" Visible='<%# CanDeletePost(Eval("userID")) %>'>
                                                        <div class="community-delete-action">
                                                            <i class="bi bi-trash community-icon text-secondary" aria-hidden="true"></i>
                                                            <asp:Button ID="btnDeletePost" runat="server" Text='<%# IsPostOwner(Eval("userID")) ? "Delete Post" : "Remove Post" %>' CssClass="dropdown-item small rounded-2" CausesValidation="false" CommandArgument='<%# Eval("postID") %>' OnClick="btnDeletePost_Click" data-confirm="Delete or remove this post from the community?" />
                                                        </div>
                                                    </asp:Panel>
                                                    <asp:PlaceHolder runat="server" Visible='<%# !currentUserIsAdministrator %>'>
                                                        <p class="small text-secondary px-3 pt-2 mb-1">You can edit posts for 7 days after publishing.</p>
                                                    </asp:PlaceHolder>
                                                </div>
                                            </details>
                                        </asp:PlaceHolder>
                                    </header>

                                    <p class="community-copy mb-3"><%#: Eval("content") %></p>

                                    <div class="community-post-media">
                                        <asp:Image ID="imgPost" runat="server" Visible='<%# !String.IsNullOrWhiteSpace(CommunityImageUrl(Eval("image"))) %>' ImageUrl='<%# CommunityImageUrl(Eval("image")) %>' CssClass="community-post-photo img-fluid rounded-2 mb-3" AlternateText="Shared cooking photo" />
                                        <asp:HyperLink ID="lnkRecipe" runat="server" Visible='<%# Eval("recipeID") != DBNull.Value %>' NavigateUrl='<%# "../Recipes/RecipeDetails.aspx?id=" + Eval("recipeID") %>' CssClass="community-recipe d-flex align-items-center gap-3 border rounded-2 p-2 mb-3 text-decoration-none">
                                            <asp:Image runat="server" Visible='<%# !String.IsNullOrEmpty(RecipeImageUrl(Eval("recipeImage"))) %>' ImageUrl='<%# RecipeImageUrl(Eval("recipeImage")) %>' CssClass="community-recipe-thumb rounded-2" AlternateText="Attached recipe" />
                                            <span>
                                                <strong class="community-recipe-title small d-block"><%#: Eval("title") %></strong>
                                                <span class="small text-secondary"><%#: Eval("recipeSummary") %></span>
                                                <span class="d-block small mt-1">View recipe →</span>
                                            </span>
                                        </asp:HyperLink>
                                    </div>

                                    <footer class="d-flex justify-content-between align-items-center gap-2">
                                        <div class="d-flex align-items-center gap-3">
                                            <asp:UpdatePanel ID="upLike" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                <ContentTemplate>
                                                    <asp:LinkButton ID="btnLike" runat="server" OnClientClick="return false;" data-like-panel='<%# Container.UniqueID + "$upLike" %>' data-like-target='<%# Container.UniqueID + "$btnLike" %>' CausesValidation="false" CommandName='<%# Convert.ToInt32(Eval("userLiked")) == 1 ? "unlike" : "like" %>' CommandArgument='<%# Eval("postID") %>' CssClass="community-like btn btn-sm p-0 border-0 d-inline-flex align-items-center gap-2 text-decoration-none" ToolTip='<%# Convert.ToInt32(Eval("userLiked")) == 1 ? "Unlike post" : "Like post" %>' aria-label='<%# Convert.ToInt32(Eval("userLiked")) == 1 ? "Unlike post" : "Like post" %>' aria-pressed='<%# Convert.ToInt32(Eval("userLiked")) == 1 ? "true" : "false" %>' Text='<%# GetLikeButtonText(Eval("likeCount")) %>' />
                                                    <asp:Label ID="lblLikeError" runat="server" EnableViewState="false" CssClass="small text-danger ms-2" role="status" data-like-error="true" />
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                            <a class="btn btn-sm text-secondary p-0 border-0 d-inline-flex align-items-center gap-2" href='<%# "Post.aspx?id=" + Eval("postID") + "#comments" %>' aria-label="Read comments">
                                                <i class="bi bi-chat community-icon" aria-hidden="true"></i>
                                                <span class="small"><%#: Eval("commentCount") %></span>
                                            </a>
                                        </div>
                                        <a class="btn btn-sm btn-outline-secondary rounded-pill px-3 d-inline-flex align-items-center gap-1" href='<%# "Post.aspx?id=" + Eval("postID") + "#comments" %>'>
                                            <i class="bi bi-chat community-icon" aria-hidden="true"></i>
                                            <span class="small">Comment</span>
                                        </a>
                                    </footer>
                                </div>
                            </div>
                        </div>
                    </article>
                </ItemTemplate>
            </asp:Repeater>

            <p id="communityFilterEmpty" class="community-card rounded-3 p-4 text-center text-secondary d-none" role="status">No posts match this search. Try another word or filter.</p>

            <% if (!currentUserIsAdministrator) { %>
            <aside class="community-notice rounded-2 d-flex align-items-center gap-2 small text-secondary px-3 py-2 mt-3">
                <i class="bi bi-info-circle community-icon" aria-hidden="true"></i>
                You can edit your post for up to 7 days after publishing.
            </aside>
            <% } %>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="card border-0 shadow-sm rounded-4 p-5 text-center bg-white mt-4">
                <div class="text-secondary mb-3 d-flex justify-content-center">
                    <i class="bi bi-book community-icon community-empty-icon" aria-hidden="true"></i>
                </div>
                <h2 class="page-heading h4 fw-bold text-dark">A fresh page in the community cookbook</h2>
                <p class="text-secondary mb-0">There are no approved community posts yet. Be the first to share a cooking moment.</p>
            </asp:Panel>

            <%-- Deleted Posts Recovery Panel --%>
            <asp:Panel ID="pnlDeletedPosts" runat="server" Visible="false" CssClass="accordion mt-5">
                <div class="accordion-item border-0 rounded-4 overflow-hidden shadow-sm bg-white">
                    <h2 class="accordion-header" id="myDeletedPostsHeading">
                        <button class="accordion-button collapsed fw-semibold text-dark shadow-none" type="button"
                            data-bs-toggle="collapse" data-bs-target="#myDeletedPostsBody"
                            aria-expanded="false" aria-controls="myDeletedPostsBody">
                            Recently deleted posts
                        </button>
                    </h2>
                    <div id="myDeletedPostsBody" class="accordion-collapse collapse" aria-labelledby="myDeletedPostsHeading">
                        <div class="accordion-body">
                            <p class="small text-secondary mb-3">Only posts you deleted yourself appear here. Posts removed by a moderator cannot be restored from this list.</p>
                            <asp:Repeater ID="rptDeletedPosts" runat="server" OnItemCommand="rptDeletedPosts_ItemCommand">
                                <ItemTemplate>
                                    <div class="d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-3 border-top py-3">
                                        <div class="flex-grow-1">
                                            <p class="community-copy mb-1 text-dark small"><%#: Eval("content") %></p>
                                            <span class="small text-secondary">Deleted · <%# ContentDate(Eval("updatedAt"), Eval("createdAt")) %></span>
                                        </div>
                                        <asp:Button ID="btnRestorePost" runat="server"
                                            Text="Restore"
                                            CssClass="btn btn-sm btn-outline-success rounded-pill px-3 flex-shrink-0 fw-semibold"
                                            CommandName="restorePost"
                                            CommandArgument='<%# Eval("postID") %>' />
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </main>

    <div class="community-floating-actions">
        <button id="communityBackToTop" type="button" class="btn btn-light community-back-to-top" aria-label="Back to top" title="Back to top" hidden>
            <i class="bi bi-arrow-up community-icon" aria-hidden="true"></i>
        </button>
        <asp:Panel ID="pnlAddPost" runat="server">
            <a href="EditPost.aspx" class="btn btn-primary rounded-pill px-4 py-3 shadow community-create-post" aria-label="Create a community post">
                <i class="bi bi-plus-lg community-icon" aria-hidden="true"></i>
                <span class="community-create-label"> Create Post</span>
            </a>
        </asp:Panel>
    </div>

    <script src="../Scripts/form-helpers.js?v=20261024-39"></script>
    <script src="../Scripts/community.js?v=20261024-40"></script>
</asp:Content>
