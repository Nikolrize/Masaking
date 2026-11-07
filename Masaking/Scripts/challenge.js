(function () {
    "use strict";
    const timer = document.getElementById("challengeTimer");
    const answers = document.getElementById("rblAnswers");
    const saveState = document.getElementById("lblSaveState");
    if (answers && saveState) {
        answers.addEventListener("change", function () {
            saveState.textContent = "Saving your answer...";
        });
    }
    if (!timer) return;

    const label = document.getElementById("lblRemaining");
    const bar = document.getElementById("timeBar");
    const warning = document.getElementById("timerWarning");
    const timeoutButton = document.getElementById("btnTimeout");
    const running = timer.dataset.running === "true";
    const remaining = Number(timer.dataset.seconds);
    const total = Number(timer.dataset.total);
    if (!Number.isFinite(remaining) || !Number.isFinite(total) || total <= 0) return;

    //deadline for display. the server checks elapsed time on every action
    const deadline = Date.now() + remaining * 1000;
    let timeoutSent = false;
    let interval = null;

    function updateCountdown() {
        const seconds = Math.max(0, running ? Math.ceil((deadline - Date.now()) / 1000) : remaining);
        label.textContent = "Time remaining: " + String(Math.floor(seconds / 60)).padStart(2, "0")
            + ":" + String(seconds % 60).padStart(2, "0");
        bar.style.width = Math.min(100, 100 * seconds / total) + "%";
        bar.setAttribute("aria-valuenow", String(Math.min(total, seconds)));
        timer.classList.toggle("time-low", running && seconds <= 10);
        if (running && seconds <= 10) warning.hidden = false;

        if (running && seconds === 0 && !timeoutSent) {
            timeoutSent = true;
            clearInterval(interval);
            warning.textContent = "Time is up. Saving your result...";
            if (timeoutButton) timeoutButton.click();
        }
    }

    if (running) {
        interval = setInterval(updateCountdown, 250);
        document.addEventListener("visibilitychange", updateCountdown);
    }
    updateCountdown();
    window.addEventListener("pagehide", function () { clearInterval(interval); });
    window.addEventListener("pageshow", function (event) {
        if (event.persisted) window.location.reload();
    });
})();
