﻿using System;
using System.Collections.Generic;
using System.Web;

using Composite.Core.Xml;
using Composite.Functions;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class FunctionbasedInputElementAttribute : InputElementTypeAttribute
    {
        private readonly string _c1FunctionName;

        protected FunctionbasedInputElementAttribute(string functionName)
        {
            _c1FunctionName = functionName;
        }

        public override IHtmlString GetHtmlString(BaseFormBuilderRequestContext context, FormField field, IDictionary<string, string> htmlAttributes)
        {
            var function = FunctionFacade.GetFunction(_c1FunctionName);
            if (function == null)
            {
                throw new InvalidOperationException("C1 function " + _c1FunctionName + " not recognized");
            }

            var parameters = new Dictionary<string, object> 
            {
                { "Name", field.Name },
                { "HtmlAttributes", htmlAttributes },
                { "IsRequired", field.IsRequired },
                { "Label", field.Label },
                { "Placeholder", field.PlaceholderText }
            };

            var result = FunctionFacade.Execute<XhtmlDocument>(function, parameters);

            return new HtmlString(result.ToString());
        }
    }
}
