﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Core.Xml;
using Composite.Functions;

using CompositeC1Contrib.FormBuilder.Web.UI;

namespace CompositeC1Contrib.FormBuilder.FunctionProviders
{
    public abstract class BaseDumpSubmittedFormValuesFunction : IFunction
    {
        public string Namespace => "FormBuilder";

        public string Name { get; protected set; }

        public EntityToken EntityToken => new FormBuilderFunctionEntityToken(typeof(FormBuilderFunctionProvider).Name, Namespace + "." + Name);

        public string Description => String.Empty;

        public Type ReturnType => typeof(XhtmlDocument);

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                var list = new List<ParameterProfile>
                {
                    new ParameterProfile("UseRenderingLayout", typeof (bool), false,
                        new ConstantValueProvider(false),
                        StandardWidgetFunctions.GetDefaultWidgetFunctionProviderByType(typeof (bool)),
                        "Use rendering layout", new HelpDefinition("Use rendering layout"))
                };

                return list;
            }
        }

        public abstract object Execute(ParameterList parameters, FunctionContextContainer context);

        public static void DumpModelValues(Form form, XhtmlDocument doc)
        {
            DumpModelValues(form, doc, false);
        }

        public static void DumpModelValues(Form form, XhtmlDocument doc, bool useRenderingLayout)
        {
            if (useRenderingLayout)
            {
                var renderingMarkup = RenderingLayoutFacade.GetRenderingLayout(form.Name);

                var elements = new List<XElement>();
                var fields = new List<FormField>();

                foreach (var el in renderingMarkup.Body.Descendants().ToList())
                {
                    var value = el.Value.Trim();

                    if (value.Length > 2 && value.First() == '%' && value.Last() == '%')
                    {
                        value = value.Substring(1, value.Length - 2);

                        var field = form.Fields.FirstOrDefault(f => f.Label != null && f.Name == value);
                        if (field == null)
                        {
                            continue;
                        }

                        elements.Add(el);

                        if (field.IsDependencyMetRecursive())
                        {
                            fields.Add(field);
                        }
                    }
                    else
                    {
                        ReplaceElementsWithTable(elements, fields);
                    }
                }

                ReplaceElementsWithTable(elements, fields);

                doc.Body.Add(renderingMarkup.Body.Elements());
            }
            else
            {
                var fields = form.Fields.Where(f => f.Label != null && f.Value != null && f.IsDependencyMetRecursive());
                var table = GetFieldsTable(fields);

                doc.Body.Add(table);
            }
        }

        private static void ReplaceElementsWithTable(IList<XElement> elements, List<FormField> fields)
        {
            if (!fields.Any())
            {
                foreach (var element in elements)
                {
                    element.Remove();
                }
            }
            else
            {
                var table = GetFieldsTable(fields);

                for (var i = 0; i < elements.Count - 1; i++)
                {
                    elements[i].Remove();
                }

                elements.Last().ReplaceWith(table);
            }

            elements.Clear();
            fields.Clear();
        }

        private static XElement GetFieldsTable(IEnumerable<FormField> fields)
        {
            var table = new XElement(Namespaces.Xhtml + "table", new XAttribute("class", "data-table"));

            foreach (var item in fields)
            {
                table.Add(GetFieldTableRow(item));
            }

            return table;
        }

        private static XElement GetFieldTableRow(FormField field)
        {
            var value = FormattingUtils.FormatFieldValue(field);

            var row = new XElement(Namespaces.Xhtml + "tr", new XAttribute("style", "vertical-align: top;"),
                new XElement(Namespaces.Xhtml + "td",
                    new XElement(Namespaces.Xhtml + "strong", field.Label.TrimEnd(':') + ":")));

            if (field.InputElementType is TextAreaInputElementAttribute)
            {
                value = HttpUtility.HtmlEncode(value);
                value = "<td xmlns=\"" + Namespaces.Xhtml + "\">" + value.Replace("\r", String.Empty).Replace("\n", "<br />") + "</td>";

                row.Add(XElement.Parse(value));
            }
            else
            {
                row.Add(new XElement(Namespaces.Xhtml + "td", value));
            }

            return row;
        }
    }
}
