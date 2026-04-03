using HRManagement.Data;
using HRManagement.Services.Emails;

namespace HRManagement.Services.Notifications
{
    public interface IExpiryNotificationProcessor
    {
        Task ProcessAsync(AppDbContext context, IEmailService emailService, string adminEmail);
    }

}
