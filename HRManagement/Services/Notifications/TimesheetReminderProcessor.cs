using HRManagement.Data;
using HRManagement.Models.Email;
using HRManagement.Services.Emails;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services.Notifications
{
    public class TimesheetReminderProcessor: ITimesheetReminderProcessor
    {
        public async Task ProcessAsync(
            AppDbContext context,
            IEmailService emailService)
        {
            var employees = await context.Employees
                .Where(e => !string.IsNullOrEmpty(e.WorkEmail))
                .ToListAsync();


            foreach (var employee in employees)
            {
                var request = new EmailRequest
                {
                    To = new List<string> { $"{employee.WorkEmail}" },
                    Cc = new List<string>(), // empty
                    Subject = "Timesheet Deadline Reminder",
                    Body = $"Hi {employee.EmployeeName},\n"+
                            "This is a reminder to submit your timesheet today, as today is the deadline.\n\n" +
                            "Please make sure your timesheet is completed and submitted on time.\n\n" +
                            "Thanks," +
                            "\nHR Team",
                    IsBodyHtml = false,
                    Attachments = new List<IFormFile>() // empty
                };


                try
                {
                    await emailService.SendEmailAsync(request);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send timesheet reminder to {employee.WorkEmail}: {ex.Message}");
                }


                
            }


            





            //const string subject = "Timesheet Submission Reminder";

            //foreach (var emp in employees)
            //{
            //    string body =
            //        $"Hi {emp.EmployeeName},\n\n" +
            //        "This is a reminder to submit your timesheet today, " +
            //        "as today is the deadline.\n\n" +
            //        "Please make sure your timesheet is completed and submitted on time.\n\n" +
            //        "Thanks,\nHR Team";

            //    emailService.SendEmail(emp.WorkEmail!, subject, body);
            //}
        }

    }
}
