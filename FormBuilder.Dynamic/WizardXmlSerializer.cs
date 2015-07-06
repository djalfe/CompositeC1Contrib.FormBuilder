﻿using System;
using System.Xml.Linq;

namespace CompositeC1Contrib.FormBuilder.Dynamic
{
    public class WizardXmlSerializer : XmlDefinitionSerializer
    {
        public override IDynamicFormDefinition Load(string name, XElement xml)
        {
            var wizard = new DynamicFormWizard { Name = name };

            var requiresCaptchaAttribute = xml.Attribute("requiresCaptcha");
            if (requiresCaptchaAttribute != null)
            {
                wizard.RequiresCaptcha = bool.Parse(requiresCaptchaAttribute.Value);
            }

            var forceHttpsConnectionAttribute = xml.Attribute("forceHttpsConnection");
            if (forceHttpsConnectionAttribute != null)
            {
                wizard.ForceHttps = bool.Parse(forceHttpsConnectionAttribute.Value);
            }

            var stepsElement = xml.Element("Steps");
            foreach (var stepElement in stepsElement.Elements("Step"))
            {
                var labelAttr = stepElement.Attribute("Label");

                var wizardStep = new FormWizardStep
                {
                    Name = stepElement.Attribute("Name").Value,
                    FormName = stepElement.Attribute("FormName").Value,
                    LocalOrdering = int.Parse(stepElement.Attribute("LocalOrdering").Value),
                };

                wizardStep.Label = labelAttr != null ? labelAttr.Value : wizardStep.Name;

                var nextButtonLabelAttribute = stepElement.Attribute("nextButtonLabel");
                if (nextButtonLabelAttribute != null)
                {
                    wizardStep.NextButtonLabel = nextButtonLabelAttribute.Value;
                }

                var previousButtonLabelAttribute = stepElement.Attribute("previousButtonLabel");
                if (previousButtonLabelAttribute != null)
                {
                    wizardStep.PreviousButtonLabel = previousButtonLabelAttribute.Value;
                }

                wizard.Steps.Add(wizardStep);
            }

            var layoutElement = xml.Element("Layout");
            if (layoutElement != null)
            {
                ParseLayout(layoutElement, wizard);
            }

            ParseSubmitHandlers(xml, wizard);
            ParseFormSettings(xml, wizard);

            return wizard;
        }

        public override void Save(IDynamicFormDefinition form)
        {
            var wizard = (DynamicFormWizard)form;

            var root = new XElement("FormBuilder.Wizard",
                new XAttribute("name", wizard.Name),
                new XAttribute("requiresCaptcha", wizard.RequiresCaptcha),
                new XAttribute("forceHttpsConnection", wizard.ForceHttps));

            var layout = new XElement("Layout");

            SaveLayout(form, layout);

            root.Add(layout);

            var steps = new XElement("Steps");

            foreach (var step in wizard.Steps)
            {
                var stepElement = new XElement("Step",
                    new XAttribute("Name", step.Name),
                    new XAttribute("FormName", step.FormName),
                    new XAttribute("Label", step.Label),
                    new XAttribute("LocalOrdering", step.LocalOrdering));

                if (!String.IsNullOrEmpty(step.NextButtonLabel))
                {
                    stepElement.Add(new XAttribute("nextButtonLabel", step.NextButtonLabel));
                }

                if (!String.IsNullOrEmpty(step.PreviousButtonLabel))
                {
                    stepElement.Add(new XAttribute("previousButtonLabel", step.PreviousButtonLabel));
                }

                steps.Add(stepElement);
            }

            root.Add(steps);

            SaveMetaData(form, root);
            SaveDefinitionFile(form.Name, root);

            base.Save(form);
        }
    }
}
