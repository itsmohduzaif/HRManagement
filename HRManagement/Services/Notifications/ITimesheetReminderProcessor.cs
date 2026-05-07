using HRManagement.Data;
using HRManagement.Services.Emails;

namespace HRManagement.Services.Notifications
{
    public interface ITimesheetReminderProcessor
    {
        Task ProcessAsync(AppDbContext context, IEmailService emailService);
    }

}
