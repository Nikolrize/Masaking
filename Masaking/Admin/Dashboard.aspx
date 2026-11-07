<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Admin/Admin.Master"
    AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Masaking.Admin.Dashboard" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="mb-4">
        <h1 class="page-heading">Admin Dashboard</h1>
        <p class="text-secondary mb-0">Review website activity and manage learning content.</p>
    </header>
    <h2 class="h5 mb-3">Website overview</h2>
    <div class="row g-3 mb-4">
        <asp:Repeater ID="rptStatistics" runat="server">
            <ItemTemplate>
                <div class="col-12 col-sm-6 col-xl-4">
                    <section class='<%# "admin-summary-card h-100 stat-card stat-" + Eval("Theme") %>'>
                        <div class="d-flex justify-content-between gap-3 mb-3">
                            <h3 class="h6 text-secondary mb-0"><%#: Eval("Title") %></h3>
                            <i class='<%# "stat-icon bi " + Eval("Icon") %>' aria-hidden="true"></i>
                        </div>
                        <p class="display-6 fw-bold mb-0"><%#: Eval("Total") %></p>
                    </section>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:Panel ID="pnlRecentRecipes" runat="server" CssClass="admin-summary-card mb-4">
        <h2 class="h5">Recent recipe submissions</h2>
        <p class="small text-secondary">The five newest submissions across all statuses. Times are shown in Malaysia time.</p>
        <div class="table-responsive">
            <asp:GridView ID="gvRecentRecipes" runat="server" AutoGenerateColumns="False"
                CssClass="table align-middle" GridLines="None" EmptyDataText="No recipes have been submitted yet.">
                <Columns>
                    <asp:BoundField DataField="recipeID" HeaderText="ID" />
                    <asp:BoundField DataField="title" HeaderText="Recipe" />
                    <asp:BoundField DataField="displayName" HeaderText="Submitted by" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# StatusClass(Eval("status")) %>'><%#: Eval("status") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="submittedLocal" HeaderText="Submitted (MYT)"
                        DataFormatString="{0:dd MMM yyyy HH:mm}" />
                </Columns>
            </asp:GridView>
        </div>
        <asp:HyperLink ID="lnkManageRecipes" runat="server" NavigateUrl="~/Admin/ManageRecipes.aspx"
            CssClass="btn btn-outline-success" Text="Review recipes" />
    </asp:Panel>

    <asp:Panel ID="pnlRecentActivity" runat="server" CssClass="admin-summary-card">
        <h2 class="h5">Recent completed learning activity</h2>
        <p class="small text-secondary">
            The five most recently completed challenges and simulations. Repeated attempts appear separately.
        </p>
        <div class="table-responsive">
            <asp:GridView ID="gvRecentActivity" runat="server" AutoGenerateColumns="False" CssClass="table align-middle" GridLines="None" EmptyDataText="No learning activities have been completed yet.">
                <Columns>
                    <asp:BoundField DataField="activityType" HeaderText="Type" />
                    <asp:BoundField DataField="title" HeaderText="Activity" />
                    <asp:BoundField DataField="displayName" HeaderText="Member" />
                    <asp:BoundField DataField="resultText" HeaderText="Result" />
                    <asp:BoundField DataField="completedLocal" HeaderText="Finished (MYT)"
                        DataFormatString="{0:dd MMM yyyy HH:mm}" />
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>
</asp:Content>
