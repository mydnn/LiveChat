using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Users.Internal;
using System.Globalization;
using System.Resources;
using System.Xml;
using System.Web.Script.Serialization;
using System.IO;
using System.Threading;
using MyDnn.Modules.Support.LiveChat.Components.Common;
using MyDnn.VisitorsOnline.Api;
using MyDnn.Modules.Support.LiveChat.Components;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using MyDnn.Modules.Support.LiveChats.ViewModels;
using MyDnn.Modules.Support.LiveChat.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using System.Text.RegularExpressions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Instrumentation;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;

namespace MyDnn.Modules.Support.LiveChat.Services
{
    #region DTO Models

    /// <summary>
    /// 
    /// </summary>
    public class DepartmentDTO
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public bool IsAgentOnline { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EmailDTO
    {
        public int DepartmentID { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    [AllowAnonymous]
    public class VisitorServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(VisitorServiceController));

        #region WebApi Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage LiveChatWidget()
        {
            try
            {
                string liveChatMinButton = string.Empty;
                bool isAgentOnline = false;
                string template = string.Empty;
                bool isAgent = false;
                string requestsString = string.Empty;
                string adminPanelUrl = string.Empty;

                var livechatEnabled = bool.Parse(PortalController.GetPortalSetting("MyDnnLiveChatEnabled", PortalSettings.PortalId, "false"));
                if (livechatEnabled)
                {
                    var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));
                    if (moduleID > 0)
                    {
                        isAgentOnline = AgentManager.Instance.IsAgentOnline(PortalSettings.PortalId);

                        var moduleInfo = new ModuleController().GetModule(moduleID);
                        var Settings = moduleInfo.ModuleSettings;
                        liveChatMinButton = (Settings["LiveChatMinBtnHtmlTemplate"] != null ? Settings["LiveChatMinBtnHtmlTemplate"].ToString() : string.Empty);
                        template = (Settings["Template"] != null ? Settings["Template"].ToString() : "default");

                        if (UserInfo.UserID != -1)
                        {
                            isAgent = AgentManager.Instance.GetAgentByUserName(PortalSettings.PortalId, UserInfo.Username) != null;
                            if (isAgent)
                            {
                                requestsString = Localization.Instance.GetString("~/DesktopModules/MVC/MyDnnSupport/LiveChat/App_LocalResources/SharedResources", PortalSettings.CultureCode, "ServeRequests.Text");
                                adminPanelUrl = DotNetNuke.Common.Globals.NavigateURL(moduleInfo.TabID, PortalSettings, "AdminPanel", "mid", moduleInfo.ModuleID.ToString(), "popUp", "true");
                            }
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    RootUrl = DotNetNuke.Common.Globals.ResolveUrl("~/"),
                    PortalID = PortalSettings.PortalId,
                    Template = template,
                    LiveChatEnabled = livechatEnabled,
                    LiveChatMinButton = liveChatMinButton,
                    IsAgentOnline = isAgentOnline,
                    IsAgent = isAgent,
                    RequestsString = requestsString,
                    AdminPanelUrl = adminPanelUrl
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetResources(string resource)
        {
            var result = Localization.Instance.GetResources(resource, CultureInfo.CurrentCulture.ToString());

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage InitialLiveChat(string visitorGUID)
        {
            try
            {
                var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));

                Requires.NotNegative("moduleID", moduleID);

                var Settings = new ModuleController().GetModule(moduleID).ModuleSettings;

                string template = (Settings["Template"] != null ? Settings["Template"].ToString() : "default");
                string liveChatWidget = Common.GetFileContent(HttpContext.Current.Request.MapPath(Globals.ResolveUrl(string.Format("~/DesktopModules/MVC/MyDnnSupport/LiveChat/Templates/{0}/view.html", template))));

                var resources = Localization.Instance.GetResources(string.Format("~/DesktopModules/MVC/MyDnnSupport/LiveChat/App_LocalResources/SharedResources"), CultureInfo.CurrentCulture.ToString());

                bool isAgentOnline = false;

                var departments = GetDepartments(out isAgentOnline);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    PortalID = PortalSettings.PortalId,
                    Resources = resources,
                    Departments = departments,
                    Settings = Settings,
                    Template = template,
                    Widget = liveChatWidget,
                    UserID = UserInfo.UserID,
                    DisplayName = (UserInfo.UserID != -1 ? UserInfo.DisplayName : string.Empty),
                    Email = (UserInfo.UserID != -1 ? UserInfo.Email : string.Empty),
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDepartmentsForLiveChat()
        {
            try
            {
                bool isAgentOnline = false;
                var departments = GetDepartments(out isAgentOnline);

                return Request.CreateResponse(HttpStatusCode.OK, new { Departments = departments, IsAgentOnline = isAgentOnline });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage IsAgentOnlineByDepartment(int departmentID)
        {
            try
            {
                var isAgentOnline = DepartmentAgentManager.Instance.IsAgentOnlineByDepartment(PortalSettings.PortalId, departmentID);
                return Request.CreateResponse(HttpStatusCode.OK, isAgentOnline);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SendEmail(EmailDTO postData)
        {
            try
            {
                Mail.SendEmail(PortalSettings.Email, postData.To, postData.Subject, postData.Body);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SendOfflineMessageEmail(EmailDTO postData)
        {
            try
            {
                var agents = DepartmentAgentManager.Instance.GetDepartmentAgents(postData.DepartmentID);
                foreach (var agent in agents)
                {
                    var user = UserController.GetUserById(PortalSettings.PortalId, agent.UserID);
                    if (user != null)
                        Mail.SendEmail(PortalSettings.Email, user.Email, postData.Subject, postData.Body);
                }

                if (agents == null || !agents.Any())
                    Mail.SendEmail(PortalSettings.Email, PortalSettings.Email, postData.Subject, postData.Body);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isAgentOnline"></param>
        /// <returns></returns>
        private List<DepartmentDTO> GetDepartments(out bool isAgentOnline)
        {
            isAgentOnline = false;

            List<DepartmentDTO> departments = new List<DepartmentDTO>();
            var usersOnline = VisitorsOnlineApi.Instance.GetVisitorsOnline(PortalSettings.PortalId);
            var deps = DepartmentManager.Instance.GetDepartmentsForLiveChat(PortalSettings.PortalId);
            foreach (DepartmentInfo dep in deps)
            {
                var department = new DepartmentDTO()
                {
                    DepartmentID = dep.DepartmentID,
                    DepartmentName = dep.DepartmentName,
                    IsAgentOnline = false
                };
                var agents = DepartmentAgentManager.Instance.GetDepartmentAgents(dep.DepartmentID);
                if (usersOnline != null && agents != null)
                {
                    isAgentOnline = usersOnline.Any(u => agents.Select(a => a.UserID).Contains(u.UserID));
                    department.IsAgentOnline = isAgentOnline;
                }
                departments.Add(department);
            }

            return departments;
        }

        #endregion
    }
}