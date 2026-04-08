using HRManagement.Enums;

namespace HRManagement.DTOs.TimesheetDTOs
{
    public class GetTimesheetsForEmployeeFilterDto
    {
        public TimesheetStatus? Status { get; set; }

        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? TimesheetId { get; set; }
    }
}
