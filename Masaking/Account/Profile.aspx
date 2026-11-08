<%@ Page Title="My profile | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="Masaking.Account.Profile" %>

<asp:Content ID="ProfileContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/profile.css" rel="stylesheet" />

    <main id="profilePage" runat="server" class="profile-screen profile-overview" data-active-tab="overview">
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" role="status" />

        <header id="profileHero" runat="server" class="profile-hero">
            <div class="profile-person">
                <div class="profile-avatar profile-avatar-large">
                    <span>
                        <asp:Label ID="lblInitials" runat="server" /></span>
                    <asp:Image runat="server" ID="imgDecoration" AlternateText="Your profile photo" data-profile-avatar />
                </div>
                <div class="profile-person-copy">
                    <h1>
                        <asp:Label ID="lblProfileName" runat="server" /></h1>
                    <p class="profile-handle">
                        <asp:Label ID="lblProfileUsername" runat="server" />
                        <asp:PlaceHolder ID="phAdminSection" runat="server"><span class="profile-role">Administrator</span></asp:PlaceHolder>
                    </p>
                    <div class="profile-contact">
                        <span><i class="bi bi-geo-alt-fill" aria-hidden="true"></i>
                            <asp:Label ID="lblProfileCountry" runat="server" /></span>
                        <span id="profileEmailContact" runat="server"><i class="bi bi-envelope profile-icon" aria-hidden="true"></i>
                            <asp:Label ID="lblProfileEmail" runat="server" /></span>
                    </div>
                    <div id="profileActions" runat="server" class="profile-hero-actions">
                        <asp:HyperLink runat="server" ID="lnkEditProfile" NavigateUrl="EditProfile.aspx" class="profile-button profile-button-primary">Edit Profile</asp:HyperLink>
                        <asp:HyperLink runat="server" ID="lnkChangePassword" NavigateUrl="EditProfile.aspx#security" class="profile-button profile-button-outline">Change Password</asp:HyperLink>
                        <asp:LinkButton ID="btnSignOut" runat="server" class="profile-button profile-button-signout" CausesValidation="false" OnClick="btnSignOut_Click">
                            <i class="bi bi-box-arrow-right" aria-hidden="true"></i> Sign Out
                        </asp:LinkButton>
                    </div>
                </div>
            </div>

            <asp:PlaceHolder ID="phAdminSection2" runat="server">
                <div class="profile-statistics">
                    <asp:HyperLink runat="server" ID="lnkRecipesToReview" NavigateUrl="../Admin/ManageRecipes.aspx" class="profile-stat-card">
                        <span class="profile-stat-icon profile-stat-orange"><i class="bi bi-journal-check profile-icon" aria-hidden="true"></i></span>
                        <span>Recipes to Review</span>
                        <strong>
                            <asp:Label ID="lblPendingRecipes" runat="server" />
                        </strong>
                    </asp:HyperLink>
                    <asp:HyperLink runat="server" ID="lnkCommunityQueue" NavigateUrl="../Admin/ManageCommunity.aspx" class="profile-stat-card">
                        <span class="profile-stat-icon"><i class="bi bi-chat-square-text profile-icon" aria-hidden="true"></i></span>
                        <span>Community Queue</span>
                        <strong>
                            <asp:Label ID="lblPendingCommunity" runat="server" />
                        </strong>
                    </asp:HyperLink>
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phMemberSection" runat="server">
                <div class="profile-statistics">
                    <asp:HyperLink runat="server" ID="lnkRecipesUploaded" NavigateUrl="../Recipes/MyRecipes.aspx" class="profile-stat-card">
                        <span class="profile-stat-icon profile-stat-orange"><i class="bi bi-book profile-icon" aria-hidden="true"></i></span>
                        <span>Recipes Uploaded</span>
                        <strong>
                            <asp:Label ID="lblRecipeCount" runat="server" />
                        </strong>
                    </asp:HyperLink>
                    <asp:HyperLink runat="server" ID="lnkCommunityPosts" NavigateUrl="#profileContent" class="profile-stat-card" data-profile-tab="posts">
                        <span class="profile-stat-icon"><i class="bi bi-chat profile-icon" aria-hidden="true"></i></span>
                        <span>Community Posts</span>
                        <strong>
                            <asp:Label ID="lblPostCount" runat="server" />
                        </strong>
                    </asp:HyperLink>
                </div>
            </asp:PlaceHolder>
        </header>

        <asp:PlaceHolder ID="phAdminSection3" runat="server">
            <section class="profile-panel profile-admin-panel">
                <div class="profile-section-heading">
                    <div>
                        <p class="profile-admin-eyebrow">ADMINISTRATOR WORKSPACE</p>
                        <h2>Keep the kitchen running smoothly</h2>
                    </div>
                    <span class="profile-role">Administration</span>
                </div>
                <p class="profile-admin-intro">Review submissions, manage community content, and look after your members.</p>
                <div class="profile-admin-shortcuts">
                    <asp:HyperLink runat="server" ID="lnkManageRecipesReviewAndOrganiseRecipeSubmissions" NavigateUrl="../Admin/ManageRecipes.aspx" class="profile-admin-shortcut">
                        <span class="profile-stat-icon profile-stat-orange"><i class="bi bi-journal-check profile-icon" aria-hidden="true"></i></span>
                        <span><strong>Manage Recipes</strong><small>Review and organise recipe submissions.</small></span>
                        <i class="bi bi-arrow-right profile-icon" aria-hidden="true"></i>
                    </asp:HyperLink>
                    <asp:HyperLink runat="server" ID="lnkManageCommunity" NavigateUrl="../Admin/ManageCommunity.aspx" class="profile-admin-shortcut">
                        <span class="profile-stat-icon"><i class="bi bi-chat-square-text profile-icon" aria-hidden="true"></i></span>
                        <span><strong>Manage Community</strong><small>Review posts, comments, and recipe reviews.</small></span>
                        <i class="bi bi-arrow-right profile-icon" aria-hidden="true"></i>
                    </asp:HyperLink>
                    <asp:HyperLink runat="server" ID="lnkManageUsersViewAndManageMemberAccounts" NavigateUrl="../Admin/ManageUsers.aspx" class="profile-admin-shortcut">
                        <span class="profile-stat-icon"><i class="bi bi-people profile-icon" aria-hidden="true"></i></span>
                        <span><strong>Manage Users</strong><small>View and manage member accounts.</small></span>
                        <i class="bi bi-arrow-right profile-icon" aria-hidden="true"></i>
                    </asp:HyperLink>
                    <asp:HyperLink runat="server" ID="lnkManageChallenges" NavigateUrl="../Admin/ManageChallenges.aspx" class="profile-admin-shortcut">
                        <span class="profile-stat-icon profile-stat-orange"><i class="bi bi-trophy profile-icon" aria-hidden="true"></i></span>
                        <span><strong>Manage Challenges</strong><small>Manage the community’s cooking challenges.</small></span>
                        <i class="bi bi-arrow-right profile-icon" aria-hidden="true"></i>
                    </asp:HyperLink>
                </div>
            </section>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phMemberSection2" runat="server">
            <section id="profileContent" class="profile-panel profile-content-panel">
                <nav class="profile-tabs" aria-label="Profile sections">
                    <button runat="server" id="btnOverview" type="button" class="is-active" data-profile-tab="overview" aria-pressed="true">Overview</button>
                    <button runat="server" id="btnRecipes" type="button" data-profile-tab="recipes" aria-pressed="false">My Recipes</button>
                    <button runat="server" id="btnPosts" type="button" data-profile-tab="posts" aria-pressed="false">My Posts</button>
                    <button runat="server" id="btnComments" type="button" data-profile-tab="comments" aria-pressed="false">My Comments</button>
                </nav>

                <section class="profile-section" data-profile-recipes>
                    <div class="profile-section-heading">
                        <h2><%= IsPublicProfile ? "Recipes" : "My Recipes" %></h2>
                        <asp:HyperLink runat="server" ID="lnkViewAll" NavigateUrl="../Recipes/MyRecipes.aspx">
                            View all <i class="bi bi-arrow-left profile-icon profile-arrow-forward" aria-hidden="true"></i>
                        </asp:HyperLink>
                    </div>
                    <div class="profile-recipes">
                        <asp:Repeater ID="rptRecipes" runat="server" OnItemDataBound="rptRecipes_ItemDataBound">
                            <itemtemplate>
                                <asp:HyperLink runat="server" ID="lnkRecipe" class="profile-recipe">
                                    <div class="profile-recipe-picture">
                                        <span class="profile-picture-fallback"><i class="bi bi-book profile-icon" aria-hidden="true"></i></span>
                                        <asp:Image runat="server" ID="imgDecoration2" loading="lazy" data-profile-photo />
                                    </div>
                                    <div class="profile-recipe-copy">
                                        <h3>
                                            <asp:Label ID="lblTitle" runat="server" />
                                        </h3>
                                        <p>
                                            <span>
                                                <asp:Label ID="lblCuisine" runat="server" />
                                            </span>
                                            <span>·</span>
                                            <span>
                                                <asp:Label ID="lblPreparationTime" runat="server" />
                                                mins</span>
                                        </p>
                                        <span class="profile-saves">
                                            <i class="bi bi-heart profile-icon" aria-hidden="true"></i>
                                            <asp:Label ID="lblSaves" runat="server" />
                                        </span>
                                        <span class="profile-recipe-status">
                                            <asp:Label ID="lblStatus" runat="server" />
                                        </span>
                                    </div>
                                </asp:HyperLink>
                            </itemtemplate>
                        </asp:Repeater>
                    </div>

                    <asp:Panel ID="pnlNoRecipes" runat="server" class="profile-empty">
                        <i class="bi bi-book profile-icon" aria-hidden="true"></i>
                        <p><%= IsPublicProfile ? "No published recipes yet." : "Your recipe collection starts here." %></p>
                        <asp:HyperLink runat="server" ID="lnkShareYourFirstRecipe" NavigateUrl="../Recipes/SubmitRecipe.aspx" class="profile-button profile-button-outline">Share your first recipe</asp:HyperLink>
                    </asp:Panel>
                </section>

                <section class="profile-section profile-activity-section" data-profile-activity>
                    <div class="profile-section-heading">
                        <h2 data-activity-heading>Recent Community Activity</h2>
                        <asp:HyperLink runat="server" ID="lnkViewAll2" NavigateUrl="#profileContent" data-profile-tab="posts">
                            View all <i class="bi bi-arrow-left profile-icon profile-arrow-forward" aria-hidden="true"></i>
                        </asp:HyperLink>
                    </div>
                    <div class="profile-activity-list">
                        <asp:Repeater ID="rptActivity" runat="server" OnItemDataBound="rptActivity_ItemDataBound">
                            <itemtemplate>
                                <asp:HyperLink ID="lnkPost" runat="server" ClientIDMode="Static" class="profile-activity-row">
                                    <span class="profile-avatar profile-avatar-small">
                                        <span>
                                            <asp:Label ID="lblInitials2" runat="server" />
                                        </span>
                                        <asp:Image runat="server" ID="imgDecoration3" AlternateText="" GenerateEmptyAlternateText="true" data-profile-avatar loading="lazy" />
                                    </span>
                                    <span class="profile-activity-copy">
                                        <span>
                                            <asp:Label ID="lblKind" runat="server" />
                                        </span>
                                        <strong>
                                            <asp:Label ID="lblContent" runat="server" />
                                        </strong>
                                    </span>
                                    <time runat="server" id="postCard">
                                        <asp:Label ID="lblActivityAge" runat="server" />
                                    </time>
                                </asp:HyperLink>
                            </itemtemplate>
                        </asp:Repeater>
                    </div>

                    <p class="profile-empty" data-activity-empty hidden>
                        No community activity here yet.
                        <asp:HyperLink runat="server" ID="lnkJoinTheConversation" NavigateUrl="../Community/Community.aspx">Join the conversation</asp:HyperLink>.
                    </p>
                </section>
            </section>
        </asp:PlaceHolder>
    </main>

    <script src="../Scripts/profile.js"></script>
</asp:Content>
