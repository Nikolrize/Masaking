(function () {
    "use strict";

    const upload = document.getElementById("uploadRecipeImage");

    if (!upload) {
        return;
    }

    const preview = document.getElementById("recipeImagePreview");
    const existing = document.getElementById("imgExistingRecipe");
    const error = document.getElementById("recipeImageError");
    const clearButton = document.getElementById("btnClearRecipePhoto");
    const emptyMessage =
        document.querySelector("[data-recipe-photo-empty]");

    let previewUrl = null;

    function releasePreview() {
        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
            previewUrl = null;
        }
    }

    function updateDisplay() {
        const hasSelectedPhoto = previewUrl !== null;

        preview.classList.toggle("d-none", !hasSelectedPhoto);
        clearButton.hidden = !hasSelectedPhoto;

        if (existing) {
            existing.hidden = hasSelectedPhoto;
        }

        emptyMessage.hidden =
            hasSelectedPhoto || Boolean(existing);
    }

    function changePhoto() {
        releasePreview();
        preview.removeAttribute("src");
        error.textContent = "";

        const file = upload.files[0];

        if (!file) {
            updateDisplay();
            return;
        }

        if (!/^image\/(jpeg|png|gif)$/.test(file.type)) {
            error.textContent = "Choose a JPG, PNG or GIF image.";
            upload.value = "";
            updateDisplay();
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            error.textContent = "Choose an image no larger than 5 MB.";
            upload.value = "";
            updateDisplay();
            return;
        }

        previewUrl = URL.createObjectURL(file);
        preview.src = previewUrl;

        updateDisplay();
    }

    upload.addEventListener("change", changePhoto);

    clearButton.addEventListener("click", function () {
        upload.value = "";
        changePhoto();
    });

    window.addEventListener("pagehide", function (event) {
        if (!event.persisted) {
            releasePreview();
        }
    });

    updateDisplay();
})();