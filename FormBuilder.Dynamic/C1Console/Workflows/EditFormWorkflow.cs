using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Workflow;
using Composite.Data;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Data.Types;

namespace CompositeC1Contrib.FormBuilder.Dynamic.C1Console.Workflows
{
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public class EditFormWorkflow : BaseEditFormWorkflow
    {
        public EditFormWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.FormBuilder.Dynamic\\EditFormWorkflow.xml") { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("BoundToken"))
            {
                return;
            }

            var form = (IForm)((DataEntityToken)EntityToken).Data;
            var definition = DynamicFormsFacade.GetFormByName(form.Name);

            SetupFormData(definition, definition.Model);

            Bindings.Add("SubmitButtonLabel", definition.Model.SubmitButtonLabel);
            Bindings.Add("BoundToken", EntityToken);
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var formToken = GetBinding<DataEntityToken>("BoundToken");
            var form = (IForm)formToken.Data;

            var definition = DynamicFormsFacade.GetFormByName(form.Name);

            var submitButtonLabel = GetBinding<string>("SubmitButtonLabel");

            var submitButtonLabelAttr = definition.Model.Attributes.OfType<SubmitButtonLabelAttribute>().SingleOrDefault();
            if (submitButtonLabel != null)
            {
                definition.Model.Attributes.Remove(submitButtonLabelAttr);
            }

            SwitchAttribute<RequiresCaptchaAttribute>("RequiresCaptcha", definition.Model.Attributes);
            SwitchAttribute<ForceHttpsConnectionAttribute>("ForceHttpsConnection", definition.Model.Attributes);

            if (!String.IsNullOrEmpty(submitButtonLabel))
            {
                submitButtonLabelAttr = new SubmitButtonLabelAttribute(submitButtonLabel);
                definition.Model.Attributes.Add(submitButtonLabelAttr);
            }

            Save(definition);
        }

        private void SwitchAttribute<T>(string bindingName, IList<Attribute> attributes) where T : Attribute, new()
        {
            var flip = GetBinding<bool>(bindingName);
            var attribute = attributes.OfType<T>().SingleOrDefault();

            if (flip)
            {
                if (attribute == null)
                {
                    attribute = new T();
                    attributes.Add(attribute);
                }
            }
            else
            {
                if (attribute != null)
                {
                    attributes.Remove(attribute);
                }
            }
        }
    }
}
