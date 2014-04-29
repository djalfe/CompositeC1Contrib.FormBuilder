﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

using Composite.AspNet.Razor;
using Composite.Functions;

using CompositeC1Contrib.FormBuilder.FunctionProviders;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class POCOBasedFormsPage<T> : FormsPage where T : IPOCOForm
    {
        protected T Form { get; private set; }

        private readonly FormBuilderRequestContext _context;
        protected override FormBuilderRequestContext RenderingContext
        {
            get { return _context; }
        }

        protected POCOBasedFormsPage()
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            foreach (var ctor in ctors)
            {
                var parameters = ctor.GetParameters();

                if (parameters.Length == 0)
                {
                    Form = Activator.CreateInstance<T>();
                }
            }

            if (Form == null)
            {
                throw new InvalidOperationException("No parameterless constructor on form");
            }

            var model = POCOFormsFacade.FromType(Form);

            _context = new POCOFormBuilderRequestContext(model.Name, Form);
        }

        public override void ExecutePageHierarchy()
        {
            var functionContext = new FunctionContextContainer(FunctionContextContainer, new Dictionary<string, object>
            {
                { BaseFormFunction.RenderingContextKey, RenderingContext },
                { BaseFormFunction.FormModelKey, RenderingContext.RenderingModel }
            });

            var functionContextField = typeof(RazorHelper).GetField("PageContext_FunctionContextContainer", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            PageData[functionContextField] = functionContext;

            _context.Execute(Context);

            base.ExecutePageHierarchy();
        }

        protected IHtmlString DependencyAttributeFor(Expression<Func<T, object>> fieldSelector)
        {
            var sb = new StringBuilder();
            var field = GetField(fieldSelector);

            FormRenderer.DependencyAttributeFor(field, sb);

            return new HtmlString(sb.ToString().Trim());
        }

        protected IHtmlString InputFor(Expression<Func<T, object>> fieldSelector)
        {
            return InputFor(fieldSelector, new { });
        }

        protected IHtmlString InputFor(Expression<Func<T, object>> fieldSelector, object htmlAttributes)
        {
            var field = GetField(fieldSelector);
            var dictionary = Functions.ObjectToDictionary(htmlAttributes).ToDictionary(t => t.Key, t => t.Value.ToString());

            return FormRenderer.InputFor(Options, field, dictionary);
        }

        protected IHtmlString FieldFor(Expression<Func<T, object>> fieldSelector)
        {
            return FieldFor(fieldSelector, new { });
        }

        protected IHtmlString FieldFor(Expression<Func<T, object>> fieldSelector, object htmlAttributes)
        {
            var field = GetField(fieldSelector);
            var dictionary = Functions.ObjectToDictionary(htmlAttributes).ToDictionary(t => t.Key, t => t.Value.ToString());

            return FormRenderer.FieldFor(Options, field, dictionary);
        }

        protected IHtmlString NameFor(Expression<Func<T, object>> fieldSelector)
        {
            var prop = GetProperty(fieldSelector);
            var field = RenderingContext.RenderingModel.Fields.Single(f => f.Name == prop.Name);

            return FormRenderer.NameFor(field);
        }

        public FormField GetField(Expression<Func<T, object>> fieldSelector)
        {
            var prop = GetProperty(fieldSelector);

            return RenderingContext.RenderingModel.Fields.Single(f => f.Name == prop.Name);
        }

        public PropertyInfo GetProperty(string name)
        {
            return GetType().GetProperty(name);
        }

        public PropertyInfo GetProperty(Expression<Func<T, object>> fieldSelector)
        {
            MemberExpression memberExpression;

            switch (fieldSelector.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var unaryExpression = (UnaryExpression)fieldSelector.Body;
                    memberExpression = (MemberExpression)unaryExpression.Operand;

                    break;

                default:
                    memberExpression = (MemberExpression)fieldSelector.Body;

                    break;
            }

            return (PropertyInfo)memberExpression.Member;
        }
    }
}
