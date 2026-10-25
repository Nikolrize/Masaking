(function () {
    'use strict';
    document.querySelectorAll('[data-profile-avatar], [data-profile-photo]').forEach(function (image) {
        var avatar = image.hasAttribute('data-profile-avatar') ? image.closest('.profile-avatar') : null;
        function fallback() {
            image.hidden = true;
            if (avatar) avatar.classList.remove('has-photo');
        }
        function loaded() {
            if (!image.getAttribute('src') || image.getAttribute('src').indexOf('data:image/gif;base64,') === 0) { fallback(); return; }
            image.hidden = false;
            if (avatar) avatar.classList.add('has-photo');
        }
        image.addEventListener('error', fallback);
        image.addEventListener('load', loaded);
        if (image.complete) {
            if (image.naturalWidth > 0) loaded();
            else fallback();
        }
    });
    var screen = document.querySelector('[data-profile-screen]');
    if (screen) {
        var rows = Array.prototype.slice.call(screen.querySelectorAll('[data-activity-kind]'));
        function selectTab(tab) {
            if (['overview', 'recipes', 'posts', 'comments'].indexOf(tab) < 0) return;
            screen.dataset.activeTab = tab;
            screen.querySelectorAll('.profile-tabs [data-profile-tab]').forEach(function (button) {
                var active = button.dataset.profileTab === tab;
                button.classList.toggle('is-active', active);
                button.setAttribute('aria-pressed', String(active));
            });
            screen.querySelector('[data-profile-recipes]').hidden = tab === 'posts' || tab === 'comments';
            screen.querySelector('[data-profile-activity]').hidden = tab === 'recipes';
            var shown = 0;
            rows.forEach(function (row) {
                var visible = tab === 'overview' ? shown < 3 : row.dataset.activityKind === (tab === 'comments' ? 'comment' : 'post');
                row.hidden = !visible;
                if (visible) shown++;
            });
            screen.querySelector('[data-activity-empty]').hidden = shown > 0;
            screen.querySelector('[data-activity-heading]').textContent = tab === 'posts' ? 'My Posts' : tab === 'comments' ? 'My Comments' : 'Recent Community Activity';
        }
        screen.querySelectorAll('[data-profile-tab]').forEach(function (button) {
            button.addEventListener('click', function (event) { event.preventDefault(); selectTab(button.dataset.profileTab); });
        });
        var requestedTab = new URLSearchParams(window.location.search).get('tab');
        selectTab(['overview', 'recipes', 'posts', 'comments'].indexOf(requestedTab) >= 0 ? requestedTab : 'overview');
    }
    var rail = document.querySelector('[data-profile-rail]');
    if (rail) {
        function railTab(tab) {
            var shown = 0;
            rail.querySelectorAll('[data-rail-kind]').forEach(function (row) {
                var visible = row.dataset.railKind === tab;
                row.hidden = !visible;
                if (visible) shown++;
            });
            rail.querySelectorAll('[data-rail-tab]').forEach(function (button) {
                var active = button.dataset.railTab === tab;
                button.classList.toggle('is-active', active);
                button.setAttribute('aria-pressed', String(active));
            });
            rail.querySelector('[data-rail-empty]').hidden = shown > 0;
            rail.querySelector('[data-rail-label]').textContent = (tab === 'post' ? 'My Posts' : 'My Comments') + ' (' + shown + ')';
        }
        rail.querySelectorAll('[data-rail-tab]').forEach(function (button) { button.addEventListener('click', function () { railTab(button.dataset.railTab); }); });
        railTab('post');
    }
    document.querySelectorAll('[data-profile-password]').forEach(function (button) {
        button.addEventListener('click', function () {
            var input = document.getElementById(button.dataset.profilePassword);
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            button.innerHTML = '<i class="bi bi-' + (show ? 'eye-slash' : 'eye') + ' profile-icon" aria-hidden="true"></i>';
            button.setAttribute('aria-pressed', String(show));
            button.setAttribute('aria-label', (show ? 'Hide' : 'Show') + ' password');
        });
    });
    var upload = document.querySelector('[data-profile-upload]');
    if (upload) {
        var previewUrl = null;
        upload.addEventListener('change', function () {
            var image = document.getElementById('imgAvatar');
            var error = document.getElementById('profilePhotoError');
            if (!image.dataset.savedSrc) image.dataset.savedSrc = image.getAttribute('src') || '';
            if (previewUrl) { URL.revokeObjectURL(previewUrl); previewUrl = null; }
            image.src = image.dataset.savedSrc;
            image.hidden = false;
            error.textContent = '';
            var file = upload.files && upload.files[0];
            if (!file) return;
            if (!/^image\/(jpeg|png|gif)$/.test(file.type) || file.size > 5 * 1024 * 1024) {
                error.textContent = file.size > 5 * 1024 * 1024 ? 'Choose an image smaller than 5 MB.' : 'Choose a JPG, PNG or GIF image.';
                upload.value = '';
                return;
            }
            previewUrl = URL.createObjectURL(file);
            image.src = previewUrl;
            image.hidden = false;
        });
        window.addEventListener('pagehide', function () { if (previewUrl) URL.revokeObjectURL(previewUrl); });
    }
    if (window.location.hash === '#security') {
        var password = document.getElementById('txtCurrentPassword');
        if (password) password.focus({ preventScroll: true });
    }
}());
