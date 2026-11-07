<%@ Page Title="Cooking Simulation" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="CookingSimulation.aspx.cs" Inherits="Masaking.Learning.CookingSimulation" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link runat="server" href="~/Content/css/simulation.css" rel="stylesheet" />
    <main class="simulation-page py-4 py-md-5">
        <div class="d-flex flex-wrap gap-2 mb-3">
            <asp:HyperLink ID="lnkBrowse" runat="server" Text="All simulations" NavigateUrl="~/Learning/Simulations.aspx" CssClass="btn btn-outline-success" />
            <asp:HyperLink ID="lnkProgress" runat="server" Text="My progress" NavigateUrl="~/Learning/Progress.aspx" CssClass="btn btn-outline-secondary" />
        </div>
        <asp:Label ID="lblTitle" runat="server" Text="Cooking simulation" CssClass="h1 d-block mb-2" />
        <asp:Label ID="lblDescription" runat="server" CssClass="d-block text-secondary mb-3" />

        <asp:UpdatePanel ID="upGame" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Label ID="lblMessage" runat="server" EnableViewState="False" CssClass="d-block mb-3" role="status" aria-live="polite" />
                <asp:Panel ID="pnlStart" runat="server" Visible="False" CssClass="card p-3 p-md-4">
                    <asp:Label ID="lblInstructions" runat="server" CssClass="d-block mb-3" />
                    <p>There is no time limit. Drag an ingredient into the cooking area, or select its Add button. Each choice is saved automatically.</p>
                    <asp:Button ID="btnStart" runat="server" Text="Start cooking"
                        CssClass="btn btn-success align-self-start" CausesValidation="False" OnClick="btnStart_Click" />
                </asp:Panel>

                <asp:Panel ID="pnlGame" runat="server" Visible="False">
                    <div class="d-flex flex-wrap justify-content-between gap-2 mb-3">
                        <asp:Label ID="lblStep" runat="server" CssClass="fw-bold" />
                        <asp:Label ID="lblMistakes" runat="server" CssClass="text-secondary" />
                    </div>
                    <asp:Label ID="lblInstruction" runat="server" CssClass="h5 d-block mb-3" />
                    <div id="progressTrack" runat="server" class="progress mb-4" role="progressbar" aria-label="Simulation progress">
                        <div id="progressBar" runat="server" class="progress-bar bg-success"></div>
                    </div>
                    <div class="row g-4">
                        <div class="col-12 col-lg-6">
                            <div id="dropZone" class="cooking-area card p-3 p-md-4"
                                aria-label="Drop an ingredient into the cooking area">
                                <h2 class="h5">Cooking area</h2>
                                <div class="cookware">
                                    <asp:Image ID="imgCookware" runat="server" AlternateText="Cookware for this recipe" CssClass="cookware-image" />
                                    <asp:Panel ID="pnlCookwarePlaceholder" runat="server" CssClass="cookware-placeholder" aria-hidden="true" />
                                    <div class="steam" aria-hidden="true"><span></span><span></span><span></span></div>
                                </div>
                                <p class="text-secondary mb-0">Drop an ingredient here, or use its Add button.</p>
                            </div>
                        </div>
                        <div class="col-12 col-lg-6">
                            <h2 class="h5">Ingredients</h2>
                            <div class="row g-3">
                                <asp:Repeater ID="rptIngredients" runat="server" OnItemCommand="rptIngredients_ItemCommand">
                                    <ItemTemplate>
                                        <div class="col-6 col-sm-4 col-lg-6">
                                            <div class="ingredient-card card p-2 h-100" draggable="true" data-ingredient='<%# Eval("ingredientID") %>'>
                                                <asp:Image ID="imgIngredient" runat="server" ImageUrl='<%# Eval("imageUrl") %>' AlternateText='<%# Eval("ingredientName") %>' Visible='<%# !String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' CssClass="ingredient-image" draggable="false" onerror="this.hidden=true;" />
                                                <span class="fw-semibold d-block mb-2"><%#: Eval("ingredientName") %></span>
                                                <asp:LinkButton ID="btnIngredient" runat="server" Text="Add" CssClass="btn btn-outline-success btn-sm mt-auto" CausesValidation="False" CommandName="AddIngredient" CommandArgument='<%# Eval("ingredientID") %>' />
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                    <p class="small text-secondary mt-3"> Leaving this page keeps your progress. Reopen this simulation to continue. Some recipes use the same ingredient in more than one step.</p>
                </asp:Panel>

                <asp:Panel ID="pnlResult" runat="server" Visible="False" ClientIDMode="Static" CssClass="card p-3 p-md-4 text-center">
                    <h2>Dish completed!</h2>
                    <asp:Image ID="imgCompleted" runat="server" AlternateText="The completed dish" CssClass="completed-image mx-auto mb-3" />
                    <asp:Label ID="lblResult" runat="server" CssClass="fs-4 d-block mb-2" />
                    <p class="small text-secondary"> Accuracy = correct ingredient additions divided by all ingredient choices.</p>
                </asp:Panel>

                <asp:Panel ID="pnlAttemptActions" runat="server" Visible="False" CssClass="d-flex flex-wrap gap-2 mt-4">
                    <asp:Button ID="btnRestart" runat="server" Text="Start a new attempt" CssClass="btn btn-outline-success" CausesValidation="False" OnClientClick="return confirm('Start a new attempt? Your current run will remain in your history.');" OnClick="btnRestart_Click" />
                </asp:Panel>
                <asp:HiddenField ID="hfIngredient" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hfEffect" runat="server" ClientIDMode="Static" />
                <asp:Button ID="btnDrop" runat="server" Text="Add dropped ingredient" ClientIDMode="Static" CausesValidation="False" UseSubmitBehavior="False" Style="display:none;" OnClick="btnDrop_Click" />
            </ContentTemplate>
        </asp:UpdatePanel>

        <div class="d-flex flex-wrap gap-2 align-items-center mt-4">
            <asp:HyperLink ID="lnkRecipe" runat="server" Text="Back to recipe details" Visible="False" CssClass="btn btn-success" />
            <button type="button" id="btnSound" class="btn btn-outline-secondary" aria-pressed="false">Sound: off</button>
        </div>
    </main>
    <script src="<%= ResolveUrl("~/Scripts/simulation.js") %>"></script>
</asp:Content>