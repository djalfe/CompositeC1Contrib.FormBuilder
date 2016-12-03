﻿using System.Globalization;
using System.Text.RegularExpressions;

using Composite.Core.ResourceSystem;

using CompositeC1Contrib.Localization;

namespace CompositeC1Contrib.FormBuilder
{
    public static class Localization
    {
        private static readonly Regex _t = new Regex("T\\((.+)\\)", RegexOptions.Compiled);

        public static string Localize(string text)
        {
            if (text.Contains("T("))
            {
                var match = _t.Match(text);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;

                    var localized = C1Res.T(key);
                    if (localized != null)
                    {
                        return text.Remove(match.Index, match.Length).Insert(match.Index, localized);
                    }

                }
            }

            if (text.Contains("${"))
            {
                return StringResourceSystemFacade.ParseString(text);
            }

            return text;
        }

        public static string EvaluateT(FormFieldModel field, string type, string defaultValue)
        {
            return EvaluateT(field.OwningForm, field.Name + "." + type, defaultValue);
        }

        public static string EvaluateT(IModel form, string type, string defaultValue)
        {
            var key = "Forms." + form.Name + "." + type;

            return EvaluateT(key, defaultValue);
        }

        public static string EvaluateT(string key, string defaultValue)
        {
            var evaluated = T(key);
            if (evaluated != null)
            {
                return evaluated;
            }

            return defaultValue == null ? null : Localize(defaultValue);
        }

        public static string T(string key)
        {
            var culture = CultureInfo.CurrentUICulture;

            return T(key, culture);
        }

        public static string T(string key, CultureInfo culture)
        {
            return C1Res.GetResourceManager("FormBuilder").GetString(key, culture) ?? ResourceFacade.InternalResourceManager.GetString(key, culture);
        }
    }
}
