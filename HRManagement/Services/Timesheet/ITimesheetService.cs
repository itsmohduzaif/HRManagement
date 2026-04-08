using HRManagement.DTOs;
using HRManagement.DTOs.TimesheetDTOs;

namespace HRManagement.Services.Timesheet
{
    public interface ITimesheetService
    {
        Task<ApiResponse> CreateTimesheet(string usernameFromClaim, TimesheetCreateDTO dto);

        Task<ApiResponse> SubmitTimesheet(int timesheetId, string usernameFromClaim);

        Task<ApiResponse> GetMyTimesheets(string usernameFromClaim, GetTimesheetsForEmployeeFilterDto filters);






        // Methods for Managers
        

        Task<ApiResponse> GetAllTimesheetsForAdmin(GetTimesheetsForAdminFilterDto filters);
        
        Task<ApiResponse> GetAllTimesheetsForManager(GetTimesheetsForAdminFilterDto filters, string usernameFromClaim);
        Task<ApiResponse> ApproveTimesheetByManager(int timesheetId, string usernameFromClaim);
        Task<ApiResponse> RejectTimesheetByManager(int timesheetId, RejectTimesheetRequestDTO dto, string usernameFromClaim);

        
    }
}
