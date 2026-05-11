//using global::Tesseract;
using HRManagement.Models.Ocr;
using Tesseract;

namespace HRManagement.Services.Tesseract
{
    public class TesseractOcrService : IOcrService
    {
        private readonly IWebHostEnvironment _env;

        public TesseractOcrService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public OcrResult ExtractText(IFormFile file)
        {
            string tessPath = Path.Combine(_env.ContentRootPath, "tessdata");

            using var engine = new TesseractEngine(tessPath, "eng", EngineMode.Default);

            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using var img = Pix.LoadFromMemory(memoryStream.ToArray());
            using var page = engine.Process(img);

            return new OcrResult
            {
                Text = page.GetText(),
                Confidence = page.GetMeanConfidence()
            };
        }










        //public OcrResult ExtractText(string imagePath)
        //{
        //    string tessPath = Path.Combine(_env.ContentRootPath, "tessdata");

        //    using var engine = new TesseractEngine(tessPath, "eng", EngineMode.Default);
        //    using var img = Pix.LoadFromFile(imagePath);

        //    using var page = engine.Process(img);

        //    return new OcrResult
        //    {
        //        Text = page.GetText(),
        //        Confidence = page.GetMeanConfidence()
        //    };
        //}
    }
}
