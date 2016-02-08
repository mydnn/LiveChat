using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyDnn.Modules.Support.LiveChat.ViewModels
{
    public class LiveChatAgentViewModel
    {
        public int AgentID { get; set; }
        public int UserID { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public bool IsOnline { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LeftDate { get; set; }
    }
}