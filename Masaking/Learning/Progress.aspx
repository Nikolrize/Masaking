<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Progress.aspx.cs" Inherits="Masaking.Learning.Progress" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>My Learning Progress - Masaking</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblTitle" runat="server" Font-Bold="True" Font-Size="X-Large" Text="My Learning Progress"></asp:Label>
            <br />
            <asp:Label ID="lblIntroduction" runat="server" Text="Review your cooking practice and quiz activities."></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblDemoNotice" runat="server" Font-Italic="True" Text="Prototype preview: the records below are sample data."></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblSimulationHeading" runat="server" Font-Bold="True" Text="Cooking Simulations"></asp:Label>
            <br />
            <asp:Label ID="lblSimulationSummary" runat="server" Text="Completed: 1 simulation"></asp:Label>
            <br />
            <asp:Label ID="lblSimulationRecord" runat="server" Text="Fried Rice — Completed"></asp:Label>
            <br />
            <asp:HyperLink ID="lnkSimulation" runat="server" NavigateUrl="~/Learning/CookingSimulation.aspx">Practice Cooking</asp:HyperLink>
            <br />
            <br />
            <asp:Label ID="lblChallengeHeading" runat="server" Font-Bold="True" Text="Quiz Challenges"></asp:Label>
            <br />
            <asp:Label ID="lblChallengeSummary" runat="server" Text="Completed: 1 quiz attempt"></asp:Label>
            <br />
            <asp:Label ID="lblChallengeRecord" runat="server" Text="Malaysian Ingredients Quiz — Score: 1 / 1"></asp:Label>
            <br />
            <asp:HyperLink ID="lnkChallenges" runat="server" NavigateUrl="~/Learning/Challenges.aspx">Try a Quiz</asp:HyperLink>
            <br />
            <br />
            <asp:HyperLink ID="lnkRecipes" runat="server" NavigateUrl="~/Recipes/BrowseRecipes.aspx">Browse Recipes</asp:HyperLink>
        </div>
    </form>
</body>
</html>
