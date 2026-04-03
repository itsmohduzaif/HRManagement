using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FakeItEasy;
using FluentAssertions;
using HRManagement.Data;
using HRManagement.Models;
using HRManagement.Services.Employees;
using HRManagement.Services.EmployeesExcel;
using Microsoft.EntityFrameworkCore;
using Xunit;


// This was whole made with chatgpt because less time available.
namespace HRManagement.Tests.Services
{
    public class EmployeeExcelTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task ExportEmployeesToExcel_ShouldGenerateExcelWithEmployeeData()
        {
            // Arrange
            var context = GetDbContext();

            // Seed 1 employee
            var emp = new Employee
            {
                EmployeeName = "John Doe",
                Status = "Active",
                EmploymentType = "Full-time",
                ContractBy = "Company",
                ContractEndDate = new DateOnly(2025, 1, 1),
                WorkLocation = "Dubai",
                Gender = "Male",
                Nationality = "Indian",
                DateOfBirth = new DateOnly(1990, 5, 20),
                MaritalStatus = "Single",
                EmiratesIdNumber = "123456",
                PassportNumber = "P123456",
                JobTitle = "Developer",
                Department = "IT",
                ManagerName = "Manager A",
                DateOfJoining = new DateOnly(2020, 10, 10),

                PersonalEmail = "john@gmail.com",
                WorkEmail = "john@company.com",
                PersonalPhone = "999999",
                WorkPhone = "888888",
                EmergencyContactName = "Father",
                EmergencyContactRelationship = "Parent",
                EmergencyContactNumber = "777777",
                CurrentAddress = "Current",
                PermanentAddress = "Permanent",
                CountryOfResidence = "UAE",
                PoBox = "1111",

                PassportExpiryDate = new DateOnly(2030, 1, 1),
                VisaExpiryDate = new DateOnly(2030, 2, 2),
                EmiratesIdExpiryDate = new DateOnly(2030, 3, 3),
                LabourCardExpiryDate = new DateOnly(2030, 4, 4),
                InsuranceExpiryDate = new DateOnly(2030, 5, 5)
            };

            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var fakeEmployeeService = A.Fake<IEmployeeService>();
            var service = new EmployeeExcel(context, fakeEmployeeService);

            // Act
            MemoryStream excelStream = await service.ExportEmployeesToExcel();
            excelStream.Position = 0;

            using var workbook = new XLWorkbook(excelStream);

            // Assert
            workbook.Worksheets.Count.Should().Be(3);

            var sheet1 = workbook.Worksheet("Employee Basic details");

            sheet1.Cell("A1").Value.ToString().Should().Be("Employee Name");
            sheet1.Cell("A2").Value.ToString().Should().Be("John Doe");
            sheet1.Cell("B2").Value.ToString().Should().Be("Active");
            sheet1.Cell("M2").Value.ToString().Should().Be("Developer");

            // Check date format for ContractEndDate
            var contractEndDateCell = sheet1.Cell("E2");
            contractEndDateCell.Style.NumberFormat.Format.Should().Be("dd-MMM-yy");

            var sheet2 = workbook.Worksheet("Contact & Address Details");
            sheet2.Cell("B2").Value.ToString().Should().Be("john@gmail.com");
            sheet2.Cell("C2").Value.ToString().Should().Be("john@company.com");

            var sheet3 = workbook.Worksheet("Visa & Legal Documents");
            sheet3.Cell("B2").GetDateTime().Should().Be(new DateTime(2030, 1, 1));
            sheet3.Cell("C2").GetDateTime().Should().Be(new DateTime(2030, 2, 2));
        }



        [Fact]
        public async Task ExportEmployeesToExcel_ShouldWork_WhenNoEmployeesExist()
        {
            // Arrange
            var context = GetDbContext();
            var fakeEmployeeService = A.Fake<IEmployeeService>();
            var service = new EmployeeExcel(context, fakeEmployeeService);

            // Act
            MemoryStream excelStream = await service.ExportEmployeesToExcel();
            excelStream.Position = 0;

            using var workbook = new XLWorkbook(excelStream);

            // Assert
            workbook.Worksheets.Count.Should().Be(3);

            var sheet1 = workbook.Worksheet("Employee Basic details");
            sheet1.RangeUsed().RowCount().Should().Be(1); // Only header

            var sheet2 = workbook.Worksheet("Contact & Address Details");
            sheet2.RangeUsed().RowCount().Should().Be(1);

            var sheet3 = workbook.Worksheet("Visa & Legal Documents");
            sheet3.RangeUsed().RowCount().Should().Be(1);
        }


        [Fact]
        public async Task ExportEmployeesToExcel_ShouldLeaveDateCellsEmpty_WhenDatesAreNull()
        {
            // Arrange
            var context = GetDbContext();

            var emp = new Employee
            {
                EmployeeName = "Null Dates",
                ContractEndDate = null,
                DateOfBirth = null,
                DateOfJoining = null,
                PassportExpiryDate = null,
                VisaExpiryDate = null,
                EmiratesIdExpiryDate = null,
                LabourCardExpiryDate = null,
                InsuranceExpiryDate = null
            };

            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var fakeEmployeeService = A.Fake<IEmployeeService>();
            var service = new EmployeeExcel(context, fakeEmployeeService);

            // Act
            var excelStream = await service.ExportEmployeesToExcel();
            excelStream.Position = 0;

            using var workbook = new XLWorkbook(excelStream);

            var sheet1 = workbook.Worksheet("Employee Basic details");

            sheet1.Cell("E2").Value.IsBlank.Should().BeTrue();
            sheet1.Cell("I2").Value.IsBlank.Should().BeTrue();
            sheet1.Cell("P2").Value.IsBlank.Should().BeTrue();

            var sheet3 = workbook.Worksheet("Visa & Legal Documents");

            sheet3.Cell("B2").Value.IsBlank.Should().BeTrue();
            sheet3.Cell("C2").Value.IsBlank.Should().BeTrue();
            sheet3.Cell("D2").Value.IsBlank.Should().BeTrue();
            sheet3.Cell("E2").Value.IsBlank.Should().BeTrue();
            sheet3.Cell("F2").Value.IsBlank.Should().BeTrue();
        }

        [Fact]
        public async Task ExportEmployeesToExcel_ShouldExportMultipleEmployeesCorrectly()
        {
            // Arrange
            var context = GetDbContext();

            context.Employees.AddRange(
                new Employee { EmployeeName = "A" },
                new Employee { EmployeeName = "B" },
                new Employee { EmployeeName = "C" }
            );

            await context.SaveChangesAsync();

            var fakeEmployeeService = A.Fake<IEmployeeService>();
            var service = new EmployeeExcel(context, fakeEmployeeService);

            // Act
            var excelStream = await service.ExportEmployeesToExcel();
            excelStream.Position = 0;

            using var workbook = new XLWorkbook(excelStream);

            var sheet1 = workbook.Worksheet("Employee Basic details");

            // Expect 4 rows → 1 header + 3 data
            sheet1.RangeUsed().RowCount().Should().Be(4);

            sheet1.Cell("A2").Value.ToString().Should().Be("A");
            sheet1.Cell("A3").Value.ToString().Should().Be("B");
            sheet1.Cell("A4").Value.ToString().Should().Be("C");
        }


        [Fact]
        public async Task ExportEmployeesToExcel_ShouldCreateTableCorrectlyInSheet1()
        {
            // Arrange
            var context = GetDbContext();
            context.Employees.Add(new Employee { EmployeeName = "Test User" });
            await context.SaveChangesAsync();

            var fakeEmployeeService = A.Fake<IEmployeeService>();
            var service = new EmployeeExcel(context, fakeEmployeeService);

            // Act
            var excelStream = await service.ExportEmployeesToExcel();
            excelStream.Position = 0;

            using var workbook = new XLWorkbook(excelStream);

            var sheet1 = workbook.Worksheet("Employee Basic details");

            var table = sheet1.Tables.First();

            table.RangeAddress.FirstAddress.ToString().Should().Be("A1");
            table.RangeAddress.LastAddress.ToString().Should().Be("P2");
        }


        [Fact]
        public async Task ExportEmployeesToExcel_ShouldApplyBoldStyleToHeader()
        {
            // Arrange
            var context = GetDbContext();
            context.Employees.Add(new Employee { EmployeeName = "John" });
            await context.SaveChangesAsync();

            var fakeEmployeeService = A.Fake<IEmployeeService>();
            var service = new EmployeeExcel(context, fakeEmployeeService);

            // Act
            var excelStream = await service.ExportEmployeesToExcel();
            excelStream.Position = 0;

            using var workbook = new XLWorkbook(excelStream);

            var sheet1 = workbook.Worksheet("Employee Basic details");

            sheet1.Row(1).Style.Font.Bold.Should().BeTrue();
        }



    }

}
