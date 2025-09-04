(function () {
  const DATE_FORMAT_DISPLAY = "d.m.Y H:i"; // user-facing
  const MINUTE_INCREMENT = 1;

  let counter = 0;
  const pad2 = (n) => String(n).padStart(2, "0");
  const toIsoNoSeconds = (d) =>
    d.getFullYear() + "-" + pad2(d.getMonth()+1) + "-" + pad2(d.getDate()) +
    "T" + pad2(d.getHours()) + ":" + pad2(d.getMinutes());

  function parseInitial(el) {
    const v = (el.getAttribute("value") || el.value || "").trim();
    const dt = v ? new Date(v) : new Date();
    return isNaN(dt.getTime()) ? new Date() : dt;
  }

  function moveLabelFor(oldId, newId) {
    const lbl = document.querySelector(`label[for="${oldId}"]`);
    if (lbl) lbl.setAttribute("for", newId);
  }

  function makeVisibleTwin(fromHidden) {
    counter += 1;
    const display = document.createElement("input");
    display.type = "text";
    display.className = fromHidden.className.replace(/\bdatepicker\b/, "datepicker-ui");
    display.id = fromHidden.id ? fromHidden.id + "_display" : "dp_display_" + counter;
    display.placeholder = fromHidden.getAttribute("placeholder") || "";
    display.autocomplete = "off";
    display.setAttribute("aria-haspopup", "dialog");

    fromHidden.insertAdjacentElement("afterend", display);
    if (fromHidden.id) moveLabelFor(fromHidden.id, display.id);

    return display;
  }

  function transformOne(src) {
    if (src.dataset.dpInit === "1") return;
    src.dataset.dpInit = "1";

    src.type = "hidden";
    src.classList.add("mvc-allow"); // allow hidden validation

    const ui = makeVisibleTwin(src);
    const initial = parseInitial(src);
    src.value = toIsoNoSeconds(initial);

    const fp = window.flatpickr(ui, {
      locale: window.flatpickr.l10ns.is,
      enableTime: true,
      time_24hr: true,
      minuteIncrement: MINUTE_INCREMENT,
      allowInput: true,
      dateFormat: DATE_FORMAT_DISPLAY,
      defaultDate: initial,

      onReady: function (selectedDates) {
        const d = (selectedDates && selectedDates[0]) || initial;
        src.value = toIsoNoSeconds(d);
      },
      onChange: function (selectedDates) {
        if (selectedDates && selectedDates[0]) {
          src.value = toIsoNoSeconds(selectedDates[0]);
          if (window.jQuery) jQuery(src).valid && jQuery(src).valid();
        }
      },
      onClose: function (selectedDates, str, instance) {
        if (!ui.value.trim()) {
          const now = new Date();
          instance.setDate(now, true);
          src.value = toIsoNoSeconds(now);
          if (window.jQuery) jQuery(src).valid && jQuery(src).valid();
        }
      }
    });

    fp.setDate(initial, false);
    src.removeAttribute("placeholder");
  }

  function initAll() {
    document.querySelectorAll("input.datepicker").forEach(transformOne);
  }

  function ready(fn) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", fn, { once: true });
    } else fn();
  }

  // ---- dynamic loader helpers ----
  function injectCSS(href) {
    return new Promise((resolve, reject) => {
      if ([...document.styleSheets].some(s => s.href && s.href.includes("flatpickr.min.css"))) return resolve();
      const link = document.createElement("link");
      link.rel = "stylesheet";
      link.href = href;
      link.onload = resolve;
      link.onerror = reject;
      document.head.appendChild(link);
    });
  }

  function injectJS(src) {
    return new Promise((resolve, reject) => {
      if ([...document.scripts].some(s => s.src && s.src.includes(src.split("/")[2] || "flatpickr"))) return resolve();
      const script = document.createElement("script");
      script.src = src;
      script.defer = true;
      script.onload = resolve;
      script.onerror = reject;
      document.head.appendChild(script);
    });
  }

  function loadDeps() {
    const css = "https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css";
    const js  = "https://cdn.jsdelivr.net/npm/flatpickr";
    const loc = "https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/is.js";
    return injectCSS(css)
      .then(() => injectJS(js))
      .then(() => injectJS(loc));
  }

  // ---- main kickoff ----
  ready(function () {
    loadDeps()
      .then(() => initAll())
      .catch(err => console.error("Failed to load datePicker deps:", err));
    window.DatePickerInitAll = initAll; // re-run if you add fields dynamically
  });
})();
