using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio;

namespace Visma_Room2Meet.Helpers
{
    public static class SMS
    {
        public static bool SendSMS(string To, string Message)
        {
            string AccountSid = "AC2e0f71513af6402d4a8dae93fb4ebdd4";
            string AuthToken = "fb5d4dd0963f78ffec53f4b7ff2cd3c2";
            var twilio = new TwilioRestClient(AccountSid, AuthToken);

            var message = twilio.SendMessage("+447481341779", To, Message);
            if (message.Sid != null)
            {
                return true;
            }

            return false;
        }
    }
}