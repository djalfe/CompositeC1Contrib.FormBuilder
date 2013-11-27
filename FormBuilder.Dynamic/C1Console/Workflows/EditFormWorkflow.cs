using System;
using System.Workflow.Activities;
using Composite.C1Console.Workflow;
using CompositeC1Contrib.FormBuilder.C1Console.Tokens;
using CompositeC1Contrib.FormBuilder.Dynamic.C1Console.Tokens;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.FormBuilder.Dynamic.C1Console.Workflows
{
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public class EditFormWorkflow : Basic1StepEditPageWorkflow
    {
        public override string FormDefinitionFileName
        {
            get { return "\\InstalledPackages\\CompositeC1Contrib.FormBuilder.Dynamic\\EditFormWorkflow.xml"; }
        }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (!BindingExist("FormName"))
            {
                var formToken = (FormInstanceEntityToken)EntityToken;
                var definition = DynamicFormsFacade.GetFormByName(formToken.FormName);

                Bindings.Add("FormName", formToken.FormName);
                Bindings.Add("FunctionExecutor", definition.FormExecutor ?? String.Empty);
            }
        }

        public override void OnSave(object sender, EventArgs e)
        {
            var formToken = (FormInstanceEntityToken)EntityToken;
            var definition = DynamicFormsFacade.GetFormByName(formToken.FormName);

            var formName = GetBinding<string>("FormName");
            var functionExecutor = GetBinding<string>("FunctionExecutor");

            definition.FormExecutor = functionExecutor;

            if (formName != formToken.FormName)
            {
                var newDefinition = DynamicFormsFacade.CopyFormByName(formToken.FormName, formName);

                DynamicFormsFacade.SaveForm(newDefinition);

                DynamicFormsFacade.DeleteModel(definition);
            }
            else
            {
                DynamicFormsFacade.SaveForm(definition);
            }

            CreateSpecificTreeRefresher().PostRefreshMesseges(new FormElementProviderEntityToken());
            SetSaveStatus(true);
        }

        public override void OnValidate(object sender, ConditionalEventArgs e)
        {
            var formToken = (FormInstanceEntityToken)EntityToken;
            var formName = GetBinding<string>("FormName");

            if (formName != formToken.FormName)
            {
                var definition = DynamicFormsFacade.GetFormByName(formName);
                if (definition != null)
                {
                    ShowFieldMessage("Form name", "Form name already exists");

                    e.Result = false;

                    return;
                }

            }

            e.Result = true;
        }
    }
}
