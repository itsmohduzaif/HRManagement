using HRManagement.DTOs;
using HRManagement.DTOs.TimesheetDTOs;
using HRManagement.Services.Timesheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimesheetController : ControllerBase
    {
        private readonly ITimesheetService _timesheetService;

        public TimesheetController(ITimesheetService timesheetService)
        {
            _timesheetService = timesheetService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTimesheet(TimesheetCreateDTO dto)
        {
            // Get current logged-in user's username from JWT claims
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetService.CreateTimesheet(usernameFromClaim, dto);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpPut("{timesheetId:int}/submit")]
        public async Task<IActionResult> SubmitTimesheet(int timesheetId)
        {
            // Get current logged-in user's username from JWT claims
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetService.SubmitTimesheet(timesheetId, usernameFromClaim);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyTimesheets()
        {
            // Get current logged-in user's username from JWT claims
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetService.GetMyTimesheets(usernameFromClaim);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet("{timesheetId:int}")]
        public async Task<IActionResult> GetTimesheetById(int timesheetId)
        {
            // Get current logged-in user's username from JWT claims
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetService.GetTimesheetById(timesheetId, usernameFromClaim);
            return StatusCode(response.StatusCode, response);
        }





        // Endpoints for managers

        [Authorize(Roles = "Manager")]
        [HttpPut("{timesheetId:int}/approve")]
        public async Task<IActionResult> ApproveTimesheetByManager(int timesheetId)
        {
            var response = await _timesheetService.ApproveTimesheetByManager(timesheetId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("{timesheetId:int}/reject")]
        public async Task<IActionResult> RejectTimesheetByManager(int timesheetId, [FromBody] RejectTimesheetRequestDTO dto)
        {
            var response = await _timesheetService.RejectTimesheetByManager(timesheetId, dto);
            return StatusCode(response.StatusCode, response);
        }





        [Authorize(Roles = "Manager")]
        [HttpGet("manager")]
        public async Task<IActionResult> GetAllTimesheetsForManager()
        {
            var response = await _timesheetService.GetAllTimesheetsForManager();
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("manager/{timesheetId:int}")]
        public async Task<IActionResult> GetTimesheetByIdForManager(int timesheetId)
        {
            var response = await _timesheetService.GetTimesheetByIdForManager(timesheetId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("manager/pending")]
        public async Task<IActionResult> GetAllPendingTimesheetsForManager()
        {
            var response = await _timesheetService.GetAllPendingTimesheetsForManager();
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Manager, Hr")]
        [HttpGet("manager/approved")]
        public async Task<IActionResult> GetAllApprovedTimesheetsHR()
        {
            var response = await _timesheetService.GetAllApprovedTimesheetsForManager();
            return StatusCode(response.StatusCode, response);
        }


    }
}

    