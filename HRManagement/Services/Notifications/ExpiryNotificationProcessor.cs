using HRManagement.Data;
using HRManagement.Models;
using HRManagement.Services.Emails;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services.Notifications
{
    public class ExpiryNotificationProcessor : IExpiryNotificationProcessor
    {
        public async Task ProcessAsync(AppDbContext context, IEmailService emailService, string adminEmail)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var thresholdDate = today.AddDays(30);

            var employees = await context.Employees
                .Where(e =>
                    (e.PassportExpiryDate != null && e.PassportExpiryDate.Value == thresholdDate) ||
                    (e.VisaExpiryDate != null && e.VisaExpiryDate.Value == thresholdDate) ||
                    (e.EmiratesIdExpiryDate != null && e.EmiratesIdExpiryDate.Value == thresholdDate) ||
                    (e.LabourCardExpiryDate != null && e.LabourCardExpiryDate.Value == thresholdDate) ||
                    (e.InsuranceExpiryDate != null && e.InsuranceExpiryDate.Value == thresholdDate)
                )
                .ToListAsync();

            if (!employees.Any())
                return;

            var adminSummary = string.Empty;

            foreach (var emp in employees)
            {
                string subject = "Document Expiry Notification";
                string body = BuildExpiryEmailBodyForEmployee(emp, thresholdDate);

                if (!string.IsNullOrEmpty(emp.WorkEmail))
                    emailService.SendEmail(emp.WorkEmail, subject, body);

                var messages = new List<string>();
                if (emp.PassportExpiryDate == thresholdDate)
                    messages.Add($"Passport (Expiry: {emp.PassportExpiryDate:dd-MMM-yyyy})");
                if (emp.VisaExpiryDate == thresholdDate)
                    messages.Add($"Visa (Expiry: {emp.VisaExpiryDate:dd-MMM-yyyy})");
                if (emp.EmiratesIdExpiryDate == thresholdDate)
                    messages.Add($"Emirates ID (Expiry: {emp.EmiratesIdExpiryDate:dd-MMM-yyyy})");
                if (emp.LabourCardExpiryDate == thresholdDate)
                    messages.Add($"Labour Card (Expiry: {emp.LabourCardExpiryDate:dd-MMM-yyyy})");
                if (emp.InsuranceExpiryDate == thresholdDate)
                    messages.Add($"Insurance (Expiry: {emp.InsuranceExpiryDate:dd-MMM-yyyy})");

                if (messages.Any())
                {
                    adminSummary += $"Employee Name: {emp.EmployeeName} (Employee ID: {emp.EmployeeId})\n" +
                                    $"{string.Join("\n", messages)}\n\n";
                }
            }

            if (!string.IsNullOrEmpty(adminEmail))
            {
                string adminBody =
                    $"Dear Admin,\n\n" +
                    $"The following employees have documents expiring in 30 days:\n\n" +
                    $"{adminSummary}" +
                    $"Please ensure they are notified accordingly.\n\nThanks,\nHR Team";

                emailService.SendEmail(adminEmail, "Document Expiry Notification", adminBody);
            }
        }

        private string BuildExpiryEmailBodyForEmployee(Employee emp, DateOnly thresholdDate)
        {
            var messages = new List<string>();
            if (emp.PassportExpiryDate == thresholdDate)
                messages.Add($"Passport (Expiry: {emp.PassportExpiryDate:dd-MMM-yyyy})");
            if (emp.VisaExpiryDate == thresholdDate)
                messages.Add($"Visa (Expiry: {emp.VisaExpiryDate:dd-MMM-yyyy})");
            if (emp.EmiratesIdExpiryDate == thresholdDate)
                messages.Add($"Emirates ID (Expiry: {emp.EmiratesIdExpiryDate:dd-MMM-yyyy})");
            if (emp.LabourCardExpiryDate == thresholdDate)
                messages.Add($"Labour Card (Expiry: {emp.LabourCardExpiryDate:dd-MMM-yyyy})");
            if (emp.InsuranceExpiryDate == thresholdDate)
                messages.Add($"Insurance (Expiry: {emp.InsuranceExpiryDate:dd-MMM-yyyy})");

            return
                $"Hi {emp.EmployeeName},\n\n" +
                "This is a reminder that the following document(s) will expire in 30 days:\n" +
                $"{string.Join("\n", messages)}\n\n" +
                "Please take the necessary actions to renew them on time.\n\nThanks,\nHR Team";
        }
    }
}
