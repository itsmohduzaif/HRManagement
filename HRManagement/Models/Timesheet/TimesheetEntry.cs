namespace HRManagement.Models.Timesheet
{
    public class TimesheetEntry
    {
        public int TimesheetEntryId { get; set; }

        public int TimesheetId { get; set; }

        public DateOnly Date { get; set; }

        public string Day => Date.DayOfWeek.ToString(); // This will return the day of the week as a string, e.g., "Monday", "Tuesday", etc.

        public string WorkingHours { get; set; } = string.Empty;
        // This is string because it is going to store something like: '9:00 AM to 6:00 PM' 
        // Reason is because of companies requirements.
        //While no of hours worked is to be stored in public int Hours { get; set; }

        public string Comments { get; set; } = string.Empty;
        public string Milestone { get; set; } = string.Empty;

        public decimal? Hours { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime  UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
    