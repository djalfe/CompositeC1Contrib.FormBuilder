using System;

using CompositeC1Contrib.FormBuilder.C1Console.EntityTokens;

namespace CompositeC1Contrib.FormBuilder.Dynamic.C1Console.Workflows
{
    public class EditFormWizardWorkflow : BaseEditFormWorkflow
    {
        public EditFormWizardWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.FormBuilder.Dynamic\\EditFormWizardWorkflow.xml") { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("BoundToken"))
            {
                return;
            }

            var formToken = (FormInstanceEntityToken)EntityToken;
            var wizard = DynamicFormWizardsFacade.GetWizard(formToken.Name);

            Bindings.Add("ForceHttpsConnection", wizard.ForceHttpSConnection);

            SetupFormData(wizard);

            Bindings.Add("BoundToken", formToken);
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var wizardToken = GetBinding<FormInstanceEntityToken>("BoundToken");
            var wizard = DynamicFormWizardsFacade.GetWizard(wizardToken.Name);

            var forceHttpsConnection = GetBinding<bool>("ForceHttpsConnection");

            wizard.ForceHttpSConnection = forceHttpsConnection;

            Save(wizard);
        }
    }
}
