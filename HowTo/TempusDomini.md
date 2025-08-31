# Gold Standard – ASP.NET Core DateTime (is-IS, 24‑hour) with Tempus Dominus v6

A reusable, minimal-markup pattern for localized, 24‑hour date‑time inputs that bind cleanly to ASP.NET Core model properties. Works with Bootstrap 5.

---

## Goals
- Guaranteed **24‑hour** UI (no AM/PM), **Icelandic** locale (`is`)
- Clean **model binding**: post ISO local `yyyy-MM-ddTHH:mm` into your `DateTime`/`DateTime?`
- Minimal markup; sensible **defaults** with optional per‑field overrides
- Fully compatible with unobtrusive **client validation**

---

## 1) Include once in `_Layout.cshtml`

```html
<!-- Tempus Dominus v6 CSS/JS + Bootstrap JS -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@eonasdan/tempus-dominus@6/dist/css/tempus-dominus.min.css">
<script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2"></script>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5/dist/js/bootstrap.bundle.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/@eonasdan/tempus-dominus@6/dist/js/tempus-dominus.min.js"></script>

<!-- Help widgets pick the right language -->
<html lang="is">
```

### (Optional) App‑wide defaults
```html
<script>
  window.TD_DEFAULTS = {
    locale: 'is',               // Icelandic UI
    hourCycle: 'h23',           // 24-hour clock
    displayFormat: 'dd.MM.yyyy HH:mm',
    seconds: false,
    step: 1                     // minute stepping
  };
</script>
```

### Reusable initializer (drop in once, after TD script)
```html
<script>
(() => {
  "use strict";
  const GLOBAL = window.TD_DEFAULTS || {};
  const pad = n => String(n).padStart(2, "0");
  const toLocalIso = d => `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  const pickBool = (v, fb) => (v == null ? fb : /^(true|1)$/i.test(String(v)));

  function findHidden(el) {
    const sel = el.getAttribute("data-td-target");
    if (sel) { const n = document.querySelector(sel); if (n) return n; }
    const sib = el.nextElementSibling;
    if (sib && sib.tagName === "INPUT" && sib.type === "hidden") return sib;
    if (el.id && el.id.endsWith("_display")) {
      const guess = document.getElementById(el.id.replace(/_display$/, "_value"));
      if (guess) return guess;
    }
    console.warn("TD: hidden input not found for", el);
    return null;
  }

  function initField(el) {
    const hidden = findHidden(el); if (!hidden) return;

    const locale = el.getAttribute("data-td-locale") || GLOBAL.locale || "is";
    const hourCycle = el.getAttribute("data-td-hourcycle") || GLOBAL.hourCycle || "h23";
    const format = el.getAttribute("data-td-display-format") || GLOBAL.displayFormat || "dd.MM.yyyy HH:mm";
    const seconds = pickBool(el.getAttribute("data-td-seconds"), GLOBAL.seconds ?? false);
    const step = parseInt(el.getAttribute("data-td-step") ?? (GLOBAL.step ?? 1), 10);

    const picker = new tempusDominus.TempusDominus(el, {
      localization: { locale: locale, hourCycle: hourCycle, format: format },
      stepping: step,
      display: { components: { calendar: true, hours: true, minutes: true, seconds: seconds } }
    });

    if (hidden.value) {
      const d = new Date(hidden.value);
      if (!Number.isNaN(d.getTime())) picker.dates.setValue(d);
    }

    el.addEventListener("change.td", e => {
      const raw = e?.detail?.date;
      const js = raw && typeof raw.toDate === "function" ? raw.toDate() : raw;
      if (js instanceof Date && !Number.isNaN(js.getTime())) {
        hidden.value = toLocalIso(js);
        hidden.dispatchEvent(new Event("input", { bubbles: true }));
        hidden.dispatchEvent(new Event("change", { bubbles: true }));
      }
    });
  }

  function initAll() { document.querySelectorAll(".td-datetime").forEach(initField); }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", initAll);
  else initAll();

  // For AJAX/partials
  window.initAllTempusDominus = initAll;
})();
</script>
```

> This script provides defaults. Per‑field overrides are optional via `data-td-*` attributes.

---

## 2) Minimal snippet (use in any Razor view)

```cshtml
@{ string baseId = Html.IdFor(m => m.EventTime); }
<div class="mb-3">
  <label for="@(baseId)_display" class="form-label">@Html.DisplayNameFor(m => m.EventTime)</label>

  <!-- Visible picker UI (no name) -->
  <input id="@(baseId)_display" type="text" class="form-control td-datetime" />

  <!-- Hidden, model-bound ISO value -->
  <input asp-for="EventTime" type="hidden" id="@(baseId)_value" />

  <span asp-validation-for="EventTime" class="text-danger"></span>
</div>
```

### Optional per‑field overrides
Add to the **visible** input only if needed:
```html
 data-td-seconds="true"
 data-td-step="5"
 data-td-display-format="dd.MM.yyyy HH:mm:ss"
 data-td-locale="is"          <!-- default is is -->
 data-td-hourcycle="h23"      <!-- default is h23 -->
```

---

## 3) Editor Template (drop‑in reuse everywhere)

Create `Views/Shared/EditorTemplates/TDDateTime.cshtml`:

```cshtml
@model DateTime?
@using Microsoft.AspNetCore.Mvc.Rendering

@{
    string fullName = Html.NameForModel();   // e.g., "EventTime" or "Events[0].EventTime"
    string baseId   = Html.IdForModel();     // safe id, e.g., "Events_0__EventTime"
    string displayId = baseId + "_display";
    string hiddenId  = baseId + "_value";
    string labelText = ViewData.ModelMetadata.GetDisplayName() ?? fullName;
}

<div class="mb-3">
  <label for="@displayId" class="form-label">@labelText</label>
  <input id="@displayId" type="text" class="form-control td-datetime" />
  <input type="hidden" id="@hiddenId" name="@fullName" value="@(Model.HasValue ? Model.Value.ToString("yyyy-MM-ddTHH\\:mm") : string.Empty)" />
  <span class="text-danger field-validation-valid" data-valmsg-for="@fullName" data-valmsg-replace="true"></span>
</div>
```

Use it:
```cshtml
@Html.EditorFor(m => m.EventTime, "TDDateTime")
```

---

## 4) Notes & gotchas
- **Why hidden input?** We keep the model‑bound field ISO‑formatted, so binding is reliable; the visible box shows localized text.
- **Validation:** The initializer fires `input`/`change` on the hidden field so unobtrusive validation updates error messages.
- **Collections/partials:** Using `Html.IdFor(...)`/`Html.IdForModel()` keeps IDs unique (e.g., `Events_0__EventTime`). After injecting partial HTML, call `window.initAllTempusDominus()`.
- **EditorTemplates:** In ASP.NET Core, use `Html.IdForModel()` / `Html.NameForModel()` (there is no `GetFullHtmlFieldId`).
- **Staying native?** If you use `<input type="datetime-local">`, you can’t reliably force 24h UI; OS/browser decide. This TD pattern guarantees 24h.



---

## 5) Tag Helper (clean syntax)

Create a Tag Helper so you can write `<td-datetime asp-for="EventTime" />` with optional overrides.

### 5.1 Add the Tag Helper class (e.g., `Arshatid/TagHelpers/TDDateTimeTagHelper.cs`)

```csharp
using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Arshatid.TagHelpers
{
    [HtmlTargetElement("td-datetime", Attributes = ForAttributeName)]
    public sealed class TDDateTimeTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; } = default!;

        // Optional per-field overrides (otherwise defaults come from the initializer)
        [HtmlAttributeName("locale")] public string? Locale { get; set; }
        [HtmlAttributeName("hour-cycle")] public string? HourCycle { get; set; }
        [HtmlAttributeName("display-format")] public string? DisplayFormat { get; set; }
        [HtmlAttributeName("seconds")] public bool? Seconds { get; set; }
        [HtmlAttributeName("step")] public int? Step { get; set; }

        // Optional CSS classes
        [HtmlAttributeName("label-class")] public string? LabelClass { get; set; }
        [HtmlAttributeName("input-class")] public string? InputClass { get; set; }
        [HtmlAttributeName("container-class")] public string? ContainerClass { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string fullName = For.Name; // e.g., "EventTime" or "Events[0].EventTime"
            string baseId = TagBuilder.CreateSanitizedId(fullName, "_"); // safe id
            string displayId = baseId + "_display";
            string hiddenId = baseId + "_value";
            string labelText = For.Metadata.DisplayName ?? fullName;

            // wrap in a div and merge any classes supplied on <td-datetime class="...">
            output.TagName = "div";
            output.Attributes.SetAttribute("class", CombineWrapperClasses("mb-3", ContainerClass, output));

            // <label for="..." class="form-label ...">Label</label>
            TagBuilder label = new TagBuilder("label");
            label.Attributes["for"] = displayId;
            label.AddCssClass(CombineClasses("form-label", LabelClass));
            label.InnerHtml.Append(labelText);

            // Visible input (no name) that Tempus Dominus controls
            TagBuilder visible = new TagBuilder("input");
            visible.Attributes["type"] = "text";
            visible.Attributes["id"] = displayId;
            visible.AddCssClass(CombineClasses("form-control td-datetime", InputClass));
            visible.Attributes["data-td-target"] = "#" + hiddenId; // pair with hidden field
            if (!string.IsNullOrEmpty(Locale)) visible.Attributes["data-td-locale"] = Locale;
            if (!string.IsNullOrEmpty(HourCycle)) visible.Attributes["data-td-hourcycle"] = HourCycle;
            if (!string.IsNullOrEmpty(DisplayFormat)) visible.Attributes["data-td-display-format"] = DisplayFormat;
            if (Seconds.HasValue) visible.Attributes["data-td-seconds"] = Seconds.Value ? "true" : "false";
            if (Step.HasValue) visible.Attributes["data-td-step"] = Step.Value.ToString(CultureInfo.InvariantCulture);

            // Hidden, model-bound ISO value (this is what ASP.NET binds to)
            TagBuilder hidden = new TagBuilder("input");
            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["id"] = hiddenId;
            hidden.Attributes["name"] = fullName; // critical for model binding
            if (For.Model is DateTime dt)
            {
                hidden.Attributes["value"] = dt.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            }
            else if (For.Model is DateTimeOffset dto)
            {
                DateTime local = dto.LocalDateTime;
                hidden.Attributes["value"] = local.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            }

            // Validation span (unobtrusive validation uses name)
            TagBuilder validation = new TagBuilder("span");
            validation.AddCssClass("text-danger field-validation-valid");
            validation.Attributes["data-valmsg-for"] = fullName;
            validation.Attributes["data-valmsg-replace"] = "true";

            // Compose
            output.Content.AppendHtml(label);
            output.Content.AppendHtml(visible);
            output.Content.AppendHtml(hidden);
            output.Content.AppendHtml(validation);
        }

        private static string CombineWrapperClasses(string defaults, string? extra, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder(defaults);
            if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute existingAttr) && existingAttr.Value is string existing && !string.IsNullOrWhiteSpace(existing))
            {
                sb.Append(' ').Append(existing);
            }
            if (!string.IsNullOrWhiteSpace(extra))
            {
                sb.Append(' ').Append(extra);
            }
            return sb.ToString();
        }

        private static string CombineClasses(string defaults, string? extra)
        {
            if (string.IsNullOrWhiteSpace(extra)) return defaults;
            return defaults + " " + extra;
        }
    }
}
```

### 5.2 Register the Tag Helper
Add to your project’s `_ViewImports.cshtml` (use your assembly name):

```cshtml
@addTagHelper *, Arshatid
```

### 5.3 Use it in Razor

Minimal (defaults from initializer):
```cshtml
<td-datetime asp-for="EventTime" />
```

With per-field overrides:
```cshtml
<td-datetime asp-for="EventTime"
             display-format="dd.MM.yyyy HH:mm:ss"
             seconds="true"
             step="5"
             input-class="form-control form-control-sm"
             container-class="mb-2" />
```

> Remember: include the Tempus Dominus CSS/JS and the reusable initializer (sections **1** and **Reusable initializer**) in your layout. The Tag Helper only emits the markup; the script wires up the behavior and locale/24‑hour defaults.

