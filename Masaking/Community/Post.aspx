<%@ Page Title="Community discussion | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Post.aspx.cs" Inherits="Masaking.Community.Post" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/community.css?v=20261024-40" rel="stylesheet" />
    <link href="../Content/css/community-responsive.css?v=2" rel="stylesheet" />

    <main class="community-view community-detail community-feed-view community-secondary-view">
        <div class="community-page-decor" aria-hidden="true">
            <img class="community-botanical community-botanical-left" src="<%= ResolveUrl("~/Content/images/community/community-botanical.png") %>" alt="" width="1024" height="1536" decoding="async" />
            <img class="community-botanical community-botanical-right" src="<%= ResolveUrl("~/Content/images/community/community-botanical.png") %>" alt="" width="1024" height="1536" decoding="async" />
        </div>

        <header class="community-banner mb-3">
            <div class="community-banner-inner container">
                <div class="community-banner-copy">
                    <a href="Community.aspx" class="community-back-link">
                        <i class="bi bi-arrow-left community-icon community-back-arrow" aria-hidden="true"></i>
                        <span>Back to Community</span>
                    </a>
                    <h1 class="display-6 fw-bold mb-2">Community discussion</h1>
                    <p class="small mb-0">Share ideas, ask questions, and keep the conversation cooking.</p>
                </div>
            </div>
        </header>

        <div class="community-body container">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <asp:Panel ID="pnlUnavailable" runat="server" Visible="false" CssClass="community-card rounded-3 p-4">
                <h1 class="h4">Post unavailable</h1>
                <p class="mb-0">This post may have been removed. Return to the community to find another conversation.</p>
            </asp:Panel>

            <asp:Panel ID="pnlThread" runat="server" Visible="false">
                <%-- Individual, softly bordered post cards match the community-feed reference. --%>
                <asp:Repeater ID="rptPosts" runat="server" OnItemCommand="rptPosts_ItemCommand">
                    <ItemTemplate>
                        <article class="card community-card rounded-3 mb-3 community-feed-post community-detail-post" data-has-recipe='<%# Eval("recipeID") != DBNull.Value ? "true" : "false" %>'>
                            <div class="card-body p-3 p-sm-4">
                                <div class="d-flex gap-3">
                                    <%# CommunityAvatar(Eval("authorImage"), Eval("displayName"), "Post author") %>
                                    <div class="community-content flex-grow-1">
                                        <header class="d-flex justify-content-between align-items-start gap-2 mb-2">
                                            <div class="community-author d-flex align-items-center gap-2">
                                                <div>
                                                    <strong class="small d-block"><%#: Eval("displayName") %></strong>
                                                    <span class="small text-secondary">@<%#: Eval("username") %> · <%#: ContentDate(Eval("updatedAt"), Eval("createdAt")) %></span>
                                                </div>
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

                                        <asp:PlaceHolder runat="server" Visible='<%# PostHeading(Eval("content")).Length > 0 %>'>
                                            <h2 class="h3 fw-bold mb-2"><%#: PostHeading(Eval("content")) %></h2>
                                        </asp:PlaceHolder>

                                        <p class="community-copy mb-3"><%#: PostBody(Eval("content")) %></p>

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
                                                <a class="btn btn-sm text-secondary p-0 border-0 d-inline-flex align-items-center gap-2" href="#comments" aria-label="Read comments">
                                                    <i class="bi bi-chat community-icon" aria-hidden="true"></i>
                                                    <span class="small"><%#: Eval("commentCount") %></span>
                                                </a>
                                            </div>
                                        </footer>
                                    </div>
                                </div>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>

                <section id="comments" class="community-card rounded-3 p-3 p-sm-4">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h2 class="h6 mb-0 fw-bold">Comments (<asp:Literal ID="litCommentCount" runat="server" />)</h2>
                        <asp:DropDownList ID="ddlCommentSort" data-community-sort="Sort comments" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCommentSort_SelectedIndexChanged" CssClass="form-select form-select-sm community-sort" aria-label="Sort comments">
                            <asp:ListItem Value="newest">Latest</asp:ListItem>
                            <asp:ListItem Value="oldest">Oldest</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <asp:Repeater ID="rptComments" runat="server">
                        <ItemTemplate>
                            <article class="community-comment-row d-flex gap-3 py-3 border-bottom">
                                <%# CommunityAvatar(Eval("profilePicture"), Eval("displayName"), "Comment author") %>
                                <div class="community-content flex-grow-1">
                                    <header class="d-flex justify-content-between gap-2">
                                        <div class="small">
                                            <strong><%#: Eval("displayName") %></strong>
                                            <span class="text-secondary ms-1">@<%#: Eval("username") %> · <%#: ContentDate(Eval("updatedAt"), Eval("createdAt")) %></span>
                                        </div>
                                        <asp:PlaceHolder runat="server" Visible='<%# IsPostOwner(Eval("userID")) %>'>
                                            <details class="community-menu dropdown position-relative">
                                                <summary class="btn btn-sm p-0" aria-label="Comment options">
                                                    <i class="bi bi-three-dots community-icon" aria-hidden="true"></i>
                                                </summary>
                                                <div class="dropdown-menu d-block end-0 p-2 shadow-sm">
                                                    <asp:PlaceHolder runat="server" Visible='<%# CanEdit(Convert.ToDateTime(Eval("createdAt"))) %>'>
                                                        <button type="button" class="dropdown-item small rounded-2" data-bs-toggle="collapse" data-bs-target='<%# "#editComment" + Eval("postCommentID") %>' data-close-post-menu="true">
                                                            <i class="bi bi-pencil community-icon me-2" aria-hidden="true"></i>Edit comment
                                                        </button>
                                                    </asp:PlaceHolder>
                                                    <asp:LinkButton ID="btnDeleteComment" runat="server" CommandArgument='<%# Eval("postCommentID") %>' OnClick="btnDeleteComment_Click" CausesValidation="false" CssClass="dropdown-item small rounded-2" data-confirm="Delete this comment?">
                                                        <i class="bi bi-trash community-icon me-2" aria-hidden="true"></i>Delete comment
                                                    </asp:LinkButton>
                                                </div>
                                            </details>
                                        </asp:PlaceHolder>
                                    </header>

                                    <p class="community-copy small mt-1 mb-0"><%#: Eval("content") %></p>

                                    <div class="collapse mt-2" id='<%# "editComment" + Eval("postCommentID") %>'>
                                        <asp:Label runat="server" AssociatedControlID="txtEditComment" Text="Edit your comment" CssClass="visually-hidden" />
                                        <asp:TextBox ID="txtEditComment" runat="server" Text='<%# Eval("content") %>' TextMode="MultiLine" Rows="2" MaxLength="1000" CssClass="form-control form-control-sm mb-2" />
                                        <asp:Button ID="btnUpdateComment" runat="server" Text="Update" CssClass="btn btn-primary btn-sm" CommandArgument='<%# Eval("postCommentID") %>' OnClick="btnUpdateComment_Click" />
                                        <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-toggle="collapse" data-bs-target='<%# "#editComment" + Eval("postCommentID") %>'>Cancel</button>
                                    </div>
                                </div>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlNoComments" runat="server" CssClass="small text-secondary py-4 text-center">
                        No comments yet. Start the conversation.
                    </asp:Panel>

                    <asp:Panel ID="pnlCommentForm" runat="server" CssClass="community-comment-form d-flex align-items-start gap-3 pt-3">
                        <%= CommunityAvatar(CurrentUserAvatarUrl, CurrentDisplayName, "Your profile") %>
                        <div class="flex-grow-1">
                            <asp:Label runat="server" AssociatedControlID="txtComment" Text="Your comment" CssClass="visually-hidden" />
                            <asp:TextBox ID="txtComment" runat="server" CssClass="form-control form-control-sm" placeholder="Write a comment..." MaxLength="1000" />
                        </div>
                        <asp:Button ID="btnComment" runat="server" Text="Comment" CssClass="btn btn-sm btn-primary px-3" OnClick="btnComment_Click" />
                    </asp:Panel>

                    <asp:HyperLink ID="lnkCommentLogin" runat="server" CssClass="btn btn-sm btn-outline-secondary mt-3">Sign in to comment</asp:HyperLink>
                </section>

                <% if (!currentUserIsAdministrator) { %>
                <aside class="community-notice small rounded-2 px-3 py-2 mt-3">
                    <i class="bi bi-info-circle community-icon me-2" aria-hidden="true"></i>
                    Editing is available for 7 days after each post or comment is created.
                </aside>
                <% } %>
            </asp:Panel>
        </div>
    </main>

    <div class="community-floating-actions">
        <button id="communityBackToTop" type="button" class="btn btn-light community-back-to-top" aria-label="Back to top" title="Back to top" hidden>
            <i class="bi bi-arrow-up community-icon" aria-hidden="true"></i>
        </button>
    </div>

    <script src="../Scripts/community.js?v=20261024-40"></script>
</asp:Content>
