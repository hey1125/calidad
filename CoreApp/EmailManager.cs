using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp
{
    public class EmailManager
    {
        public string ApiKey { get; set; }
        public EmailManager(string apiKey)
        {
            ApiKey = apiKey;
        }

        public async void SendEmail(string nameParam, string toParam, string subjectParam, string bodyParam)
        {
            var client = new SendGridClient(ApiKey);
            var from = new EmailAddress("notifiacionesbilletico@euucn.com", "BilleTico");
            var to = new EmailAddress(toParam, nameParam);
            var msg = MailHelper.CreateSingleEmail(from, to, subjectParam, bodyParam, bodyParam);
            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to send email: {response.StatusCode} - {response.Body.ReadAsStringAsync().Result}");
            }
            else
                Console.WriteLine($"Email sent successfully to {toParam} with subject '{subjectParam}'.");
        }
    }
}
