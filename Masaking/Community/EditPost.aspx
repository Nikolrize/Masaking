<%@ Page Title="Share with the community | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EditPost.aspx.cs" Inherits="Masaking.Community.EditPost" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/community.css" rel="stylesheet" />
    <link href="../Content/css/community-responsive.css" rel="stylesheet" />

    <main data-community-page data-community-feed data-community-secondary class="position-relative community-composer">
        <div class="community-page-decor" aria-hidden="true">
            <asp:Image runat="server" ID="imgDecoration" class="community-botanical community-botanical-left" ImageUrl="~/Content/images/community/community-botanical.png" AlternateText="" GenerateEmptyAlternateText="true" decoding="async" />
            <asp:Image runat="server" ID="imgDecoration2" class="community-botanical community-botanical-right" ImageUrl="~/Content/images/community/community-botanical.png" AlternateText="" GenerateEmptyAlternateText="true" decoding="async" />
        </div>

        <header class="community-banner mb-3">
            <div class="community-banner-inner">
                <div class="community-banner-copy">
                    <asp:HyperLink runat="server" ID="lnkBackToCommunity" NavigateUrl="Community.aspx" class="community-back-link">
                        <i class="bi bi-arrow-left community-icon community-back-arrow" aria-hidden="true"></i>
                        <span>Back to Community</span>
                    </asp:HyperLink>
                    <h1 class="mb-2">Share with the community</h1>
                    <p class="small mb-0">Share your thoughts, photos or recipes with fellow food lovers.</p>
                </div>
            </div>
        </header>

        <div class="community-body py-3">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <asp:Panel ID="pnlComposer" runat="server" Visible="false" class="community-card rounded-3 p-3 p-sm-4" data-community-composer="true">
                <div class="d-flex align-items-center gap-3 mb-3">
                    <asp:Label ID="lblCurrentUserAvatar" runat="server" class="d-inline-flex flex-shrink-0" />
                    <div class="small">
                        <strong class="d-block"><asp:Label ID="lblCurrentDisplayName" runat="server" /></strong>
                        <span class="text-secondary">@<asp:Label ID="lblCurrentUsername" runat="server" /></span>
                    </div>
                </div>

                <asp:Label runat="server" AssociatedControlID="txtContent" Text="Your post" class="visually-hidden" />
                <asp:TextBox ID="txtContent" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="5" MaxLength="2000" class="form-control community-composer-text" placeholder="What's cooking or on your mind?" data-character-count="postCharacterCount" />
                
                <div class="community-composer-count d-flex justify-content-between small text-secondary mt-2 mb-4">
                    <span>Text-only posts are always welcome!</span>
                    <span id="postCharacterCount" aria-live="polite">0/2000</span>
                </div>

                <div class="row g-3 mb-4">
                    <div class="col-12 col-md-6">
                        <section class="community-attachment border rounded-2 p-3 h-100">
                            <asp:Label runat="server" AssociatedControlID="uploadImage" class="small fw-semibold d-block mb-2">Photo <span class="text-secondary fw-normal">(optional)</span></asp:Label>
                            
                            <div class="community-upload rounded-2 position-relative text-center p-4" data-photo-drop="true">
                                <i class="bi bi-camera community-icon mb-2" aria-hidden="true"></i>
                                <strong class="small d-block">Add photo (optional)</strong>
                                <span class="small text-secondary d-block mt-1">Drag and drop or click to upload<br />JPG, PNG or GIF, up to 5 MB</span>
                                <asp:FileUpload ID="uploadImage" ClientIDMode="Static" runat="server" class="community-upload-input" accept="image/jpeg,image/png,image/gif" aria-label="Upload a post photo" />
                            </div>

                            <div id="composerPhotoError" class="small text-danger mt-2" role="alert"></div>
                            
                            <asp:Image ID="imgExisting" runat="server" ClientIDMode="Static" Visible="false" class="community-composer-preview rounded-2 mt-3" AlternateText="Current post photo" />
                            <asp:Image runat="server" ClientIDMode="Static" ID="composerPhotoPreview" class="community-composer-preview rounded-2 mt-3 d-none" AlternateText="Selected photo preview" />
                            <asp:CheckBox ID="chkRemoveImage" ClientIDMode="Static" runat="server" Text=" Remove photo" class="small d-block mt-2" />
                        </section>
                    </div>

                    <div class="col-12 col-md-6">
                        <section class="community-attachment border rounded-2 p-3 h-100">
                            <asp:Label runat="server" AssociatedControlID="ddlRecipe" class="small fw-semibold d-block mb-2">Recipe <span class="text-secondary fw-normal">(optional)</span></asp:Label>
                            <asp:Label runat="server" ID="lblComposerRecipeSearch" AssociatedControlID="composerRecipeSearch" class="visually-hidden">Search recipes</asp:Label>
                            <asp:TextBox runat="server" ClientIDMode="Static" ID="composerRecipeSearch" TextMode="Search" class="form-control form-control-sm mb-2" placeholder="Search for a recipe..." />
                            <asp:DropDownList ID="ddlRecipe" ClientIDMode="Static" runat="server" class="form-select form-select-sm mb-3" />
                            
                            <div id="composerRecipeCard" class="community-recipe border rounded-2 p-2 d-none">
                                <div class="d-flex align-items-center gap-2">
                                    <asp:Image runat="server" ClientIDMode="Static" ID="composerRecipeImage" class="community-recipe-thumb rounded-2" AlternateText="Selected recipe" />
                                    <div class="flex-grow-1 small">
                                        <strong id="composerRecipeTitle" class="d-block"></strong>
                                        <span id="composerRecipeSummary" class="text-secondary"></span>
                                    </div>
                                    <button runat="server" ClientIDMode="Static" type="button" id="composerRemoveRecipe" class="btn-close flex-shrink-0" aria-label="Remove recipe"></button>
                                </div>
                            </div>
                            
                            <p id="composerRecipeHint" class="small text-secondary mb-0">Choose a recipe to share its ingredients and cooking steps.</p>
                        </section>
                    </div>
                </div>

                <footer class="community-composer-actions d-flex flex-wrap justify-content-between align-items-center gap-3">
                    <asp:PlaceHolder ID="phMemberSection" runat="server">
                        <p class="small text-secondary mb-0">
                            <i class="bi bi-lightbulb community-icon me-1" aria-hidden="true"></i> You can edit this post for up to 7 days after publishing.
                        </p>
                    </asp:PlaceHolder>

                    <div class="d-flex gap-2 ms-auto">
                        <asp:HyperLink ID="lnkCancel" runat="server" NavigateUrl="Community.aspx" class="btn btn-sm btn-outline-secondary px-4">Cancel</asp:HyperLink>
                        <asp:Button ID="btnPublish" runat="server" Text="Publish Post" OnClick="btnPublish_Click" class="btn btn-sm btn-primary px-4" />
                    </div>
                </footer>
            </asp:Panel>
        </div>
    </main>

    <div class="community-floating-actions">
        <button runat="server" ClientIDMode="Static" ID="communityBackToTop" type="button" class="btn btn-light community-back-to-top" aria-label="Back to top" title="Back to top" hidden>
            <i class="bi bi-arrow-up community-icon" aria-hidden="true"></i>
        </button>
    </div>

    <script src="../Scripts/community.js?v=20261104-1"></script>
    <script src="../Scripts/community-composer.js"></script>
</asp:Content>