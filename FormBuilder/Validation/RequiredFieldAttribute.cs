﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class RequiredFieldAttribute : FormValidationAttribute
    {
        private readonly bool _isRequired;

        public RequiredFieldAttribute() : this(null) { }

        public RequiredFieldAttribute(string message)
            : base(message)
        {
            _isRequired = true;
        }

        public virtual bool IsRequired(FormField field)
        {
            return _isRequired;
        }

        public override FormValidationRule CreateRule(FormField field)
        {
            var valueType = field.ValueType;
            var value = field.Value;

            return CreateRule(field, () =>
            {
                if (!IsRequired(field))
                {
                    return true;
                }

                if (value == null)
                {
                    return false;
                }

                if (valueType == typeof(string))
                {
                    return !String.IsNullOrWhiteSpace((string)value);
                }
                if (valueType == typeof(IEnumerable<string>))
                {
                    return ((IEnumerable<string>)value).Any(f => !String.IsNullOrWhiteSpace(f));
                }

                if (valueType == typeof(bool))
                {
                    return (bool)value;
                }

                if (valueType == typeof(int))
                {
                    return (int)value > 0;
                }
                if (valueType == typeof(int?))
                {
                    return ((int?)value).HasValue;
                }

                if (valueType == typeof(Guid))
                {
                    return (Guid)value != Guid.Empty;
                }
                if (valueType == typeof(Guid?))
                {
                    return ((Guid?)value).HasValue;
                }

                if (valueType == typeof(DateTime))
                {
                    return (DateTime)value > DateTime.MinValue;
                }
                if (valueType == typeof(DateTime?))
                {
                    return ((DateTime?)value).HasValue;
                }

                if (valueType == typeof(FormFile))
                {
                    return ((FormFile)value).ContentLength > 0;
                }
                if (valueType == typeof(IEnumerable<FormFile>))
                {
                    return ((IEnumerable<FormFile>)value).Any(f => f.ContentLength > 0);
                }

                return true;
            });
        }
    }
}
