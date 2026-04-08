using HRManagement.Enums;

namespace HRManagement.DTOs.TimesheetDTOs
{
    public class GetTimesheetsForAdminFilterDto
    {
        public TimesheetStatus? Status { get; set; }
            
        public int? Month { get; set; }
        public int? Year { get; set; }

        public int? EmployeeId { get; set; }
        public int? TimesheetId { get; set; }

    }
}
