﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Composite;

using CompositeC1Contrib.FormBuilder.Configuration;
using CompositeC1Contrib.FormBuilder.Dynamic.C1Console;
using CompositeC1Contrib.FormBuilder.Web.UI;

namespace CompositeC1Contrib.FormBuilder.Dynamic.Configuration
{
    public class DynamicFormBuilderConfiguration : IPluginConfiguration
    {
        public IList<InputElementHandler> InputElementHandlers { get; }
        public IList<SubmitHandlerElement> SubmitHandlers { get; }
        public Type SettingsHandler { get; private set; }

        public DynamicFormBuilderConfiguration()
        {
            InputElementHandlers = new List<InputElementHandler>();
            SubmitHandlers = new List<SubmitHandlerElement>();
        }

        public void Initialize(XmlNode element)
        {
            var formSettingsHandler = element.Attributes.Cast<XmlAttribute>().SingleOrDefault(attr => attr.Name == "settingsHandler");
            if (formSettingsHandler != null)
            {
                var type = Type.GetType(formSettingsHandler.Value);
                
                SettingsHandler = type;
            }

            var inputElementHandlers = element.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.LocalName == "inputElementHandlers");
            if (inputElementHandlers != null)
            {
                foreach (XmlNode add in inputElementHandlers.ChildNodes)
                {
                    var attributes = add.Attributes.Cast<XmlAttribute>().ToDictionary(attr => attr.Name);

                    var name = attributes["name"].Value;
                    var type = Type.GetType(attributes["type"].Value);

                    Verify.IsNotNull(type, "Unrecognized inputelement type");

                    var inputElementHandler = new InputElementHandler
                    {
                        Name = name,
                        ElementType = (InputElementTypeAttribute)Activator.CreateInstance(type)
                    };

                    if (attributes.ContainsKey("settingsHandler"))
                    {
                        var settingsHandler = Type.GetType(attributes["settingsHandler"].Value);

                        Verify.IsNotNull(settingsHandler, "Unrecognized settingshandler");

                        inputElementHandler.SettingsHandler = (IInputTypeSettingsHandler)Activator.CreateInstance(settingsHandler);
                    }

                    InputElementHandlers.Add(inputElementHandler);
                }
            }

            var submitHandlers = element.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.LocalName == "submitHandlers");
            if (submitHandlers != null)
            {
                foreach (XmlNode add in submitHandlers.ChildNodes)
                {
                    var attributes = add.Attributes.Cast<XmlAttribute>().ToDictionary(attr => attr.Name);

                    var name = attributes["name"].Value;
                    var type = Type.GetType(attributes["type"].Value);
                    var allowMultiple = attributes.ContainsKey("allowMultiple") && bool.Parse(attributes["allowMultiple"].Value);

                    Verify.IsNotNull(type, "Unrecognized submithandler type");

                    SubmitHandlers.Add(new SubmitHandlerElement
                    {
                        Name = name,
                        Type = type,
                        AllowMultiple = allowMultiple
                    });
                }
            }
        }
    }
}
