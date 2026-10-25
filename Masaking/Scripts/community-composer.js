(function () {
    'use strict';

    var composer = document.querySelector('[data-community-composer]');
    if (!composer) return;

    var content = document.getElementById('txtContent');
    var counter = document.getElementById('postCharacterCount');
    function updateCount() {
        counter.textContent = content.value.length + '/2000';
    }
    content.addEventListener('input', updateCount);
    updateCount();

    var upload = document.getElementById('uploadImage');
    var preview = document.getElementById('composerPhotoPreview');
    var existing = document.getElementById('imgExisting');
    var remove = document.getElementById('chkRemoveImage');
    var photoError = document.getElementById('composerPhotoError');
    var drop = composer.querySelector('[data-photo-drop]');
    var previewUrl = null;

    function clearPreview() {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = null;
        preview.removeAttribute('src');
        preview.classList.add('d-none');
    }

    function previewPhoto() {
        clearPreview();
        photoError.textContent = '';
        var file = upload.files && upload.files[0];
        if (!file) {
            if (existing) existing.classList.toggle('d-none', remove.checked);
            return;
        }
        if (file.size > 5 * 1024 * 1024 || !/^image\/(jpeg|png|gif)$/.test(file.type)) {
            photoError.textContent = file.size > 5 * 1024 * 1024
                ? 'Choose an image no larger than 5 MB.' : 'Choose a JPG, PNG or GIF image.';
            upload.value = '';
            if (existing) existing.classList.toggle('d-none', remove.checked);
            return;
        }
        remove.checked = false;
        if (existing) existing.classList.add('d-none');
        previewUrl = URL.createObjectURL(file);
        preview.src = previewUrl;
        preview.classList.remove('d-none');
    }

    upload.addEventListener('change', previewPhoto);
    remove.addEventListener('change', function () {
        if (remove.checked) { upload.value = ''; clearPreview(); }
        if (existing) existing.classList.toggle('d-none', remove.checked);
    });
    ['dragenter', 'dragover'].forEach(function (name) {
        drop.addEventListener(name, function (event) {
            event.preventDefault();
            drop.classList.add('community-upload-active');
        });
    });
    drop.addEventListener('dragleave', function () { drop.classList.remove('community-upload-active'); });
    drop.addEventListener('drop', function (event) {
        event.preventDefault();
        drop.classList.remove('community-upload-active');
        if (!event.dataTransfer.files.length) return;
        if (event.dataTransfer.files.length !== 1) {
            photoError.textContent = 'Choose one photo per post.';
            return;
        }
        upload.files = event.dataTransfer.files;
        previewPhoto();
    });
    window.addEventListener('pagehide', function () {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
    });

    var recipes = document.getElementById('ddlRecipe');
    var search = document.getElementById('composerRecipeSearch');
    var card = document.getElementById('composerRecipeCard');
    var image = document.getElementById('composerRecipeImage');
    var hint = document.getElementById('composerRecipeHint');
    // Clone the server-rendered options so searches preserve values and metadata.
    var allOptions = Array.from(recipes.options).map(function (option) { return option.cloneNode(true); });

    function showRecipe() {
        var option = recipes.options[recipes.selectedIndex];
        var selected = option && option.value !== '';
        card.classList.toggle('d-none', !selected);
        hint.classList.toggle('d-none', !!selected);
        if (!selected) return;
        document.getElementById('composerRecipeTitle').textContent = option.text;
        document.getElementById('composerRecipeSummary').textContent = option.dataset.summary || '';
        image.classList.toggle('d-none', !option.dataset.image);
        if (option.dataset.image) image.src = option.dataset.image;
        else image.removeAttribute('src');
    }

    search.addEventListener('input', function () {
        var selected = recipes.value;
        var term = search.value.trim().toLowerCase();
        recipes.replaceChildren();
        allOptions.forEach(function (option) {
            // Keep the attached recipe selected when a new search is entered.
            if (!option.value || option.value === selected || option.text.toLowerCase().includes(term)) {
                recipes.appendChild(option.cloneNode(true));
            }
        });
        recipes.value = selected;
        showRecipe();
    });
    recipes.addEventListener('change', showRecipe);
    document.getElementById('composerRemoveRecipe').addEventListener('click', function () {
        recipes.value = '';
        showRecipe();
    });
    showRecipe();
}());
