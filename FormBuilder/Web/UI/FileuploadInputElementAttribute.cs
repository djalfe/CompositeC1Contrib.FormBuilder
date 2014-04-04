﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class FileuploadInputElementAttribute : InputElementTypeAttribute
    {
        public override string ElementName
        {
            get { return "file"; }
        }

        public override IHtmlString GetHtmlString(FormsPage page, FormField field, IDictionary<string, string> htmlAttributes)
        {
            var sb = new StringBuilder();
            var htmlAttributesDictionary = MapHtmlTagAttributes(field, htmlAttributes);

            sb.AppendFormat("<input type=\"{0}\" name=\"{1}\" id=\"{2}\"",
                        ElementName,
                        HttpUtility.HtmlAttributeEncode(field.Name),
                        HttpUtility.HtmlAttributeEncode(field.Id));

            if (field.ValueType == typeof(IEnumerable<FormFile>))
            {
                htmlAttributesDictionary.Add("multiple", new List<string>() { "multiple" });
            }

            var fileMimeTypeValidatorAttr = field.Attributes.OfType<FileMimeTypeValidatorAttribute>().SingleOrDefault();
            if (fileMimeTypeValidatorAttr != null)
            {
                htmlAttributesDictionary.Add("accept", fileMimeTypeValidatorAttr.MimeTypes);
            }

            RenderReadOnlyAttribute(sb, field);
            RenderMaxLengthAttribute(sb, field);
            RenderExtraHtmlTags(sb, htmlAttributesDictionary);

            sb.Append(" />");

            return new HtmlString(sb.ToString());
        }
    }
}
