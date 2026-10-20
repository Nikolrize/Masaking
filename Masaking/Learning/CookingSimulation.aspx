<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CookingSimulation.aspx.cs" Inherits="Masaking.Learning.CookingSimulation" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Fried Rice Simulation - Masaking</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:HyperLink ID="lnkBackToRecipe" runat="server" NavigateUrl="~/Recipes/BrowseRecipes.aspx">Back to Recipes</asp:HyperLink>
            <br />
            <br />
            <asp:Label ID="lblTitle" runat="server" Font-Bold="True" Font-Size="X-Large" Text="Fried Rice Simulation"></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblIntroduction" runat="server" Text="Add the ingredients in the correct order. Take your time!"></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblStep" runat="server" Text="Step 1 of 5" ClientIDMode="Static"></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblInstruction" runat="server" Text="First, add the cooking oil." ClientIDMode="Static"></asp:Label>
            <br />
            <br />
            <asp:Panel ID="pnlIngredients" runat="server">
                <asp:Label ID="lblIngredientsTitle" runat="server" Text="Ingredients"></asp:Label>
                <br />
                <br />
                <asp:Image
                    ID="imgOil" runat="server" AlternateText="Cooking oil" ClientIDMode="Static" ImageUrl="~/Content/images/simulation/oil.png" Width="94px" Height="77px" draggable="true" ondragstart="dragIngredient(event)"/>
                <br />
                <asp:Label ID="lblOil" runat="server" Text="Cooking Oil"></asp:Label>
                <br />
                <br />
                <asp:Image ID="imgGarlic" runat="server" AlternateText="Garlic" ClientIDMode="Static" draggable="true" ondragstart="dragIngredient(event)"/>
                <br />
                <asp:Label ID="lblGarlic" runat="server" Text="Chopped garlic "></asp:Label>
                <br />
                <br />
                <asp:Image ID="imgEgg" runat="server" AlternateText="Egg" ClientIDMode="Static" draggable="true" ondragstart="dragIngredient(event)"/>
                <br />
                <asp:Label ID="lblEgg" runat="server" Text="Beaten egg "></asp:Label>
                <br />
                <br />
                <asp:Image ID="imgRice" runat="server" AlternateText="Rice" ClientIDMode="Static" draggable="true" ondragstart="dragIngredient(event)"/>
                <br />
                <asp:Label ID="lblRice" runat="server" Text="Cooked rice "></asp:Label>
                <br />
                <br />
                <asp:Image ID="imgSeasoning" runat="server" AlternateText="Seasoning" ClientIDMode="Static" draggable="true" ondragstart="dragIngredient(event)"/>
                <br />
                <asp:Label ID="lblSeasoning" runat="server" Text="Seasoning"></asp:Label>
            </asp:Panel>
            <br />
            <asp:Panel ID="pnlWok" runat="server" ClientIDMode="Static" ondragover="allowDrop(event)" ondrop="dropIngredient(event)" BorderStyle="Dashed" BorderWidth="2px" Height="220px" Width="300px">
                <asp:Label ID="lblWokTitle" runat="server" Text="Your wok"></asp:Label>
                <br />
                <br />
                <asp:Image ID="imgWok" runat="server" AlternateText="Cooking wok" ClientIDMode="Static" />
                <br />
                <br />
                <asp:Label ID="lblDropHint" runat="server" Text="Drop ingredients here.  "></asp:Label>
                <br />
            </asp:Panel>
            <br />
            <asp:Panel ID="pnlCompletion" runat="server" ClientIDMode="Static" style="display: none;">
                <asp:Label ID="lblCompletedTitle" runat="server" Text="Well done! Your fried rice is ready."></asp:Label>
                <br />
                <asp:Image ID="imgCompletedDish" runat="server" AlternateText="Completed fried rice" Width="250px" />
                <br />
                <br />
                <asp:Label ID="lblCompletedMessage" runat="server" Text="You added all ingredients in the correct order."></asp:Label>
                <br />
                <asp:HyperLink ID="lnkCompletedBack" runat="server" NavigateUrl="~/Recipes/BrowseRecipes.aspx">Back to Recipes</asp:HyperLink>
            </asp:Panel>
            <br />
            <asp:Label ID="lblFeedback" runat="server" Text="Ready to begin? Find the cooking oil." ClientIDMode="Static"></asp:Label>
            <br />
            <br />
            <asp:Button ID="btnRestart" runat="server" Text="Restart" ClientIDMode="Static" OnClientClick="restartSimulation(); return false"/>
            <br />
        </div>
    </form>

    <script>
        const cookingSteps = [
            {
                ingredientId: "imgOil",
                instruction: "First, add the cooking oil to the heated wok."
            },
            {
                ingredientId: "imgGarlic",
                instruction: "Next, add the chopped garlic and stir until fragrant."
            },
            {
                ingredientId: "imgEgg",
                instruction: "Add the beaten egg and scramble it."
            },
            {
                ingredientId: "imgRice",
                instruction: "Add the cooked rice and stir to combine."
            },
            {
                ingredientId: "imgSeasoning",
                instruction: "Finally, add the seasoning and mix well."
            }
        ];

        let currentStep = 0;

        function dragIngredient(event) {
            event.dataTransfer.setData("text/plain", event.target.id);
            event.dataTransfer.effectAllowed = "move";
        }

        function allowDrop(event) {
            event.preventDefault();
            event.dataTransfer.dropEffect = "move";
        }

        function dropIngredient(event) {
            event.preventDefault();

            if (currentStep >= cookingSteps.length) {
                return;
            }

            const ingredientId =
                event.dataTransfer.getData("text/plain");

            const expectedStep = cookingSteps[currentStep];

            const feedback =
                document.getElementById("lblFeedback");

            if (ingredientId !== expectedStep.ingredientId) {
                feedback.textContent =
                    "Not yet! " + expectedStep.instruction;
                return;
            }

            document.getElementById(ingredientId).hidden = true;

            currentStep++;

            if (currentStep === cookingSteps.length) {
                document.getElementById("lblStep").textContent =
                    "Completed: 5 of 5 steps";

                document.getElementById("lblInstruction").textContent =
                    "Your fried rice is ready!";

                feedback.textContent =
                    "Well done! You added every ingredient in the correct order.";

                document.getElementById("pnlWok").style.display = "none";

                document.getElementById("pnlCompletion").style.display = "block";
            }
            else {
                document.getElementById("lblStep").textContent =
                    "Step " + (currentStep + 1) + " of " + cookingSteps.length;

                document.getElementById("lblInstruction").textContent =
                    cookingSteps[currentStep].instruction;

                feedback.textContent =
                    "Correct! Follow the next instruction.";
            }
        }

        function restartSimulation() {
            currentStep = 0;

            document.getElementById("pnlWok").style.display = "block";

            document.getElementById("pnlCompletion").style.display = "none";

            for (const step of cookingSteps) {
                document.getElementById(step.ingredientId).hidden = false;
            }

            document.getElementById("lblStep").textContent =
                "Step 1 of " + cookingSteps.length;

            document.getElementById("lblInstruction").textContent =
                cookingSteps[0].instruction;

            document.getElementById("lblFeedback").textContent =
                "Ready to begin? Find the cooking oil.";
        }
    </script>
</body>
</html>
