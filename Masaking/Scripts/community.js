(function () {
    // Filter only the posts already rendered on this page; no database requests.
    var page = document.querySelector('.community-view');
    var search = document.getElementById('communitySearch');
    if (!page) return;
    var filter = 'all';
    function filterPosts() {
        var term = search.value.trim().toLowerCase();
        var visible = 0;
        page.querySelectorAll('.community-feed-post').forEach(function (post) {
            var copy = post.querySelector('.community-copy').textContent.toLowerCase();
            var text = post.textContent.toLowerCase();
            var recipe = post.dataset.hasRecipe === 'true';
            var matches = filter === 'all' || (filter === 'recipes' && recipe) || (filter === 'discussions' && !recipe);
            var show = matches && text.includes(term);
            post.classList.toggle('d-none', !show);
            if (show) visible++;
        });
        document.getElementById('communityFilterEmpty').classList.toggle('d-none', visible > 0 || !page.querySelector('.community-feed-post'));
    }
    if (search) search.addEventListener('input', filterPosts);
    page.querySelectorAll('[data-community-filter]').forEach(function (button) {
        button.addEventListener('click', function () {
            filter = button.dataset.communityFilter;
            page.querySelectorAll('[data-community-filter]').forEach(function (item) {
                var active = item === button;
                item.classList.toggle('btn-primary', active);
                item.classList.toggle('btn-outline-secondary', !active);
                item.setAttribute('aria-pressed', String(active));
            });
            filterPosts();
        });
    });
    document.addEventListener('click', function (event) {
        var editButton = event.target.closest('[data-close-post-menu]');
        if (editButton) editButton.closest('details').open = false;
        page.querySelectorAll('.community-menu[open]').forEach(function (menu) {
            if (!menu.contains(event.target)) menu.open = false;
        });
    });
    document.addEventListener('click', function (event) {
        var button = event.target.closest('[data-confirm]');
        if (button && !window.confirm(button.dataset.confirm)) {
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    }, true);
    page.addEventListener('keydown', function (event) {
        if (event.key !== 'Escape') return;
        page.querySelectorAll('.community-menu[open]').forEach(function (menu) {
            menu.open = false;
            menu.querySelector('summary').focus();
        });
    });
}());

(function () {
    var pendingLikeId = null;
    var likeScroll = null;
    var restoreLikeFocus = false;
    var initialized = false;
    var requestedLikeButton = null;

    function rememberScroll() {
        likeScroll = { left: window.scrollX, top: window.scrollY };
    }

    function restoreScroll(position) {
        // Override smooth scrolling so restoring the viewport does not animate.
        window.scrollTo({ left: position.left, top: position.top, behavior: 'instant' });
    }

    function pop(button, liked) {
        button.dataset.likeAnimation = liked ? 'like' : 'unlike';
        button.classList.remove('is-popping');
        void button.offsetWidth;
        button.classList.add('is-popping');
    }

    // Send likes asynchronously, including clicks on the heart icon.
    document.addEventListener('click', function (event) {
        var button = event.target.closest('.community-like');
        if (!button) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        var status = button.parentElement.querySelector('[data-like-error]');
        initializeLikeRequests();
        if (!initialized || !button.dataset.likeTarget || !button.dataset.likePanel) {
            if (status) status.textContent = 'Likes are temporarily unavailable. Please reload the page and try again.';
            return;
        }
        var manager = Sys.WebForms.PageRequestManager.getInstance();
        if (pendingLikeId || manager.get_isInAsyncPostBack()) return;
        if (status) status.textContent = '';
        pop(button, button.getAttribute('aria-pressed') !== 'true');
        requestedLikeButton = button;
        try {
            manager.beginAsyncPostBack([button.dataset.likePanel], button.dataset.likeTarget, '', false);
        } finally {
            requestedLikeButton = null;
        }
    }, true);

    document.addEventListener('animationend', function (event) {
        var button = event.target.closest('.community-like');
        if (button) button.classList.remove('is-popping');
    });

    function initializeLikeRequests() {
        if (initialized || !window.Sys || !Sys.WebForms) return;
        initialized = true;
        var manager = Sys.WebForms.PageRequestManager.getInstance();

        manager.add_initializeRequest(function (sender, args) {
            var button = args.get_postBackElement() || requestedLikeButton;
            if (!button || !button.classList.contains('community-like')) return;
            // Avoid a new click cancelling an earlier save on this page.
            if (pendingLikeId || manager.get_isInAsyncPostBack()) {
                args.set_cancel(true);
                return;
            }
            pendingLikeId = button.id;
            rememberScroll();
            restoreLikeFocus = document.activeElement === button;
            button.setAttribute('aria-busy', 'true');
            button.setAttribute('aria-disabled', 'true');
        });

        manager.add_pageLoading(function () {
            // Take the latest position just before the panel is replaced. This
            // also respects any scrolling while the request was in progress.
            if (!pendingLikeId) return;
            rememberScroll();
            restoreLikeFocus = document.activeElement === document.getElementById(pendingLikeId);
        });

        manager.add_endRequest(function (sender, args) {
            if (!pendingLikeId) return;
            var button = document.getElementById(pendingLikeId);
            var position = likeScroll;
            var shouldFocus = restoreLikeFocus;
            pendingLikeId = null;
            likeScroll = null;
            // ASP.NET finishes its own focus handling after endRequest. Restore
            // on the next frame, after that work, without scrolling to the link.
            window.requestAnimationFrame(function () {
                if (button && shouldFocus) button.focus({ preventScroll: true });
                if (position) restoreScroll(position);
            });
            if (!button) return;
            button.removeAttribute('aria-busy');
            button.removeAttribute('aria-disabled');
            var status = button.parentElement.querySelector('[data-like-error]');
            if (args.get_error()) {
                if (status) status.textContent = 'Your like could not be saved. Please try again.';
                args.set_errorHandled(true);
                return;
            }
            if (!status || !status.textContent.trim()) pop(button, button.getAttribute('aria-pressed') === 'true');
        });
    }

    window.addEventListener('load', initializeLikeRequests);
    if (window.Sys && Sys.Application) Sys.Application.add_load(initializeLikeRequests);
}());

(function () {
    var button = document.getElementById('communityBackToTop');
    if (!button) return;
    function updateVisibility() {
        button.hidden = window.scrollY < Math.max(400, window.innerHeight * .75);
    }
    window.addEventListener('scroll', updateVisibility, { passive: true });
    window.addEventListener('resize', updateVisibility);
    updateVisibility();
    button.addEventListener('click', function () {
        var heading = document.querySelector('.community-feed-view h1');
        if (heading) {
            heading.setAttribute('tabindex', '-1');
            heading.focus({ preventScroll: true });
        }
        var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        window.scrollTo({ top: 0, behavior: reduceMotion ? 'instant' : 'smooth' });
    });
}());

(function () {
    // Keep the server select as the source of truth while styling its option list.
    document.querySelectorAll('select[data-community-sort]').forEach(function (select) {
        if (select.dataset.sortEnhanced) return;
        select.dataset.sortEnhanced = 'true';
        var dropdown = document.createElement('details');
        dropdown.className = 'community-sort-dropdown';
        var summary = document.createElement('summary');
        summary.className = 'community-sort-toggle';
        summary.setAttribute('aria-label', select.dataset.communitySort);
        var caption = document.createElement('span');
        summary.appendChild(caption);
        var arrow = document.createElement('span');
        arrow.className = 'community-sort-arrow';
        arrow.setAttribute('aria-hidden', 'true');
        arrow.innerHTML = '<i class="bi bi-chevron-down community-icon" aria-hidden="true"></i>';
        summary.appendChild(arrow);
        var list = document.createElement('div');
        list.className = 'community-sort-options';
        function sync() {
            caption.textContent = select.options[select.selectedIndex].text;
            list.querySelectorAll('button').forEach(function (button) {
                button.setAttribute('aria-pressed', String(button.dataset.value === select.value));
            });
        }
        Array.prototype.forEach.call(select.options, function (option) {
            var button = document.createElement('button');
            button.type = 'button';
            button.className = 'community-sort-option';
            button.textContent = option.text;
            button.dataset.value = option.value;
            button.disabled = option.disabled;
            button.addEventListener('click', function () {
                var changed = select.value !== option.value;
                select.value = option.value;
                sync();
                dropdown.open = false;
                summary.focus({ preventScroll: true });
                if (changed) select.dispatchEvent(new Event('change', { bubbles: true }));
            });
            list.appendChild(button);
        });
        dropdown.appendChild(summary);
        dropdown.appendChild(list);
        select.insertAdjacentElement('afterend', dropdown);
        sync();
        select.hidden = true;
        select.addEventListener('change', sync);
        document.addEventListener('click', function (event) {
            if (!dropdown.contains(event.target)) dropdown.open = false;
        });
        dropdown.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                dropdown.open = false;
                summary.focus();
            }
        });
    });
}());

(function () {
    function fallback(image) {
        if (!image || !image.hasAttribute('data-community-avatar-photo')) return;
        image.hidden = true;
        image.closest('.community-avatar').dataset.avatarPhoto = 'false';
    }
    document.addEventListener('error', function (event) { fallback(event.target); }, true);
    document.querySelectorAll('[data-community-avatar-photo]').forEach(function (image) {
        if (image.complete && image.naturalWidth === 0) fallback(image);
    });
}());
