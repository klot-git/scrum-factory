
namespace ScrumFactory.Services {

    public interface IMailerService {

  

        void AttachProjectMembers(Project project);

        bool SendEmail(string to, string subject, string body);

        bool SendEmail(string[] toList, string subject, string body);

        bool SendEmail(Project project, string subject, string body, bool PONotification = false);

    }
}
