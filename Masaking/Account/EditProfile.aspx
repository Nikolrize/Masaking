<%@ Page Title="Edit profile | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EditProfile.aspx.cs" Inherits="Masaking.Account.EditProfile" %>

<asp:Content ID="EditProfileContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/profile.css" rel="stylesheet" />

    <main class="profile-screen profile-edit-screen">
        <div class="profile-edit-topline">
            <asp:HyperLink runat="server" ID="lnkBackToProfile" NavigateUrl="Profile.aspx" class="profile-back">
                <i class="bi bi-arrow-left profile-icon" aria-hidden="true"></i> <span>Back to profile</span>
            </asp:HyperLink>
            <span>Make yourself at home.</span>
        </div>

        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" role="status" />

        <div class="profile-edit-grid">
            <section class="profile-panel profile-editor">
                <h1 class="profile-panel-heading"><i class="bi bi-pencil profile-icon" aria-hidden="true"></i> Edit Profile</h1>

                <div class="profile-photo-editor">
                    <div class="profile-avatar profile-avatar-medium">
                        <span><asp:Label ID="lblInitials" runat="server" /></span>
                        <asp:Image ID="imgAvatar" runat="server" ClientIDMode="Static" AlternateText="Your profile photo" data-profile-avatar="true" />
                    </div>
                    <div>
                        <asp:Label runat="server" ID="lblAvatar" AssociatedControlID="uploadAvatar" class="profile-button profile-button-outline profile-upload-label">
                            Change photo
                            <asp:FileUpload ID="uploadAvatar" runat="server" ClientIDMode="Static" accept="image/jpeg,image/png,image/gif" data-profile-upload="true" />
                        </asp:Label>
                        <span class="profile-photo-hint">JPG, PNG or GIF · Up to 5 MB</span>
                        <asp:Button ID="btnResetAvatar" runat="server" Text="Use default photo" class="profile-text-button" CausesValidation="false" formnovalidate="formnovalidate" OnClick="btnResetAvatar_Click" />
                        <span id="profilePhotoError" class="profile-field-error" role="status"></span>
                    </div>
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtUsername">Username <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtUsername" runat="server" ClientIDMode="Static" class="form-control" MaxLength="50" autocomplete="username" required="required" />
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtName">Display name <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtName" runat="server" ClientIDMode="Static" class="form-control" MaxLength="100" autocomplete="name" required="required" />
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtEmail">Email <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" ClientIDMode="Static" class="form-control" TextMode="Email" MaxLength="255" autocomplete="email" required="required" />
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtCountry">Country <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtCountry" runat="server" ClientIDMode="Static" class="form-control" MaxLength="100" autocomplete="country-name" list="profileCountries" required="required" />
                    <datalist id="profileCountries">
                        <option value="Malaysia" />
                        <option value="Singapore" />
                        <option value="Indonesia" />
                        <option value="Thailand" />
                        <option value="Brunei" />
                        <option value="Philippines" />
                        <option value="Vietnam" />
                        <option value="India" />
                        <option value="China" />
                        <option value="United Kingdom" />
                        <option value="Australia" />
                        <option value="United States" />
                    </datalist>
                </div>

                <div class="profile-editor-actions">
                    <asp:Button ID="btnSaveProfile" runat="server" Text="Save Changes" class="profile-button profile-button-primary" OnClick="btnSaveProfile_Click" />
                    <asp:HyperLink runat="server" ID="lnkCancel" NavigateUrl="Profile.aspx" class="profile-button profile-button-outline">Cancel</asp:HyperLink>
                </div>
            </section>

            <div class="profile-edit-rail">
                <section id="security" class="profile-panel profile-security">
                    <h2 class="profile-panel-heading"><i class="bi bi-lock profile-icon" aria-hidden="true"></i> Security</h2>

                    <div class="profile-field">
                        <asp:Label runat="server" AssociatedControlID="txtCurrentPassword">Current password <span class="profile-required">*</span></asp:Label>
                        <div class="profile-password">
                            <asp:TextBox ID="txtCurrentPassword" runat="server" ClientIDMode="Static" TextMode="Password" class="form-control" placeholder="Enter your current password" MaxLength="256" autocomplete="current-password" />
                            <button runat="server" id="btnShowCurrentPassword" type="button" data-profile-password="txtCurrentPassword" aria-label="Show current password" aria-pressed="false">
                                <i class="bi bi-eye profile-icon" aria-hidden="true"></i>
                            </button>
                        </div>
                    </div>

                    <div class="profile-field">
                        <asp:Label runat="server" AssociatedControlID="txtNewPassword">New password <span class="profile-required">*</span></asp:Label>
                        <div class="profile-password">
                            <asp:TextBox ID="txtNewPassword" runat="server" ClientIDMode="Static" TextMode="Password" class="form-control" placeholder="Enter new password" MaxLength="64" autocomplete="new-password" />
                            <button runat="server" id="btnShowNewPassword" type="button" data-profile-password="txtNewPassword" aria-label="Show new password" aria-pressed="false">
                                <i class="bi bi-eye profile-icon" aria-hidden="true"></i>
                            </button>
                        </div>
                    </div>

                    <div class="profile-field">
                        <asp:Label runat="server" AssociatedControlID="txtConfirmPassword">Confirm new password <span class="profile-required">*</span></asp:Label>
                        <div class="profile-password">
                            <asp:TextBox ID="txtConfirmPassword" runat="server" ClientIDMode="Static" TextMode="Password" class="form-control" placeholder="Confirm new password" MaxLength="64" autocomplete="new-password" />
                            <button runat="server" id="btnShowConfirmNewPassword" type="button" data-profile-password="txtConfirmPassword" aria-label="Show confirm new password" aria-pressed="false">
                                <i class="bi bi-eye profile-icon" aria-hidden="true"></i>
                            </button>
                        </div>
                    </div>

                    <p class="profile-password-help">Use 8 or more characters.</p>
                    <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" class="profile-button profile-button-primary" formnovalidate="formnovalidate" OnClick="btnChangePassword_Click" />
                </section>

                <section class="profile-panel profile-edit-activity" data-profile-rail>
                    <h2>My Community Activity</h2>

                    <nav class="profile-tabs" aria-label="Community activity">
                        <button runat="server" id="btnPost" type="button" class="is-active" data-rail-tab="post" aria-pressed="true">My Posts</button>
                        <button runat="server" id="btnComment" type="button" data-rail-tab="comment" aria-pressed="false">My Comments</button>
                    </nav>

                    <div class="profile-rail-summary">
                        <span data-rail-label>My Posts</span>
                        <asp:HyperLink runat="server" ID="lnkMyRecipes" NavigateUrl="../Recipes/MyRecipes.aspx">
                            My Recipes · <asp:Label ID="lblRecipeCount" runat="server" />
                        </asp:HyperLink>
                    </div>

                    <asp:Repeater ID="rptActivity" runat="server" OnItemDataBound="rptActivity_ItemDataBound">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkPost" runat="server" ClientIDMode="Static" class="profile-rail-row">
                                <span class="profile-rail-photo">
                                    <i class="bi bi-chat profile-icon" aria-hidden="true"></i>
                                    <asp:Image runat="server" ID="imgDecoration" AlternateText="" GenerateEmptyAlternateText="true" loading="lazy" data-profile-photo />
                                </span>
                                <span class="profile-rail-copy">
                                    <strong><asp:Label ID="lblContent" runat="server" /></strong>
                                    <span><asp:Label ID="lblRecipeTitle" runat="server" /></span>
                                    <time><asp:Label ID="lblActivityAge" runat="server" /></time>
                                </span>
                                <i class="bi bi-arrow-left profile-icon profile-arrow-forward" aria-hidden="true"></i>
                            </asp:HyperLink>
                        </ItemTemplate>
                    </asp:Repeater>

                    <p class="profile-empty" data-rail-empty hidden>
                        No activity here yet. <asp:HyperLink runat="server" ID="lnkVisitTheCommunity" NavigateUrl="../Community/Community.aspx">Visit the community</asp:HyperLink>.
                    </p>
                </section>
            </div>
        </div>
    </main>

    <script src="../Scripts/profile.js?v=5"></script>
</asp:Content>