(function () {
    'use strict';
    var page = document.querySelector('[data-recipe-form]');
    if (!page) return;
    var ingredients = document.getElementById('txtIngredients');
    var instructions = document.getElementById('txtSteps');
    var stepPhotos = document.getElementById('txtStepImages');
    var ingredientList = document.getElementById('recipeIngredientRows');
    var stepList = document.getElementById('recipeStepRows');
    var serial = 0;
    var urls = new Set();
    function lines(text) { return (text || '').replace(/\r\n/g, '\n').split('\n'); }
    function singleLine(text) { return text.replace(/[\r\n]+/g, ' ').trim(); }
    function revoke(row) { if (row.previewUrl) { URL.revokeObjectURL(row.previewUrl); urls.delete(row.previewUrl); row.previewUrl = null; } }
    function sync() {
        ingredients.value = Array.from(ingredientList.children).map(function (row) {
            return [row.querySelector('[data-name]').value, row.querySelector('[data-quantity]').value, row.querySelector('[data-unit]').value].map(singleLine).join(' | ');
        }).join('\n');
        var text = [], photos = [];
        Array.from(stepList.children).forEach(function (row, index) {
            row.querySelector('[data-step-number]').textContent = 'Step ' + (index + 1);
            row.querySelector('input[type=file]').name = 'stepUpload_' + index;
            row.querySelector('[data-remove-step]').setAttribute('aria-label', 'Remove step ' + (index + 1));
            text.push(singleLine(row.querySelector('textarea').value));
            photos.push(row.existingPhoto || '');
        });
        instructions.value = text.join('\n'); stepPhotos.value = photos.join('\n');
    }
    function ingredientRow(name, quantity, unit) {
        var row = document.createElement('div'), id = 'ingredient' + (++serial);
        row.className = 'recipe-ingredient-row';
        row.innerHTML = '<div><label for="' + id + 'Name">Ingredient</label><input id="' + id + 'Name" class="form-control" data-name maxlength="50" placeholder="e.g. Rice" required></div><div><label for="' + id + 'Quantity">Quantity</label><input id="' + id + 'Quantity" class="form-control" data-quantity type="number" min="0" max="99999999.99" step="0.01" placeholder="300" required></div><div><label for="' + id + 'Unit">Unit</label><input id="' + id + 'Unit" class="form-control" data-unit maxlength="30" list="recipeUnits" placeholder="g" required></div><button type="button" class="recipe-remove-row" aria-label="Remove ingredient"><i class="bi bi-trash" aria-hidden="true"></i></button>';
        row.querySelector('[data-name]').value = name || '';
        row.querySelector('[data-quantity]').value = quantity || '';
        row.querySelector('[data-unit]').value = unit || '';
        row.addEventListener('input', function () {
            [row.querySelector('[data-name]'), row.querySelector('[data-unit]')].forEach(function (input) {
                input.setCustomValidity(/[|\r\n]/.test(input.value) ? 'Use a name or unit without vertical bars or line breaks.' : input.value.trim() ? '' : 'Fill in this field.');
            });
            sync();
        });
        row.querySelector('button').addEventListener('click', function () { row.remove(); if (!ingredientList.children.length) ingredientRow('', '', ''); sync(); });
        ingredientList.appendChild(row);
        return row;
    }
    function photoSource(value) {
        if (/^https?:\/\//i.test(value)) return value;
        var normalized = value.replace('/Content/Uploads/Steps/', '/Content/images/Uploads/Steps/');
        return /^\/Content\/images\/Uploads\/Steps\/[a-f0-9]{32}\.jpg$/i.test(normalized) ? normalized : '';
    }
    function stepRow(text, existingPhoto) {
        var row = document.createElement('div'), id = 'recipeStep' + (++serial);
        row.className = 'recipe-step-card'; row.existingPhoto = existingPhoto || '';
        row.innerHTML = '<div class="recipe-step-title"><strong data-step-number></strong><button type="button" class="recipe-remove-row" data-remove-step><i class="bi bi-trash" aria-hidden="true"></i></button></div><label for="' + id + 'Text">What should the cook do?</label><textarea id="' + id + 'Text" class="form-control" rows="3" maxlength="2000" placeholder="e.g. Rinse the rice until the water runs clear, then drain." required></textarea><div class="recipe-step-photo"><label for="' + id + 'Photo"><i class="bi bi-camera" aria-hidden="true"></i> Step photo <span>(optional)</span></label><input id="' + id + 'Photo" type="file" class="form-control" accept="image/jpeg,image/png,image/gif"><span class="form-text">JPG, PNG or GIF · Up to 5 MB.</span><p class="small text-danger" data-step-error role="status"></p><img class="recipe-step-preview" alt="Step photo preview" hidden><button type="button" class="recipe-remove-photo" hidden>Remove photo</button></div>';
        var instruction = row.querySelector('textarea'), upload = row.querySelector('input[type=file]'), preview = row.querySelector('img'), removePhoto = row.querySelector('.recipe-remove-photo'), error = row.querySelector('[data-step-error]');
        instruction.value = text || '';
        function showPhoto(src) {
            preview.hidden = !src; removePhoto.hidden = !src;
            if (src) preview.src = src; else preview.removeAttribute('src');
        }
        preview.addEventListener('error', function () { preview.hidden = true; });
        showPhoto(photoSource(row.existingPhoto));
        instruction.addEventListener('input', function () { instruction.setCustomValidity(instruction.value.trim() ? '' : 'Describe this cooking step.'); sync(); });
        upload.addEventListener('change', function () {
            revoke(row); error.textContent = '';
            var file = upload.files && upload.files[0];
            if (!file) { showPhoto(photoSource(row.existingPhoto)); return; }
            if (!/^image\/(jpeg|png|gif)$/.test(file.type) || file.size > 5 * 1024 * 1024) {
                error.textContent = file.size > 5 * 1024 * 1024 ? 'Choose an image smaller than 5 MB.' : 'Choose a JPG, PNG or GIF image.';
                upload.value = ''; showPhoto(photoSource(row.existingPhoto)); return;
            }
            row.previewUrl = URL.createObjectURL(file); urls.add(row.previewUrl); showPhoto(row.previewUrl); sync();
        });
        removePhoto.addEventListener('click', function () { revoke(row); row.existingPhoto = ''; upload.value = ''; error.textContent = ''; showPhoto(''); sync(); });
        row.querySelector('[data-remove-step]').addEventListener('click', function () { revoke(row); row.remove(); if (!stepList.children.length) stepRow('', ''); sync(); });
        stepList.appendChild(row);
        return row;
    }
    lines(ingredients.value).filter(function (line) { return line.trim(); }).forEach(function (line) { var p = line.split('|'); ingredientRow(p[0] && p[0].trim(), p[1] && p[1].trim(), p[2] && p[2].trim()); });
    if (!ingredientList.children.length) ingredientRow('', '', '');
    var photos = lines(stepPhotos.value);
    lines(instructions.value).forEach(function (text, index) { if (text.trim()) stepRow(text, photos[index] || ''); });
    if (!stepList.children.length) stepRow('', '');
    page.querySelector('[data-add-ingredient]').addEventListener('click', function () { var row = ingredientRow('', '', ''); sync(); row.querySelector('input').focus(); });
    page.querySelector('[data-add-step]').addEventListener('click', function () { var row = stepRow('', ''); sync(); row.querySelector('textarea').focus(); });
    page.querySelectorAll('[data-ingredient-fallback], [data-step-fallback]').forEach(function (area) { area.hidden = true; });
    ingredientList.hidden = stepList.hidden = false;
    page.querySelectorAll('[data-add-ingredient], [data-add-step]').forEach(function (button) { button.hidden = false; });
    page.closest('form').addEventListener('submit', sync);
    sync();
    var mainUpload = document.getElementById('uploadRecipeImage'), existing = document.getElementById('imgExistingRecipe'), coverPreview = document.getElementById('recipeImagePreview'), empty = page.querySelector('[data-recipe-photo-empty]');
    function coverState() { var file = mainUpload.files && mainUpload.files[0]; var selected = !!file && /^image\/(jpeg|png|gif)$/.test(file.type) && file.size <= 5 * 1024 * 1024; if (existing) existing.hidden = selected; empty.hidden = selected || !!existing; if (!selected) coverPreview.classList.remove('is-visible'); }
    mainUpload.addEventListener('change', coverState); coverState();
    window.addEventListener('pagehide', function () { urls.forEach(function (value) { URL.revokeObjectURL(value); }); });
}());
