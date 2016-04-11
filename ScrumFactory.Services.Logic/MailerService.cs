using System;
using System.ComponentModel.Composition;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;

namespace ScrumFactory.Services.Logic {

    [Export(typeof(IMailerService))]
    public class MailerService : IMailerService {

        [Import]
        private ITeamService_ServerSide teamService { get; set; }

        private string ScrumFactorySenderEmail {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["ScrumFactorySenderEmail"];
            }
        }

        private string ScrumFactorySenderName {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["ScrumFactorySenderName"];
            }
        }

       
        private bool SmtpEnableSsl {
            get {
                bool setting = false;
                string settingStr = System.Configuration.ConfigurationManager.AppSettings["SmtpEnableSsl"];
                if (String.IsNullOrEmpty(settingStr))
                    return setting;
                bool.TryParse(settingStr, out setting);

                return setting;
            }
        }

        

        public void AttachProjectMembers(Project project) {
            ICollection<MemberProfile> members = teamService.GetProjectMembers_skipAuth(project.ProjectUId);
            foreach (ProjectMembership ms in project.Memberships.Where(ms => ms.IsActive))
                ms.Member = members.SingleOrDefault(m => m.MemberUId == ms.MemberUId);
        }

        public bool SendEmail(string to, string subject, string body) {
            return SendEmail(new string[] { to }, subject, body);
        }

        public bool SendEmail(string[] toList, string subject, string body) {

            SmtpClient client = new SmtpClient();
            client.EnableSsl = SmtpEnableSsl;
            MailMessage msg = new MailMessage();

            try {
                msg.From = new MailAddress(ScrumFactorySenderEmail, ScrumFactorySenderName);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;

                foreach (string to in toList)
                    msg.To.Add(to);

                client.SendAsync(msg, "ok");
            }
            catch (Exception) {
                return false;
            }

            return true;


        }

        public bool SendEmail(Project project, string subject, string body) {
            List<string> teamEmails = new List<string>();
            
            foreach (ProjectMembership ms in project.Memberships.Where(ms => ms.IsActive)) {
                if (ms.Member != null && ms.Member.IsContactMember == false && !teamEmails.Contains(ms.Member.EmailAccount) && !String.IsNullOrEmpty(ms.Member.EmailAccount))
                    teamEmails.Add(ms.Member.EmailAccount);
                
            }

            if (teamEmails.Count == 0)
                return true;

            return SendEmail(teamEmails.ToArray(), subject, body);

        }

       
    }

}
