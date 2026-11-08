(function () {
    "use strict";
    let timer = null;
    const searches = document.querySelectorAll("input[data-auto-filter]");
    for (const input of searches) {
        const key = "admin-search:" + location.pathname + ":" + input.id;
        try {
            const saved = JSON.parse(sessionStorage.getItem(key) || "null");
            sessionStorage.removeItem(key);
            if (saved && saved.value === input.value) {
                input.focus({ preventScroll: true });
                input.setSelectionRange(saved.start, saved.end);
            }
        } catch (_) { /* filtering also works if browser storage is unavailable */ }

        input.addEventListener("input", function () {
            clearTimeout(timer);
            timer = setTimeout(function () {
                const button = document.getElementById(input.dataset.filterTarget);
                if (!button) return;
                try {
                    sessionStorage.setItem(key, JSON.stringify({
                        value: input.value, start: input.selectionStart, end: input.selectionEnd
                    }));
                } catch (_) { }
                button.click();
            }, 650);
        });
    }
    window.addEventListener("beforeunload", function () { clearTimeout(timer); });
})();
