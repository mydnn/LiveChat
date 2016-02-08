// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using DotNetNuke.Entities.Portals;
using System.Threading;
using MyDnn.VisitorsOnline.Components;
using MyDnn.VisitorsOnline.Models;
using DotNetNuke.Entities.Users;
using MyDnn.VisitorsOnline.Components.Enums;

namespace MyDnn.VisitorsOnline.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    [HubName("MyDnnVisitorsOnlineHub")]
    public class VisitorsOnlineHub : Hub
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
        private static Timer PurgeVisitorOnlineTimer { get; set; }

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
            VisitorOnlineManager.Instance.DeleteConnection(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        #endregion

        #region Hub Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="referrerDomain"></param>
        public void JoinVisitor(int portalID, string visitorGUID, string name, string email, string referrerDomain)
        {
            if (PurgeVisitorOnlineTimer == null)
                PurgeVisitorOnlineTimer = new Timer(new TimerCallback(this.PurgeVisitorOnline), portalID, 59000, 59000);

            bool isUpdated = false;
            bool checkRoles = true;

            var visitor = VisitorOnlineManager.Instance.GetVisitorByGUID(portalID, visitorGUID);
            string lastUrl = this.getURL();
            string userName = this.UserName;

            if (visitor == null)
            {
                visitor = new OnlineVisitorInfo()
                {
                    VisitorGUID = visitorGUID,
                    PortalID = portalID,
                    UserName = userName,
                    DisplayName = name,
                    Email = email,
                    OnlineDate = DateTime.Now,
                    IP = getIpAddress(),
                    UserAgent = getVisitorAgent(),
                    LastURL = lastUrl,
                    ReferrerURL = referrerDomain,
                };
                VisitorOnlineManager.Instance.AddVisitorOnline(visitor, Context.ConnectionId);
            }
            else
            {
                VisitorOnlineManager.Instance.UpdateVisitorOnline(portalID, visitorGUID, userName, visitor.DisplayName, visitor.Email, lastUrl, Context.ConnectionId, false);
                isUpdated = true;
            }

            if (isUpdated)
            {
                if (!string.IsNullOrEmpty(visitor.UserName) && string.IsNullOrEmpty(userName)) // in yani user login bode va logoff shode
                    OfflineRoles(portalID);
                else if (!string.IsNullOrEmpty(visitor.UserName) && !string.IsNullOrEmpty(userName)) // in yani user az ghabl login bode va niazi be check kardane role haye an nist
                    checkRoles = false;

                //in ghesmat ro baraye in neveshtam ke agar visitor faghat urlesh taghir karde kole visitor ro dobare fetch nakone va faghat urlesh taghir kone
                if (visitor.UserName == userName) // in yani user login bode va logoff shode
                    visitor.LastURL = lastUrl;
                else
                    visitor = VisitorOnlineManager.Instance.GetVisitorByGUID(portalID, visitorGUID);

                Clients.Group("MyDnnVisitorsOnline").updateVisitorInfo(visitor);
            }
            else
            {
                if (!string.IsNullOrEmpty(visitor.UserName)) // agar user login shod visitor dobare bayad get shavad ta displayname va emailash ham biayad
                    visitor = VisitorOnlineManager.Instance.GetVisitorByGUID(portalID, visitorGUID);

                Clients.Group("MyDnnVisitorsOnline").populateVisitorsOnline(visitor);
            }

            if (!string.IsNullOrEmpty(visitor.UserName) && checkRoles) //zamani ke yek karbar login mikonad bayad naghshhaye karbar baresi shavad va chenanche naghshe karbar joze listener ha bod script marbote ejra shavad
            {
                var user = UserController.GetUserByName(visitor.UserName);

                var rolesOnline = RoleOnlineManager.Instance.GetRolesOnline(portalID);

                IEnumerable<string> roles;
                if (user.IsSuperUser)
                    roles = RoleOnlineManager.Instance.GetSuperUserRoles(portalID, user.UserID);
                else
                    roles = user.Roles;

                if (!rolesOnline.Where(r => roles.Contains(r.RoleName)).Any())
                {
                    foreach (var role in roles)
                    {
                        RoleOnlineManager.Instance.AddRoleOnline(new OnlineRoleInfo()
                        {
                            PortalID = portalID,
                            RoleName = role,
                            OnlineDate = DateTime.Now
                        });
                    }
                }

                var listeners = ListenerManager.Instance.GetListeners(portalID).Where(l => l.LoginState == LoginState.LoggedIn);

                foreach (var item in listeners.Join(roles, l => l.RoleName, r => r, (l, r) => l))
                {
                    Clients.All.invokeScript(item.InvokeScript);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task JoinGroup(string groupName)
        {
            await Groups.Add(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="referrerDomain"></param>
        public void Ping(int portalID, string visitorGUID, string name, string email, string referrerDomain)
        {
            var visitor = VisitorOnlineManager.Instance.GetVisitorByGUID(portalID, visitorGUID);
            if (visitor != null)
                VisitorOnlineManager.Instance.Ping(visitorGUID);
            else
                JoinVisitor(portalID, visitorGUID, name, email, referrerDomain);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void PurgeVisitorOnline(object state)
        {
            int portalID = (int)state;

            var hasOfflineVisitors = VisitorOnlineManager.Instance.PurgeVisitorsOnline(portalID);
            if (hasOfflineVisitors.Any())
            {
                Clients.Group("MyDnnVisitorsOnline").populateVisitorsOffline(hasOfflineVisitors.Select(o => o.VisitorGUID).ToArray());
            }

            if (hasOfflineVisitors.Where(v => !string.IsNullOrEmpty(v.UserName)).Any())
                OfflineRoles(portalID);

            var visitors = VisitorOnlineManager.Instance.GetVisitorsOnline(portalID);

            //check kardan baraye inke agar karbari digar online nist timer motovaghef shavad
            if ((visitors == null || !visitors.Any()) && PurgeVisitorOnlineTimer != null)
            {
                PurgeVisitorOnlineTimer.Dispose();
                PurgeVisitorOnlineTimer = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        private void OfflineRoles(int portalID)
        {
            var visitors = VisitorOnlineManager.Instance.GetVisitorsOnline(portalID);

            var rolesOnline = RoleOnlineManager.Instance.GetRolesOnline(portalID);
            if (rolesOnline.Any())
            {
                //get online roles
                var visitorsRoles = new List<string>();
                foreach (OnlineVisitorInfo onlineVisitor in visitors.Where(v => !string.IsNullOrEmpty(v.UserName)))
                {
                    var user = UserController.GetUserByName(onlineVisitor.UserName);
                    if (user != null)
                    {
                        IEnumerable<string> roles;
                        if (user.IsSuperUser)
                            roles = RoleOnlineManager.Instance.GetSuperUserRoles(portalID, user.UserID);
                        else
                            roles = user.Roles;

                        visitorsRoles.AddRange(roles);
                    }
                }

                var listeners = ListenerManager.Instance.GetListeners(portalID).Where(l => l.LoginState == LoginState.LoggedOff);

                foreach (OnlineRoleInfo item in rolesOnline.Where(r => !visitorsRoles.Contains(r.RoleName)))
                {
                    RoleOnlineManager.Instance.DeleteRoleOnline(item);

                    var listener = listeners.SingleOrDefault(l => l.RoleName == item.RoleName);
                    if (listener != null)
                        Clients.All.invokeScript(listener.InvokeScript);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string getVisitorAgent()
        {
            return Context.Request.Headers["User-Agent"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string getURL()
        {
            return Context.Request.Headers["Referer"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string getIpAddress()
        {
            string ipAddress;
            object tempObject;

            Context.Request.Environment.TryGetValue("server.RemoteIpAddress", out tempObject);

            if (tempObject != null)
            {
                ipAddress = (string)tempObject;
            }
            else
            {
                ipAddress = string.Empty;
            }

            return ipAddress;
        }

        #endregion
    }
}