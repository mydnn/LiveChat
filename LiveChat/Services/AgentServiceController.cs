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
using System.Threading.Tasks;
using DotNetNuke.Common.Utilities;

namespace MyDnn.Modules.Support.LiveChat.Services
{
    #region DTO Models

    /// <summary>
    /// 
    /// </summary>
    public class PostData
    {
        public int ID { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WidgetSettingsDTO
    {
        public Hashtable WidgetSettings { get; set; }
        public Hashtable MinButtonSettings { get; set; }
        public Hashtable Locales { get; set; }
    }

    public class BasicSettingsDTO
    {
        public Hashtable PortalSettings { get; set; }
        public Hashtable ModuleSettings { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HistoryDTO
    {
        public int[] Items { get; set; }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [SupportedModules("MyDnnSupport.LiveChat")]
    [DnnAuthorize(StaticRoles = "MyDnnSupportAgent")]
    public class AgentServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AgentServiceController));

        #region WebApi Methods

        #region Common

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            int portalID = PortalSettings.PortalId;
            var homeDirectory = PortalSettings.HomeDirectory;

            var destPath = PortalSettings.HomeDirectoryMapPath + "\\MyDnn\\LiveChat\\Admin\\";

            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            var provider = new CustomMultipartFormDataStreamProvider(destPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            var filename = Path.GetFileName(provider.FileData[0].LocalFileName);

            return Request.CreateResponse(HttpStatusCode.Created, homeDirectory + string.Format("MyDnn/LiveChat/Admin/{0}", filename));
        }

        #endregion

        #region Visitor List

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetVisitorList()
        {
            try
            {
                Thread.Sleep(500);

                var visitors = VisitorsOnlineApi.Instance.GetVisitorsOnline(PortalSettings.PortalId);

                //agent nabayd khodesh be onvane visitor bebinad
                //visitors = visitors.Except(visitors.Where(v => v.UserID == UserInfo.UserID));

                var incomingLiveChats = LiveChatManager.Instance.GetIncomingLiveChats(PortalSettings.PortalId);

                var currentLiveChats = LiveChatManager.Instance.GetCurrentLiveChats(PortalSettings.PortalId);

                var result = from v in visitors
                             join i in incomingLiveChats on v.VisitorGUID equals i.VisitorGUID into al
                             from f in al.DefaultIfEmpty()
                             join c in currentLiveChats on v.VisitorGUID equals c.VisitorGUID into r
                             from cc in r.DefaultIfEmpty()
                             select new
                             {
                                 VisitorID = v.VisitorID,
                                 VisitorGUID = v.VisitorGUID,
                                 PortalID = v.PortalID,
                                 UserName = v.UserName,
                                 DisplayName = v.DisplayName,
                                 Email = v.Email,
                                 OnlineDate = v.OnlineDate,
                                 IP = v.IP,
                                 UserAgent = v.UserAgent,
                                 LastURL = v.LastURL,
                                 ReferrerURL = v.ReferrerURL,
                                 TotalConnections = v.TotalConnections,
                                 UserID = v.UserID,
                                 IncomingLiveChat = (f == null ? false : true),
                                 CurrentLiveChat = (cc == null ? false : true),
                                 Message = (f == null ? string.Empty : f.VisitorMessage)
                             };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Department

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDepartments()
        {
            try
            {
                var departments = DepartmentManager.Instance.GetDepartmentsViewModel(PortalSettings.PortalId);
                return Request.CreateResponse(HttpStatusCode.OK, departments);
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
        public HttpResponseMessage GetNamesOfDepartments()
        {
            try
            {
                var departments = DepartmentManager.Instance.GetDepartmentsViewModel(PortalSettings.PortalId);
                var result = from department in departments select department.DepartmentName;
                return Request.CreateResponse(HttpStatusCode.OK, result);
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
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage SearchDepartments(string filter)
        {
            try
            {
                var departments = DepartmentManager.Instance.GetDepartments(PortalSettings.PortalId);
                if (filter != null)
                    departments = departments.Where(d => d.DepartmentName.ToLower().Contains(filter));
                return Request.CreateResponse(HttpStatusCode.OK, departments);
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
        /// <param name="department"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage AddDepartment(DepartmentViewModel department)
        {
            try
            {
                var objDepartmentInfo = new DepartmentInfo()
                {
                    PortalID = PortalSettings.PortalId,
                    DepartmentName = department.DepartmentName,
                    Description = department.Description,
                    TicketEnabled = department.TicketEnabled,
                    LiveChatEnabled = department.LiveChatEnabled,
                    CreateByUser = UserInfo.UserID,
                    CreateDate = DateTime.Now
                };
                int departmentID = DepartmentManager.Instance.AddDepartment(objDepartmentInfo);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, DepartmentID = departmentID });
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
        /// <param name="department"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage UpdateDepartment(DepartmentViewModel department)
        {
            try
            {
                var objDepartmentInfo = DepartmentManager.Instance.GetDepartment(PortalSettings.PortalId, department.DepartmentID);
                objDepartmentInfo.DepartmentName = department.DepartmentName;
                objDepartmentInfo.Description = department.Description;
                objDepartmentInfo.TicketEnabled = department.TicketEnabled;
                objDepartmentInfo.LiveChatEnabled = department.LiveChatEnabled;
                DepartmentManager.Instance.UpdateDepartment(objDepartmentInfo);

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
        public HttpResponseMessage DeleteDepartment(PostData postData)
        {
            try
            {
                DepartmentManager.Instance.DeleteDepartment(PortalSettings.PortalId, postData.ID);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                string msg = string.Empty;
                if (((System.Data.SqlClient.SqlException)(ex)).Number == 547)
                    msg = Localization.Instance.GetString(DotNetNuke.Common.Globals.ResolveUrl("~/MyDnnPackage/dnn8contest/Website/DesktopModules/MVC/MyDnnSupport/LiveChat/App_LocalResources/SharedResources"), PortalSettings.CultureCode, "DeleteDepartmentError.Text");

                return Request.CreateResponse(HttpStatusCode.InternalServerError, msg);
            }
        }

        #endregion

        #region Agent

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAgents()
        {
            try
            {
                var agents = AgentManager.Instance.GetAgentsViewModel(PortalSettings.PortalId);
                return Request.CreateResponse(HttpStatusCode.OK, agents);
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
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetUserByDisplayName(string name)
        {
            try
            {
                var users = Common.Instance.GetUsersByDisplayName(PortalSettings.PortalId, name);
                return Request.CreateResponse(HttpStatusCode.OK, users);
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
        /// <param name="searchType"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage SearchAgents(int searchType, string filter)
        {
            try
            {
                var agents = AgentManager.Instance.GetAgentsViewModel(PortalSettings.PortalId);
                if (searchType == 1 && filter != null)
                    agents = agents.Where(a => a.DisplayName.ToLower().Contains(filter));
                else if (searchType == 2 && filter != null)
                    agents = agents.Where(a => a.Departments.Any(d => filter.Split(',').Contains(d)));

                return Request.CreateResponse(HttpStatusCode.OK, agents);
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
        /// <param name="agent"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage AddAgent(AgentViewModel agent)
        {
            try
            {
                if (agent.UserID == 0)
                    throw new Exception();

                Requires.NotNegative("agent.UserID", agent.UserID);

                var user = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, agent.UserID);

                var objAgentInfo = new AgentInfo()
                {
                    PortalID = PortalSettings.PortalId,
                    UserID = agent.UserID,
                    Priority = agent.Priority,
                    Enabled = agent.Enabled,
                    CreateByUser = UserInfo.UserID,
                    CreateDate = DateTime.Now
                };
                int agentID = AgentManager.Instance.AddAgent(objAgentInfo);
                agent.AgentID = agentID;

                if (!user.IsInRole("MyDnnSupportAgent"))
                {
                    var role = RoleController.Instance.GetRoleByName(PortalSettings.PortalId, "MyDnnSupportAgent");
                    RoleController.AddUserRole(user, role, PortalSettings, DotNetNuke.Security.Roles.RoleStatus.Approved, Null.NullDate, Null.NullDate, true, false);
                }

                var departments = DepartmentManager.Instance.GetDepartments(PortalSettings.PortalId);
                foreach (var item in agent.Departments)
                {
                    var department = departments.FirstOrDefault(d => d.DepartmentName == item);
                    if (department == null) continue;
                    var objDepartmentAgentInfo = new DepartmentAgentInfo()
                    {
                        DepartmentID = department.DepartmentID,
                        AgentID = agentID,
                        UserID = agent.UserID
                    };
                    DepartmentAgentManager.Instance.AddDepartmentAgent(objDepartmentAgentInfo);
                }

                agent.Email = user.Email;

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Agent = agent });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                string msg = ex.Message;

                if (((System.Data.SqlClient.SqlException)(ex)).Number == 2627) // user entekhab shode ghablan dar system be onvane agent sabt shode ast
                    msg = string.Format(Localization.Instance.GetString(DotNetNuke.Common.Globals.ResolveUrl("~/MyDnnPackage/dnn8contest/Website/DesktopModules/MVC/MyDnnSupport/LiveChat/App_LocalResources/SharedResources"), PortalSettings.CultureCode, "DuplicateAgent.Text"), agent.DisplayName);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, msg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage UpdateAgent(AgentViewModel agent)
        {
            try
            {
                var objAgentInfo = AgentManager.Instance.GetAgent(PortalSettings.PortalId, agent.AgentID);
                objAgentInfo.PortalID = PortalSettings.PortalId;
                objAgentInfo.Priority = agent.Priority;
                objAgentInfo.Enabled = agent.Enabled;
                AgentManager.Instance.UpdateAgent(objAgentInfo);

                DepartmentAgentManager.Instance.DeleteAgentDepartments(agent.AgentID);
                var departments = DepartmentManager.Instance.GetDepartments(PortalSettings.PortalId);
                foreach (var item in agent.Departments)
                {
                    var department = departments.FirstOrDefault(d => d.DepartmentName == item);
                    if (department == null) continue;
                    var objDepartmentAgentInfo = new DepartmentAgentInfo()
                    {
                        DepartmentID = department.DepartmentID,
                        AgentID = agent.AgentID,
                        UserID = agent.UserID
                    };
                    DepartmentAgentManager.Instance.AddDepartmentAgent(objDepartmentAgentInfo);
                }

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
        public HttpResponseMessage DeleteAgent(PostData postData)
        {
            try
            {
                var agent = AgentManager.Instance.GetAgent(PortalSettings.PortalId, postData.ID);

                DepartmentAgentManager.Instance.DeleteAgentDepartments(postData.ID);
                AgentManager.Instance.DeleteAgent(PortalSettings.PortalId, postData.ID);

                var user = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, agent.UserID);
                var role = RoleController.Instance.GetRoleByName(PortalSettings.PortalId, "MyDnnSupportAgent");
                RoleController.DeleteUserRole(user, role, PortalSettings, false);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region LiveChat

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetIncomingLiveChats()
        {
            try
            {
                var incomingLiveChats = LiveChatManager.Instance.GetIncomingLiveChats(PortalSettings.PortalId);
                return Request.CreateResponse(HttpStatusCode.OK, incomingLiveChats);
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
        /// <param name="type"></param>
        /// <param name="departments"></param>
        /// <param name="agents"></param>
        /// <param name="visitorEmail"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="rating"></param>
        /// <param name="unread"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetChatHistory(int type, string departments, string agents, string visitorEmail, DateTime? fromDate, DateTime? toDate, int rating, bool unread, int pageIndex, int pageSize, int totalCount)
        {
            try
            {
                var result = LiveChatManager.Instance.GetChatHistory(PortalSettings.PortalId, type, departments, agents, visitorEmail, fromDate, toDate, rating, unread, pageIndex, pageSize, totalCount);

                int pageCount = (int)Math.Ceiling((double)result.TotalCount / pageSize);

                var paging = new
                {
                    PageIndex = result.PageIndex,
                    PageSize = result.PageSize,
                    PageCount = pageCount,
                    TotalCount = result.TotalCount,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage,
                    IsFirstPage = result.IsFirstPage,
                    IsLastPage = result.IsLastPage,
                    Pages = new int[pageCount]
                };
                return Request.CreateResponse(HttpStatusCode.OK, new { LiveChats = result, Paging = paging });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage PurgeHistory(HistoryDTO postData)
        {
            try
            {
                LiveChatManager.Instance.PurgeHistory(postData.Items);

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
        /// <param name="livechatID"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetMessages(int livechatID)
        {
            try
            {
                var messages = LiveChatMessageManager.Instance.GetMessages(livechatID);

                return Request.CreateResponse(HttpStatusCode.OK, new { Messages = messages });

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
        public HttpResponseMessage GetBasicSettings()
        {
            try
            {
                Hashtable moduleSettings = new Hashtable();
                Hashtable portalSettings = new Hashtable();

                var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));
                if (moduleID > 0)
                {
                    var moduleInfo = new ModuleController().GetModule(moduleID);
                    moduleSettings = moduleInfo.ModuleSettings;

                    portalSettings.Add("VisitorsOnlineEnabled", PortalController.GetPortalSetting("MyDnnVisitorsOnlineEnabled", PortalSettings.PortalId, "false"));
                    portalSettings.Add("LiveChatEnabled", PortalController.GetPortalSetting("MyDnnLiveChatEnabled", PortalSettings.PortalId, "false"));
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    PortalSettings = portalSettings,
                    ModuleSettings = moduleSettings
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
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage UpdateBasicSettings(BasicSettingsDTO postData)
        {
            try
            {
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "MyDnnVisitorsOnlineEnabled", postData.PortalSettings["VisitorsOnlineEnabled"].ToString(), true);
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "MyDnnLiveChatEnabled", postData.PortalSettings["LiveChatEnabled"].ToString(), true);

                var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));

                Requires.NotNegative("moduleID", moduleID);

                ModuleController.Instance.UpdateModuleSetting(moduleID, "UpdateBasicSettings", "True");

                ModuleController.Instance.UpdateModuleSetting(moduleID, "PlaySoundWhenNewMsg", postData.ModuleSettings["PlaySoundWhenNewMsg"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "ShowDekstopNotificationForIncoming", postData.ModuleSettings["ShowDekstopNotificationForIncoming"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "ShowDekstopNotificationForNewMsg", postData.ModuleSettings["ShowDekstopNotificationForNewMsg"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "AgentsViewPermission", postData.ModuleSettings["AgentsViewPermission"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "SendEmailForOffline", postData.ModuleSettings["SendEmailForOffline"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "SendEmailAfterChat", postData.ModuleSettings["SendEmailAfterChat"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "TranscriptEmailTemplate", postData.ModuleSettings["TranscriptEmailTemplate"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "OfflineEmailTemplate", postData.ModuleSettings["OfflineEmailTemplate"].ToString());

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
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetWidgetSettings()
        {
            try
            {
                string template = "default";
                Hashtable Settings = new Hashtable();

                var livechatEnabled = bool.Parse(PortalController.GetPortalSetting("MyDnnLiveChatEnabled", PortalSettings.PortalId, "false"));
                if (livechatEnabled)
                {
                    var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));
                    if (moduleID > 0)
                    {
                        var moduleInfo = new ModuleController().GetModule(moduleID);
                        Settings = moduleInfo.ModuleSettings;
                        template = (Settings["Template"] != null ? Settings["Template"].ToString() : "default");
                    }
                }

                string liveChatWidget = Common.GetFileContent(HttpContext.Current.Request.MapPath(Globals.ResolveUrl(string.Format("~/DesktopModules/MVC/MyDnnSupport/LiveChat/Templates/{0}/view.html", template))));

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Template = template,
                    Settings = Settings,
                    LiveChatWidget = liveChatWidget
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
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage UpdateWidgetSettings(WidgetSettingsDTO postData)
        {
            try
            {
                var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", PortalSettings.PortalId, "-1"));

                Requires.NotNegative("moduleID", moduleID);

                ModuleController.Instance.UpdateModuleSetting(moduleID, "UpdateWidgetSettings", "True");

                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatThemeColor", postData.WidgetSettings["LiveChatThemeColor"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatTitleColor", postData.WidgetSettings["LiveChatTitleColor"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatWindowSize", postData.WidgetSettings["LiveChatWindowSize"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatWidgetPosition", postData.WidgetSettings["LiveChatWidgetPosition"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatEnableRating", postData.WidgetSettings["LiveChatEnableRating"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatShowAvatar", postData.WidgetSettings["LiveChatShowAvatar"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMessageStyle", postData.WidgetSettings["LiveChatMessageStyle"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "VisitorDefaultAvatar", postData.WidgetSettings["VisitorDefaultAvatar"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "AgentDefaultAvatar", postData.WidgetSettings["AgentDefaultAvatar"].ToString());

                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnOnline", postData.MinButtonSettings["OnlineButton"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnOnlineBgColor", postData.MinButtonSettings["OnlineButtonBGColor"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnOnlineColor", postData.MinButtonSettings["OnlineButtonColor"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnOffline", postData.MinButtonSettings["OfflineButton"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnOfflineBgColor", postData.MinButtonSettings["OfflineButtonBGColor"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnOfflineColor", postData.MinButtonSettings["OfflineButtonColor"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnHPos", postData.MinButtonSettings["HorizontalPosition"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnVPos", postData.MinButtonSettings["VerticalPosition"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnRotate", postData.MinButtonSettings["Rotate"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnCssStyle", postData.MinButtonSettings["CssStyle"].ToString());
                ModuleController.Instance.UpdateModuleSetting(moduleID, "LiveChatMinBtnHtmlTemplate", postData.MinButtonSettings["HtmlTemplate"].ToString().Replace("ng-hide", string.Empty));

                Localization.Instance.UpdateStrings("~/DesktopModules/MVC/MyDnnSupport/LiveChat/App_LocalResources/SharedResources", PortalSettings.CultureCode, postData.Locales);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage SendEmail(EmailDTO postData)
        {
            try
            {
                DotNetNuke.Services.Mail.Mail.SendEmail(PortalSettings.Email, postData.To, postData.Subject, postData.Body);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #endregion
    }
}