using HRManagement.Enums;


namespace HRManagement.Models.Timesheet
{
    public class Timesheet
    {
        public int TimesheetId { get; set; }

        public int EmployeeId { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;

        public DateTime? SubmittedDate { get; set; }
        public DateTime? ManagerApprovedDate { get; set; }
        public string? ManagerRejectionReason { get; set; } = string.Empty;

        // Commenting for now since there is not clear requirement guideline on whether HR approval is needed or not. If needed, we can easily add these fields back in the future.
        // public DateTime? HrApprovedDate { get; set; }
        // public string? HrRejectionReason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
    