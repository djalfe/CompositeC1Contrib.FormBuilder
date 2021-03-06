﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Validation;
using CompositeC1Contrib.FormBuilder.Web.UI;

namespace CompositeC1Contrib.FormBuilder.Dynamic
{
    public class FormXmlSerializer : XmlDefinitionSerializer
    {
        public override IDynamicDefinition Load(string name, XElement xml)
        {
            var model = new FormModel(name)
            {
                SetDefaultValuesHandler = DynamicFormsFacade.SetDefaultValues
            };

            var fields = xml.Element("Fields").Elements("Add");
            foreach (var f in fields)
            {
                var attrs = new List<Attribute>();
                var fieldName = f.Attribute("name").Value;

                foreach (var el in f.Elements())
                {
                    switch (el.Name.LocalName)
                    {
                        case "InputElement": ParseInputElement(el, attrs); break;
                        case "ValidationRules": ParseValidationRules(el, attrs); break;
                        case "DataSource": ParseDataSource(el, attrs); break;
                        case "Dependencies": ParseDependencies(el, attrs); break;
                    }
                }

                var valueTypeAttr = f.Attribute("valueType");
                var valueType = valueTypeAttr != null ? Type.GetType(valueTypeAttr.Value) : typeof(string);

                var formField = new FormFieldModel(model, fieldName, valueType, attrs);

                var label = Localization.EvaluateT(formField, "Label", null);
                if (label == null)
                {
                    formField.Attributes.Add(new HiddenFieldAttribute());
                }
                else
                {
                    formField.Attributes.Add(new FieldLabelAttribute());
                }

                var placeholderText = Localization.EvaluateT(formField, "PlaceholderText", null);
                if (placeholderText != null)
                {
                    formField.Attributes.Add(new PlaceholderTextAttribute());
                }

                var help = Localization.EvaluateT(formField, "Help", null);
                if (help != null)
                {
                    formField.Attributes.Add(new FieldHelpAttribute());
                }

                var isReadOnlyAttr = f.Attribute("isReadOnly");
                if (isReadOnlyAttr != null)
                {
                    formField.IsReadOnly = bool.Parse(isReadOnlyAttr.Value);
                }

                var formatAttribute = f.Attribute("format");
                if (formatAttribute != null)
                {
                    formField.Attributes.Add(new DisplayFormatAttribute(formatAttribute.Value));
                }

                if (valueTypeAttr == null)
                {
                    formField.EnsureValueType();
                }

                model.Fields.Add(formField);
            }

            var definition = new DynamicFormDefinition(model);

            ParseMetaData(xml, definition);

            return definition;
        }

        public override void Save(IDynamicDefinition form)
        {
            var definition = (DynamicFormDefinition)form;
            var model = definition.Model;

            var root = new XElement("FormBuilder.DynamicForm",
                new XAttribute("name", definition.Name));

            var metaData = new XElement("MetaData");

            if (model.Attributes.OfType<ForceHttpsConnectionAttribute>().Any())
            {
                metaData.Add(new XElement("forceHttpsConnection"));
            }

            if (model.Attributes.OfType<RequiresCaptchaAttribute>().Any())
            {
                metaData.Add(new XElement("requiresCaptcha"));
            }

            if (definition.DefaultValues.Any())
            {
                var defaultValues = new XElement("DefaultValues");

                foreach (var kvp in definition.DefaultValues)
                {
                    var defaultValueElement = new XElement("Add",
                        new XAttribute("field", kvp.Key),
                        new XAttribute("functionMarkup", kvp.Value.ToString()));

                    defaultValues.Add(defaultValueElement);
                }

                metaData.Add(defaultValues);
            }

            SaveMetaData(definition, metaData);

            if (metaData.HasElements)
            {
                root.Add(metaData);
            }

            var fields = new XElement("Fields");

            foreach (var field in model.Fields)
            {
                var add = new XElement("Add",
                    new XAttribute("name", field.Name),
                    new XAttribute("valueType", field.ValueType.AssemblyQualifiedName));


                if (field.IsReadOnly)
                {
                    add.Add(new XAttribute("isReadOnly", true));
                }

                var formatAttribute = field.Attributes.OfType<DisplayFormatAttribute>().SingleOrDefault();
                if (formatAttribute != null)
                {
                    add.Add(new XAttribute("format", formatAttribute.FormatString));
                }

                if (field.InputElementType != null)
                {
                    var inputElement = new XElement("InputElement");
                    var inputElementType = field.InputElementType.GetType();

                    inputElement.Add(new XAttribute("type", inputElementType.AssemblyQualifiedName));

                    SerializeInstanceWithArgument(field.InputElementType, inputElement);

                    add.Add(inputElement);
                }

                if (field.ValidationAttributes.Any())
                {
                    var validationRules = new XElement("ValidationRules");

                    foreach (var rule in field.ValidationAttributes)
                    {
                        var ruleType = rule.GetType();
                        var ruleElement = new XElement("Add",
                            new XAttribute("type", ruleType.AssemblyQualifiedName));

                        SerializeInstanceWithArgument(rule, ruleElement);

                        validationRules.Add(ruleElement);
                    }

                    add.Add(validationRules);
                }

                if (field.DependencyAttributes.Any())
                {
                    var dependencies = new XElement("Dependencies");

                    foreach (var dep in field.DependencyAttributes)
                    {
                        var dependency = new XElement("Add",
                            new XAttribute("type", dep.GetType().AssemblyQualifiedName),
                            new XAttribute("field", dep.ReadFromFieldName));

                        var values = new XElement("values");

                        foreach (var value in dep.ResolveRequiredFieldValues())
                        {
                            values.Add(new XElement("item",
                                new XAttribute("type", value.GetType().AssemblyQualifiedName),
                                new XAttribute("value", value)));
                        }

                        dependency.Add(values);

                        dependencies.Add(dependency);
                    }

                    add.Add(dependencies);
                }

                var dataSourceAttribute = field.Attributes.OfType<DataSourceAttribute>().FirstOrDefault();
                if (dataSourceAttribute != null)
                {
                    var datasource = new XElement("DataSource",
                        new XAttribute("type", dataSourceAttribute.GetType().AssemblyQualifiedName));

                    var stringBasedDataSourceAttribute = dataSourceAttribute as StringBasedDataSourceAttribute;
                    if (stringBasedDataSourceAttribute != null)
                    {
                        var values = new XElement("values");

                        foreach (var value in stringBasedDataSourceAttribute.Values)
                        {
                            values.Add(new XElement("item",
                                new XAttribute("value", value)));
                        }

                        datasource.Add(values);
                    }

                    add.Add(datasource);
                }

                fields.Add(add);
            }

            root.Add(fields);

            SaveDefinitionFile(definition.Name, root);

            base.Save(definition);
        }

        private static void ParseInputElement(XElement xml, IList<Attribute> attrs)
        {
            var type = Type.GetType(xml.Attribute("type").Value);
            if (type == null)
            {
                return;
            }

            var inputElementAttr = (InputElementTypeAttribute)XElementHelper.DeserializeInstanceWithArgument(type, xml);

            attrs.Add(inputElementAttr);
        }

        private static void ParseMetaData(XElement xml, DynamicFormDefinition definition)
        {
            var metaData = xml.Element("MetaData");
            if (metaData == null)
            {
                return;
            }

            var forceHttpsConnectionElement = metaData.Element("forceHttpsConnection");
            if (forceHttpsConnectionElement != null)
            {
                definition.Model.Attributes.Add(new ForceHttpsConnectionAttribute());
            }

            var requiresCaptchaElement = metaData.Element("requiresCaptcha");
            if (requiresCaptchaElement != null)
            {
                definition.Model.Attributes.Add(new RequiresCaptchaAttribute());
            }

            ParseMetaDataDefaultValues(metaData, definition);
            ParseSubmitHandlers(metaData, definition);
            ParseFormSettings(metaData, definition);
        }

        private static void ParseMetaDataDefaultValues(XElement metaData, DynamicFormDefinition definition)
        {
            var defaultValuesElement = metaData.Element("DefaultValues");
            if (defaultValuesElement != null)
            {
                foreach (var handler in defaultValuesElement.Elements("Add"))
                {
                    var fieldName = handler.Attribute("field").Value;
                    var functionMarkup = XElement.Parse(handler.Attribute("functionMarkup").Value);

                    definition.DefaultValues.Add(fieldName, functionMarkup);
                }
            }
        }

        private static void ParseDependencies(XElement el, IList<Attribute> attrs)
        {
            var dependencies = el.Elements("Add");
            foreach (var dep in dependencies)
            {
                var typeString = dep.Attribute("type").Value;
                var type = Type.GetType(typeString);

                if (!typeof(DependsOnConstantAttribute).IsAssignableFrom(type))
                {
                    return;
                }

                var field = dep.Attribute("field").Value;
                var valuesElement = dep.Element("values") ?? dep;

                var values = valuesElement.Elements("item").Select(itm =>
                {
                    var value = itm.Attribute("value").Value;
                    var typeAttr = itm.Attribute("type");

                    return Convert.ChangeType(value, typeAttr != null ? Type.GetType(typeAttr.Value) : typeof(string));
                }).ToArray();

                var attribute = (Attribute)Activator.CreateInstance(type, new object[] { field, values });

                attrs.Add(attribute);
            }
        }

        private static void ParseDataSource(XElement el, IList<Attribute> attrs)
        {
            var typeString = el.Attribute("type").Value;
            var type = Type.GetType(typeString);

            if (!typeof(StringBasedDataSourceAttribute).IsAssignableFrom(type))
            {
                return;
            }

            var valuesElement = el.Element("values");
            if (valuesElement == null)
            {
                return;
            }

            var values = valuesElement.Elements("item").Select(itm => itm.Attribute("value").Value).ToArray();
            var attribute = (Attribute)Activator.CreateInstance(type, new object[] { values });

            attrs.Add(attribute);
        }

        private static void ParseValidationRules(XElement el, ICollection<Attribute> attrs)
        {
            var rules = el.Elements("Add");
            foreach (var rule in rules)
            {
                var typeString = rule.Attribute("type").Value;

                var type = Type.GetType(typeString);
                if (type == null)
                {
                    continue;
                }

                var ruleAttribute = (FormValidationAttribute)XElementHelper.DeserializeInstanceWithArgument(type, rule);

                attrs.Add(ruleAttribute);
            }
        }
    }
}
