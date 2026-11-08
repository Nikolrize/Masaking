(function () {
    "use strict";

    let nextRowId = 0;

    document.querySelectorAll("[data-ingredient-editor]")
        .forEach(setupEditor);

    function setupEditor(editor) {
        const dataField = editor.querySelector('input[type="hidden"]');
        const list = editor.querySelector("[data-ingredient-list]");
        const template = editor.querySelector("[data-ingredient-template]");
        const addButton = editor.querySelector("[data-add-ingredient]");

        function saveRows() {
            const ingredients = [];

            for (const row of list.children) {
                ingredients.push({
                    name: row.querySelector('[data-field="name"]').value,
                    quantity: row.querySelector('[data-field="quantity"]').value,
                    unit: row.querySelector('[data-field="unit"]').value
                });
            }

            dataField.value = JSON.stringify(ingredients);
        }

        function addRow(ingredient) {
            const row =
                template.content.firstElementChild.cloneNode(true);

            nextRowId++;

            for (const field of row.querySelectorAll("[data-field]")) {
                const fieldName = field.dataset.field;
                const label =
                    row.querySelector('[data-label="' + fieldName + '"]');

                field.id = "ingredient_" + nextRowId + "_" + fieldName;
                label.htmlFor = field.id;

                field.value = ingredient[fieldName] || "";
            }

            row.addEventListener("input", saveRows);

            row.querySelector("[data-remove-ingredient]")
                .addEventListener("click", function () {
                    row.remove();

                    if (list.children.length === 0) {
                        addRow({});
                    }

                    saveRows();
                });

            list.appendChild(row);

            return row;
        }

        let ingredients;

        try {
            ingredients = JSON.parse(dataField.value || "[]");

            if (!Array.isArray(ingredients)) {
                throw new Error("Invalid ingredient data.");
            }
        } catch (error) {
            editor.textContent =
                "Ingredients could not be loaded. Please reload the page.";

            return;
        }

        for (const ingredient of ingredients) {
            addRow(ingredient || {});
        }

        if (list.children.length === 0) {
            addRow({});
        }

        addButton.addEventListener("click", function () {
            if (list.children.length >= 100) {
                return;
            }

            const row = addRow({});

            saveRows();
            row.querySelector('[data-field="name"]').focus();
        });

        editor.closest("form").addEventListener("submit", saveRows);

        saveRows();
    }
})();