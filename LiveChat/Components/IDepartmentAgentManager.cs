// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDnn.Modules.Support.LiveChats.ViewModels;
using MyDnn.Modules.Support.LiveChat.Models;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface IDepartmentAgentManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentAgentInfo"></param>
        void AddDepartmentAgent(DepartmentAgentInfo objDepartmentAgentInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentAgentInfo"></param>
        void UpdateDepartmentAgent(DepartmentAgentInfo objDepartmentAgentInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DepartmentAgentID"></param>
        void DeleteDepartmentAgent(int departmentID, int departmentAgentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agentID"></param>
        void DeleteAgentDepartments(int agentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        bool IsAgentOnlineByDepartment(int portalID, int departmentID);
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DepartmentAgentID"></param>
        /// <returns></returns>
        DepartmentAgentInfo GetDepartmentAgent(int departmentID, int departmentAgentID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        IEnumerable<DepartmentAgentInfo> GetDepartmentAgents(int departmentID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        IEnumerable<DepartmentAgentViewModel> GetDepartmentAgentsViewModel(int departmentID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        IEnumerable<DepartmentAgentInfo> GetAgentDepartments(int agentID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        IEnumerable<string> GetNamesOfDepartments(int portalID, int agentID);
    }
}
