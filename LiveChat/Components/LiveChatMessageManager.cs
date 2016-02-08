using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using MyDnn.Modules.Support.LiveChat.Models;
using DotNetNuke.Common;
using DotNetNuke.Framework;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class LiveChatMessageManager : ServiceLocator<ILiveChatMessageManager, LiveChatMessageManager>, ILiveChatMessageManager
    {
        protected override Func<ILiveChatMessageManager> GetFactory()
        {
            return () => new LiveChatMessageManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "LiveChatMessages_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatMessageInfo"></param>
        /// <returns></returns>
        public int AddMessage(LiveChatMessageInfo objLiveChatMessageInfo)
        {
            Requires.NotNull(objLiveChatMessageInfo);
            Requires.PropertyNotNegative(objLiveChatMessageInfo, "LiveChatID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatMessageInfo>();
                rep.Insert(objLiveChatMessageInfo);

                DataCache.ClearCache(CachePrefix);

                return objLiveChatMessageInfo.MessageID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        public void SeenMessage(int messageID)
        {
            Requires.NotNegative("messageID", messageID);

            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatSeenMessage", messageID);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="liveChatID"></param>
        public void DeleteMessage(int liveChatID)
        {
            Requires.NotNegative("liveChatID", liveChatID);

            LiveChatMessageInfo objLiveChatMessageInfo = GetMessageByID(liveChatID);
            Requires.NotNull(objLiveChatMessageInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatMessageInfo>();
                rep.Delete(objLiveChatMessageInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatMessageID"></param>
        /// <returns></returns>
        public LiveChatMessageInfo GetMessageByID(int livechatMessageID)
        {
            Requires.NotNegative("livechatMessageID", livechatMessageID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatMessageInfo>();
                return rep.GetById(livechatMessageID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatMessageInfo> GetMessages(int livechatID)
        {
            Requires.NotNegative("livechatMessageID", livechatID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatMessageInfo>();
                return rep.Get(livechatID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <param name="messageID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatMessageInfo> GetUnreadMessages(int livechatID, int messageID)
        {
            Requires.NotNegative("livechatID", livechatID);
            Requires.NotNegative("messageID", messageID);

            var messages = GetMessages(livechatID);
            return (messages != null ? messages.Where(m => m.MessageID > messageID) : null);
        }
    }
}