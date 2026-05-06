namespace HRManagement.Models.Email
{
    public class EmailRequest
    {
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; } = false;
        public List<IFormFile> Attachments { get; set; } = new();
    }

}
