using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyDnn.Modules.Support.LiveChat.ViewModels
{
    public class LiveChatVisitorViewModel
    {
        public int VisitorID { get; set; }
        public int UserID { get; set; }
        public string VisitorGUID { get; set; }
        public string Avatar { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string IP { get; set; }
        public string LastURL { get; set; }
        public string ReferrerURL { get; set; }
        public string UserAgent { get; set; }
        public int PastVisits { get; set; }
        public int PastChats { get; set; }
        public DateTime OnlineDate { get; set; }
    }
}