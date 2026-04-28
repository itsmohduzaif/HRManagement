using System.Text;

namespace HRManagement.Services.DocumentParse
{
    public class DocumentParser
    {
        public async Task<string> ExtractTextAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            using var stream = file.OpenReadStream();

            return extension switch
            {
                ".txt" => await ExtractFromTxt(stream),
                ".pdf" => ExtractFromPdf(stream),
                ".docx" => ExtractFromDocx(stream),
                _ => throw new NotSupportedException($"File type {extension} not supported")
            };
        }

        private async Task<string> ExtractFromTxt(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private string ExtractFromPdf(Stream stream)
        {
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(stream);

            var text = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }

        private string ExtractFromDocx(Stream stream)
        {
            using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart.Document.Body;

            return body?.InnerText ?? "";
        }

        

    }
}
