﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;

using CompositeC1Contrib.FormBuilder.Web.UI;

namespace CompositeC1Contrib.FormBuilder.Web
{
    public abstract class BaseFormBuilderRequestContext<T> where T : IFormModel
    {
        protected HttpContextBase HttpContext { get; private set; }
        protected string FormName { get; private set; }

        public FormOptions Options { get; private set; }

        public bool IsOwnSubmit
        {
            get { return HttpContext.Request.RequestType == "POST" && HttpContext.Request.Form["__type"] == RenderingModel.Name; }
        }

        public bool IsSuccess
        {
            get
            {
                if (!IsOwnSubmit)
                {
                    return false;
                }

                if (!RenderingModel.DisableAntiForgery)
                {
                    try
                    {
                        AntiForgery.Validate();
                    }
                    catch { return false; }
                }

                return !RenderingModel.ValidationResult.Any();
            }
        }

        public abstract T RenderingModel { get; }

        protected BaseFormBuilderRequestContext(string name)
        {
            FormName = name;
            Options = new FormOptions();
        }

        public virtual void Execute(HttpContextBase context)
        {
            HttpContext = context;

            var request = HttpContext.Request;

            if (!request.IsLocal && RenderingModel.ForceHttps && !request.IsSecureConnection)
            {
                string redirectUrl = request.Url.ToString().Replace("http:", "https:");

                HttpContext.Response.Redirect(redirectUrl, true);
            }

            RenderingModel.SetDefaultValues();

            if (!IsOwnSubmit)
            {
                return;
            }

            var requestFiles = request.Files;
            var files = new List<FormFile>();

            for (int i = 0; i < requestFiles.Count; i++)
            {
                var f = requestFiles[i];
                if (f == null)
                {
                    continue;
                }

                if (f.ContentLength > 0)
                {
                    files.Add(new FormFile()
                    {
                        Key = requestFiles.AllKeys[i],
                        ContentLength = f.ContentLength,
                        ContentType = f.ContentType,
                        FileName = Path.GetFileName(f.FileName),
                        InputStream = f.InputStream
                    });
                }
            }

            RenderingModel.MapValues(request.Form, files);
            OnMappedValues();
            RenderingModel.Validate(true);
        }

        public virtual void OnMappedValues() { }
        public virtual void Submit() { }
    }
}
