using HRManagement.DTOs;
using HRManagement.DTOs.TimesheetDTOs;

namespace HRManagement.Services.Timesheet
{
    public interface ITimesheetEntryService
    {
        Task<ApiResponse> AddSingleEntry(int timesheetId, string usernameFromClaim, TimesheetEntryCreateDTO dto);
        //Task<ApiResponse> AddBulkEntries(int timesheetId, string usernameFromClaim, List<TimesheetBulkEntriesCreateDTO> dto);
        Task<ApiResponse> GetEntries(int timesheetId, string usernameFromClaim);
        Task<ApiResponse> GetEntryById(int timesheetId, int entryId, string usernameFromClaim);
        Task<ApiResponse> UpdateEntry(int timesheetId, int entryId, string usernameFromClaim, TimesheetEntryUpdateDTO dto);
        Task<ApiResponse> DeleteEntry(int timesheetId, int entryId, string usernameFromClaim);


        // For Managers
        Task<ApiResponse> GetEntriesForManager(int timesheetId);
        Task<ApiResponse> GetEntryByIdForManager(int timesheetId, int entryId);
            
    }
}
