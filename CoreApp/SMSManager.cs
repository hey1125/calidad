using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CoreApp
{
    public class SMSManager
    {
        string AccountSid;
        string AuthToken;
        string MessagingServiceSid;

        public SMSManager(string accountSid, string authToken, string messagingServiceSid)
        {
            AccountSid = accountSid;
            AuthToken = authToken;
            MessagingServiceSid = messagingServiceSid;
        }

        public void SendSMS(string phoneNumber, string messageString)
        {
            var accountSid = AccountSid;
            var authToken = AuthToken;
            TwilioClient.Init(accountSid, authToken);
            string phone =  "+506" + phoneNumber; 
            var messageOptions = new CreateMessageOptions(
              new PhoneNumber(phone));
            messageOptions.MessagingServiceSid = MessagingServiceSid;
            messageOptions.Body = messageString;
            var message = MessageResource.Create(messageOptions);
            Console.WriteLine(message.Body);
        }
    }
}
