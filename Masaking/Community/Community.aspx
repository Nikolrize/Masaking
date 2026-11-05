<%@ Page Title="Community Kitchen | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Community.aspx.cs" Inherits="Masaking.Community.Community" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/community.css" rel="stylesheet" />
    <link href="../Content/css/community-responsive.css" rel="stylesheet" />

    <main data-community-page data-community-feed class="position-relative">
        <header class="row align-items-center g-4 mb-3">
            <div class="col-md-6">
                <h1 class="mb-2">Community Kitchen</h1>
                <p class="lead mb-4">Share what you're cooking, ask questions, and exchange kitchen tips.</p>

                <div class="community-search-wrap w-100" role="search">
                    <asp:Label runat="server" ID="lblCommunitySearch" AssociatedControlID="communitySearch" class="visually-hidden">Search community posts</asp:Label>
                    <div class="input-group">
                        <asp:TextBox runat="server" ID="communitySearch" ClientIDMode="Static" TextMode="Search" class="form-control border-end-0" placeholder="Search community posts..." />
                        <span class="input-group-text border-start-0" aria-hidden="true"><i class="bi bi-search"></i></span>
                    </div>
                </div>
            </div>

            <div class="col-md-6">
                <div class="community-hero-art position-relative d-flex align-items-center justify-content-center" aria-hidden="true">
                    <asp:Image runat="server" ID="imgDecoration" class="community-hero-botanical community-hero-botanical-left" ImageUrl="~/Content/images/community/community-botanical.png" AlternateText="" GenerateEmptyAlternateText="true" decoding="async" />
                    <asp:Image runat="server" ID="imgDecoration3" class="community-hero-image img-fluid d-block mx-auto" ImageUrl="~/Content/images/community/three_plates_food.png?v=20261024-34" AlternateText="" GenerateEmptyAlternateText="true" />
                    <asp:Image runat="server" ID="imgDecoration2" class="community-hero-botanical community-hero-botanical-right" ImageUrl="~/Content/images/community/community-botanical.png" AlternateText="" GenerateEmptyAlternateText="true" decoding="async" />
                </div>
            </div>

            <asp:Panel ID="pnlSort" runat="server" class="community-filter-row col-12">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2">
                    <nav class="d-flex flex-wrap gap-2" aria-label="Filter community posts">
                        <button runat="server" id="btnAll" type="button" class="btn btn-sm btn-primary rounded-pill px-3" data-community-filter="all" aria-pressed="true">All Posts</button>
                        <button runat="server" id="btnRecipes" type="button" class="btn btn-sm btn-outline-secondary rounded-pill px-3" data-community-filter="recipes" aria-pressed="false">With Recipes</button>
                        <button runat="server" id="btnDiscussions" type="button" class="btn btn-sm btn-outline-secondary rounded-pill px-3" data-community-filter="discussions" aria-pressed="false">Discussions</button>
                    </nav>
                    <div>
                        <asp:Label runat="server" AssociatedControlID="ddlSort" class="visually-hidden" Text="Sort posts" />
                        <asp:DropDownList ID="ddlSort" runat="server" data-community-sort="Sort posts" class="form-select form-select-sm rounded-pill" AutoPostBack="true" OnSelectedIndexChanged="ddlSort_SelectedIndexChanged">
                            <asp:ListItem Value="newest">Latest</asp:ListItem>
                            <asp:ListItem Value="popular">Popular</asp:ListItem>
                            <asp:ListItem Value="oldest">Oldest</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
            </asp:Panel>
        </header>

        <div class="community-body py-3">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" class="d-block mb-3" />

            <!-- Prompt for unregistered users -->
            <asp:Panel ID="pnlGuest" runat="server" Visible="false" class="card community-card rounded-3 mb-4">
                <div class="card-body d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 p-4">
                    <div>
                        <h2 class="h5 mb-1 fw-bold text-dark">Join the conversation</h2>
                        <p class="text-secondary mb-0">Sign in to share cooking notes, like posts, and comment.</p>
                    </div>
                    <asp:HyperLink runat="server" ID="lnkSignIn" class="btn btn-primary rounded-pill px-4 py-2 flex-shrink-0 fw-semibold" NavigateUrl="../Account/Login.aspx">Sign in</asp:HyperLink>
                </div>
            </asp:Panel>

            <!-- Individual, softly bordered post cards match the community-feed reference. -->
            <asp:Repeater ID="rptPosts" runat="server" OnItemCommand="rptPosts_ItemCommand" OnItemDataBound="rptPosts_ItemDataBound">
                <ItemTemplate>
                    <article id="postCard" runat="server" class="card community-card rounded-3 mb-3 community-feed-post">
                        <div class="card-body p-3 p-sm-4">
                            <div class="community-post-layout d-flex gap-3">
                                <asp:Label ID="lblAuthorAvatar" runat="server" class="community-post-avatar d-inline-flex flex-shrink-0" />

                                <div class="community-content flex-grow-1">
                                    <header class="community-post-header d-flex justify-content-between align-items-start gap-2 mb-2">
                                        <div class="community-author d-flex flex-wrap align-items-baseline gap-2">
                                            <strong class="small">
                                                <asp:Label ID="lblDisplayName" runat="server" />
                                            </strong>
                                            <span class="small text-secondary">@<asp:Label ID="lblUsername" runat="server" /></span>
                                            <span class="small text-secondary">·
                                                <asp:Label ID="lblContentDate" runat="server" />
                                            </span>
                                        </div>

                                        <asp:PlaceHolder ID="phOwnerActions" runat="server">
                                            <details class="community-menu dropdown position-relative flex-shrink-0">
                                                <summary class="btn btn-sm btn-light bg-transparent border-0 rounded-2 p-1 text-secondary" aria-label="Post options">
                                                    <i class="bi bi-three-dots community-icon" aria-hidden="true"></i>
                                                </summary>
                                                <div class="dropdown-menu d-block end-0 rounded-2 shadow-sm p-2">
                                                    <asp:HyperLink ID="lnkEditPost" runat="server" class="dropdown-item rounded-2 small d-flex align-items-center gap-2">
                                                        <i class="bi bi-pencil community-icon" aria-hidden="true"></i>Edit Post
                                                    </asp:HyperLink>

                                                    <asp:Panel ID="pnlDeletePost" runat="server">
                                                        <div class="community-delete-action">
                                                            <i class="bi bi-trash community-icon text-secondary" aria-hidden="true"></i>
                                                            <asp:Button ID="btnDeletePost" runat="server" class="dropdown-item small rounded-2" CausesValidation="false" OnClick="btnDeletePost_Click" data-confirm="Delete or remove this post from the community?" />
                                                        </div>
                                                    </asp:Panel>

                                                    <asp:PlaceHolder ID="phMemberSection2" runat="server">
                                                        <p class="small text-dark px-3 pt-2 mb-1">You can edit posts for 7 days after publishing.</p>
                                                    </asp:PlaceHolder>
                                                </div>
                                            </details>
                                        </asp:PlaceHolder>
                                    </header>

                                    <p class="community-copy mb-3"><asp:Label ID="lblContent" runat="server" /></p>

                                    <div class="community-post-media">
                                        <asp:Image ID="imgPost" runat="server" class="community-post-photo img-fluid rounded-2 mb-3" AlternateText="Shared cooking photo" />
                                        <asp:HyperLink ID="lnkRecipe" runat="server" class="community-recipe d-flex align-items-center gap-3 border rounded-2 p-2 mb-3 text-decoration-none">
                                            <asp:Image ID="imgAttachedRecipe" runat="server" class="community-recipe-thumb rounded-2" AlternateText="Attached recipe" />
                                            <span>
                                                <strong class="community-recipe-title small d-block">
                                                    <asp:Label ID="lblTitle" runat="server" />
                                                </strong>
                                                <span class="small text-secondary">
                                                    <asp:Label ID="lblRecipeSummary" runat="server" />
                                                </span>
                                                <span class="d-block small mt-1">View recipe →</span>
                                            </span>
                                        </asp:HyperLink>
                                    </div>

                                    <asp:UpdatePanel ID="upComments" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                        <contenttemplate>
                                            <footer class="d-flex justify-content-between align-items-center gap-2">
                                                <div class="d-flex align-items-center gap-3">
                                                    <asp:UpdatePanel ID="upLike" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                        <contenttemplate>
                                                            <asp:LinkButton ID="btnLike" runat="server" OnClientClick="return false;" CausesValidation="false" class="community-like btn btn-sm p-0 border-0 d-inline-flex align-items-center gap-2 text-decoration-none" />
                                                            <asp:Label ID="lblLikeError" runat="server" EnableViewState="false" class="small text-danger ms-2" role="status" data-like-error="true" />
                                                        </contenttemplate>
                                                    </asp:UpdatePanel>

                                                    <asp:LinkButton ID="lnkPost" runat="server" role="button" CommandName="toggleComments" CausesValidation="false" Text="0" class="bi bi-chat btn btn-sm text-secondary p-0 border-0 d-inline-flex align-items-center gap-2" aria-label="Read comments" />
                                                </div>

                                            </footer>

                                            <asp:Panel ID="pnlComments" runat="server" Visible="false" class="community-comments border-top mt-3 pt-3" role="region" aria-label="Post comments">
                                                <asp:Label ID="lblCommentMessage" runat="server" EnableViewState="false" role="status" />

                                                <div class="community-thread-heading d-flex justify-content-between align-items-center mb-2">
                                                    <h2 class="h6 mb-0 fw-bold">Comments (<asp:Label ID="lblThreadCommentCount" runat="server" />)</h2>
                                                    <asp:DropDownList ID="ddlCommentSort" runat="server" data-community-sort="Sort comments" data-community-comment-sort="true" class="form-select form-select-sm community-sort" aria-label="Sort comments">
                                                        <asp:ListItem Value="newest">Latest</asp:ListItem>
                                                        <asp:ListItem Value="oldest">Oldest</asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>

                                                <asp:Repeater ID="rptComments" runat="server" OnItemDataBound="rptComments_ItemDataBound">
                                                    <itemtemplate>
                                                        <article id="commentRow" runat="server" class="community-comment-row d-flex gap-3 py-3 border-bottom">
                                                            <asp:Label ID="lblAuthorAvatar2" runat="server" class="d-inline-flex flex-shrink-0" />
                                                            <div class="community-content flex-grow-1">
                                                                <header class="d-flex justify-content-between gap-2">
                                                                    <div class="small">
                                                                        <strong>
                                                                            <asp:Label ID="lblDisplayName2" runat="server" />
                                                                        </strong>
                                                                        <span class="community-comment-meta text-secondary ms-1">
                                                                            <span class="d-none d-md-inline">@<asp:Label ID="lblUsername2" runat="server" />
                                                                                · </span>
                                                                            <asp:Label ID="lblContentDate2" runat="server" />
                                                                        </span>
                                                                    </div>

                                                                    <asp:PlaceHolder ID="phOwnerActions3" runat="server">
                                                                        <details class="community-menu dropdown position-relative">
                                                                            <summary class="btn btn-sm p-0" aria-label="Comment options">
                                                                                <i class="bi bi-three-dots community-icon" aria-hidden="true"></i>
                                                                            </summary>
                                                                            <div class="dropdown-menu d-block end-0 p-2 shadow-sm">
                                                                                <asp:PlaceHolder ID="phOwnerActions4" runat="server">
                                                                                    <button id="btnToggleSection" runat="server" type="button" class="dropdown-item small rounded-2" data-bs-toggle="collapse" data-close-post-menu="true">
                                                                                        <i class="bi bi-pencil community-icon me-2" aria-hidden="true"></i>Edit comment
                                                                                    </button>
                                                                                </asp:PlaceHolder>

                                                                                <asp:LinkButton ID="btnDeleteComment" runat="server" OnClick="btnDeleteComment_Click" CausesValidation="false" class="dropdown-item small rounded-2" data-confirm="Delete this comment?">
                                                                                    <i class="bi bi-trash community-icon me-2" aria-hidden="true"></i>Delete comment
                                                                                </asp:LinkButton>
                                                                            </div>
                                                                        </details>
                                                                    </asp:PlaceHolder>
                                                                </header>

                                                                <p class="community-copy small mt-1 mb-0"><asp:Label ID="lblContent3" runat="server" /></p>

                                                                <asp:Panel ID="pnlEditComment" runat="server" class="collapse mt-2">
                                                                    <asp:Label runat="server" AssociatedControlID="txtEditComment" Text="Edit your comment" class="visually-hidden" />
                                                                    <asp:TextBox ID="txtEditComment" runat="server" TextMode="MultiLine" Rows="2" MaxLength="1000" class="form-control form-control-sm mb-2" />
                                                                    <asp:Button ID="btnUpdateComment" runat="server" Text="Update" class="btn btn-primary btn-sm" OnClick="btnUpdateComment_Click" />
                                                                    <button id="btnToggleSection2" runat="server" type="button" class="btn btn-outline-secondary btn-sm" data-bs-toggle="collapse">Cancel</button>
                                                                </asp:Panel>
                                                            </div>
                                                        </article>
                                                    </itemtemplate>
                                                </asp:Repeater>

                                                <asp:Panel ID="pnlNoComments" runat="server" class="small text-secondary py-4 text-center">
                                                    No comments yet. Start the conversation.
                                                </asp:Panel>

                                                <asp:Panel ID="pnlCommentForm" runat="server" class="community-comment-form d-flex align-items-start gap-3 pt-3" DefaultButton="btnComment">
                                                    <asp:Label ID="lblCurrentUserAvatar" runat="server" class="d-inline-flex flex-shrink-0" />
                                                    <div class="flex-grow-1">
                                                        <asp:Label runat="server" AssociatedControlID="txtComment" Text="Your comment" class="visually-hidden" />
                                                        <asp:TextBox ID="txtComment" runat="server" class="form-control form-control-sm" placeholder="Write a comment..." MaxLength="1000" />
                                                    </div>
                                                    <asp:Button ID="btnComment" runat="server" Text="Comment" class="btn btn-sm btn-primary px-3" OnClick="btnComment_Click" />
                                                </asp:Panel>

                                                <asp:HyperLink ID="lnkCommentLogin" runat="server" class="btn btn-sm btn-outline-secondary mt-3">Sign in to comment</asp:HyperLink>
                                            </asp:Panel>
                                        </contenttemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                        </div>
                    </article>
                </ItemTemplate>
            </asp:Repeater>

            <p id="communityFilterEmpty" class="community-card rounded-3 p-4 text-center text-secondary d-none" role="status">
                No posts match this search. Try another word or filter.
            </p>

            <asp:PlaceHolder ID="phMemberSection" runat="server">
                <aside class="community-notice rounded-2 d-flex align-items-center gap-2 small text-secondary px-3 py-2 mt-3">
                    <i class="bi bi-info-circle community-icon" aria-hidden="true"></i>You can edit your post for up to 7 days after publishing.
                </aside>
            </asp:PlaceHolder>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" class="card border-0 shadow-sm rounded-4 p-5 text-center bg-white mt-4">
                <div class="text-secondary mb-3 d-flex justify-content-center">
                    <i class="bi bi-book community-icon community-empty-icon" aria-hidden="true"></i>
                </div>
                <h2 class="h4 fw-bold text-dark">A fresh page in the community cookbook</h2>
                <p class="text-secondary mb-0">There are no approved community posts yet. Be the first to share a cooking moment.</p>
            </asp:Panel>

            <!-- Deleted Posts Recovery Panel -->
            <asp:Panel ID="pnlDeletedPosts" runat="server" Visible="false" class="accordion mt-5">
                <div class="accordion-item border-0 rounded-4 overflow-hidden shadow-sm bg-white">
                    <h2 class="accordion-header" id="myDeletedPostsHeading">
                        <button id="btnToggleSection" runat="server" class="accordion-button collapsed fw-semibold text-dark shadow-none" type="button" data-bs-toggle="collapse" data-bs-target="#myDeletedPostsBody" aria-expanded="false" aria-controls="myDeletedPostsBody">Recently deleted posts</button>
                    </h2>
                    <div id="myDeletedPostsBody" class="accordion-collapse collapse" aria-labelledby="myDeletedPostsHeading">
                        <div class="accordion-body">
                            <p class="small text-secondary mb-3">Only posts you deleted yourself appear here. Posts removed by a moderator cannot be restored from this list.</p>

                            <asp:Repeater ID="rptDeletedPosts" runat="server" OnItemCommand="rptDeletedPosts_ItemCommand" OnItemDataBound="rptDeletedPosts_ItemDataBound">
                                <itemtemplate>
                                    <div class="d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-3 border-top py-3">
                                        <div class="flex-grow-1">
                                            <p class="community-copy mb-1 text-dark small"><asp:Label ID="lblContent2" runat="server" /></p>
                                            <span class="small text-secondary">Deleted ·
                                                <asp:Label ID="lblContentDate2" runat="server" />
                                            </span>
                                        </div>
                                        <asp:Button ID="btnRestorePost" runat="server" Text="Restore" class="btn btn-sm btn-outline-success rounded-pill px-3 flex-shrink-0 fw-semibold" CommandName="restorePost" />
                                    </div>
                                </itemtemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </main>

    <div class="community-floating-actions">
        <button id="communityBackToTop" runat="server" clientidmode="Static" type="button" class="btn btn-light community-back-to-top" aria-label="Back to top" title="Back to top" hidden>
            <i class="bi bi-arrow-up community-icon" aria-hidden="true"></i>
        </button>

        <asp:Panel ID="pnlAddPost" runat="server">
            <asp:HyperLink ID="lnkCreatePost" runat="server" NavigateUrl="EditPost.aspx" class="btn btn-primary rounded-pill px-4 py-3 shadow community-create-post" aria-label="Create a community post">
                <i class="bi bi-plus-lg community-icon" aria-hidden="true"></i>
                <span class="community-create-label">Create Post</span>
            </asp:HyperLink>
        </asp:Panel>
    </div>

    <script src="../Scripts/form-helpers.js"></script>
    <script src="../Scripts/community.js"></script>
</asp:Content>
