using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;

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
