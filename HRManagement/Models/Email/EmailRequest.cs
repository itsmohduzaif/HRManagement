namespace HRManagement.Models.Email
{
    public class EmailRequest
    {
        public List<string> To { get; set; } = new();
        // The above is equalent to: public List<string> To { get; set; } = new List<string>();
        /// <summary>
        ///  Without initialization like: public List<string> To { get; set; } will still work, but it will throw a NullReferenceException if you try to add items to the list without first initializing it.
        ///  By initializing it with new List<string>(), you ensure that the list is ready to use and won't cause errors when adding recipients.
        /// </summary>
        public List<string> Cc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; } = false;
        public List<IFormFile> Attachments { get; set; } = new();
    }

}
