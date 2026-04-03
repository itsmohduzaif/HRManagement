using HRManagement.DTOs;
using HRManagement.DTOs.TimesheetDTOs;

namespace HRManagement.Services.Timesheet
{
    public interface ITimesheetService
    {
        Task<ApiResponse> CreateTimesheet(string usernameFromClaim, TimesheetCreateDTO dto);

        Task<ApiResponse> SubmitTimesheet(int timesheetId, string usernameFromClaim);

        Task<ApiResponse> GetMyTimesheets(string usernameFromClaim);

        Task<ApiResponse> GetTimesheetById(int timesheetId, string usernameFromClaim);





        // Methods for Managers
        Task<ApiResponse> ApproveTimesheetByManager(int timesheetId);
        Task<ApiResponse> RejectTimesheetByManager(int timesheetId, RejectTimesheetRequestDTO dto);

        Task<ApiResponse> GetAllTimesheetsForManager();
        Task<ApiResponse> GetTimesheetByIdForManager(int timesheetId);
        Task<ApiResponse> GetAllPendingTimesheetsForManager();
        Task<ApiResponse> GetAllApprovedTimesheetsForManager();

        //ApproveTimesheetByManager
        //Task<ApiResponse> GetPendingForManager(int managerId);



        //Task<ApiResponse> GetPendingForHR();
        //Task<ApiResponse> ApproveByHR(int timesheetId);
        //Task<ApiResponse> RejectByHR(int timesheetId, string reason);
    }
}
