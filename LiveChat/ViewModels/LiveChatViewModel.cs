using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChat.Components.Enums;

namespace MyDnn.Modules.Support.LiveChat.ViewModels
{
    public class LiveChatViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int LiveChatID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LiveChatVisitorViewModel Visitor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<LiveChatDepartmentViewModel> Departments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<LiveChatAgentViewModel> Agents { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AgentDefaultAvatar { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<LiveChatMessageInfo> Messages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LiveChatRating Rate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ChatStarted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}