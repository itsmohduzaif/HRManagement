using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using HRManagement.Data;
using HRManagement.DTOs;
using HRManagement.DTOs.TimesheetDTOs;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services.Timesheet
{
    public class TimesheetEntryService : ITimesheetEntryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TimesheetEntryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse> AddSingleEntry(int timesheetId, string usernameFromClaim, TimesheetEntryCreateDTO dto)
        {
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);

            if (timesheet.Status != Enums.TimesheetStatus.Draft)
                return new ApiResponse(false, "Cannot add entries to a timesheet that is not in Draft status", 400, null);

            if (timesheet.Month != dto.Date.Month || timesheet.Year != dto.Date.Year)
                return new ApiResponse(false, "Entry date must be within the same month and year as the timesheet", 400, null);


            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (employee == null)
                return new ApiResponse(false, "Employee not found", 404, null);


            // Check if the timesheet belongs to the employee trying to add an entry. This is important for security, otherwise any employee could add entries to any timesheet.
            if (timesheet.EmployeeId != employee.EmployeeId)
                return new ApiResponse(false, "Unauthorized to add entry to this timesheet", 403, null);


            var existingEntry = await _context.TimesheetEntries.FirstOrDefaultAsync(te => te.TimesheetId == timesheetId && te.Date == dto.Date);

            if (existingEntry != null)
                return new ApiResponse(false, "An entry for the specified date already exists in this timesheet", 400, null);

            var timesheetEntry = new HRManagement.Models.Timesheet.TimesheetEntry
            {
                TimesheetId = timesheetId,
                Date = dto.Date,
                WorkingHours = dto.WorkingHours,
                Comments = dto.Comments,
                Milestone = dto.Milestone,
                Hours = dto.Hours
            };

            timesheetEntry.CreatedAt = DateTime.UtcNow;
            timesheet.UpdatedAt = DateTime.UtcNow;

            await _context.TimesheetEntries.AddAsync(timesheetEntry);
            await _context.SaveChangesAsync();


            return new ApiResponse(true, "Timesheet entry added successfully", 201, dto);
        }






        //public async Task<ApiResponse> AddBulkEntries(int timesheetId, string usernameFromClaim, TimesheetBulkEntriesCreateDTO dto)
        //{
        //    var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

        //    if (timesheet == null)
        //        return new ApiResponse(false, "Timesheet not found", 404, null);

        //    if(timesheet.Status != Enums.TimesheetStatus.Draft)
        //        return new ApiResponse(false, "Cannot add entries to a timesheet that is not in Draft status", 400, null);

        //    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

        //    if (employee == null)
        //        return new ApiResponse(false, "Employee not found", 404, null);


        //    // Check if the timesheet belongs to the employee trying to add an entry. This is important for security, otherwise any employee could add entries to any timesheet.
        //    if (timesheet.EmployeeId != employee.EmployeeId)
        //        return new ApiResponse(false, "Unauthorized to add entry to this timesheet", 403, null);




        //    if (dto == null || dto.timesheetEntries == null || !dto.timesheetEntries.Any())
        //        return new ApiResponse(false, "Entries not found", 400, null);


        //    if (dto.timesheetEntries.Count > 31)
        //        return new ApiResponse(false, "Maximum 31 entries allowed in one request", 400, null);


        //    var duplicateDatesInRequest = dto.timesheetEntries
        //        .GroupBy(x => x.Date)
        //        .Where(g => g.Count() > 1)
        //        .Select(g => g.Key.ToString("yyyy-MM-dd"))
        //        .ToList();




        //    var timesheetEntry = new HRManagement.Models.Timesheet.TimesheetEntry
        //    {
        //        TimesheetId = timesheetId,
        //        Date = dto.Date,
        //        WorkingHours = dto.WorkingHours,
        //        Comments = dto.Comments,
        //        Milestone = dto.Milestone,
        //        Hours = dto.Hours
        //    };

        //    timesheetEntry.CreatedAt = DateTime.UtcNow;
        //    timesheet.UpdatedAt = DateTime.UtcNow;

        //    await _context.TimesheetEntries.AddAsync(timesheetEntry);
        //    await _context.SaveChangesAsync();


        //    return new ApiResponse(true, "Timesheet entry added successfully", 201, dto);
        //}


        public async Task<ApiResponse> GetEntries(int timesheetId, string usernameFromClaim)
        {
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (employee == null)
                return new ApiResponse(false, "Employee not found", 404, null);


            // Check if the timesheet belongs to the employee trying to add an entry. This is important for security, otherwise any employee could add entries to any timesheet.
            if (timesheet.EmployeeId != employee.EmployeeId)
                return new ApiResponse(false, "Unauthorized to add entry to this timesheet", 403, null);

            var entries = await _context.TimesheetEntries.Where(te => te.TimesheetId == timesheetId).ToListAsync();

            if (entries == null || !entries.Any())
                return new ApiResponse(false, "Timesheet entries not found", 404, null);

            return new ApiResponse(true, "Timesheet entries retrieved successfully", 200, entries);
        }

        public async Task<ApiResponse> GetEntryById(int timesheetId, int entryId, string usernameFromClaim)
        {
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (employee == null)
                return new ApiResponse(false, "Employee not found", 404, null);


            // Check if the timesheet belongs to the employee trying to add an entry. This is important for security, otherwise any employee could add entries to any timesheet.
            if (timesheet.EmployeeId != employee.EmployeeId)
                return new ApiResponse(false, "Unauthorized to add entry to this timesheet", 403, null);

            var entry = await _context.TimesheetEntries.Where(te => te.TimesheetId == timesheetId && te.TimesheetEntryId == entryId).ToListAsync();

            if (entry == null)
                return new ApiResponse(false, "Timesheet entries not found", 404, null);

            return new ApiResponse(true, "Timesheet entries retrieved successfully", 200, entry);
        }

        public async Task<ApiResponse> UpdateEntry(int timesheetId, int entryId, string usernameFromClaim, TimesheetEntryUpdateDTO dto)
        {
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);
            
            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);
            
            if (timesheet.Status != Enums.TimesheetStatus.Draft)
                return new ApiResponse(false, "Cannot update entries of a timesheet that is not in Draft status", 400, null);


            if (timesheet.Month != dto.Date.Month || timesheet.Year != dto.Date.Year)
                return new ApiResponse(false, "Entry date must be within the same month and year as the timesheet", 400, null);


            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);
            
            if (employee == null)
                return new ApiResponse(false, "Employee not found", 404, null);
            
            // Check if the timesheet belongs to the employee trying to add an entry. This is important for security, otherwise any employee could add entries to any timesheet.
            if (timesheet.EmployeeId != employee.EmployeeId)
                return new ApiResponse(false, "Unauthorized to update entry to this timesheet", 403, null);
            
            var entry = await _context.TimesheetEntries.FirstOrDefaultAsync(te => te.TimesheetId == timesheetId && te.TimesheetEntryId == entryId);
            if (entry == null)
                return new ApiResponse(false, "Timesheet entry not found", 404, null);


            // If the date is being updated, check if another entry with the new date already exists in the same timesheet to prevent duplicate entries for the same date.
            if (entry.Date != dto.Date)
            {
                var existingEntry = await _context.TimesheetEntries.FirstOrDefaultAsync(te => te.TimesheetId == timesheetId && te.Date == dto.Date);

                if (existingEntry != null)
                    return new ApiResponse(false, "An entry for the specified date already exists in this timesheet", 400, null);
            }



            // Update the entry with the new values from the DTO
            entry.Date = dto.Date;
            entry.WorkingHours = dto.WorkingHours;
            entry.Comments = dto.Comments;
            entry.Milestone = dto.Milestone;
            entry.Hours = dto.Hours;
            entry.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return new ApiResponse(true, "Timesheet entry updated successfully", 200, dto);
        }



        public async Task<ApiResponse> DeleteEntry(int timesheetId, int entryId, string usernameFromClaim)
        {
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);

            if (timesheet.Status != Enums.TimesheetStatus.Draft)
                return new ApiResponse(false, "Cannot update entries of a timesheet that is not in Draft status", 400, null);

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == usernameFromClaim);

            if (employee == null)
                return new ApiResponse(false, "Employee not found", 404, null);

            // Check if the timesheet belongs to the employee trying to add an entry. This is important for security, otherwise any employee could add entries to any timesheet.
            if (timesheet.EmployeeId != employee.EmployeeId)
                return new ApiResponse(false, "Unauthorized to update entry to this timesheet", 403, null);

            var entry = await _context.TimesheetEntries.FirstOrDefaultAsync(te => te.TimesheetId == timesheetId && te.TimesheetEntryId == entryId);
            if (entry == null)
                return new ApiResponse(false, "Timesheet entry not found", 404, null);

            _context.Remove(entry);
            await _context.SaveChangesAsync();

            return new ApiResponse(true, "Timesheet entry deleted successfully", 200, null);

        }


        public async Task<ApiResponse> GetEntriesForManager(int timesheetId)
        {
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);
    
            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);
    
            var entries = await _context.TimesheetEntries.Where(te => te.TimesheetId == timesheetId).ToListAsync();
    
            if (entries == null || !entries.Any())
                return new ApiResponse(false, "Timesheet entries not found", 404, null);
    
            return new ApiResponse(true, "Timesheet entries retrieved successfully", 200, entries);
        }

        public async Task<ApiResponse> GetEntryByIdForManager(int timesheetId, int entryId)
        {
            var timesheet = await _context.Timesheets.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);

            if (timesheet == null)
                return new ApiResponse(false, "Timesheet not found", 404, null);

            var entry = await _context.TimesheetEntries.FirstOrDefaultAsync(te => te.TimesheetId == timesheetId && te.TimesheetEntryId == entryId);

            if (entry == null)
                return new ApiResponse(false, "Timesheet entries not found", 404, null);

            return new ApiResponse(true, "Timesheet entries retrieved successfully", 200, entry);
        }

    }
}
