// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using System.Threading;
using MyDnn.VisitorsOnline;
using System.Collections;
using System.Xml;
using System.IO;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MyDnn.VisitorsOnline.Api;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChat.Components.Enums;
using MyDnn.Modules.Support.LiveChat.Components;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using MyDnn.VisitorsOnline.Hubs;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;

namespace MyDnn.Modules.Support.LiveChat.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    [HubName("MyDnnSupportLiveChatHub")]
    public class LiveChatHub : Hub
    {
        #region Variable && Properties

        /// <summary>
        /// 
        /// </summary>
        private string UserName
        {
            get
            {
                if (Context.User.Identity.IsAuthenticated)
                {
                    return Context.User.Identity.Name;
                }
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static Timer LiveChatRequestTimer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private const string DepartmentAgentsGroupName = "LiveChat-Agents-Department-";

        /// <summary>
        /// 
        /// </summary>
        private const string AgentGroupName = "LiveChat-Agent-";

        /// <summary>
        /// 
        /// </summary>
        private const string AllAgentsGroupName = "LiveChat-AllAgents";

        #endregion

        #region Override Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        #endregion

        #region Hub Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="loadIncommingLiveChats"></param>
        /// <param name="loadLiveChats"></param>
        /// <returns></returns>
        public object JoinAgent(int portalID, bool loadIncommingLiveChats, bool loadLiveChats)
        {
            var agent = AgentManager.Instance.GetAgentByUserName(portalID, UserName);
            if (agent == null) return null;

            var context = GlobalHost.ConnectionManager.GetHubContext<VisitorsOnlineHub>();

            Groups.Add(Context.ConnectionId, AllAgentsGroupName);
            Groups.Add(Context.ConnectionId, AgentGroupName + agent.UserID);

            var agentDepartments = DepartmentAgentManager.Instance.GetAgentDepartments(agent.AgentID);
            foreach (var department in agentDepartments)
            {
                Groups.Add(Context.ConnectionId, DepartmentAgentsGroupName + department.DepartmentID);

                //in bakhsh enghad pichidas ke hata nemitonam tozih bedam !!
                context.Groups.Add(Context.ConnectionId, "MyDnnLiveChatAgents-" + department.DepartmentID);
            }

            //hazf tamame darkhast haye chati ke visitorash rafte peye karash
            LiveChatManager.Instance.CloseLeftLiveChats(portalID);

            List<LiveChatViewModel> livechats = null;
            IEnumerable<LiveChatInfo> incomingLiveChats = null;

            if (loadIncommingLiveChats)
            {
                incomingLiveChats = LiveChatManager.Instance.GetIncomingLiveChats(portalID);
            }

            if (loadLiveChats)
            {
                livechats = new List<LiveChatViewModel>();

                var currentLiveChats = LiveChatManager.Instance.GetCurrentLiveChatsByAgent(portalID, agent.UserID);
                foreach (LiveChatInfo objLiveChatInfo in currentLiveChats)
                {
                    var livechat = GetLiveChatViewModel(objLiveChatInfo);
                    livechat.ChatStarted = true;
                    livechats.Add(livechat);
                }
            }

            var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", portalID, "-1"));
            var Settings = new ModuleController().GetModule(moduleID).ModuleSettings;

            return new
            {
                IncomingLiveChats = incomingLiveChats.Select(i => new { LiveChatID = i.LiveChatID, VisitorGUID = i.VisitorGUID }),
                LiveChats = livechats,
                Me = agent,
                Settings = Settings
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        public object JoinVisitor(int portalID, string visitorGUID)
        {
            Groups.Add(Context.ConnectionId, portalID + "-" + visitorGUID);

            var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", portalID, "-1"));
            Requires.NotNegative("moduleID", moduleID);
            var Settings = new ModuleController().GetModule(moduleID).ModuleSettings;

            var currentLiveChat = LiveChatManager.Instance.GetCurrentLiveChatByVisitor(portalID, visitorGUID);

            LiveChatViewModel livechat = null;

            if (currentLiveChat != null)
            {
                if (currentLiveChat.LastMessageDate != null && (DateTime.Now - currentLiveChat.LastMessageDate).TotalHours < 10) // in ghesmat baraye in ast ke agar az akharin post visitor bish az 10 saat gozashte bashad live chat ra neshan nadahad
                {
                    livechat = GetLiveChatViewModel(currentLiveChat);
                }
                else
                    LiveChatManager.Instance.CloseLiveChat(currentLiveChat.LiveChatID, false);
            }

            return livechat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechat"></param>
        /// <returns></returns>
        public int StartLiveChatByVisitor(LiveChatViewModel livechat)
        {
            if (LiveChatManager.Instance.hasVisitorOpenedLiveChat(livechat.PortalID, livechat.Visitor.VisitorGUID))
                return 0;

            var visitor = VisitorsOnlineApi.Instance.GetVisitorByGUID(livechat.PortalID, livechat.Visitor.VisitorGUID);

            var objLiveChatInfo = new LiveChatInfo()
            {
                PortalID = livechat.PortalID,
                VisitorGUID = livechat.Visitor.VisitorGUID,
                VisitorName = livechat.Visitor.DisplayName,
                VisitorEmail = livechat.Visitor.Email,
                VisitorUserID = visitor.UserID,
                VisitorIP = visitor.IP,
                VisitorUserAgent = visitor.UserAgent,
                VisitorMessage = livechat.Message,
                CreateDate = DateTime.Now,
                CloseDate = DateTime.MaxValue,
                IsClosed = false,
            };
            int livechatID = LiveChatManager.Instance.AddLiveChat(objLiveChatInfo);
            LiveChatDepartmentManager.Instance.AddLiveChatDepartment(new LiveChatDepartmentInfo()
            {
                LiveChatID = livechatID,
                DepartmentID = livechat.Departments.First().DepartmentID,
                CreateDate = DateTime.Now
            });

            //create messages
            var objLiveChatMessageInfo = new LiveChatMessageInfo()
            {
                LiveChatID = livechatID,
                SentBy = MessageSentBy.System,
                CreateDate = DateTime.Now,
                MessageType = MessageType.Text,
                Message = "Chat started",
            };
            LiveChatMessageManager.Instance.AddMessage(objLiveChatMessageInfo);

            if (!string.IsNullOrEmpty(livechat.Message))
            {
                objLiveChatMessageInfo = new LiveChatMessageInfo()
                {
                    LiveChatID = livechatID,
                    SentBy = MessageSentBy.Visitor,
                    CreateDate = DateTime.Now,
                    MessageType = MessageType.Text,
                    Message = livechat.Message
                };
                LiveChatMessageManager.Instance.AddMessage(objLiveChatMessageInfo);
            }

            livechat = GetLiveChatViewModel(objLiveChatInfo);

            //ersale live chat kamel shode be khode visitor(darkhast konande chat)
            Clients.Group(livechat.PortalID + "-" + visitor.VisitorGUID).startLiveChat(livechat);
            //send live chat request to agents
            var department = livechat.Departments.First();
            Clients.Group(DepartmentAgentsGroupName + department.DepartmentID).incomingLiveChat(livechatID, visitor.VisitorGUID, livechat.Message, "add");

            var context = GlobalHost.ConnectionManager.GetHubContext<VisitorsOnlineHub>();
            context.Clients.Group("MyDnnLiveChatAgents-" + department.DepartmentID).invokeScript(string.Format("mydnnLiveChatRequests({0},'add');", livechatID));

            //update visitor info in visitor list 
            visitor.DisplayName = livechat.Visitor.DisplayName;
            visitor.Email = livechat.Visitor.Email;

            //eslah shavad
            ///VisitorsOnlineApi.Instance.UpdateVisitorOnline(livechat.PortalID, visitor.VisitorGUID, visitor.UserName, visitor.DisplayName, visitor.Email, visitor.LastURL, Context.ConnectionId, true);
            //var context = GlobalHost.ConnectionManager.GetHubContext<VisitorsOnlineHub>();
            ///context.Clients.Group("MyDnnVisitorsOnline").updateVisitorInfo(visitor);

            return livechatID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <param name="reInitialize"></param>
        /// <returns></returns>
        public async Task InitialLiveChatForAgent(int livechatID, bool reInitialize = false)
        {
            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(livechatID);

            if (objLiveChatInfo != null)
            {
                var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", objLiveChatInfo.PortalID, "-1"));
                Requires.NotNegative("moduleID", moduleID);
                var Settings = new ModuleController().GetModule(moduleID).ModuleSettings;

                var agent = AgentManager.Instance.GetAgentByUserName(objLiveChatInfo.PortalID, this.UserName);
                bool isAdmin = agent.IsAdmin;

                var livechat = GetLiveChatViewModel(objLiveChatInfo);

                //if (reInitialize)
                //agentUserID = agent.UserID;

                if (!livechat.Agents.Any() || Settings["AgentsViewPermission"].ToString() == LiveChatViewPermission.AllAgentsInDepartment.ToString() ||
                    (Settings["AgentsViewPermission"].ToString() == LiveChatViewPermission.OnlyCurrentAgentsAndAdmin.ToString() && isAdmin))
                {
                    await Clients.Group(AgentGroupName + agent.UserID).initialLiveChatForAgent(livechat);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        public void StartLiveChatByAgent(int livechatID)
        {
            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(livechatID);
            if (objLiveChatInfo != null)
            {
                var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", objLiveChatInfo.PortalID, "-1"));
                Requires.NotNegative("moduleID", moduleID);
                var Settings = new ModuleController().GetModule(moduleID).ModuleSettings;

                var livechat = GetLiveChatViewModel(objLiveChatInfo);

                var meAsAgent = AgentManager.Instance.GetAgentByUserName(objLiveChatInfo.PortalID, this.UserName);

                Clients.Group(DepartmentAgentsGroupName + livechat.Departments.First().DepartmentID).incomingLiveChat(objLiveChatInfo.LiveChatID, null, null, "remove");

                if (!livechat.Agents.Any())
                {
                    //in ghesmat baraye in ast ke pas az shoro goftego tavasote karshenas livechat az tamame karshenas hai ke permission didan goftefo ra nadaran hazf shavad
                    if (Settings["AgentsViewPermission"].ToString() != LiveChatViewPermission.AllAgentsInDepartment.ToString())
                    {
                        foreach (var agent in DepartmentAgentManager.Instance.GetDepartmentAgents(livechat.Departments.First().DepartmentID))
                        {
                            if (meAsAgent.UserID == agent.UserID) continue;

                            var objAgentView = AgentManager.Instance.GetAgentByUserID(objLiveChatInfo.PortalID, agent.UserID);
                            bool isAdmin = objAgentView.IsAdmin;

                            if (Settings["AgentsViewPermission"].ToString() == LiveChatViewPermission.OnlyCurrentAgents.ToString() ||
                                (Settings["AgentsViewPermission"].ToString() == LiveChatViewPermission.OnlyCurrentAgentsAndAdmin.ToString() && !isAdmin))
                            {
                                Clients.Group(AgentGroupName + agent.UserID).removeLiveChat(objLiveChatInfo.LiveChatID);
                            }
                        }
                    }
                }

                AgentHasJoin(objLiveChatInfo, meAsAgent, true, true);

                Clients.Group(AllAgentsGroupName).visitorIsChatting(objLiveChatInfo.VisitorGUID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="message"></param>
        /// <param name="reOpen"></param>
        /// <returns></returns>
        public int SendMessage(int portalID, LiveChatMessageInfo message, bool reOpen = false)
        {
            try
            {
                if (message.SentBy == MessageSentBy.System || message.SentBy == MessageSentBy.Visitor)
                    message.AgentUserID = -1;
                else
                {
                    message.AgentUserID = AgentManager.Instance.GetAgentByUserName(portalID, UserName).UserID;
                }

                message.CreateDate = DateTime.Now;
                message.MessageID = LiveChatMessageManager.Instance.AddMessage(message);

                var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(message.LiveChatID);

                //ersal payam be visitor be joz tab jari
                Clients.Group(portalID + "-" + objLiveChatInfo.VisitorGUID, Context.ConnectionId).reciveMessage(message);

                //ersal payam be agent ha
                var agents = LiveChatAgentManager.Instance.GetLiveChatAgents(objLiveChatInfo.LiveChatID);
                if (!agents.Any(a => a.hasLeft == false)) // agar hich agenti join nashode bayad payam roye tamame agenthaye department ersal shavad
                {
                    var departments = LiveChatDepartmentManager.Instance.GetLiveChatDepartments(objLiveChatInfo.LiveChatID);
                    Clients.Group(DepartmentAgentsGroupName + departments.First().DepartmentID).reciveMessage(message);
                }
                else
                {
                    foreach (var agent in agents)
                    {
                        Clients.Group(AgentGroupName + agent.UserID, Context.ConnectionId).reciveMessage(message);
                    }
                }

                if (objLiveChatInfo.IsClosed)
                {
                    //re open live chat -- next version :)
                }

                return message.MessageID;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="livechatID"></param>
        public void VisitorIsTyping(int portalID, int livechatID)
        {
            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(livechatID);

            var agents = LiveChatAgentManager.Instance.GetLiveChatAgents(objLiveChatInfo.LiveChatID);
            if (!agents.Any(a => a.hasLeft == false)) // agar hich agenti join nashode bayad payam roye tamame agenthaye department ersal shavad
            {
                var departments = LiveChatDepartmentManager.Instance.GetLiveChatDepartments(objLiveChatInfo.LiveChatID);
                Clients.Group(DepartmentAgentsGroupName + departments.First().DepartmentID).visitorIsTyping();
            }
            else
            {
                foreach (var agent in agents)
                {
                    Clients.Group(AgentGroupName + agent.UserID).visitorIsTyping();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        public void AgentIsTyping(int portalID, string visitorGUID)
        {
            Clients.Group(portalID + "-" + visitorGUID).agentIsTyping();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="message"></param>
        public void SeenMessage(int portalID, LiveChatMessageInfo message)
        {
            LiveChatMessageManager.Instance.SeenMessage(message.MessageID);

            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(message.LiveChatID);
            Clients.Group(portalID + "-" + objLiveChatInfo.VisitorGUID).seenMessage(message.MessageID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="liveChatID"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public bool RateChat(int liveChatID, LiveChatRating rate)
        {
            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(liveChatID);

            objLiveChatInfo.Rate = rate;
            LiveChatManager.Instance.UpdateLiveChat(objLiveChatInfo);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        public void VisitorHasLeftChat(int livechatID)
        {
            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(livechatID);
            LiveChatManager.Instance.CloseLiveChat(objLiveChatInfo.LiveChatID, false);

            var objLiveChatMessageInfo = new LiveChatMessageInfo()
            {
                LiveChatID = objLiveChatInfo.LiveChatID,
                CreateDate = DateTime.Now,
                SentBy = MessageSentBy.System,
                MessageType = MessageType.Text,
                Message = "Visitor has left chat",
            };
            int messageID = LiveChatMessageManager.Instance.AddMessage(objLiveChatMessageInfo);
            objLiveChatMessageInfo.MessageID = messageID;

            //ersal payam be agent ha
            var agents = LiveChatAgentManager.Instance.GetLiveChatAgents(objLiveChatInfo.LiveChatID);
            if (!agents.Any(a => a.hasLeft == false)) // agar hich agenti join nashode bayad payam roye tamame agenthaye department ersal shavad
            {
                var departments = LiveChatDepartmentManager.Instance.GetLiveChatDepartments(objLiveChatInfo.LiveChatID);
                Clients.Group(DepartmentAgentsGroupName + departments.First().DepartmentID).reciveMessage(objLiveChatMessageInfo);
            }
            else
            {
                foreach (var agent in agents)
                {
                    Clients.Group(AgentGroupName + agent.UserID).reciveMessage(objLiveChatMessageInfo);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="livechatID"></param>
        /// <param name="agentUserID"></param>
        /// <param name="lastMessageID"></param>
        /// <returns></returns>
        public object VisitorReconnectedToLiveChat(int portalID, int livechatID, int agentUserID, int lastMessageID)
        {
            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(livechatID);
            if (objLiveChatInfo != null)
            {
                var messages = LiveChatMessageManager.Instance.GetUnreadMessages(objLiveChatInfo.LiveChatID, lastMessageID); //daryaft payamhai ke visitor baad az ghat shodane internetash nadide ast

                //if (objLiveChatInfo.AgentUserID != agentUserID) // in yani visitor be karshenasi vasl nabode va zamani ke internetesh ghat mishavad yek karshenas be darkhastash pasokh mosbat midahad
                //{
                //    AgentHasJoin(objLiveChatInfo, null, true, false);
                //}

                return new { IsClosed = objLiveChatInfo.IsClosed, Messages = messages };
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="livechatID"></param>
        /// <param name="lastMessageDate"></param>
        public void AgentReconnectedToLiveChat(int portalID, int livechatID, DateTime lastMessageDate)
        {
            // -- next version :)
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        public bool CloseLiveChatByVisitor(int portalID, int livechatID)
        {
            var isClosed = false;

            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(livechatID);
            if (objLiveChatInfo != null && !objLiveChatInfo.IsClosed)
            {
                LiveChatManager.Instance.CloseLiveChat(livechatID, false);

                var objLiveChatMessageInfo = new LiveChatMessageInfo()
                {
                    LiveChatID = objLiveChatInfo.LiveChatID,
                    CreateDate = DateTime.Now,
                    SentBy = MessageSentBy.System,
                    MessageType = MessageType.Text,
                    Message = "Visitor closed the live chat",
                };
                int messageID = LiveChatMessageManager.Instance.AddMessage(objLiveChatMessageInfo);
                objLiveChatMessageInfo.MessageID = messageID;

                Clients.Group(portalID + "-" + objLiveChatInfo.VisitorGUID).closeLiveChatByVisitor(objLiveChatInfo.LiveChatID);
                Clients.Group(portalID + "-" + objLiveChatInfo.VisitorGUID).reciveMessage(objLiveChatMessageInfo);

                //ersal payam be agent ha
                var agents = LiveChatAgentManager.Instance.GetLiveChatAgents(objLiveChatInfo.LiveChatID);
                if (!agents.Any(a => a.hasLeft == false)) // agar hich agenti join nashode bayad payam roye tamame agenthaye department ersal shavad
                {
                    var departments = LiveChatDepartmentManager.Instance.GetLiveChatDepartments(objLiveChatInfo.LiveChatID);
                    Clients.Group(DepartmentAgentsGroupName + departments.First().DepartmentID).reciveMessage(objLiveChatMessageInfo);
                    Clients.Group(DepartmentAgentsGroupName + departments.First().DepartmentID).closeLiveChatByVisitor(objLiveChatInfo.LiveChatID);
                }
                else
                {
                    foreach (var agent in agents)
                    {
                        Clients.Group(AgentGroupName + agent.UserID).reciveMessage(objLiveChatMessageInfo);
                        Clients.Group(AgentGroupName + agent.UserID).closeLiveChatByVisitor(objLiveChatInfo.LiveChatID);
                    }
                }

                isClosed = true;
            }
            return isClosed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        public bool CloseLiveChatByAgent(int portalID, int livechatID)
        {
            var isClosed = false;

            var objLiveChatInfo = LiveChatManager.Instance.GetLiveChatByID(livechatID);
            if (objLiveChatInfo != null && !objLiveChatInfo.IsClosed)
            {
                LiveChatManager.Instance.CloseLiveChat(livechatID, true);

                var objLiveChatMessageInfo = new LiveChatMessageInfo()
                {
                    LiveChatID = objLiveChatInfo.LiveChatID,
                    CreateDate = DateTime.Now,
                    SentBy = MessageSentBy.System,
                    MessageType = MessageType.Text,
                    Message = "Agent closed the live chat",
                };
                int messageID = LiveChatMessageManager.Instance.AddMessage(objLiveChatMessageInfo);
                objLiveChatMessageInfo.MessageID = messageID;

                Clients.Group(portalID + "-" + objLiveChatInfo.VisitorGUID).closeLiveChatByAgent(objLiveChatInfo.LiveChatID);
                Clients.Group(portalID + "-" + objLiveChatInfo.VisitorGUID).reciveMessage(objLiveChatMessageInfo);

                //ersal payam be agent ha
                var agents = LiveChatAgentManager.Instance.GetLiveChatAgents(objLiveChatInfo.LiveChatID);
                foreach (var agent in agents)
                {
                    Clients.Group(AgentGroupName + agent.UserID, Context.ConnectionId).reciveMessage(objLiveChatMessageInfo);
                    Clients.Group(AgentGroupName + agent.UserID, Context.ConnectionId).closeLiveChatByAgent(objLiveChatInfo.LiveChatID);
                }

                isClosed = true;
            }
            return isClosed;
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatInfo"></param>
        /// <returns></returns>
        private LiveChatViewModel GetLiveChatViewModel(LiveChatInfo objLiveChatInfo)
        {
            var livechat = new LiveChatViewModel();
            livechat.LiveChatID = objLiveChatInfo.LiveChatID;
            livechat.PortalID = objLiveChatInfo.PortalID;

            var moduleID = int.Parse(PortalController.GetPortalSetting("MyDnnLiveChatModuleID", objLiveChatInfo.PortalID, "-1"));
            var Settings = new ModuleController().GetModule(moduleID).ModuleSettings;

            //Visitor
            livechat.Visitor = new LiveChatVisitorViewModel()
            {
                UserID = objLiveChatInfo.VisitorUserID,
                VisitorGUID = objLiveChatInfo.VisitorGUID,
                DisplayName = objLiveChatInfo.VisitorName,
                Email = objLiveChatInfo.VisitorEmail,
                IP = objLiveChatInfo.VisitorIP,
                UserAgent = objLiveChatInfo.VisitorUserAgent,
                Avatar = (Settings["VisitorDefaultAvatar"] != null ? Settings["VisitorDefaultAvatar"].ToString() : string.Empty)
            };
            var visitor = VisitorsOnlineApi.Instance.GetVisitorByGUID(objLiveChatInfo.PortalID, objLiveChatInfo.VisitorGUID);
            if (visitor != null)
            {
                if (visitor.UserID > 0) livechat.Visitor.Avatar = DotNetNuke.Common.Globals.ResolveUrl("~/dnnimagehandler.ashx?mode=profilepic&userid=" + visitor.UserID);
                livechat.Visitor.LastURL = visitor.LastURL;
                livechat.Visitor.ReferrerURL = visitor.ReferrerURL;
                livechat.Visitor.PastVisits = visitor.TotalConnections;
                livechat.Visitor.OnlineDate = visitor.OnlineDate;
            }

            //Departments
            livechat.Departments = LiveChatDepartmentManager.Instance.GetLiveChatDepartmentsViewModel(objLiveChatInfo.PortalID, objLiveChatInfo.LiveChatID);

            //Agents
            livechat.Agents = LiveChatAgentManager.Instance.GetLiveChatAgentsViewModel(objLiveChatInfo.PortalID, objLiveChatInfo.LiveChatID);
            livechat.AgentDefaultAvatar = (Settings["AgentDefaultAvatar"] != null ? Settings["AgentDefaultAvatar"].ToString() : string.Empty);

            //Messages
            var messages = LiveChatMessageManager.Instance.GetMessages(objLiveChatInfo.LiveChatID);
            livechat.Messages = messages;

            livechat.Rate = objLiveChatInfo.Rate;

            livechat.IsClosed = objLiveChatInfo.IsClosed;

            return livechat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatInfo"></param>
        /// <param name="objAgentView"></param>
        /// <param name="sendToVisitor"></param>
        /// <param name="sendToAgent"></param>
        private void AgentHasJoin(LiveChatInfo objLiveChatInfo, AgentView objAgentView, bool sendToVisitor, bool sendToAgent)
        {
            var agent = new LiveChatAgentViewModel()
            {
                AgentID = objAgentView.AgentID,
                UserID = objAgentView.UserID,
                DisplayName = objAgentView.DisplayName,
                Email = objAgentView.Email,
                Avatar = DotNetNuke.Common.Globals.ResolveUrl("~/dnnimagehandler.ashx?mode=profilepic&userid=" + objAgentView.UserID),
                IsOnline = VisitorsOnlineApi.Instance.IsVisitorOnline(objLiveChatInfo.PortalID, objAgentView.UserID),
                JoinDate = DateTime.Now
            };

            LiveChatAgentManager.Instance.AddLiveChatAgent(new LiveChatAgentInfo() { AgentID = agent.AgentID, UserID = agent.UserID, LiveChatID = objLiveChatInfo.LiveChatID, JoinDate = DateTime.Now });

            var objLiveChatMessageInfo = new LiveChatMessageInfo()
            {
                LiveChatID = objLiveChatInfo.LiveChatID,
                SentBy = MessageSentBy.System,
                CreateDate = DateTime.Now,
                MessageType = MessageType.Text,
                Message = objAgentView.DisplayName + " join chat"
            };
            int messageID = LiveChatMessageManager.Instance.AddMessage(objLiveChatMessageInfo);
            objLiveChatMessageInfo.MessageID = messageID;

            if (sendToVisitor)
            {
                Clients.Group(objLiveChatInfo.PortalID + "-" + objLiveChatInfo.VisitorGUID).agentHasJoin(objLiveChatInfo.LiveChatID, agent);
                Clients.Group(objLiveChatInfo.PortalID + "-" + objLiveChatInfo.VisitorGUID).reciveMessage(objLiveChatMessageInfo);
            }

            if (sendToAgent)
            {
                foreach (var item in LiveChatAgentManager.Instance.GetLiveChatAgents(objLiveChatInfo.LiveChatID))
                {
                    Clients.Group(AgentGroupName + item.UserID).agentHasJoin(objLiveChatInfo.LiveChatID, agent);
                    Clients.Group(AgentGroupName + item.UserID).reciveMessage(objLiveChatMessageInfo);
                }
            }
        }

        #endregion
    }
}