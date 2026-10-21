(function () {
    var eye = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z"/><circle cx="12" cy="12" r="3"/></svg>';
    var eyeOff = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m3 3 18 18M10.6 10.6a2 2 0 0 0 2.8 2.8"/><path d="M9.9 5.2A10.7 10.7 0 0 1 12 5c6.4 0 10 7 10 7a14 14 0 0 1-3.1 3.7M6.2 6.3C3.5 8.1 2 12 2 12s3.6 7 10 7a10.4 10.4 0 0 0 3-.4"/></svg>';
    document.addEventListener('click', function (event) {
        var button = event.target.closest('[data-toggle-password]');
        if (!button) return;
        var input = document.getElementById(button.getAttribute('data-toggle-password'));
        if (!input) return;
        var show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        button.innerHTML = show ? eyeOff : eye;
        button.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        button.setAttribute('aria-pressed', show ? 'true' : 'false');
    });
    document.addEventListener('change', function (event) {
        var input = event.target.closest('[data-image-preview]');
        if (!input) return;
        var preview = document.getElementById(input.getAttribute('data-image-preview'));
        var errorId = input.getAttribute('data-image-error') || (input.getAttribute('data-image-preview') === 'profileAvatarPreview' ? 'profileAvatarError' : 'communityImageError');
        var error = document.getElementById(errorId);
        if (error) error.textContent = '';
        if (!preview) return;
        if (input._previewUrl) URL.revokeObjectURL(input._previewUrl);
        input._previewUrl = null; preview.removeAttribute('src'); preview.classList.remove('is-visible');
        var file = input.files && input.files[0];
        var avatar = input.getAttribute('data-image-preview') === 'profileAvatarPreview' ? document.getElementById('imgAvatar') : null;
        if (avatar) {
            if (!avatar.dataset.savedImageUrl) avatar.dataset.savedImageUrl = avatar.src;
            avatar.src = avatar.dataset.savedImageUrl;
        }
        if (!file) {
            if (input.name.indexOf('stepUpload_') === 0) {
                var stepUrl = document.querySelector('.step-image-url[data-step-line="' + input.name.substring('stepUpload_'.length) + '"]');
                if (stepUrl && /^https?:\/\//i.test(stepUrl.value.trim())) { preview.src = stepUrl.value.trim(); preview.classList.add('is-visible'); }
            }
            return;
        }
        if (!/^image\/(jpeg|png|gif)$/.test(file.type) || file.size > 5 * 1024 * 1024) {
            if (error) error.textContent = file.size > 5 * 1024 * 1024 ? 'Choose an image smaller than 5 MB.' : 'Choose a JPG, PNG or GIF image.';
            input.value = ''; return;
        }
        input._previewUrl = URL.createObjectURL(file); preview.src = input._previewUrl; preview.classList.add('is-visible');
        if (avatar) avatar.src = input._previewUrl;
    });

    function initStepImageEditors() {
        var instructions = document.querySelector('[data-step-instructions]');
        var imageState = document.querySelector('[data-step-image-state]');
        var editors = document.getElementById('stepImageEditors');
        if (!instructions || !imageState || !editors) return;

        function lines(value) { return (value || '').replace(/\r\n/g, '\n').split('\n'); }
        function syncImageState() {
            var values = lines(instructions.value).map(function () { return ''; });
            editors.querySelectorAll('.step-image-url').forEach(function (field) { values[Number(field.getAttribute('data-step-line'))] = field.value.trim(); });
            imageState.value = values.join('\n');
        }
        function render() {
            var imageLines = lines(imageState.value);
            editors.querySelectorAll('.step-image-url').forEach(function (field) { imageLines[Number(field.getAttribute('data-step-line'))] = field.value.trim(); });
            imageState.value = imageLines.join('\n');
            var instructionLines = lines(instructions.value);
            editors.innerHTML = '';
            instructionLines.forEach(function (instruction, index) {
                if (!instruction.trim()) return;
                var row = document.createElement('div');
                row.className = 'step-image-editor';
                var heading = document.createElement('strong');
                heading.className = 'step-image-heading';
                heading.textContent = 'Step ' + (index + 1) + ': ' + instruction.trim();
                row.appendChild(heading);

                var fields = document.createElement('div');
                fields.className = 'row g-3 mt-1';
                var urlColumn = document.createElement('div');
                urlColumn.className = 'col-md-6';
                var urlLabel = document.createElement('label');
                urlLabel.className = 'form-label'; urlLabel.textContent = 'Image URL';
                var url = document.createElement('input');
                url.type = 'text'; url.name = 'stepUrl_' + index; url.className = 'form-control step-image-url';
                url.placeholder = 'https://example.com/step.jpg'; url.setAttribute('data-step-line', index);
                url.value = imageLines[index] || '';
                var previewId = 'stepPreview_' + index;
                urlColumn.appendChild(urlLabel); urlColumn.appendChild(url);

                var uploadColumn = document.createElement('div');
                uploadColumn.className = 'col-md-6';
                var uploadLabel = document.createElement('label');
                uploadLabel.className = 'form-label'; uploadLabel.textContent = 'Upload from device';
                var upload = document.createElement('input');
                upload.type = 'file'; upload.name = 'stepUpload_' + index; upload.accept = 'image/jpeg,image/png,image/gif';
                upload.className = 'form-control'; upload.setAttribute('data-image-preview', 'stepPreview_' + index); upload.setAttribute('data-image-error', 'stepError_' + index);
                var error = document.createElement('div'); error.id = 'stepError_' + index; error.className = 'small text-danger mt-1';
                var preview = document.createElement('img'); preview.id = previewId; preview.className = 'image-preview step-image-preview mt-2'; preview.alt = 'Step ' + (index + 1) + ' image preview';
                function showUrlPreview() {
                    if (upload.files && upload.files.length) return;
                    var value = url.value.trim();
                    if (/^https?:\/\//i.test(value)) { preview.src = value; preview.classList.add('is-visible'); }
                    else { preview.removeAttribute('src'); preview.classList.remove('is-visible'); }
                }
                url.addEventListener('input', showUrlPreview);
                showUrlPreview();
                uploadColumn.appendChild(uploadLabel); uploadColumn.appendChild(upload); uploadColumn.appendChild(error); uploadColumn.appendChild(preview);
                fields.appendChild(urlColumn); fields.appendChild(uploadColumn); row.appendChild(fields); editors.appendChild(row);
            });
        }

        instructions.addEventListener('input', render);
        instructions.form.addEventListener('submit', syncImageState);
        render();
    }

    initStepImageEditors();
}());
