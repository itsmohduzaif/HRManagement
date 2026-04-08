using HRManagement.DTOs;
using HRManagement.DTOs.TimesheetDTOs;
using HRManagement.Services.Timesheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagement.Controllers
{
    [Route("api/timesheet/{timesheetId:int}/entries")]
    [ApiController]
    public class TimesheetEntryController : ControllerBase
    {
        private readonly ITimesheetEntryService _timesheetEntryService;

        public TimesheetEntryController(ITimesheetEntryService timesheetEntryService)
        {
            _timesheetEntryService = timesheetEntryService;
        }


        [HttpPost]
        public async Task<IActionResult> AddSingleEntry(int timesheetId, [FromBody] TimesheetEntryCreateDTO dto)
        {
            // Get current logged-in user's username from JWT claims
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetEntryService.AddSingleEntry(timesheetId, usernameFromClaim, dto);
            return StatusCode(response.StatusCode, response);
        }




        [HttpGet]
        public async Task<IActionResult> GetEntries(int timesheetId)
        {
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetEntryService.GetEntries(timesheetId, usernameFromClaim);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{entryId:int}")]
        public async Task<IActionResult> GetEntryById(int timesheetId, int entryId)
        {
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetEntryService.GetEntryById(timesheetId, entryId, usernameFromClaim);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{entryId:int}")]
        public async Task<IActionResult> UpdateEntry(int timesheetId, int entryId, [FromBody] TimesheetEntryUpdateDTO dto)
        {
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetEntryService.UpdateEntry(timesheetId, entryId, usernameFromClaim, dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{entryId:int}")]
        public async Task<IActionResult> DeleteEntry(int timesheetId, int entryId)
        {
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetEntryService.DeleteEntry(timesheetId, entryId, usernameFromClaim);
            return StatusCode(response.StatusCode, response);
        }






        // For Managers
        [Authorize(Roles = "Manager")]
        [HttpGet("manager")]
        public async Task<IActionResult> GetEntriesForManager(int timesheetId)
        {
            string usernameFromClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(usernameFromClaim))
                return Unauthorized(new ApiResponse(false, "User identity not found", 401, null));

            var response = await _timesheetEntryService.GetEntriesForManager(timesheetId, usernameFromClaim);
            return StatusCode(response.StatusCode, response);
        }

        






        // For Managers
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetEntriesForAdmin(int timesheetId)
        {
            var response = await _timesheetEntryService.GetEntriesForAdmin(timesheetId);
            return StatusCode(response.StatusCode, response);
        }

        





    }
}
