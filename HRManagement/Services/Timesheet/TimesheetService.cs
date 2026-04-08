using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using HRManagement.Data;
using HRManagement.DTOs;
using HRManagement.DTOs.TimesheetDTOs;
using HRManagement.Entities;
using HRManagement.Enums;
using HRManagement.Models;
using HRManagement.Models.Timesheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
//namespace HRManagement.Models;


namespace HRManagement.Services.Timesheet
{
    public class TimesheetService : ITimesheetService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public TimesheetService(AppDbContext context, UserManager<User> userManager, IMapper mapper) {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ApiResponse> CreateTimesheet(string usernameFromClaim, TimesheetCreateDTO dto)
        {
            
            var employee = _context.Employees.FirstOrDefault(e => e.UserName == usernameFromClaim);


            var timesheet = new HRManagement.Models.Timesheet.Timesheet
            {
                EmployeeId = employee.EmployeeId,
                Month = dto.Month,
                Year = dto.Year,
                Status = HRManagement.Enums.TimesheetStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };


            await _context.Timesheets.AddAsync(timesheet);
            await _context.SaveChangesAsync();


            return new ApiResponse(true, "Timesheet Created", 201,  dto);
        } // end of CreateTimesheet method



        public async Task<ApiResponse> SubmitTimesheet(int timesheetId, string usernameFromClaim)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.UserName == usernameFromClaim);

            var timesheet = await _context.Timesheets
                .FirstOrDefaultAsync(t => t.TimesheetId == timesheetId && t.EmployeeId == employee.EmployeeId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found or access denied", 404, null);

            if (timesheet.Status != TimesheetStatus.Draft)
                return new ApiResponse(false, "Timesheet cannot be submitted as the status is not draft", 400, null);

            // optionally validate all entries exist, Hours, etc.
            var entries = await _context.TimesheetEntries
                .Where(e => e.TimesheetId == timesheetId)
                .ToListAsync();

            if (!entries.Any())
                return new ApiResponse(false, "At least one entry required", 400, null);

            timesheet.Status = TimesheetStatus.Submitted;
            timesheet.SubmittedDate = DateTime.UtcNow;
            timesheet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse(true, "Timesheet submitted for manager review", 200, timesheet);
        } // end of SubmitTimesheet method



        public async Task<ApiResponse> GetMyTimesheets(string usernameFromClaim)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (employee == null)
                return new ApiResponse(false, "User not found", 404, null);

            var timesheets = await _context.Timesheets.Where(t => t.EmployeeId == employee.EmployeeId).ToListAsync();
            
            if (!timesheets.Any())
                return new ApiResponse(false, "Timesheet not found", 404, null);


            return new ApiResponse(true, "My timesheets retrieved", 200, timesheets);
        }



        public async Task<ApiResponse> GetTimesheetById(int timesheetId, string usernameFromClaim)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (employee == null)
                return new ApiResponse(false, "User not found", 404, null);

            
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.EmployeeId == employee.EmployeeId && t.TimesheetId == timesheetId);


            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found or access denied", 404, null);

            return new ApiResponse(true, "My timesheets retrieved", 200, timesheet);
        }








        public async Task<ApiResponse> GetAllTimesheetsForAdmin(GetTimesheetsForAdminFilterDto filters)
        {
            var query = _context.Timesheets.AsQueryable();

            if(filters.Status.HasValue)
                query = query.Where(t => t.Status == filters.Status.Value);

            if(filters.Month.HasValue)
                query = query.Where(t => t.Month == filters.Month.Value);

            if(filters.Year.HasValue)
                query = query.Where(t => t.Year == filters.Year.Value);

            if(filters.EmployeeId.HasValue)
                query = query.Where(t => t.EmployeeId == filters.EmployeeId.Value);

            if(filters.TimesheetId.HasValue)
                query = query.Where(t => t.TimesheetId == filters.TimesheetId.Value);

            if (!await query.AnyAsync())
                return new ApiResponse(false, "Timesheet not found with current filters", 404, null);


            var timesheets = await query.ToListAsync();


            if (!timesheets.Any())
                return new ApiResponse(false, "Timesheet not found with current filters", 404, null);


            return new ApiResponse(true, "Timesheets retrieved", 200, timesheets);
        }





        public async Task<ApiResponse> GetAllTimesheetsForManager(string usernameFromClaim)
        {
            var manager = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (manager == null)
                return new ApiResponse(false, "Unauthorized", 401, null);

            var employeesUnderManager = await _context.Employees.Where(e => e.ManagerEmployeeId == manager.EmployeeId).Select(e => e.EmployeeId).ToListAsync();



            var timesheetsOfEmployeesUnderManager = await _context.Timesheets.Where(t => employeesUnderManager.Contains(t.EmployeeId)).ToListAsync();



            if (!timesheetsOfEmployeesUnderManager.Any())
                return new ApiResponse(false, "No Timesheets found", 404, null);


            return new ApiResponse(true, "My timesheets retrieved", 200, timesheetsOfEmployeesUnderManager);
        }













        public async Task<ApiResponse> GetAllTimesheetsForManager(GetTimesheetsForAdminFilterDto filters, string usernameFromClaim)
        {
            // Step 1: Get manager
            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (manager == null)
                return new ApiResponse(false, "Unauthorized", 401, null);

            // Step 2: Base query (ONLY employees under this manager)
            var query = _context.Timesheets.Where(t => _context.Employees.Any(e => e.EmployeeId == t.EmployeeId && e.ManagerEmployeeId == manager.EmployeeId));

            // Step 3: Apply filters (same as admin)
            if (filters.Status.HasValue)
                query = query.Where(t => t.Status == filters.Status.Value);

            if (filters.Month.HasValue)
                query = query.Where(t => t.Month == filters.Month.Value);

            if (filters.Year.HasValue)
                query = query.Where(t => t.Year == filters.Year.Value);

            if (filters.EmployeeId.HasValue)
                query = query.Where(t => t.EmployeeId == filters.EmployeeId.Value);

            if (filters.TimesheetId.HasValue)
                query = query.Where(t => t.TimesheetId == filters.TimesheetId.Value);

            // Step 4: Check existence
            if (!await query.AnyAsync())
                return new ApiResponse(false, "No timesheets found with current filters", 404, null);

            // Step 5: Fetch data
            var timesheets = await query.ToListAsync();

            return new ApiResponse(true, "Timesheets retrieved successfully", 200, timesheets);
        }






        public async Task<ApiResponse> ApproveTimesheetByManager(int timesheetId, string usernameFromClaim)
        {
            var manager = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (manager == null)
                return new ApiResponse(false, "Unauthorized", 401, null);

            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);


            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == timesheet.EmployeeId);
            if (employee == null)
                return new ApiResponse(false, "Employee of the timesheet id not found", 404, null);

            if (employee.ManagerEmployeeId != manager.EmployeeId)
                return new ApiResponse(false, "Access denied", 401, null);


            if (timesheet.Status != TimesheetStatus.Submitted)
                return new ApiResponse(false, "Timesheet status is not correct", 404, null);


            timesheet.Status = TimesheetStatus.ManagerApproved;
            timesheet.ManagerApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();


            return new ApiResponse(true, "Timesheet approved by manager", 200, timesheet);
        }


        public async Task<ApiResponse> RejectTimesheetByManager(int timesheetId, RejectTimesheetRequestDTO dto, string usernameFromClaim)
        {
            var manager = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (manager == null)
                return new ApiResponse(false, "Unauthorized", 401, null);

            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);


            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == timesheet.EmployeeId);
            if (employee == null)
                return new ApiResponse(false, "Employee of the timesheet id not found", 404, null);

            if (employee.ManagerEmployeeId != manager.EmployeeId)
                return new ApiResponse(false, "Access denied", 401, null);



            if (timesheet.Status != TimesheetStatus.Submitted)
                return new ApiResponse(false, "Timesheet status is not correct", 404, null);


            timesheet.Status = TimesheetStatus.ManagerRejected;
            timesheet.ManagerRejectionReason = dto.ManagerRejectionReason;

            await _context.SaveChangesAsync();


            return new ApiResponse(true, "Timesheet rejected by manager", 200, timesheet);

        }



    }
}
