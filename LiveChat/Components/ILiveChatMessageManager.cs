using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using MyDnn.Modules.Support.LiveChat.Models;
using DotNetNuke.Common;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface ILiveChatMessageManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatMessageInfo"></param>
        /// <returns></returns>
        int AddMessage(LiveChatMessageInfo objLiveChatMessageInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        void SeenMessage(int messageID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="liveChatID"></param>
        void DeleteMessage(int liveChatID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatMessageID"></param>
        /// <returns></returns>
        LiveChatMessageInfo GetMessageByID(int livechatMessageID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatMessageInfo> GetMessages(int livechatID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <param name="messageID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatMessageInfo> GetUnreadMessages(int livechatID, int messageID);
    }
}