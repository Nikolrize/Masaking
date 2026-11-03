(function () {
    var eye = '<i class="bi bi-eye" aria-hidden="true"></i>';
    var eyeOff = '<i class="bi bi-eye-slash" aria-hidden="true"></i>';
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
        if (!file) return;
        if (!/^image\/(jpeg|png|gif)$/.test(file.type) || file.size > 5 * 1024 * 1024) {
            if (error) error.textContent = file.size > 5 * 1024 * 1024 ? 'Choose an image smaller than 5 MB.' : 'Choose a JPG, PNG or GIF image.';
            input.value = ''; return;
        }
        input._previewUrl = URL.createObjectURL(file); preview.src = input._previewUrl; preview.classList.add('is-visible');
        if (avatar) avatar.src = input._previewUrl;
    });

}());
