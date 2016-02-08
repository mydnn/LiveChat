using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using MyDnn.VisitorsOnline.Api;
using MyDnn.VisitorsOnline.Models;
using DotNetNuke.Common.Utilities;

namespace MyDnn.Modules.Support.LiveChat.Controllers
{
    public class HomeController : DnnController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));

            if (moduleID == -1)
            {
                moduleID = ActiveModule.ModuleID;
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "MyDnnLiveChatModuleID", moduleID.ToString(), true);

                var role = RoleController.Instance.GetRoleByName(PortalSettings.PortalId, "MyDnnSupportAgent");
                if (role == null)
                {
                    var objRoleInfo = new RoleInfo();
                    objRoleInfo.PortalID = PortalSettings.PortalId;
                    objRoleInfo.Description = "mydnn live chat agent";
                    objRoleInfo.RoleName = "MyDnnSupportAgent";
                    objRoleInfo.IsPublic = false;
                    objRoleInfo.Status = RoleStatus.Approved;
                    objRoleInfo.RoleGroupID = Null.NullInteger;
                    RoleController.Instance.AddRole(objRoleInfo);

                    var listeners = VisitorsOnlineApi.Instance.GetListeners(PortalSettings.PortalId);
                    if (listeners == null || !listeners.Any(l => l.RoleName == "MyDnnSupportAgent" && l.LoginState == VisitorsOnline.Components.Enums.LoginState.LoggedIn))
                    {
                        VisitorsOnlineApi.Instance.AddListener(new ListenerInfo()
                        {
                            PortalID = PortalSettings.PortalId,
                            RoleName = "MyDnnSupportAgent",
                            LoginState = VisitorsOnline.Components.Enums.LoginState.LoggedIn,
                            InvokeScript = "$('#mydnnLiveChatMinButton').find('[data-livechat-isonline]').hide();$('#mydnnLiveChatMinButton').find('[data-livechat-isonline=\"online\"]').show();",
                            CreatedByModuleName = "MyDnnSupportLiveChat",
                            CreatedOnDate = DateTime.Now
                        });
                    }
                    if (listeners == null || !listeners.Any(l => l.RoleName == "MyDnnSupportAgent" && l.LoginState == VisitorsOnline.Components.Enums.LoginState.LoggedOff))
                    {
                        VisitorsOnlineApi.Instance.AddListener(new ListenerInfo()
                        {
                            PortalID = PortalSettings.PortalId,
                            RoleName = "MyDnnSupportAgent",
                            LoginState = VisitorsOnline.Components.Enums.LoginState.LoggedOff,
                            InvokeScript = "$('#mydnnLiveChatMinButton').find('[data-livechat-isonline]').hide();$('#mydnnLiveChatMinButton').find('[data-livechat-isonline=\"offline\"]').show();",
                            CreatedByModuleName = "MyDnnSupportLiveChat",
                            CreatedOnDate = DateTime.Now
                        });
                    }
                }
            }

            return View();
        }
    }
}
