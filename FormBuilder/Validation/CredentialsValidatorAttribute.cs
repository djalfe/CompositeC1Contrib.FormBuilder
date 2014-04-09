﻿using System.Linq;
using System.Web.Security;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class CredentialsValidatorAttribute : FormValidationAttribute
    {
        private readonly string _passwordField;

        public bool CheckUserName { get; set; }
        public bool CheckEmail { get; set; }

        public CredentialsValidatorAttribute(string message, string passwordField)
            : base(message)
        {
            _passwordField = passwordField;

            CheckUserName = false;
            CheckEmail = true;
        }

        public override FormValidationRule CreateRule(FormField field)
        {
            var value = (string)field.Value;
            var password = (string)field.OwningForm.Fields.Single(f => f.Name == _passwordField).Value;

            return new FormValidationRule(new[] { field.Name, _passwordField }, Message)
            {
                Rule = () =>
                {
                    var isValid = false;

                    if (!isValid && CheckUserName)
                    {
                        var user = Membership.GetUser(value);

                        isValid = user != null;
                    }

                    if (!isValid && CheckEmail)
                    {
                        var username = Membership.GetUserNameByEmail(value);
                        if (username != null)
                        {
                            isValid = true;
                            value = username;
                        }
                    }

                    if (!isValid)
                    {
                        return false;
                    }

                    return UserValidationFacade.IsLoggedIn() || Membership.ValidateUser(value, password);
                }
            };
        }
    }
}
