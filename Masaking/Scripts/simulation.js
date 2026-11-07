(function () {
    "use strict";
    let soundOn = false;
    let audio = null;
    let draggedCard = null;
    let selectedCard = null;
    const page = document.querySelector(".simulation-page");
    if (!page) return;

    const manager = window.Sys && Sys.WebForms
        ? Sys.WebForms.PageRequestManager.getInstance() : null;

    function busy() {
        return manager && manager.get_isInAsyncPostBack();
    }

    function tone(frequency, duration, delay) {
        if (!soundOn || !audio) return;
        const oscillator = audio.createOscillator();
        const gain = audio.createGain();
        const start = audio.currentTime + (delay || 0);
        oscillator.connect(gain);
        gain.connect(audio.destination);
        oscillator.frequency.value = frequency;
        gain.gain.setValueAtTime(0.055, start);
        gain.gain.exponentialRampToValueAtTime(0.001, start + duration);
        oscillator.start(start);
        oscillator.stop(start + duration);
    }

    function animateResult() {
        const effect = document.getElementById("hfEffect");
        if (!effect || !effect.value) return;
        const result = effect.value;
        effect.value = "";
        const area = document.getElementById("dropZone");
        if (area && result === "correct") {
            area.classList.add("cooking-correct");
            window.setTimeout(function () { area.classList.remove("cooking-correct"); }, 850);
        }
        if (result === "wrong") {
            const cards = page.querySelectorAll("[data-ingredient]");
            for (const card of cards)
                if (card.dataset.ingredient === selectedCard) {
                    card.classList.add("ingredient-wrong");
                    window.setTimeout(function () { card.classList.remove("ingredient-wrong"); }, 500);
                }
            tone(180, 0.18);
        } else if (result === "complete") {
            const completed = document.getElementById("pnlResult");
            if (completed) completed.classList.add("dish-complete");
            tone(523, 0.13); tone(659, 0.13, 0.15); tone(784, 0.2, 0.3);
        } else tone(660, 0.15);
    }

    page.addEventListener("dragstart", function (event) {
        const card = event.target.closest("[data-ingredient]");
        if (!card || busy()) { event.preventDefault(); return; }
        draggedCard = card.dataset.ingredient;
        event.dataTransfer.setData("text/plain", draggedCard);
        event.dataTransfer.effectAllowed = "move";
    });

    page.addEventListener("dragover", function (event) {
        const area = event.target.closest("#dropZone");
        if (area && draggedCard && !busy()) {
            event.preventDefault();
            event.dataTransfer.dropEffect = "move";
            area.classList.add("drag-over");
        }
    });

    page.addEventListener("dragleave", function (event) {
        const area = event.target.closest("#dropZone");
        if (area && !area.contains(event.relatedTarget)) area.classList.remove("drag-over");
    });

    page.addEventListener("drop", function (event) {
        const area = event.target.closest("#dropZone");
        if (!area) return;
        event.preventDefault();
        area.classList.remove("drag-over");
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
            selectedCard = card.dataset.ingredient;
        }
    });

    const soundButton = document.getElementById("btnSound");
    soundButton.addEventListener("click", function () {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) {
            soundButton.textContent = "Sound unavailable";
            soundButton.disabled = true;
            return;
        }
        if (!audio) audio = new AudioContext();
        audio.resume();
        soundOn = !soundOn;
        soundButton.textContent = soundOn ? "Sound: on" : "Sound: off";
        soundButton.setAttribute("aria-pressed", soundOn ? "true" : "false");
    });

    if (manager) {
        manager.add_beginRequest(function () { page.setAttribute("aria-busy", "true"); });
        manager.add_endRequest(function () { page.removeAttribute("aria-busy"); });
    }
    // UpdatePanel replaces its contents. Delegated events above survive the replacement.
    if (window.Sys) Sys.Application.add_load(animateResult);
    else animateResult();
})();