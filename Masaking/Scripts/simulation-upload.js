(function () {
    "use strict";
    const previews = new Map();
    document.addEventListener("change", function (event) {
        const input = event.target;
        if (!input.matches("input[type=file][data-simulation-preview]")) return;
        const image = document.getElementById(input.dataset.simulationPreview);
        const error = document.getElementById(input.dataset.simulationError);
        if (!image) return;
        if (error) error.textContent = "";
        if (!image.dataset.original) {
            image.dataset.original = image.getAttribute("src") || "";
            image.dataset.originalDisplay = image.style.display;
        }
        if (previews.has(input)) {
            URL.revokeObjectURL(previews.get(input));
            previews.delete(input);
        }
        const file = input.files && input.files[0];
        if (!file) {
            image.src = image.dataset.original;
            image.style.display = image.dataset.originalDisplay;
            return;
        }
        const validType = ["image/jpeg", "image/png", "image/gif"].includes(file.type);
        if (!validType || file.size > 5 * 1024 * 1024) {
            if (error) error.textContent = "Choose a JPG, PNG or GIF image up to 5 MB.";
            input.value = "";
            image.src = image.dataset.original;
            image.style.display = image.dataset.originalDisplay;
            return;
        }
        const url = URL.createObjectURL(file);
        previews.set(input, url);
        image.src = url;
        image.style.display = "block";
    });
    window.addEventListener("pagehide", function () {
        for (const url of previews.values()) URL.revokeObjectURL(url);
        previews.clear();
    });
})();