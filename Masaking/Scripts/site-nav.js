(function () {
    'use strict';
    var search = document.querySelector('[data-nav-search]');
    if (search) {
        var input = search.querySelector('input');
        function submitSearch(event) {
            event.preventDefault();
            var query = input.value.trim();
            if (!query) { input.focus(); return; }
            window.location.assign(search.dataset.searchUrl + '?q=' + encodeURIComponent(query));
        }
        search.querySelector('[data-nav-search-submit]').addEventListener('click', submitSearch);
        input.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') submitSearch(event);
            if (event.key === 'Escape') { input.value = ''; input.blur(); }
        });
    }
    var activity = document.querySelector('[data-nav-activity]');
    if (activity) {
        activity.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') { activity.open = false; activity.querySelector('summary').focus(); }
        });
        document.addEventListener('click', function (event) { if (!activity.contains(event.target)) activity.open = false; });
    }
    document.querySelectorAll('.site-nav-avatar').forEach(function (image) {
        image.addEventListener('error', function () { image.hidden = true; });
        if (image.complete && image.naturalWidth === 0) image.hidden = true;
    });
}());
