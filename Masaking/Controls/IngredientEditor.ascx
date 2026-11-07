<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IngredientEditor.ascx.cs" Inherits="Masaking.Controls.IngredientEditor" %>

<div data-ingredient-editor>
    <asp:HiddenField ID="ingredientData" runat="server" />

    <div class="recipe-ingredient-rows" data-ingredient-list></div>

    <button type="button"
        class="btn btn-outline-success recipe-add-row"
        data-add-ingredient>
        <i class="bi bi-plus-lg"></i>
        Add ingredient
   
    </button>

    <template data-ingredient-template>
        <div class="recipe-ingredient-row">
            <div>
                <label data-label="name">Ingredient</label>
                <input type="text"
                    class="form-control"
                    data-field="name"
                    maxlength="50"
                    placeholder="e.g. Rice"
                    required />
            </div>

            <div>
                <label data-label="quantity">Quantity</label>
                <input type="number"
                    class="form-control"
                    data-field="quantity"
                    min="0"
                    max="99999999.99"
                    step="0.01"
                    placeholder="300"
                    required />
            </div>

            <div>
                <label data-label="unit">Unit</label>
                <input type="text"
                    class="form-control"
                    data-field="unit"
                    maxlength="30"
                    placeholder="g"
                    required />
            </div>

            <button type="button"
                class="recipe-remove-row"
                data-remove-ingredient>
                <i class="bi bi-trash"></i>
            </button>
        </div>
    </template>
</div>
