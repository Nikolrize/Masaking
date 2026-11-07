(function () {
    "use strict";

    let nextRowId = 0;

    document.querySelectorAll("[data-step-editor]")
        .forEach(setupEditor);

    function setupEditor(editor) {
        const dataField = editor.querySelector('input[type="hidden"]');
        const list = editor.querySelector("[data-step-list]");
        const template = editor.querySelector("[data-step-template]");
        const addButton = editor.querySelector("[data-add-step]");

        function saveRows() {
            const steps = [];

            Array.from(list.children).forEach(function (row, index) {
                row.querySelector("[data-step-number]").textContent =
                    index + 1;

                row.querySelector("[data-remove-step]").setAttribute(
                    "aria-label",
                    "Remove step " + (index + 1)
                );

                row.querySelector("[data-step-photo]").name =
                    "stepUpload_" + index;

                steps.push({
                    instruction: row.querySelector(
                        '[data-field="instruction"]'
                    ).value,

                    minutes: row.querySelector(
                        '[data-field="minutes"]'
                    ).value,

                    image: row.dataset.existingImage || ""
                });
            });

            dataField.value = JSON.stringify(steps);
        }

        function addRow(step) {
            const row =
                template.content.firstElementChild.cloneNode(true);

            nextRowId++;
            row.dataset.existingImage = step.image || "";

            for (const field of row.querySelectorAll("[data-field]")) {
                const fieldName = field.dataset.field;
                const label = row.querySelector(
                    '[data-label="' + fieldName + '"]'
                );

                field.id = "cookingStep_" + nextRowId + "_" + fieldName;
                label.htmlFor = field.id;

                field.value = step[fieldName] ??
                    (fieldName === "minutes" ? "0" : "");
            }

            row.addEventListener("input", saveRows);

            row.querySelector("[data-remove-step]")
                .addEventListener("click", function () {
                    releasePhotoPreview(row);
                    row.remove();

                    if (list.children.length === 0) {
                        addRow({});
                    }

                    saveRows();
                });

            setupStepPhoto(row);

            list.appendChild(row);

            return row;
        }

        function releasePhotoPreview(row) {
            if (row.previewUrl) {
                URL.revokeObjectURL(row.previewUrl);
                row.previewUrl = null;
            }
        }

        function setupStepPhoto(row) {
            const upload = row.querySelector("[data-step-photo]");
            const label = row.querySelector("[data-photo-label]");
            const preview = row.querySelector("[data-photo-preview]");
            const error = row.querySelector("[data-photo-error]");
            const removeButton = row.querySelector("[data-remove-photo]");

            upload.id = "cookingStep_" + nextRowId + "_photo";
            label.htmlFor = upload.id;

            function showPhoto(source) {
                preview.hidden = !source;
                removeButton.hidden = !source;

                if (source) {
                    preview.src = source;
                } else {
                    preview.removeAttribute("src");
                }
            }

            function getExistingPhoto() {
                const image = (row.dataset.existingImage || "").replace(
                    "/Content/Uploads/Steps/",
                    "/Content/images/Uploads/Steps/"
                );

                const localPhoto =
                    /^\/Content\/images\/Uploads\/Steps\/[a-f0-9]{32}\.jpg$/i;

                return localPhoto.test(image) || /^https?:\/\//i.test(image)
                    ? image
                    : "";
            }

            upload.addEventListener("change", function () {
                releasePhotoPreview(row);
                error.textContent = "";

                const file = upload.files[0];

                if (!file) {
                    showPhoto(getExistingPhoto());
                    saveRows();
                    return;
                }

                const validType = /^image\/(jpeg|png|gif)$/.test(file.type);
                const validSize = file.size <= 5 * 1024 * 1024;

                if (!validType || !validSize) {
                    error.textContent = !validType
                        ? "Choose a JPG, PNG or GIF image."
                        : "Choose an image no larger than 5 MB.";

                    upload.value = "";
                    showPhoto(getExistingPhoto());
                    saveRows();
                    return;
                }

                row.previewUrl = URL.createObjectURL(file);

                showPhoto(row.previewUrl);
                saveRows();
            });

            removeButton.addEventListener("click", function () {
                releasePhotoPreview(row);

                upload.value = "";
                row.dataset.existingImage = "";
                error.textContent = "";

                showPhoto("");
                saveRows();
            });

            preview.addEventListener("error", function () {
                preview.hidden = true;
            });

            showPhoto(getExistingPhoto());
        }

        let steps;

        try {
            steps = JSON.parse(dataField.value || "[]");

            if (!Array.isArray(steps)) {
                throw new Error("Invalid step data.");
            }
        } catch (error) {
            editor.textContent =
                "Cooking steps could not be loaded. Please reload the page.";

            return;
        }

        for (const step of steps) {
            addRow(step || {});
        }

        if (list.children.length === 0) {
            addRow({});
        }

        addButton.addEventListener("click", function () {
            if (list.children.length >= 50) {
                return;
            }

            const row = addRow({});

            saveRows();
            row.querySelector('[data-field="instruction"]').focus();
        });

        editor.closest("form").addEventListener("submit", saveRows);

        saveRows();

        window.addEventListener("pagehide", function (event) {
            if (event.persisted) {
                return;
            }

            for (const row of list.children) {
                releasePhotoPreview(row);
            }
        });
    }
})();