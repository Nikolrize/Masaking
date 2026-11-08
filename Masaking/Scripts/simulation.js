(function () {
    "use strict";
    const page = document.querySelector(".simulation-page");
    if (!page) return;
    const soundButton = document.getElementById("btnSound");
    const status = document.getElementById("simulationStatus");
    const reloadButton = document.getElementById("btnReloadSimulation");
    const reducedMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const manager = window.Sys && Sys.WebForms ? Sys.WebForms.PageRequestManager.getInstance() : null;
    let soundOn = false;
    let audio = null;
    let draggedCard = null;
    let selectedCard = null;
    let requestFailed = false;
    try { soundOn = localStorage.getItem("simulation-sound") === "on"; } catch (_) { }

    function busy() { return requestFailed || (manager && manager.get_isInAsyncPostBack()); }
    function message(text) { if (status) status.textContent = text; }
    function updateSoundButton() {
        if (!soundButton || soundButton.disabled) return;
        soundButton.textContent = soundOn ? "Sound: on" : "Sound: off";
        soundButton.setAttribute("aria-pressed", soundOn ? "true" : "false");
    }
    function unlockAudio() {
        if (!soundOn) return;
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) throw new Error("Audio unavailable");
            if (!audio) audio = new AudioContext();
            const resumed = audio.resume();
            if (resumed && resumed.catch) resumed.catch(function () { message("Sound could not start. You can continue without sound."); });
        } catch (_) {
            soundOn = false;
            updateSoundButton();
            if (soundButton) { soundButton.textContent = "Sound unavailable"; soundButton.disabled = true; }
        }
    }
    function tone(frequency, duration, delay) {
        if (!soundOn || !audio || audio.state !== "running") return;
        const oscillator = audio.createOscillator();
        const gain = audio.createGain();
        const start = audio.currentTime + (delay || 0);
        oscillator.connect(gain); gain.connect(audio.destination);
        oscillator.frequency.value = frequency;
        gain.gain.setValueAtTime(0.045, start);
        gain.gain.exponentialRampToValueAtTime(0.001, start + duration);
        oscillator.start(start); oscillator.stop(start + duration);
        oscillator.onended = function () { oscillator.disconnect(); gain.disconnect(); };
    }
    function sizzle() {
        if (!soundOn || !audio || audio.state !== "running") return;
        const buffer = audio.createBuffer(1, Math.floor(audio.sampleRate * 0.22), audio.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < data.length; i++) data[i] = Math.random() * 2 - 1;
        const source = audio.createBufferSource();
        const gain = audio.createGain();
        source.buffer = buffer; source.connect(gain); gain.connect(audio.destination);
        gain.gain.setValueAtTime(0.025, audio.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, audio.currentTime + 0.22);
        source.start();
        source.onended = function () { source.disconnect(); gain.disconnect(); };
    }
    function selectedIngredient() {
        return Array.from(page.querySelectorAll("[data-ingredient]")).find(function (card) {
            return card.dataset.ingredient === selectedCard;
        });
    }
    function flyIngredient(card, area, accepted) {
        if (reducedMotion || !card || !area || !Element.prototype.animate) return;
        const from = (accepted ? card : area).getBoundingClientRect();
        const to = (accepted ? area : card).getBoundingClientRect();
        const image = card.querySelector(".ingredient-image:not([hidden])");
        const ghost = document.createElement(image ? "img" : "span");
        if (image) { ghost.src = image.src; ghost.alt = ""; }
        else ghost.textContent = card.querySelector(".ingredient-name").textContent;
        ghost.className = "ingredient-flight";
        ghost.setAttribute("aria-hidden", "true");
        ghost.style.left = (from.left + from.width / 2 - 36) + "px";
        ghost.style.top = (from.top + from.height / 2 - 36) + "px";
        page.appendChild(ghost);
        const animation = ghost.animate([
            { transform: "translate(0,0) scale(1)", opacity: 1 },
            { transform: "translate(" + (to.left + to.width / 2 - from.left - from.width / 2) + "px,"
                + (to.top + to.height / 2 - from.top - from.height / 2) + "px) scale(.5)", opacity: 0 }
        ], { duration: 650, easing: "ease-in-out", fill: "forwards" });
        animation.onfinish = function () { ghost.remove(); };
        window.setTimeout(function () { ghost.remove(); }, 850);
    }
    function celebrate(panel) {
        if (!panel || reducedMotion) return;
        panel.classList.add("dish-complete");
        for (let i = 0; i < 12; i++) {
            const piece = document.createElement("span");
            piece.className = "completion-confetti";
            piece.setAttribute("aria-hidden", "true");
            piece.style.setProperty("--offset", ((i - 5.5) * 24) + "px");
            piece.style.setProperty("--turn", ((i % 2 ? 1 : -1) * (90 + i * 20)) + "deg");
            piece.style.animationDelay = ((i % 3) * 0.06) + "s";
            panel.appendChild(piece);
            window.setTimeout(function () { piece.remove(); }, 1200);
        }
    }
    function animateResult() {
        const effect = document.getElementById("hfEffect");
        if (!effect || !effect.value) return;
        // Effects are played only after the server confirms and saves the choice.
        const result = effect.value;
        effect.value = "";
        const area = document.getElementById("dropZone");
        const card = selectedIngredient();
        if (result === "correct") {
            if (area) {
                area.classList.add("cooking-correct");
                window.setTimeout(function () { area.classList.remove("cooking-correct"); }, 900);
            }
            flyIngredient(card, area, true);
            sizzle(); tone(660, 0.12, 0.12);
        } else if (result === "wrong") {
            if (card) {
                card.classList.add("ingredient-wrong");
                window.setTimeout(function () { card.classList.remove("ingredient-wrong"); }, 500);
            }
            flyIngredient(card, area, false);
            tone(180, 0.18);
        } else if (result === "complete") {
            celebrate(document.getElementById("pnlResult"));
            tone(523, 0.13); tone(659, 0.13, 0.15); tone(784, 0.2, 0.3);
        }
    }

    page.addEventListener("dragstart", function (event) {
        const card = event.target.closest("[data-ingredient]");
        if (!card || busy()) { event.preventDefault(); return; }
        unlockAudio(); draggedCard = card.dataset.ingredient;
        event.dataTransfer.setData("text/plain", draggedCard);
        event.dataTransfer.effectAllowed = "move";
    });
    page.addEventListener("dragover", function (event) {
        const area = event.target.closest("#dropZone");
        if (area && draggedCard && !busy()) {
            event.preventDefault(); event.dataTransfer.dropEffect = "move"; area.classList.add("drag-over");
        }
    });
    page.addEventListener("dragleave", function (event) {
        const area = event.target.closest("#dropZone");
        if (area && !area.contains(event.relatedTarget)) area.classList.remove("drag-over");
    });
    page.addEventListener("drop", function (event) {
        const area = event.target.closest("#dropZone");
        if (!area) return;
        event.preventDefault(); area.classList.remove("drag-over");
        if (!draggedCard || busy()) return;
        selectedCard = draggedCard;
        const input = document.getElementById("hfIngredient");
        const submit = document.getElementById("btnDrop");
        if (input && submit) { input.value = draggedCard; submit.click(); }
        draggedCard = null;
    });
    page.addEventListener("dragend", function () {
        draggedCard = null;
        const area = document.getElementById("dropZone");
        if (area) area.classList.remove("drag-over");
    });
    page.addEventListener("click", function (event) {
        const card = event.target.closest("[data-ingredient]");
        if (card) {
            if (busy()) { event.preventDefault(); return; }
            selectedCard = card.dataset.ingredient; unlockAudio();
        }
    });
    if (soundButton) soundButton.addEventListener("click", function () {
        soundOn = !soundOn;
        unlockAudio(); updateSoundButton();
        try { localStorage.setItem("simulation-sound", soundOn ? "on" : "off"); } catch (_) { }
    });
    updateSoundButton();
    if (reloadButton) reloadButton.addEventListener("click", function () { window.location.reload(); });
    if (manager) {
        manager.add_beginRequest(function () {
            page.setAttribute("aria-busy", "true");
            message("Saving your choice...");
        });
        manager.add_endRequest(function (sender, args) {
            page.removeAttribute("aria-busy");
            if (args.get_error()) {
                requestFailed = true;
                message("The save could not be confirmed. Reload to check your latest saved progress.");
                if (reloadButton) reloadButton.hidden = false;
                args.set_errorHandled(true);
            } else message("");
        });
    }
    // Delegated events survive the controls being replaced by the UpdatePanel.
    if (window.Sys) Sys.Application.add_load(animateResult);
    else animateResult();
})();
