namespace HRManagement.DTOs.TimesheetDTOs
{
    public class TimesheetBulkEntriesCreateDTO
    {
        public List<TimesheetEntryCreateDTO> timesheetEntries { get; set; } = new List<TimesheetEntryCreateDTO>();
        //public List<TimesheetEntryRowDTO> E { get; set; } = new();
    }

    //public class TimesheetEntryRowDTO
    //{
    //    public int? TimesheetEntryId { get; set; } // null = new entry, value = update existing entry

    //    public DateTime Date { get; set; }

    //    public string WorkingHours { get; set; } = string.Empty;

    //    public string Comments { get; set; } = string.Empty;

    //    public string Milestone { get; set; } = string.Empty;

    //    public int? Hours { get; set; }
    //}

}
