using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace Visma_Room2Meet.Helpers
{
    public static class LogHandler
    {
        private static room2meet_dbEntities db = new room2meet_dbEntities();

        public static Log HandleLog(LogType LogType, string Location, string Message, string InnerException = "", System.Collections.Specialized.NameValueCollection Params = null)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (Params != null)
            {
                foreach (string key in Params.Keys)
                {
                    param.Add(key, Json.Encode(Params.GetValues(key)));
                }
            }

            Log log = new Log()
            {
                DateTime = DateTime.UtcNow,
                LogTypeID = (int)LogType,
                Location = Location,
                Message = Message,
                InnerException = InnerException,
                Params = Json.Encode(param)
            };
            db.Logs.Add(log);
            db.SaveChanges();
            return log;
        }
    }

    public enum LogType
    {
        Low = 1,
        Alert = 2,
        Warning = 3,
        Critical = 4
    }
}