﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Composite.AspNet.Razor;

using CompositeC1Contrib.FormBuilder.Attributes;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class HtmlForm : IDisposable
    {
        private readonly FormsPage _page;
        private bool _disposed;

        public HtmlForm(FormsPage page, FormModel model, object htmlAttributes)
        {
            _page = page;

            var htmlAttributesDictionary = new Dictionary<string, IList<string>> 
            {
                {
                    "class", new List<string> 
                    {
                        "form",
                        "formbuilder-" + model.Name.ToLowerInvariant()
                    }
                }
            };

            var htmlElementAttributes = model.Attributes.OfType<HtmlTagAttribute>();
            var action = String.Empty;

            foreach (var attr in htmlElementAttributes)
            {
                if (attr.Attribute == "method")
                {
                    continue;
                }

                if (attr.Attribute == "action")
                {
                    action = attr.Value;

                    continue;
                }

                IList<string> list;
                if (!htmlAttributesDictionary.TryGetValue(attr.Attribute, out list))
                {
                    htmlAttributesDictionary.Add(attr.Attribute, new List<string>());
                }

                htmlAttributesDictionary[attr.Attribute].Add(attr.Value);
            }

            var dictionary = Functions.ObjectToDictionary(htmlAttributes);
            if (dictionary != null)
            {
                if (dictionary.ContainsKey("class"))
                {
                    htmlAttributesDictionary["class"].Add((string)dictionary["class"]);
                }

                if (dictionary.ContainsKey("action"))
                {
                    action = (string)dictionary["action"];
                }
            }

            page.WriteLiteral(String.Format("<form method=\"post\" action=\"{0}\"", action));

            foreach (var kvp in htmlAttributesDictionary)
            {
                page.WriteLiteral(" " + kvp.Key + "=\"");
                foreach (var itm in kvp.Value)
                {
                    page.WriteLiteral(itm + " ");
                }

                page.WriteLiteral("\"");
            }

            if (model.HasFileUpload)
            {
                page.WriteLiteral(" enctype=\"multipart/form-data\"");
            }

            page.WriteLiteral(">");

            page.WriteLiteral("<input type=\"hidden\" name=\"__type\" value=\"" + HttpUtility.HtmlAttributeEncode(model.Name) + "\" />");

            foreach (var field in model.Fields.Where(f => f.Label == null))
            {
                RenderHiddenField(field.Name, field.Id, field.Value == null ? String.Empty : FormRenderer.GetValue(field));
            }

            var requiresCaptchaAttr = model.Attributes.OfType<RequiresCaptchaAttribute>().SingleOrDefault();
            if (requiresCaptchaAttr != null)
            {
                RenderHiddenField(RequiresCaptchaAttribute.HiddenFieldName, RequiresCaptchaAttribute.HiddenFieldName, requiresCaptchaAttr.EncryptedValue);
            }
        }

        private void RenderHiddenField(string name, string id, string value)
        {
            var s = String.Format("<input type=\"hidden\" name=\"{0}\" id=\"{1}\" value=\"{2}\" />",
                    HttpUtility.HtmlAttributeEncode(name),
                    HttpUtility.HtmlAttributeEncode(id),
                    HttpUtility.HtmlAttributeEncode(value));

            _page.WriteLiteral(s);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _page.WriteLiteral("</form>");

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void EndForm()
        {
            Dispose(true);
        }
    }
}
