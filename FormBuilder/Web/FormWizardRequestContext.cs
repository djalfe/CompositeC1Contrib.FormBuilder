﻿using System.Collections.Generic;

using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder.Web
{
    public abstract class FormWizardRequestContext : BaseFormBuilderRequestContext<FormWizard>
    {
        public List<FormValidationRule> ValidationResult { get; private set; }

        public FormWizard Wizard
        {
            get { return RenderingModel; }
        }

        protected FormWizardRequestContext(string name)
            : base(name)
        {
            ValidationResult = new List<FormValidationRule>();
        }

        public override void Submit()
        {
            Wizard.Submit();

            base.Submit();
        }
    }
}
