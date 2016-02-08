// Copyright (c); MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Framework;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChat.ViewModels;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface ILiveChatDepartmentManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatDepartmentInfo"></param>
        /// <returns></returns>
        int AddLiveChatDepartment(LiveChatDepartmentInfo objLiveChatDepartmentInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatDepartmentID"></param>
        void DeleteLiveChatDepartment(int livechatDepartmentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatDepartmentID"></param>
        /// <returns></returns>
        LiveChatDepartmentInfo GetLiveChatDepartmentByID(int livechatDepartmentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatDepartmentInfo> GetLiveChatDepartments(int livechatID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatDepartmentViewModel> GetLiveChatDepartmentsViewModel(int portalID, int livechatID);
    }
}