﻿using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

using CompositeC1Contrib.FormBuilder.Dynamic.C1Console.EntityTokens;

namespace CompositeC1Contrib.FormBuilder.Dynamic.C1Console.Actions
{
    [ActionExecutor(typeof(DeleteSubmitHandlerActionExecutor))]
    public class DeleteSubmitHandlerActionToken : ActionToken
    {
        private static readonly IEnumerable<PermissionType> _permissionTypes = new[] { PermissionType.Delete };

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
            return new DeleteSubmitHandlerActionToken();
        }
    }

    public class DeleteSubmitHandlerActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (FormSubmitHandlerEntityToken)entityToken;
            var definition = DefinitionsFacade.GetDefinition(token.FormName);
            var handler = definition.SubmitHandlers.Single(h => h.Name == token.Name);

            definition.SubmitHandlers.Remove(handler);

            handler.Delete(definition);

            var serializer = XmlDefinitionSerializer.GetSerializer(definition.Name);
            serializer.Save(definition);

            new ParentTreeRefresher(flowControllerServicesContainer).PostRefreshMessages(entityToken);

            return null;
        }
    }
}
