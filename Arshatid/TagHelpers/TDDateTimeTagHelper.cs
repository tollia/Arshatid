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

        // Optional per-field overrides used by your JS initializer
        [HtmlAttributeName("locale")] public string? Locale { get; set; }
        [HtmlAttributeName("hour-cycle")] public string? HourCycle { get; set; }
        [HtmlAttributeName("display-format")] public string? DisplayFormat { get; set; }
        [HtmlAttributeName("seconds")] public bool? Seconds { get; set; }
        [HtmlAttributeName("step")] public int? Step { get; set; }

        // Styling hooks
        [HtmlAttributeName("label-class")] public string? LabelClass { get; set; }
        [HtmlAttributeName("input-class")] public string? InputClass { get; set; }
        [HtmlAttributeName("container-class")] public string? ContainerClass { get; set; }
        [HtmlAttributeName("group-class")] public string? GroupClass { get; set; }
        [HtmlAttributeName("toggle-class")] public string? ToggleClass { get; set; }
        [HtmlAttributeName("toggle-icon")] public string? ToggleIcon { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!; // kept for future needs; not used here

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string fullName = For.Name;                           // e.g., "EventTime" or "Events[0].EventTime"
            string baseId = TagBuilder.CreateSanitizedId(fullName, "_");
            string displayId = baseId + "_display";
            string hiddenId = baseId + "_value";
            string groupId = baseId + "_group";
            string labelText = For.Metadata.DisplayName ?? fullName;

            // Resolve model value (server-side) and format both ISO + display text
            DateTime? value = null;
            if (For.Model is DateTime dt) { value = dt; }
            else if (For.Model is DateTimeOffset dto) { value = dto.LocalDateTime; }

            string isoValue = value.HasValue ? value.Value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) : string.Empty;
            string displayFmt = DisplayFormat ?? "dd.MM.yyyy HH:mm";
            CultureInfo isIs = CultureInfo.GetCultureInfo("is-IS");
            string displayText = value.HasValue ? value.Value.ToString(displayFmt, isIs) : string.Empty;

            // Wrapper
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", CombineWrapperClasses("mb-3", ContainerClass, output));

            // Label
            TagBuilder label = new TagBuilder("label");
            label.Attributes["for"] = displayId;
            label.AddCssClass(CombineClasses("form-label", LabelClass));
            label.InnerHtml.Append(labelText);

            // Input group
            TagBuilder group = new TagBuilder("div");
            group.Attributes["id"] = groupId;
            group.AddCssClass(CombineClasses("input-group", GroupClass));

            // Visible input (seeded server-side so it’s never blank)
            TagBuilder visible = new TagBuilder("input");
            visible.Attributes["type"] = "text";
            visible.Attributes["id"] = displayId;
            visible.Attributes["value"] = displayText;                        // <= server-seeded
            visible.AddCssClass(CombineClasses("form-control td-datetime", InputClass));
            // Your initializer will read these data-attrs and instantiate TD on the input
            visible.Attributes["data-td-target"] = "#" + displayId;           // target the input itself
            if (!string.IsNullOrEmpty(Locale)) visible.Attributes["data-td-locale"] = Locale;
            if (!string.IsNullOrEmpty(HourCycle)) visible.Attributes["data-td-hourcycle"] = HourCycle;
            if (!string.IsNullOrEmpty(DisplayFormat)) visible.Attributes["data-td-display-format"] = DisplayFormat;
            if (Seconds.HasValue) visible.Attributes["data-td-seconds"] = Seconds.Value ? "true" : "false";
            if (Step.HasValue) visible.Attributes["data-td-step"] = Step.Value.ToString(CultureInfo.InvariantCulture);

            // Toggle button (calendar icon)
            TagBuilder btn = new TagBuilder("button");
            btn.Attributes["type"] = "button";
            btn.AddCssClass(CombineClasses("btn btn-outline-secondary border-start-0", ToggleClass));
            btn.Attributes["data-td-target"] = "#" + displayId;               // click targets the input
            btn.Attributes["data-td-toggle"] = "datetimepicker";
            TagBuilder icon = new TagBuilder("i");
            icon.AddCssClass(string.IsNullOrWhiteSpace(ToggleIcon) ? "bi bi-calendar3" : ToggleIcon!);
            btn.InnerHtml.AppendHtml(icon);

            group.InnerHtml.AppendHtml(visible);
            group.InnerHtml.AppendHtml(btn);

            // Hidden ISO field used for model binding
            TagBuilder hidden = new TagBuilder("input");
            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["id"] = hiddenId;
            hidden.Attributes["name"] = fullName;
            hidden.Attributes["value"] = isoValue;

            // Validation span
            TagBuilder validation = new TagBuilder("span");
            validation.AddCssClass("text-danger field-validation-valid");
            validation.Attributes["data-valmsg-for"] = fullName;
            validation.Attributes["data-valmsg-replace"] = "true";

            // Compose
            output.Content.AppendHtml(label);
            output.Content.AppendHtml(group);
            output.Content.AppendHtml(hidden);
            output.Content.AppendHtml(validation);
        }

        private static string CombineWrapperClasses(string defaults, string? extra, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder(defaults);
            if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute existingAttr) &&
                existingAttr.Value is string existing && !string.IsNullOrWhiteSpace(existing))
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
            => string.IsNullOrWhiteSpace(extra) ? defaults : defaults + " " + extra;
    }
}
