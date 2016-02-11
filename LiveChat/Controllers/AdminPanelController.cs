using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyDnn.Modules.Support.LiveChat.Controllers
{
    public class AdminPanelController : DnnController
    {
        public ActionResult AdminPanel()
        {
            bool mustRedirect = false;
            string newUrl = string.Empty;

            if (PortalSettings.EnablePopUps)
            {
                if (Request.QueryString["popUp"] == null || Request.QueryString["popUp"] != "true" || Request.Url.Query.IndexOf("popUp") == -1)
                {
                    mustRedirect = true;
                    newUrl = ModuleContext.EditUrl("popUp", "true", "AdminPanel", "controller", "AdminPanel", "action", "AdminPanel");
                }
            }
            else
            {
                if (Request.QueryString["SkinSrc"] == null)
                {
                    mustRedirect = true;
                    newUrl = ModuleContext.EditUrl("", "", "AdminPanel", "controller", "AdminPanel", "action", "AdminPanel", "SkinSrc=[G]Skins%2f_default%2fNo+Skin&ContainerSrc=[G]Containers%2f_default%2fNo+Container");
                }
            }

            if (!User.IsInRole("MyDnnSupportAgent"))
            {
                //add agent role to current user
                var role = RoleController.Instance.GetRoleByName(PortalSettings.PortalId, "MyDnnSupportAgent");
                RoleController.AddUserRole(User, role, PortalSettings, DotNetNuke.Security.Roles.RoleStatus.Approved, Null.NullDate, Null.NullDate, true, false);
            }

            var model = new ModuleConfigStatusViewModel();
            model.VisitorsOnlineEnabled = bool.Parse(PortalController.GetPortalSetting("MyDnnVisitorsOnlineEnabled", PortalSettings.PortalId, "false")); ;
            model.LiveChatEnabled = bool.Parse(PortalController.GetPortalSetting("MyDnnLiveChatEnabled", PortalSettings.PortalId, "false")); ;

            var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));
            if (moduleID != -1)
            {
                var objModuleInfo = new ModuleController().GetModule(moduleID);
                var Settings = objModuleInfo.ModuleSettings;

                model.ModuleID = objModuleInfo.ModuleID;
                model.TabID = objModuleInfo.TabID;
                model.BasicSettingsUpdated = Settings["UpdateBasicSettings"] != null ? bool.Parse(Settings["UpdateBasicSettings"].ToString()) : false;
                model.WidgetSettingsUpdated = Settings["UpdateWidgetSettings"] != null ? bool.Parse(Settings["UpdateWidgetSettings"].ToString()) : false; ;
            }

            if (mustRedirect)
                return Redirect(newUrl);
            else
                return View(model);
        }
    }
}
