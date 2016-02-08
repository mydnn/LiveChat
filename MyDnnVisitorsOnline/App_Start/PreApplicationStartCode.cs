using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using MyDnn.VisitorsOnline.App_Start;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStartCode), "Start")]
namespace MyDnn.VisitorsOnline.App_Start
{
    public class PreApplicationStartCode
    {
        public static void Start()
        {
            // Register our module
            DynamicModuleUtility.RegisterModule(typeof(MyDnnVisitorsOnlineHttpModule));
        }
    }

    public class MyDnnVisitorsOnlineHttpModule : IHttpModule
    {
        #region IHttpModule Members

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
        }

        #endregion

        protected void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            try
            {
                HttpContext currentContext = HttpContext.Current;
                Page page = currentContext.CurrentHandler as Page;

                if (page != null)
                {
                    page.LoadComplete += new EventHandler(page_LoadComplete);
                }
            }
            catch (Exception)
            {
            }
        }

        protected void page_LoadComplete(object sender, EventArgs e)
        {
            try
            {
                Page page = sender as Page;

                if (page.IsCallback == false && page.IsPostBack == false)
                {
                    if (!page.ClientScript.IsClientScriptIncludeRegistered("MydnnVisitorsOnlineInit"))
                    {
                        page.ClientScript.RegisterClientScriptInclude("MydnnVisitorsOnlineInit", DotNetNuke.Common.Globals.ResolveUrl("~/DesktopModules/MVC/MyDnnSupport/LiveChat/Scripts/visitors-online.js"));
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}