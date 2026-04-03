using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using HRManagement.Data;
using HRManagement.Models;
using HRManagement.Services.Emails;
using HRManagement.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HRManagement.Tests.Services
{
    
    public class ExpiryNotificationProcessorTests
    {
        private static AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
        private static DateOnly Threshold => Today.AddDays(30);

        [Fact]
        public async Task NoEmployees_NoEmailsSent()
        {
            // Arrange
            var db = CreateDb();
            var email = A.Fake<IEmailService>();
            var processor = new ExpiryNotificationProcessor();

            // Act
            await processor.ProcessAsync(db, email, "admin@example.com");

            // Assert
            A.CallTo(email).MustNotHaveHappened();
        }

        [Fact]
        public async Task EmployeeWithExpiringPassport_ShouldSendEmployeeAndAdminEmails()
        {
            // Arrange
            var db = CreateDb();
            var email = A.Fake<IEmailService>();
            var processor = new ExpiryNotificationProcessor();

            db.Employees.Add(new Employee
            {
                EmployeeId = 1,
                EmployeeName = "Test User",
                WorkEmail = "emp@example.com",
                PassportExpiryDate = Threshold
            });

            await db.SaveChangesAsync();

            // Act
            await processor.ProcessAsync(db, email, "admin@example.com");

            // Assert
            A.CallTo(() => email.SendEmail("emp@example.com",
                "Document Expiry Notification",
                A<string>.That.Contains("Passport")))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => email.SendEmail("admin@example.com",
                "Document Expiry Notification",
                A<string>.That.Contains("Test User")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task EmployeeWithMultipleExpiries_ShouldIncludeAllInEmail()
        {
            // Arrange
            var db = CreateDb();
            var email = A.Fake<IEmailService>();
            var processor = new ExpiryNotificationProcessor();

            db.Employees.Add(new Employee
            {
                EmployeeId = 2,
                EmployeeName = "Multi Doc User",
                WorkEmail = "emp2@example.com",
                PassportExpiryDate = Threshold,
                VisaExpiryDate = Threshold,
                InsuranceExpiryDate = Threshold
            });

            await db.SaveChangesAsync();

            // Act
            await processor.ProcessAsync(db, email, "admin@example.com");

            // Assert employee email contains 3 different lines
            A.CallTo(() => email.SendEmail("emp2@example.com",
                "Document Expiry Notification",
                A<string>.That.Matches(x =>
                    x.Contains("Passport") &&
                    x.Contains("Visa") &&
                    x.Contains("Insurance"))))
                .MustHaveHappenedOnceExactly();

            // Assert admin summary contains same entries
            A.CallTo(() => email.SendEmail("admin@example.com",
                "Document Expiry Notification",
                A<string>.That.Matches(x =>
                    x.Contains("Passport") &&
                    x.Contains("Visa") &&
                    x.Contains("Insurance"))))
                .MustHaveHappenedOnceExactly();
        }
    }

}
