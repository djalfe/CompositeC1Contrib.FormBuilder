﻿using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Dynamic.C1Console.EntityTokens;

namespace CompositeC1Contrib.FormBuilder.Dynamic.C1Console.Actions
{
    [ActionExecutor(typeof(DeleteDataSourceActionExecutor))]
    public class DeleteDataSourceActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Delete };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize() => String.Empty;

        public static ActionToken Deserialize(string serializedData) => new DeleteDataSourceActionToken();
    }

    public class DeleteDataSourceActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var entryToken = (DataSourceEntityToken)entityToken;
            var definition = DynamicFormsFacade.GetFormByName(entryToken.FormName);
            var field = definition.Model.Fields.Get(entryToken.FieldName);
            var dataSourceAttribute = field.Attributes.OfType<DataSourceAttribute>().First();

            field.Attributes.Remove(dataSourceAttribute);

            field.EnsureValueType();

            DynamicFormsFacade.SaveForm(definition);

            new ParentTreeRefresher(flowControllerServicesContainer).PostRefreshMessages(entityToken);

            return null;
        }
    }
}
