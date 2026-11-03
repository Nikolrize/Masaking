<%@ Page Title="My profile | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="Masaking.Account.Profile" %>

<asp:Content ID="ProfileContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/css/profile.css?v=10") %>" rel="stylesheet" />

    <main class="profile-screen profile-overview" <%= IsAdministrator ? "" : "data-profile-screen" %> data-active-tab="overview">
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" role="status" />

        <header class="profile-hero">
            <div class="profile-person">
                <div class="profile-avatar profile-avatar-large">
                    <span><%= H(Initials) %></span>
                    <img src="<%= H(AvatarDisplayUrl) %>" alt="Your profile photo" data-profile-avatar />
                </div>
                <div class="profile-person-copy">
                    <h1><%= H(ProfileName) %></h1>
                    <p class="profile-handle">
                        @<%= H(ProfileUsername) %>
                        <% if (IsAdministrator) { %> <span class="profile-role">Administrator</span><% } %>
                    </p>
                    <div class="profile-contact">
                        <span><i class="bi bi-geo-alt-fill" aria-hidden="true"></i><%= H(ProfileCountry) %></span>
                        <span><%= ContactIcon("envelope") %><%= H(ProfileEmail) %></span>
                    </div>
                    <div class="profile-hero-actions">
                        <a href="EditProfile.aspx" class="profile-button profile-button-primary">Edit Profile</a>
                        <a href="EditProfile.aspx#security" class="profile-button profile-button-outline">Change Password</a>
                        <asp:LinkButton ID="btnSignOut" runat="server" CssClass="profile-button profile-button-signout" CausesValidation="false" OnClick="btnSignOut_Click">
                            <i class="bi bi-box-arrow-right" aria-hidden="true"></i> Sign Out
                        </asp:LinkButton>
                    </div>
                </div>
            </div>

            <% if (IsAdministrator) { %>
            <div class="profile-statistics">
                <a href="../Admin/ManageRecipes.aspx" class="profile-stat-card">
                    <span class="profile-stat-icon profile-stat-orange"><%= Icon("journal-check") %></span>
                    <span>Recipes to Review</span>
                    <strong><%= PendingRecipes %></strong>
                </a>
                <a href="../Admin/ManageCommunity.aspx" class="profile-stat-card">
                    <span class="profile-stat-icon"><%= Icon("chat-square-text") %></span>
                    <span>Community Queue</span>
                    <strong><%= PendingCommunity %></strong>
                </a>
            </div>
            <% } else { %>
            <div class="profile-statistics">
                <a href="../Recipes/MyRecipes.aspx" class="profile-stat-card">
                    <span class="profile-stat-icon profile-stat-orange"><%= Icon("book") %></span>
                    <span>Recipes Uploaded</span>
                    <strong><%= RecipeCount %></strong>
                </a>
                <a href="#profileContent" class="profile-stat-card" data-profile-tab="posts">
                    <span class="profile-stat-icon"><%= Icon("chat") %></span>
                    <span>Community Posts</span>
                    <strong><%= PostCount %></strong>
                </a>
            </div>
            <% } %>
        </header>

        <% if (IsAdministrator) { %>
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
                <a href="../Admin/ManageRecipes.aspx" class="profile-admin-shortcut">
                    <span class="profile-stat-icon profile-stat-orange"><%= Icon("journal-check") %></span>
                    <span><strong>Manage Recipes</strong><small>Review and organise recipe submissions.</small></span>
                    <%= Icon("arrow-right") %>
                </a>
                <a href="../Admin/ManageCommunity.aspx" class="profile-admin-shortcut">
                    <span class="profile-stat-icon"><%= Icon("chat-square-text") %></span>
                    <span><strong>Manage Community</strong><small>Review posts, comments, and recipe reviews.</small></span>
                    <%= Icon("arrow-right") %>
                </a>
                <a href="../Admin/ManageUsers.aspx" class="profile-admin-shortcut">
                    <span class="profile-stat-icon"><%= Icon("people") %></span>
                    <span><strong>Manage Users</strong><small>View and manage member accounts.</small></span>
                    <%= Icon("arrow-right") %>
                </a>
                <a href="../Admin/ManageChallenges.aspx" class="profile-admin-shortcut">
                    <span class="profile-stat-icon profile-stat-orange"><%= Icon("trophy") %></span>
                    <span><strong>Manage Challenges</strong><small>Manage the community’s cooking challenges.</small></span>
                    <%= Icon("arrow-right") %>
                </a>
            </div>
        </section>
        <% } else { %>
        <section id="profileContent" class="profile-panel profile-content-panel">
            <nav class="profile-tabs" aria-label="Profile sections">
                <button type="button" class="is-active" data-profile-tab="overview" aria-pressed="true">Overview</button>
                <button type="button" data-profile-tab="recipes" aria-pressed="false">My Recipes</button>
                <button type="button" data-profile-tab="posts" aria-pressed="false">My Posts</button>
                <button type="button" data-profile-tab="comments" aria-pressed="false">My Comments</button>
            </nav>

            <section class="profile-section" data-profile-recipes>
                <div class="profile-section-heading">
                    <h2>My Recipes</h2>
                    <a href="../Recipes/MyRecipes.aspx">View all <%= Icon("arrow-left", "profile-arrow-forward") %></a>
                </div>
                <div class="profile-recipes">
                    <asp:Repeater ID="rptRecipes" runat="server">
                        <ItemTemplate>
                            <a class="profile-recipe" href='<%# RecipeUrl(Eval("recipeID"), Eval("status")) %>'>
                                <div class="profile-recipe-picture">
                                    <span class="profile-picture-fallback"><%# Icon("book") %></span>
                                    <img src='<%#: Picture(Eval("image")) %>' alt='<%#: Eval("title") %>' loading="lazy" data-profile-photo />
                                </div>
                                <div class="profile-recipe-copy">
                                    <h3><%#: Eval("title") %></h3>
                                    <p>
                                        <span><%#: Eval("cuisine") %></span>
                                        <span>·</span>
                                        <span><%#: Eval("preparationTime") %> mins</span>
                                    </p>
                                    <span class="profile-saves"><%# Icon("heart") %><%#: Eval("saves") %></span>
                                    <span class="profile-recipe-status"><%#: Convert.ToString(Eval("status")) == "Approved" ? "" : Eval("status") %></span>
                                </div>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:Panel ID="pnlNoRecipes" runat="server" CssClass="profile-empty">
                    <%= Icon("book") %>
                    <p>Your recipe collection starts here.</p>
                    <a href="../Recipes/SubmitRecipe.aspx" class="profile-button profile-button-outline">Share your first recipe</a>
                </asp:Panel>
            </section>

            <section class="profile-section profile-activity-section" data-profile-activity>
                <div class="profile-section-heading">
                    <h2 data-activity-heading>Recent Community Activity</h2>
                    <a href="#profileContent" data-profile-tab="posts">View all <%= Icon("arrow-left", "profile-arrow-forward") %></a>
                </div>
                <div class="profile-activity-list">
                    <asp:Repeater ID="rptActivity" runat="server">
                        <ItemTemplate>
                            <a class="profile-activity-row" data-activity-kind='<%#: Eval("kind") %>' href='<%# "../Community/Post.aspx?id=" + Eval("postID") + (Convert.ToString(Eval("kind")) == "comment" ? "#comments" : "") %>'>
                                <span class="profile-avatar profile-avatar-small">
                                    <span><%# H(Initials) %></span>
                                    <img src='<%#: AvatarDisplayUrl %>' alt="" data-profile-avatar loading="lazy" />
                                </span>
                                <span class="profile-activity-copy">
                                    <span><%#: ActivityText(Eval("kind")) %></span>
                                    <strong><%#: Excerpt(Eval("content")) %></strong>
                                </span>
                                <time datetime='<%# Convert.ToDateTime(Eval("createdAt")).ToString("s") + "Z" %>'><%#: Age(Eval("createdAt")) %></time>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <p class="profile-empty" data-activity-empty hidden>
                    No community activity here yet. <a href="../Community/Community.aspx">Join the conversation</a>.
                </p>
            </section>
        </section>
        <% } %>
    </main>

    <script src="<%= ResolveUrl("~/Scripts/profile.js?v=5") %>"></script>
</asp:Content>
