<%@ Page Title="Community moderation" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageCommunity.aspx.cs" Inherits="Masaking.Admin.ManageCommunity" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <p class="text-brand fw-semibold">ADMINISTRATION</p>
    <h1 class="page-heading">Community moderation</h1>
    <p class="text-secondary">Review posts, comments and ratings before they are shown publicly.</p>

    <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

    <section class="masaking-card p-3 p-md-4">
        <asp:Repeater ID="rptQueue" runat="server" OnItemCommand="rptQueue_ItemCommand">
            <HeaderTemplate>
                <div class="table-responsive">
                    <table class="table align-middle">
                        <thead>
                            <tr>
                                <th>Type</th>
                                <th>Content</th>
                                <th>Submitted by</th>
                                <th>Date</th>
                                <th class="text-end">Review</th>
                            </tr>
                        </thead>
                        <tbody>
            </HeaderTemplate>

            <ItemTemplate>
                            <tr>
                                <td>
                                    <span class="badge text-bg-light"><%#: Eval("kind") %></span>
                                    <div class="small text-secondary"><%#: Eval("relatedTitle") %></div>
                                </td>
                                <td style="min-width:260px; max-width:480px">
                                    <%#: Eval("content") %>
                                </td>
                                <td><%#: Eval("displayName") %></td>
                                <td class="text-nowrap"><%#: Eval("createdAt", "{0:dd MMM yyyy}") %></td>
                                <td class="text-end text-nowrap">
                                    <asp:LinkButton ID="btnApprove" runat="server"
                                        CssClass="btn btn-sm btn-primary"
                                        CommandName="approve"
                                        CommandArgument='<%# Eval("kind") + ":" + Eval("itemID") %>'>Approve</asp:LinkButton>
                                    <asp:LinkButton ID="btnReject" runat="server"
                                        CssClass="btn btn-sm btn-outline-danger"
                                        CommandName="reject"
                                        CommandArgument='<%# Eval("kind") + ":" + Eval("itemID") %>'
                                        OnClientClick="return confirm('Reject this community content?');">Reject</asp:LinkButton>
                                </td>
                            </tr>
            </ItemTemplate>

            <FooterTemplate>
                        </tbody>
                    </table>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="text-center py-5 text-secondary">
            The moderation queue is clear.
        </asp:Panel>
    </section>

</asp:Content>
