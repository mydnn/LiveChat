using System.Web.Routing;
using DotNetNuke.Web.Api;
using System;

namespace MyDnn.Modules.Support.LiveChat.App_Start
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("MyDnnSupport.LiveChat", "default", "{controller}/{action}", new[] { "MyDnn.Modules.Support.LiveChat.Services" });
        }
    }
}