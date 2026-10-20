<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Challenges.aspx.cs" Inherits="Masaking.Learning.Challenges" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblTitle" runat="server" Text="Cooking Quiz Challenge" Font-Bold="True" Font-Size="X-Large"></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblIntroduction" runat="server" Text="Test your knowledge of Malaysian cooking."></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblQuestionNumber" runat="server" Text="Sample Question 1 of 1"></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblQuestion" runat="server" Text="&quot;What is the exact ratio of coconut milk to water for perfectly fluffy coconut rice?&quot;"></asp:Label>
            <asp:RadioButtonList ID="rblAnswers" runat="server">
                <asp:ListItem Value="3:1">3 parts coconut milk to 1 part water</asp:ListItem>
                <asp:ListItem Value="1:3">1 part coconut milk to 3 parts water</asp:ListItem>
                <asp:ListItem Value="1:1">1 part coconut milk to 1 part water (1:1)</asp:ListItem>
                <asp:ListItem Value="1:0">Pure coconut milk with no added water at all</asp:ListItem>
            </asp:RadioButtonList>
            <br />
            <asp:Button ID="btnCheckAnswer" runat="server" OnClick="btnCheckAnswer_Click" Text="Check Answer" />
            <br />
            <br />
            <asp:Label ID="lblFeedback" runat="server"></asp:Label>
            <br />
            <br />
            <asp:HyperLink ID="lnkSimulation" runat="server" NavigateUrl="~/Learning/CookingSimulation.aspx">Try Cooking Simulation</asp:HyperLink>
            <br />
            <br />
            <asp:HyperLink ID="lnkProgress" runat="server" NavigateUrl="~/Learning/Progress.aspx">My Progress</asp:HyperLink>
        </div>
    </form>
</body>
</html>
