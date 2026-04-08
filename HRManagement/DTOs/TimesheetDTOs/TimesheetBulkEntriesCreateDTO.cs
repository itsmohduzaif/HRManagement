namespace HRManagement.DTOs.TimesheetDTOs
{
    public class TimesheetBulkEntriesCreateDTO
    {
        public List<TimesheetEntryCreateDTO> timesheetEntries { get; set; } = new List<TimesheetEntryCreateDTO>();
    }
}
