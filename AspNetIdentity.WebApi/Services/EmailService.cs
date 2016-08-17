using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SendGrid;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Configuration;

namespace AspNetIdentity.WebApi.Services
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            await configSendGridasync(message);
        }

        private async Task configSendGridasync(IdentityMessage message)
        {
            var myMessage = new SendGridMessage();
            myMessage.AddTo(message.Destination);
            myMessage.From = new System.Net.Mail.MailAddress("99284419@qq.com", "99284419");
            myMessage.Subject = message.Subject;
            myMessage.Text = message.Body;
            myMessage.Html = message.Body;

            var credentials = new NetworkCredential(ConfigurationManager.AppSettings["emailService:Account"],
                        ConfigurationManager.AppSettings["emailService:Password"]);
            var transportweb = new Web(credentials);

            if(transportweb != null)
            {
                await transportweb.DeliverAsync(myMessage);
            }
            else
            {
                await Task.FromResult(0);
            }
        }
    }
}