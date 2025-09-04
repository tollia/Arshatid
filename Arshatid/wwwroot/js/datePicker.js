// wwwroot/js/datePicker.js
// Time-only pickers -> add data-no-calendar="true" and tweak the script slightly,
// Per-field minute increments -> use data-minute-increment="5",
// UTC posting -> add data - utc="true"

(function () {
  "use strict";

  // ---- Config ----
  const DATE_FORMAT_DISPLAY = "d.m.Y H:i";   // Icelandic-friendly view
  const DEFAULT_MINUTE_INCREMENT = 1;        // minutes step
  const INJECT_BOOTSTRAP_ICONS = false;       // set false if you include BI elsewhere

  let counter = 0;

  // ---- Utils ----
  const pad2 = (n) => String(n).padStart(2, "0");
  const toIsoNoSeconds = (d, useUtc) => {
    const Y = useUtc ? d.getUTCFullYear() : d.getFullYear();
    const M = (useUtc ? d.getUTCMonth() : d.getMonth()) + 1;
    const D = useUtc ? d.getUTCDate() : d.getDate();
    const h = useUtc ? d.getUTCHours() : d.getHours();
    const m = useUtc ? d.getUTCMinutes() : d.getMinutes();
    const iso = `${Y}-${pad2(M)}-${pad2(D)}T${pad2(h)}:${pad2(m)}`;
    return useUtc ? iso + "Z" : iso;
  };
  const byQSA = (sel, root) => (root || document).querySelectorAll(sel);

  function parseInitial(el) {
    const v = (el.getAttribute("value") || el.value || "").trim();
    if (v) {
      const dt = new Date(v);
      if (!isNaN(dt.getTime())) return dt;
    }
    return new Date();
  }

  function moveLabelFor(oldId, newId) {
    if (!oldId) return;
    const lbl = document.querySelector(`label[for="${oldId}"]`);
    if (lbl) lbl.setAttribute("for", newId);
  }

  function injectStyleOnce(id, cssText) {
    if (document.getElementById(id)) return;
    const style = document.createElement("style");
    style.id = id;
    style.textContent = cssText;
    document.head.appendChild(style);
  }

  // ---- Dynamic loaders ----
  function injectCSS(href, containsHint) {
    return new Promise((resolve, reject) => {
      const exists = [...document.styleSheets].some(s => s.href && s.href.includes(containsHint || href));
      if (exists) return resolve();
      const link = document.createElement("link");
      link.rel = "stylesheet";
      link.href = href;
      link.onload = resolve;
      link.onerror = () => reject(new Error("Failed to load CSS " + href));
      document.head.appendChild(link);
    });
  }
  function injectJS(src, globalCheck) {
    return new Promise((resolve, reject) => {
      if (globalCheck && globalCheck()) return resolve();
      const already = [...document.scripts].some(s => s.src && s.src === src);
      if (already) return setTimeout(() => resolve(), 0);
      const script = document.createElement("script");
      script.src = src;
      script.onload = () => {
        if (globalCheck) queueMicrotask(() => globalCheck() ? resolve() : reject(new Error("Global not ready after " + src)));
        else resolve();
      };
      script.onerror = () => reject(new Error("Failed to load JS " + src));
      document.head.appendChild(script);
    });
  }
  function loadDeps() {
    const fpCss = "https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css";
    const fpJs  = "https://cdn.jsdelivr.net/npm/flatpickr";
    const fpLoc = "https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/is.js";
    const biCss = "https://cdn.jsdelivr.net/npm/bootstrap-icons/font/bootstrap-icons.css";

    const chain = injectCSS(fpCss, "flatpickr.min.css")
      .then(() => injectJS(fpJs,  () => !!window.flatpickr))
      .then(() => injectJS(fpLoc, () => !!(window.flatpickr && window.flatpickr.l10ns && window.flatpickr.l10ns.is)));

    return INJECT_BOOTSTRAP_ICONS ? chain.then(() => injectCSS(biCss, "bootstrap-icons")) : chain;
  }

  // ---- Build a wrapper that puts an icon inside the input ----
  function makeInlineIcon(fromHidden) {
    counter += 1;

    // Wrapper sits around the input, controls positioning
    const wrap = document.createElement("div");
    wrap.className = "dp-wrap position-relative w-100";

    // Visible input (form-control) — keep original classes (minus .datepicker)
    const display = document.createElement("input");
    display.type = "text";
    const base = (fromHidden.className || "").replace(/\bdatepicker\b/, "datepicker-ui").trim();
    display.className = base.includes("form-control") ? base : (base + " form-control");
    display.id = fromHidden.id ? fromHidden.id + "_display" : ("dp_display_" + counter);
    display.placeholder = fromHidden.getAttribute("placeholder") || "";
    display.autocomplete = "off";
    display.setAttribute("aria-haspopup", "dialog");

    // The icon element INSIDE the input area (absolute positioned)
    const icon = document.createElement("i");
    icon.className = "bi bi-calendar4-event dp-icon";
    icon.setAttribute("role", "button");
    icon.setAttribute("tabindex", "0");
    icon.setAttribute("aria-label", "Opna dagatal");

    // Place wrapper right after hidden, then input + icon inside
    fromHidden.insertAdjacentElement("afterend", wrap);
    wrap.appendChild(display);
    wrap.appendChild(icon);

    // Make the label target the visible input
    moveLabelFor(fromHidden.id, display.id);

    // Interactions
    const openPicker = () => {
      if (display._flatpickr) display._flatpickr.open();
      else display.focus();
    };
    icon.addEventListener("click", openPicker);
    icon.addEventListener("keydown", (e) => {
      if (e.key === "Enter" || e.key === " ") { e.preventDefault(); openPicker(); }
    });

    return { display, wrap, icon };
  }

  // ---- Transform one asp-for input ----
  function transformOne(src) {
    if (src.dataset.dpInit === "1") return;
    src.dataset.dpInit = "1";

    const useUtc = (src.dataset.utc || "").toLowerCase() === "true";
    const minuteInc = parseInt(src.dataset.minuteIncrement || DEFAULT_MINUTE_INCREMENT, 10) || DEFAULT_MINUTE_INCREMENT;

    // Original becomes hidden, posted, and validated
    src.type = "hidden";
    src.classList.add("mvc-allow"); // allow validation on hidden via ignore override

    // Build visible control with inline icon
    const { display } = makeInlineIcon(src);

    // Initial value (server or now)
    const initial = parseInitial(src);
    src.value = toIsoNoSeconds(initial, useUtc);

    const localeIs = (window.flatpickr && window.flatpickr.l10ns && window.flatpickr.l10ns.is) || undefined;

    const fp = window.flatpickr(display, {
      locale: localeIs,
      enableTime: true,
      time_24hr: true,                    // 24h only
      minuteIncrement: minuteInc,
      allowInput: true,
      dateFormat: DATE_FORMAT_DISPLAY,    // what user sees
      defaultDate: initial,

      onReady(selectedDates) {
        const d = (selectedDates && selectedDates[0]) || initial;
        src.value = toIsoNoSeconds(d, useUtc);
      },
      onChange(selectedDates) {
        if (selectedDates && selectedDates[0]) {
          src.value = toIsoNoSeconds(selectedDates[0], useUtc);
          if (window.jQuery && jQuery.fn && jQuery.fn.valid) { try { jQuery(src).valid(); } catch {} }
        }
      },
      onClose(selectedDates, str, instance) {
        if (!display.value.trim()) {
          const now = new Date();
          instance.setDate(now, true);
          src.value = toIsoNoSeconds(now, useUtc);
          if (window.jQuery && jQuery.fn && jQuery.fn.valid) { try { jQuery(src).valid(); } catch {} }
        }
      }
    });

    // Reflect value immediately
    fp.setDate(initial, false);
    src.removeAttribute("placeholder");
  }

  function initAll() {
    byQSA("input.datepicker").forEach(transformOne);
  }

  function ready(fn) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", fn, { once: true });
    } else fn();
  }

  // ---- Kickoff ----
  ready(function () {
    loadDeps()
      .then(() => {
        // Validation: include our hidden inputs
        if (window.jQuery && jQuery.validator) {
          jQuery.validator.setDefaults({ ignore: ":hidden:not(.mvc-allow)" });
        }
        // CSS to make the icon live INSIDE the input (right edge)
        injectStyleOnce("dp-inline-icon-style", `
          .dp-wrap { display: inline-block; }
          .dp-wrap .datepicker-ui { padding-right: 2.25rem; } /* room for icon */
          .dp-icon {
            position: absolute;
            top: 50%;
            right: .65rem;
            transform: translateY(-50%);
            pointer-events: auto;
            cursor: pointer;
            font-size: 1.1rem;
            line-height: 1;
            color: var(--bs-secondary-color, #6c757d);
          }
          .dp-icon:focus { outline: none; }
        `);
        initAll();
      })
      .catch(err => console.error("Failed to load datePicker deps:", err));

    // Expose for dynamic fragments (HTMX/Ajax)
    window.DatePickerInitAll = initAll;
  });
})();
