<%@ Page Title="Share with the community | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EditPost.aspx.cs" Inherits="Masaking.Community.EditPost" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/community.css?v=20261024-40" rel="stylesheet" />
    <link href="../Content/css/community-responsive.css?v=2" rel="stylesheet" />

    <main class="community-view community-composer community-feed-view community-secondary-view">
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
                    <h1 class="display-6 fw-bold mb-2">Share with the community</h1>
                    <p class="small mb-0">Share your thoughts, photos or recipes with fellow food lovers.</p>
                </div>
            </div>
        </header>

        <div class="community-body container">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <asp:Panel ID="pnlComposer" runat="server" Visible="false" CssClass="community-card rounded-3 p-3 p-sm-4" data-community-composer="true">
                <div class="d-flex align-items-center gap-3 mb-3">
                    <%= CommunityAvatar(CurrentUserAvatarUrl, CurrentDisplayName, "Your profile") %>
                    <div class="small">
                        <strong class="d-block"><%= Server.HtmlEncode(CurrentDisplayName) %></strong>
                        <span class="text-secondary">@<%= Server.HtmlEncode(CurrentUsername) %></span>
                    </div>
                </div>

                <asp:Label runat="server" AssociatedControlID="txtContent" Text="Your post" CssClass="visually-hidden" />
                <asp:TextBox ID="txtContent" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="5" MaxLength="2000" CssClass="form-control community-composer-text" placeholder="What's cooking or on your mind?" data-character-count="postCharacterCount" />

                <div class="community-composer-count d-flex justify-content-between small text-secondary mt-2 mb-4">
                    <span>Text-only posts are always welcome!</span>
                    <span id="postCharacterCount" aria-live="polite">0/2000</span>
                </div>

                <div class="row g-3 mb-4">
                    <div class="col-12 col-md-6">
                        <section class="community-attachment border rounded-2 p-3 h-100">
                            <asp:Label runat="server" AssociatedControlID="uploadImage" CssClass="small fw-semibold d-block mb-2">Photo <span class="text-secondary fw-normal">(optional)</span></asp:Label>
                            <div class="community-upload rounded-2 position-relative text-center p-4" data-photo-drop="true">
                                <i class="bi bi-camera community-icon mb-2" aria-hidden="true"></i>
                                <strong class="small d-block">Add photo (optional)</strong>
                                <span class="small text-secondary d-block mt-1">Drag and drop or click to upload<br />JPG, PNG or GIF, up to 5 MB</span>
                                <asp:FileUpload ID="uploadImage" ClientIDMode="Static" runat="server" CssClass="community-upload-input" accept="image/jpeg,image/png,image/gif" aria-label="Upload a post photo" />
                            </div>
                            <div id="composerPhotoError" class="small text-danger mt-2" role="alert"></div>
                            <asp:Image ID="imgExisting" runat="server" ClientIDMode="Static" Visible="false" CssClass="community-composer-preview rounded-2 mt-3" AlternateText="Current post photo" />
                            <img id="composerPhotoPreview" class="community-composer-preview rounded-2 mt-3 d-none" alt="Selected photo preview" />
                            <asp:CheckBox ID="chkRemoveImage" ClientIDMode="Static" runat="server" Text=" Remove photo" CssClass="small d-block mt-2" />
                        </section>
                    </div>

                    <div class="col-12 col-md-6">
                        <section class="community-attachment border rounded-2 p-3 h-100">
                            <asp:Label runat="server" AssociatedControlID="ddlRecipe" CssClass="small fw-semibold d-block mb-2">Recipe <span class="text-secondary fw-normal">(optional)</span></asp:Label>
                            <label for="composerRecipeSearch" class="visually-hidden">Search recipes</label>
                            <input id="composerRecipeSearch" type="search" class="form-control form-control-sm mb-2" placeholder="Search for a recipe..." />
                            <asp:DropDownList ID="ddlRecipe" ClientIDMode="Static" runat="server" CssClass="form-select form-select-sm mb-3" />
                            
                            <div id="composerRecipeCard" class="community-recipe border rounded-2 p-2 d-none">
                                <div class="d-flex align-items-center gap-2">
                                    <img id="composerRecipeImage" class="community-recipe-thumb rounded-2" alt="Selected recipe" />
                                    <div class="flex-grow-1 small">
                                        <strong id="composerRecipeTitle" class="d-block"></strong>
                                        <span id="composerRecipeSummary" class="text-secondary"></span>
                                    </div>
                                    <button type="button" id="composerRemoveRecipe" class="btn-close flex-shrink-0" aria-label="Remove recipe"></button>
                                </div>
                            </div>
                            <p id="composerRecipeHint" class="small text-secondary mb-0">Choose a recipe to share its ingredients and cooking steps.</p>
                        </section>
                    </div>
                </div>

                <footer class="community-composer-actions d-flex flex-wrap justify-content-between align-items-center gap-3">
                    <% if (!currentUserIsAdministrator) { %>
                    <p class="small text-secondary mb-0">
                        <i class="bi bi-lightbulb community-icon me-1" aria-hidden="true"></i> You can edit this post for up to 7 days after publishing.
                    </p>
                    <% } %>
                    <div class="d-flex gap-2 ms-auto">
                        <asp:HyperLink ID="lnkCancel" runat="server" NavigateUrl="Community.aspx" CssClass="btn btn-sm btn-outline-secondary px-4">Cancel</asp:HyperLink>
                        <asp:Button ID="btnPublish" runat="server" Text="Publish Post" OnClick="btnPublish_Click" CssClass="btn btn-sm btn-primary px-4" />
                    </div>
                </footer>
            </asp:Panel>
        </div>
    </main>

    <div class="community-floating-actions">
        <button id="communityBackToTop" type="button" class="btn btn-light community-back-to-top" aria-label="Back to top" title="Back to top" hidden>
            <i class="bi bi-arrow-up community-icon" aria-hidden="true"></i>
        </button>
    </div>

    <script src="../Scripts/community.js?v=20261024-40"></script>
    <script src="../Scripts/community-composer.js"></script>
</asp:Content>
