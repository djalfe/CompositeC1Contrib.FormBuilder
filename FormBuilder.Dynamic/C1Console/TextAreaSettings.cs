﻿using System;
using System.Linq;

using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Dynamic
{
    [Serializable]
    public class TextAreaSettings : InputTypeSettingsHandler
    {
        public int MaxLength { get; set; }
        public int Cols { get; set; }
        public int Rows { get; set; }

        public void Load(FormField field)
        {
            var existingValidator = field.ValidationAttributes.OfType<MaxFieldLengthAttribute>().FirstOrDefault();
            if (existingValidator != null)
            {
                MaxLength = existingValidator.Length;
            }
        }

        public void Save(FormField field)
        {
            var existingValidator = field.ValidationAttributes.OfType<MaxFieldLengthAttribute>().FirstOrDefault();
            if (existingValidator != null)
            {
                field.Attributes.Remove(existingValidator);
            }

            if (MaxLength > 0)
            {
                var maxLengthValidator = new MaxFieldLengthAttribute("Max length is " + MaxLength, MaxLength);
                
                field.Attributes.Add(maxLengthValidator);
            }
        }
    }
}