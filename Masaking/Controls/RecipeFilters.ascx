<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecipeFilters.ascx.cs" Inherits="Masaking.Controls.RecipeFilters" %>

<div class="card p-3 border border-subtle">
    <h2 class="h5">Category</h2>

    <!-- Categories -->
    <asp:CheckBoxList ID="cblCategories" runat="server" CssClass="category-filters" />

    <hr />

    <!-- Cuisine abd Region -->
    <h2 class="h5">Cuisine</h2>
    <asp:DropDownList ID="ddlCuisine" runat="server" CssClass="form-select" />

    <h2 class="h5 mt-3">Region</h2>
    <asp:DropDownList ID="ddlRegion" runat="server" CssClass="form-select" />

    <!-- Difficulty -->
    <h2 class="h5 mt-3">Difficulty</h2>
    <asp:RadioButtonList ID="rblDifficulty" runat="server"
        RepeatLayout="Flow"
        CssClass="filter-options">
        <asp:ListItem Value="" Selected="True">All</asp:ListItem>
        <asp:ListItem Value="Easy">Easy</asp:ListItem>
        <asp:ListItem Value="Medium">Medium</asp:ListItem>
        <asp:ListItem Value="Hard">Hard</asp:ListItem>
    </asp:RadioButtonList>

    <!-- Max Budget and Prep Time -->
    <h2 class="h5 mt-3">Maximum budget</h2>
    <span id="budgetValue" class="small text-muted">Any budget</span>
    <asp:TextBox ID="rngBudget" runat="server"
        TextMode="Range"
        Text="0"
        min="0"
        step="1"
        CssClass="form-range" />

    <h2 class="h5 mt-3">Maximum preparation time</h2>
    <span id="timeValue" class="small text-muted">Any duration</span>
    <asp:TextBox ID="rngTime" runat="server"
        TextMode="Range"
        Text="0"
        min="0"
        step="1"
        CssClass="form-range" />

    <!-- Min Serving -->
    <h2 class="h5 mt-3">Minimum servings</h2>
    <asp:TextBox ID="txtServings" runat="server"
        TextMode="Number"
        Text="0"
        min="0"
        step="1"
        CssClass="form-control" />
    <p class="form-text">Enter 0 for any number of servings.</p>

    <!-- Error and Submit -->
    <asp:Label ID="lblFilterError" runat="server"
        Visible="false"
        CssClass="text-danger small d-block" />

    <asp:Button ID="btnApplyFilters"
        runat="server"
        Text="Apply filters"
        CssClass="btn btn-primary mt-3"
        OnClick="btnApplyFilters_Click" />
</div>

 <script>
    (function () {

        const budgetSlider = document.getElementById("<%= rngBudget.ClientID %>");
             const timeSlider = document.getElementById("<%= rngTime.ClientID %>");

             const budgetDisplay = document.getElementById("budgetValue");
             const timeDisplay = document.getElementById("timeValue");

             function updateBudget() {
                 const text = budgetSlider.value === "0"
                     ? "Any budget"
                     : "Up to RM" + budgetSlider.value;

                 budgetDisplay.textContent = text;
             }

             function updateTime() {
                 const text = timeSlider.value === "0"
                     ? "Any duration"
                     : "Up to " + timeSlider.value + " minutes";

                 timeDisplay.textContent = text;
             }

             budgetSlider.addEventListener("input", updateBudget);
             timeSlider.addEventListener("input", updateTime);

             updateBudget();
             updateTime();
         })();
 </script>