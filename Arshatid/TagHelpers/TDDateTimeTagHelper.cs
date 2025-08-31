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

        // Optional per-field overrides (initializer supplies defaults if omitted)
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
        [HtmlAttributeName("toggle-class")] public string? ToggleClass { get; set; } // extra classes for the button
        [HtmlAttributeName("toggle-icon")] public string? ToggleIcon { get; set; }   // e.g. "bi bi-calendar3"

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string fullName = For.Name; // e.g., "EventTime" or "Events[0].EventTime"
            string baseId = TagBuilder.CreateSanitizedId(fullName, "_");
            string groupId = baseId + "_group";
            string displayId = baseId + "_display";
            string hiddenId = baseId + "_value";
            string labelText = For.Metadata.DisplayName ?? fullName;

            // Wrapper
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.SetAttribute("class", CombineWrapperClasses("mb-3", ContainerClass, output));

            // Label
            TagBuilder label = new TagBuilder("label");
            label.Attributes["for"] = displayId;
            label.AddCssClass(CombineClasses("form-label", LabelClass));
            label.InnerHtml.Append(labelText);

            // Input group (make both input and button target this group)
            TagBuilder group = new TagBuilder("div");
            group.Attributes["id"] = groupId;
            group.AddCssClass(CombineClasses("input-group", GroupClass));
            group.Attributes["data-td-target-input"] = "nearest";
            group.Attributes["data-td-target-toggle"] = "nearest";

            // Visible input (no name; TD binds to it)
            TagBuilder visible = new TagBuilder("input");
            visible.Attributes["type"] = "text";
            visible.Attributes["id"] = displayId;
            visible.Attributes["data-td-target"] = "#" + displayId;
            visible.AddCssClass(CombineClasses("form-control td-datetime", InputClass));
            // point TD at the group so both input & toggle work
            visible.Attributes["data-td-target"] = "#" + groupId;

            // Optional per-field overrides (read by your initializer)
            if (!string.IsNullOrEmpty(Locale)) visible.Attributes["data-td-locale"] = Locale;
            if (!string.IsNullOrEmpty(HourCycle)) visible.Attributes["data-td-hourcycle"] = HourCycle;
            if (!string.IsNullOrEmpty(DisplayFormat)) visible.Attributes["data-td-display-format"] = DisplayFormat;
            if (Seconds.HasValue) visible.Attributes["data-td-seconds"] = Seconds.Value ? "true" : "false";
            if (Step.HasValue) visible.Attributes["data-td-step"] = Step.Value.ToString(CultureInfo.InvariantCulture);

            // Toggle button (fully clickable, no grey input-group-text)
            TagBuilder btn = new TagBuilder("button");
            btn.Attributes["type"] = "button";
            btn.AddCssClass(CombineClasses("btn btn-outline-secondary border-start-0", ToggleClass));
            btn.Attributes["data-td-target"] = "#" + displayId;         // ✅
            btn.Attributes["data-td-toggle"] = "datetimepicker";
            // Icon
            TagBuilder icon = new TagBuilder("i");
            icon.AddCssClass(string.IsNullOrWhiteSpace(ToggleIcon) ? "bi bi-calendar3" : ToggleIcon!);
            btn.InnerHtml.AppendHtml(icon);

            group.InnerHtml.AppendHtml(visible);
            group.InnerHtml.AppendHtml(btn);

            // Hidden field (model-bound ISO value)
            TagBuilder hidden = new TagBuilder("input");
            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["id"] = hiddenId;
            hidden.Attributes["name"] = fullName; // critical for model binding
            if (For.Model is DateTime dt)
                hidden.Attributes["value"] = dt.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            else if (For.Model is DateTimeOffset dto)
                hidden.Attributes["value"] = dto.LocalDateTime.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

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
                existingAttr.Value is string existing &&
                !string.IsNullOrWhiteSpace(existing))
            {
                sb.Append(' ').Append(existing);
            }
            if (!string.IsNullOrWhiteSpace(extra)) sb.Append(' ').Append(extra);
            return sb.ToString();
        }

        private static string CombineClasses(string defaults, string? extra)
            => string.IsNullOrWhiteSpace(extra) ? defaults : (defaults + " " + extra);
    }
}
