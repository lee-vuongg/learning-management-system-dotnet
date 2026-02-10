using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LQV_BlockchainCertificate.Services
{
    //public interface IViewRenderService
    //{
    //    Task<string> RenderToStringAsync(string viewPath, object model);
    //}

    public class RenderViewService : IViewRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public RenderViewService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        public async Task<string> RenderToStringAsync(string viewPath, object model)
        {
            var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            // ✅ Tạo RouteData có chứa Endpoint giả lập cho view
            var routeData = new RouteData();
            routeData.Routers.Add(new RouteCollection());
            routeData.Values["area"] = "Student";

            var actionContext = new ActionContext(
                httpContext,
                routeData,
                new ActionDescriptor()
            );

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.GetView(executingFilePath: null, viewPath, isMainPage: true);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"Không tìm thấy view: {viewPath}");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var tempData = new TempDataDictionary(httpContext, _tempDataProvider);
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    tempData,
                    sw,
                    new HtmlHelperOptions()
                );

                // ✅ Thiết lập UrlHelper cho ViewContext (fix lỗi IRouter)
                var urlHelperFactory = (IUrlHelperFactory)_serviceProvider.GetService(typeof(IUrlHelperFactory));
                if (urlHelperFactory != null)
                {
                    var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
                    viewContext.RouteData = routeData;
                    viewContext.HttpContext.Items["__UrlHelper"] = urlHelper;
                }

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}
