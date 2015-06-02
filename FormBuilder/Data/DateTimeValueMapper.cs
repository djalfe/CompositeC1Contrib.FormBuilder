﻿using System;
using System.ComponentModel.Composition;

namespace CompositeC1Contrib.FormBuilder.Data
{
    [Export(typeof(IValueMapper))]
    public class DateTimeValueMapper : IValueMapper
    {
        public Type ValueMapperFor
        {
            get { return typeof(DateTime); }
        }

        public void MapValue(FormField field, string value)
        {
            DateTime val;
            DateTime.TryParse(value, out val);

            field.Value = val;
        }
    }
}