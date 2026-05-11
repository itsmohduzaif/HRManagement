using HRManagement.Models.Ocr;

namespace HRManagement.Services.Tesseract
{
    public interface IOcrService
    {
        OcrResult ExtractText(IFormFile file);
    }
}
