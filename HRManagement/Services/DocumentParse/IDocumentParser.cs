namespace HRManagement.Services.DocumentParse
{
    public interface IDocumentParser
    {
        Task<string> ExtractTextAsync(IFormFile file);
    }
}
