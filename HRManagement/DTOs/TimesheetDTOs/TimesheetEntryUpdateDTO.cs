namespace HRManagement.DTOs.TimesheetDTOs
{
    public class TimesheetEntryUpdateDTO
    {
        public DateOnly Date { get; set; }
        public string WorkingHours { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string Milestone { get; set; } = string.Empty;
        public decimal? Hours { get; set; }
    }
}
