<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="Masaking.Admin.Dashboard" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Admin Dashboard - Masaking</title>

    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/admin.css" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server">
        <div class="admin-layout">

            <aside class="admin-sidebar">
                <asp:HyperLink ID="lnkBrand" runat="server" NavigateUrl="~/Default.aspx" CssClass="admin-brand" Text="Masaking" />

                <p class="small">Administration</p>

                <nav aria-label="Administrator navigation">
                    <asp:HyperLink ID="lnkDashboard" runat="server" NavigateUrl="~/Admin/Dashboard.aspx" aria-current="page" Text="Dashboard" />
                    <asp:HyperLink ID="lnkRecipes" runat="server" NavigateUrl="~/Admin/ManageRecipes.aspx" Text="Manage recipes" />
                    <asp:HyperLink ID="lnkUsers" runat="server" NavigateUrl="~/Admin/ManageUsers.aspx" Text="Manage users" />
                    <asp:HyperLink ID="lnkCommunity" runat="server" NavigateUrl="~/Admin/ManageCommunity.aspx" Text="Moderate community" />
                    <asp:HyperLink ID="lnkQuizzes" runat="server" NavigateUrl="~/Admin/ManageChallenges.aspx" Text="Manage quizzes" />
                    <asp:HyperLink ID="lnkWebsite" runat="server" NavigateUrl="~/Default.aspx" Text="Back to website" />
                </nav>
            </aside>
            <main class="admin-main">
                <header class="mb-4">
                    <h1>Admin Dashboard</h1>
                    <p class="text-secondary">Review website activity and manage learning content. </p>
                </header>

                <h2 class="h5 mb-3">Website overview</h2>

                <div class="row g-3 mb-4">
                    <asp:Repeater ID="rptStatistics" runat="server">
                        <ItemTemplate>
                            <div class="col-sm-6 col-xl-4">
                                <section class="admin-summary-card h-100">
                                    <h3 class="h6 text-secondary"> <%#: Eval("Title") %> </h3>
                                    <p class="display-6 fw-bold mb-0"> <%#: Eval("Total") %> </p>
                                </section>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlRecentRecipes" runat="server"
                    CssClass="admin-summary-card mb-4">

                    <h2 class="h5">Recent recipe submissions</h2>
                    <p class="text-secondary">The five newest recipes, including all approval statuses   </p>

                    <div class="table-responsive">
                        <asp:GridView ID="gvRecentRecipes" runat="server" AutoGenerateColumns="False" CssClass="table align-middle" GridLines="None" EmptyDataText="No recipes have been submitted yet.">
                            <Columns>
                                <asp:BoundField DataField="recipeID" HeaderText="ID" />
                                <asp:BoundField DataField="title" HeaderText="Recipe" />
                                <asp:BoundField DataField="displayName" HeaderText="Submitted by" />
                                <asp:BoundField DataField="status" HeaderText="Status" />
                                <asp:BoundField DataField="createdAt" HeaderText="Submitted" DataFormatString="{0:dd MMM yyyy}" />
                            </Columns>
                        </asp:GridView>
                    </div>

                    <asp:HyperLink ID="lnkManageRecipes" runat="server" NavigateUrl="~/Admin/ManageRecipes.aspx" CssClass="btn btn-outline-success" Text="Manage recipes" />
                </asp:Panel>

                <asp:Panel ID="pnlShortcuts" runat="server"
                    CssClass="admin-summary-card">

                    <h2 class="h5">Management shortcuts</h2>

                    <div class="d-flex flex-wrap gap-2">
                        <asp:HyperLink ID="lnkManageUsers" runat="server" NavigateUrl="~/Admin/ManageUsers.aspx" CssClass="btn btn-success" Text="Manage users" />

                        <asp:HyperLink ID="lnkModeration" runat="server" NavigateUrl="~/Admin/ManageCommunity.aspx" CssClass="btn btn-success" Text="Review community content" />

                        <asp:HyperLink ID="lnkManageQuizzes" runat="server" NavigateUrl="~/Admin/ManageChallenges.aspx" CssClass="btn btn-success" Text="Manage quizzes" />
                    </div>

                    <p class="small text-secondary mt-3 mb-0"> Quiz and simulation counts show active records. Their management features are being completed.
                    </p>
                </asp:Panel>
            </main>
        </div>
    </form>
</body>
</html>