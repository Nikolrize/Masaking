<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CookingStepEditor.ascx.cs" Inherits="Masaking.Controls.CookingStepEditor" %>

<div data-step-editor>
    <!-- Store step data as JSON -->
    <asp:HiddenField ID="stepData" runat="server" />

     <!-- Step rows -->
    <div class="recipe-step-rows" data-step-list></div>

    <button type="button"
        class="btn btn-outline-success recipe-add-row"
        data-add-step>
        <i class="bi bi-plus-lg"></i>
        Add step
    </button>

    <!-- JS template for clone -->
    <template data-step-template>
        <div class="recipe-step-card">

            <!-- Step number and remove button -->
            <div class="recipe-step-title">
                <div class="d-flex align-items-center gap-2">
                    <span class="detail-step-number" data-step-number></span>
                    <strong>Cooking step</strong>
                </div>

                <button type="button"
                    class="recipe-remove-row"
                    data-remove-step
                    aria-label="Remove step">
                    <i class="bi bi-trash"></i>
                </button>
            </div>

            <!-- Instruction -->
            <label class="form-label" data-label="instruction">
                Cooking instruction
            </label>

            <textarea class="form-control mb-3"
                data-field="instruction"
                rows="3"
                maxlength="2000"
                placeholder="Explain what the cook should do."
                required></textarea>

            <label class="form-label" data-label="minutes">
                Estimated minutes
            </label>

            <input type="number"
                class="form-control"
                data-field="minutes"
                min="0"
                max="1440"
                step="1"
                required />

            <!-- Photo upload and preview -->
            <div class="recipe-step-photo">
                <label class="form-label" data-photo-label>
                    Step photo
       
                    <span class="recipe-optional">(optional)</span>
                </label>

                <input type="file"
                    class="form-control"
                    accept="image/jpeg,image/png,image/gif"
                    data-step-photo />

                <p class="form-text">
                    JPG, PNG or GIF. Maximum size: 5 MB.
                </p>

                <p class="text-danger small"
                    data-photo-error role="status">
                </p>

                <img class="recipe-step-preview"
                    data-photo-preview
                    alt="Cooking step photo"
                    hidden />

                <button type="button"
                    class="recipe-remove-photo"
                    data-remove-photo hidden>
                    Remove photo
                </button>
            </div>
        </div>
    </template>
</div>
