using System.Web.Routing;
using DotNetNuke.Web.Api;
using System;

namespace MyDnn.VisitorsOnline.App_Start
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("MyDnnVisitorsOnline", "default", "{controller}/{action}", new[] { "MyDnn.VisitorsOnline.Services" });
        }
    }
}