﻿using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.FormBuilder.Dynamic.Wizard.C1Console.EntityTokens;

namespace CompositeC1Contrib.FormBuilder.Dynamic.Wizard.C1Console.Actions
{
    [ActionExecutor(typeof(DeleteFormWizardStepActionExecutor))]
    public class DeleteFormWizardStepActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return String.Empty;
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new DeleteFormWizardStepActionToken();
        }
    }

    public class DeleteFormWizardStepActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (FormWizardStepEntityToken)entityToken;
            var wizard = DynamicFormWizardsFacade.GetWizard(token.WizardName);
            var step = wizard.Steps.Single(h => h.Name == token.StepName);

            wizard.Steps.Remove(step);

            DynamicFormWizardsFacade.SaveWizard(wizard);

            new ParentTreeRefresher(flowControllerServicesContainer).PostRefreshMesseges(entityToken);

            return null;
        }
    }
}