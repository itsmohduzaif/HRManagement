using FakeItEasy;
using FluentAssertions;
using HRManagement.Data;
using HRManagement.Enums;
using HRManagement.Models;
using HRManagement.Models.Leaves;
using HRManagement.Services.BlobStorage;
using HRManagement.Services.Emails;
using HRManagement.Services.LeaveRequests;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagement.Tests.Services
{
    public class LeaveRequestHelperTests
    {

        private async Task<AppDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();



            // Clearing the Seeding that happens
            databaseContext.Employees.RemoveRange(databaseContext.Employees);
            await databaseContext.SaveChangesAsync();


            if (await databaseContext.Employees.CountAsync() <= 0)
            {
                databaseContext.Employees.Add(
                    new Employee
                    {
                        UserName = "user1",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = "System",
                        ModifiedBy = "System",
                        EmployeeRole = "Employee",
                        EmployeeName = "A. Gaud",
                        WorkEmail = "user1@datafirstservices.com"
                    }
                    );

                databaseContext.Users.Add(
                    new Entities.User
                    {
                        UserName = "user1",
                        Email = "user1@datafirstservices.com",
                        EmployeeName = "A. Gaud",
                        Id = Guid.NewGuid().ToString()
                    }
                );
                await databaseContext.SaveChangesAsync();
            }
            return databaseContext;
        }


        // --------------------------------------------------------------------------------------
        // TEST 1 — Leave type not found
        // --------------------------------------------------------------------------------------

        [Fact]
        public async Task CheckLeaveBalanceCoreAsync_ShouldReturn404_WhenLeaveTypeNotFound()
        {
            var context = await GetDatabaseContext();

            // No leave type added → FindAsync returns null
            var service = new LeaveRequestHelper(context);

            var resp = await service.CheckLeaveBalanceCoreAsync(
                employeeId: 1,
                leaveTypeId: 99, // Does not exist
                startDate: new DateOnly(2025, 10, 20),
                endDate: new DateOnly(2025, 10, 20),
                isStartDateHalfDay: false,
                isEndDateHalfDay: false
            );

            resp.IsSuccess.Should().BeFalse();
            resp.StatusCode.Should().Be(404);
            resp.Message.Should().Be("Leave type not found.");
        }

        // --------------------------------------------------------------------------------------
        // TEST 2 — Sufficient leave balance
        // --------------------------------------------------------------------------------------

        [Fact]
        public async Task CheckLeaveBalanceCoreAsync_ShouldReturnSuccess_WhenBalanceIsEnough()
        {
            var context = await GetDatabaseContext();

            var service = new LeaveRequestHelper(context);

            var resp = await service.CheckLeaveBalanceCoreAsync(
                employeeId: 1,
                leaveTypeId: 2,
                startDate: new DateOnly(2025, 10, 20),
                endDate: new DateOnly(2025, 10, 22), // 3 days
                isStartDateHalfDay: false,
                isEndDateHalfDay: false
            );

            resp.IsSuccess.Should().BeTrue();
            resp.StatusCode.Should().Be(200);
        }

        // --------------------------------------------------------------------------------------
        // TEST 3 — Insufficient leave balance
        // --------------------------------------------------------------------------------------

        [Fact]
        public async Task CheckLeaveBalanceCoreAsync_ShouldReturnFailure_WhenBalanceIsInsufficient()
        {
            var context = await GetDatabaseContext();

            context.LeaveTypes.Add(new LeaveType
            {
                LeaveTypeId = 999,
                LeaveTypeName = "xyz leave",
                DefaultAnnualAllocation = 1,
                LeaveTypeDescription = "xyz description"
            });
            await context.SaveChangesAsync();



            // Already used 2 days
            context.LeaveRequests.Add(new LeaveRequest
            {
                EmployeeId = 1,
                LeaveTypeId = 999,
                Status = LeaveRequestStatus.Approved,
                StartDate = new DateOnly(2025, 11, 14),
                EndDate = new DateOnly(2025, 1, 14),
                LeaveDaysUsed = 1
            });

            await context.SaveChangesAsync();

            var service = new LeaveRequestHelper(context);

            // New request needs 1 day → total = 3 > 2 → FAIL
            var resp = await service.CheckLeaveBalanceCoreAsync(
                employeeId: 1,
                leaveTypeId: 999,
                startDate: new DateOnly(2025, 10, 20),
                endDate: new DateOnly(2025, 10, 20),
                isStartDateHalfDay: false,
                isEndDateHalfDay: false
            );

            resp.IsSuccess.Should().BeFalse();
            resp.StatusCode.Should().Be(400);
            resp.Message.Should().Be("Insufficient leave balance for this request.");
        }

        // --------------------------------------------------------------------------------------
        // TEST 4 — Combined leave types (1, 3, 4 share same allocation)
        // --------------------------------------------------------------------------------------

        [Fact]
        public async Task CheckLeaveBalanceCoreAsync_ShouldSumCombinedLeaveTypes_ReturnErrorWhenBalanceNotThere()
        {
            var context = await GetDatabaseContext();

            // Leave type 1 allocation = 15 days


            // User already used:
            // Type 1 → 5 days
            // Type 3 → 4 days
            // Type 4 → 15 days
            // Total = 24 days
            context.LeaveRequests.AddRange(
                new LeaveRequest
                {
                    EmployeeId = 1,
                    LeaveTypeId = 1,
                    Status = LeaveRequestStatus.Approved,
                    StartDate = new DateOnly(2025, 1, 5),
                    EndDate = new DateOnly(2025, 1, 10),
                    LeaveDaysUsed = 5
                },
                new LeaveRequest
                {
                    EmployeeId = 1,
                    LeaveTypeId = 3,
                    Status = LeaveRequestStatus.Approved,
                    StartDate = new DateOnly(2025, 2, 5),
                    EndDate = new DateOnly(2025, 2, 9),
                    LeaveDaysUsed = 4
                },
                new LeaveRequest
                {
                    EmployeeId = 1,
                    LeaveTypeId = 4,
                    Status = LeaveRequestStatus.Approved,
                    StartDate = new DateOnly(2025, 3, 5),
                    EndDate = new DateOnly(2025, 3, 20),
                    LeaveDaysUsed = 15
                }
            );

            await context.SaveChangesAsync();

            var service = new LeaveRequestHelper(context);

            // Request = 3 days → total = 12 + 3 = 15 → EXACT MATCH → allowed
            var resp = await service.CheckLeaveBalanceCoreAsync(
                employeeId: 1,
                leaveTypeId: 1, // triggers combined calculation logic
                startDate: new DateOnly(2025, 10, 20),
                endDate: new DateOnly(2025, 10, 25),
                isStartDateHalfDay: false,
                isEndDateHalfDay: false
            );

            resp.IsSuccess.Should().BeFalse();
            resp.StatusCode.Should().Be(400);
            resp.Message.Should().Be("Insufficient leave balance for this request.");
        }











    }
}
