<%@ Page Title="Edit profile | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EditProfile.aspx.cs" Inherits="Masaking.Account.EditProfile" %>

<asp:Content ID="EditProfileContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/css/profile.css?v=9") %>" rel="stylesheet" />

    <main class="profile-screen profile-edit-screen">
        <div class="profile-edit-topline">
            <a href="Profile.aspx" class="profile-back"><%= Icon("arrow-left") %> <span>Back to profile</span></a>
            <span>Make yourself at home.</span>
        </div>

        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" role="status" />

        <div class="profile-edit-grid">
            <section class="profile-panel profile-editor">
                <h1 class="profile-panel-heading"><%= Icon("pencil") %> Edit Profile</h1>

                <div class="profile-photo-editor">
                    <div class="profile-avatar profile-avatar-medium">
                        <span><%= H(Initials) %></span>
                        <asp:Image ID="imgAvatar" runat="server" ClientIDMode="Static" AlternateText="Your profile photo" data-profile-avatar="true" />
                    </div>
                    <div>
                        <label for="uploadAvatar" class="profile-button profile-button-outline profile-upload-label">
                            Change photo
                            <asp:FileUpload ID="uploadAvatar" runat="server" ClientIDMode="Static" accept="image/jpeg,image/png,image/gif" data-profile-upload="true" />
                        </label>
                        <span class="profile-photo-hint">JPG, PNG or GIF · Up to 5 MB</span>
                        <asp:Button ID="btnResetAvatar" runat="server" Text="Use default photo" CssClass="profile-text-button" CausesValidation="false" formnovalidate="formnovalidate" OnClick="btnResetAvatar_Click" />
                        <span id="profilePhotoError" class="profile-field-error" role="status"></span>
                    </div>
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtUsername">Username <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtUsername" runat="server" ClientIDMode="Static" CssClass="form-control" MaxLength="50" autocomplete="username" required="required" />
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtName">Display name <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtName" runat="server" ClientIDMode="Static" CssClass="form-control" MaxLength="100" autocomplete="name" required="required" />
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtEmail">Email <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" ClientIDMode="Static" CssClass="form-control" TextMode="Email" MaxLength="255" autocomplete="email" required="required" />
                </div>

                <div class="profile-field">
                    <asp:Label runat="server" AssociatedControlID="txtCountry">Country <span class="profile-required">*</span></asp:Label>
                    <asp:TextBox ID="txtCountry" runat="server" ClientIDMode="Static" CssClass="form-control" MaxLength="100" autocomplete="country-name" list="profileCountries" required="required" />
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
                    <asp:Button ID="btnSaveProfile" runat="server" Text="Save Changes" CssClass="profile-button profile-button-primary" OnClick="btnSaveProfile_Click" />
                    <a href="Profile.aspx" class="profile-button profile-button-outline">Cancel</a>
                </div>
            </section>

            <div class="profile-edit-rail">
                <section id="security" class="profile-panel profile-security">
                    <h2 class="profile-panel-heading"><%= ContactIcon("lock") %> Security</h2>

                    <div class="profile-field">
                        <asp:Label runat="server" AssociatedControlID="txtCurrentPassword">Current password <span class="profile-required">*</span></asp:Label>
                        <div class="profile-password">
                            <asp:TextBox ID="txtCurrentPassword" runat="server" ClientIDMode="Static" TextMode="Password" CssClass="form-control" placeholder="Enter your current password" MaxLength="256" autocomplete="current-password" />
                            <button type="button" data-profile-password="txtCurrentPassword" aria-label="Show current password" aria-pressed="false"><%= ContactIcon("eye") %></button>
                        </div>
                    </div>

                    <div class="profile-field">
                        <asp:Label runat="server" AssociatedControlID="txtNewPassword">New password <span class="profile-required">*</span></asp:Label>
                        <div class="profile-password">
                            <asp:TextBox ID="txtNewPassword" runat="server" ClientIDMode="Static" TextMode="Password" CssClass="form-control" placeholder="Enter new password" MaxLength="64" autocomplete="new-password" />
                            <button type="button" data-profile-password="txtNewPassword" aria-label="Show new password" aria-pressed="false"><%= ContactIcon("eye") %></button>
                        </div>
                    </div>

                    <div class="profile-field">
                        <asp:Label runat="server" AssociatedControlID="txtConfirmPassword">Confirm new password <span class="profile-required">*</span></asp:Label>
                        <div class="profile-password">
                            <asp:TextBox ID="txtConfirmPassword" runat="server" ClientIDMode="Static" TextMode="Password" CssClass="form-control" placeholder="Confirm new password" MaxLength="64" autocomplete="new-password" />
                            <button type="button" data-profile-password="txtConfirmPassword" aria-label="Show confirm new password" aria-pressed="false"><%= ContactIcon("eye") %></button>
                        </div>
                    </div>

                    <p class="profile-password-help">Use 8 or more characters.</p>
                    <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="profile-button profile-button-primary" formnovalidate="formnovalidate" OnClick="btnChangePassword_Click" />
                </section>

                <section class="profile-panel profile-edit-activity" data-profile-rail>
                    <h2>My Community Activity</h2>
                    <nav class="profile-tabs" aria-label="Community activity">
                        <button type="button" class="is-active" data-rail-tab="post" aria-pressed="true">My Posts</button>
                        <button type="button" data-rail-tab="comment" aria-pressed="false">My Comments</button>
                    </nav>

                    <div class="profile-rail-summary">
                        <span data-rail-label>My Posts</span>
                        <a href="../Recipes/MyRecipes.aspx">My Recipes · <%= RecipeCount %></a>
                    </div>

                    <asp:Repeater ID="rptActivity" runat="server">
                        <ItemTemplate>
                            <a class="profile-rail-row" href='<%# "../Community/Post.aspx?id=" + Eval("postID") + (Convert.ToString(Eval("kind")) == "comment" ? "#comments" : "") %>' data-rail-kind='<%#: Eval("kind") %>'>
                                <span class="profile-rail-photo">
                                    <%# Icon("chat") %>
                                    <img src='<%#: Picture(Eval("image")) %>' alt="" loading="lazy" data-profile-photo />
                                </span>
                                <span class="profile-rail-copy">
                                    <strong><%#: Excerpt(Eval("content")) %></strong>
                                    <span><%#: Convert.ToString(Eval("recipeTitle")) == "" ? "Community discussion" : Eval("recipeTitle") %></span>
                                    <time><%#: Age(Eval("createdAt")) %></time>
                                </span>
                                <%# Icon("arrow-left", "profile-arrow-forward") %>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>

                    <p class="profile-empty" data-rail-empty hidden>No activity here yet. <a href="../Community/Community.aspx">Visit the community</a>.</p>
                </section>
            </div>
        </div>
    </main>

    <script src="<%= ResolveUrl("~/Scripts/profile.js?v=5") %>"></script>
</asp:Content>
