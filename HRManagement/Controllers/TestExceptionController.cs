
// Will Delete this Controller

using HRManagement.Exceptions;
using HRManagement.Helpers;
using HRManagement.Services.EmployeesExcel;
using HRManagement.Services.Tesseract;
using Microsoft.AspNetCore.Mvc;
using Tesseract;

[ApiController]
[Route("api/[controller]")]
public class TestExceptionController : ControllerBase
{
    private readonly ILogger<TestExceptionController> _logger;
    //private readonly EmployeeExcelExporter _employeeExcelExporter;
    //private readonly EmployeeExcelImporter _employeeExcelImporter;
    private readonly IEmployeeExcel _employeeExcel;
    private readonly IOcrService _ocrService;


    //Microsoft Identity Client

    //public TestExceptionController(ILogger<TestExceptionController> logger, EmployeeExcelExporter employeeExcelExporter, EmployeeExcelImporter employeeExcelImporter, IEmployeeExcel employeeExcel)
    public TestExceptionController(ILogger<TestExceptionController> logger, IEmployeeExcel employeeExcel, IOcrService ocrService)
    {
        _logger = logger;
        //_employeeExcelExporter = employeeExcelExporter;
        //_employeeExcelImporter = employeeExcelImporter;
        _employeeExcel = employeeExcel;
        _ocrService = ocrService;
    }

    // created this endpoint just for debugging purpose, will delete later
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        Console.WriteLine("\n\n\n\n\n hehehehn\n\n");

        //Commenting the export code to test import code
        //_employeeExcelExporter.ExportEmployeesToExcel(@"C:\Users\user\Downloads\test7.xlsx");


        //_employeeExcelImporter.ReadEmployeesFromExcel(@"C:\Users\user\Downloads\test7.xlsx");



        // Calling from scoped

        //await _employeeExcel.ExportEmployeesToExcel(@"C:\Users\user\Downloads\test7.xlsx");

        //await _employeeExcel.ReadEmployeesFromExcel(@"C:\Users\user\Downloads\test7.xlsx");



        return Ok("Excel file exported successfully.");
    }


    //// created this endpoint just for debugging purpose, will delete later
    //[HttpGet("check-leave-days")]
    //public async Task<IActionResult> CheckLeaveDays()
    //{
    //    DateTime startDate = new DateTime(2025, 9, 22, 15, 30, 0);
    //    DateTime endDate = new DateTime(2025, 9, 24, 13, 1, 0);
    //    decimal effectiveLeaveDays = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(startDate, endDate);
    //    return Ok($"Endpoint Executed, the value of effectiveLeaveDays is: {effectiveLeaveDays}");
    //}


    [HttpGet("bad-request")]
    public IActionResult ThrowBadRequest()
    {
        throw new BadRequestException("This is a bad request test exception.");
    }

    [HttpGet("not-found")]
    public IActionResult ThrowNotFound()
    {
        throw new NotFoundException("This is a not found test exception.");
    }

    [HttpGet("server-error")]
    public IActionResult ThrowServerError()
    {
        throw new Exception("This is a generic server error test exception.");
    }

    [HttpGet("TestTesseract")]
    public IActionResult TestTesseract()
    {

        Console.WriteLine("\n\n\n\n\n Hello World, This is testing Tesseract Endpoint\n\n");

        //string testDataPath = @"C:\Users\user\Documents\testDataPath";
        string testDataPath = @"C:\Users\user\Documents\testDataPath\tessdata";
        string imagePath = @"C:\Users\user\Documents\OCR_Folder\image.png";
        
        try
        {
            using (var engine = new TesseractEngine(testDataPath, "eng", EngineMode.Default))
            {
                // Load Data
                using (var img = Pix.LoadFromFile(imagePath))
                { 
                    var result = engine.Process(img);
                    Console.WriteLine($"Recognized Text: {result.GetText()}");
                    Console.WriteLine($"Confidence: {result.GetMeanConfidence()}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OCR processing: {ex.Message}");
            return StatusCode(500, "An error occurred while processing the image.");
        }



        // This is just a placeholder for testing Tesseract OCR functionality.
        // You can implement the actual OCR logic here and return the results.
        return Ok("Tesseract OCR test endpoint hit successfully.");
    }



    [HttpGet("TestTesseract-new")]
    public IActionResult TestTesseractNew(IFormFile file)
    {

        Console.WriteLine("\n\n\n\n\n Hello World, This is papa testing Tesseract Endpoint\n\n");

        //string testDataPath = @"C:\Users\user\Documents\testDataPath";
        string testDataPath = @"C:\Users\user\Documents\testDataPath\tessdata";
        string imagePath = @"C:\Users\user\Documents\OCR_Folder\image.png";


        string tessPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "tessdata"
                        );

        try
        {
            using (var engine = new TesseractEngine(tessPath, "eng", EngineMode.Default))
            {
                // Load Data
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    var result = engine.Process(img);
                    Console.WriteLine($"Recognized Text: {result.GetText()}");
                    Console.WriteLine($"Confidence: {result.GetMeanConfidence()}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OCR processing: {ex.Message}");
            return StatusCode(500, "An error occurred while processing the image.");
        }



        // This is just a placeholder for testing Tesseract OCR functionality.
        // You can implement the actual OCR logic here and return the results.
        return Ok("Tesseract OCR test endpoint hit successfully.");
    }


    [HttpGet("verify-tesseract")]
    public IActionResult VerifyTesseract()
    {
        string tessPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "tessdata"
        );

        string trainedData = Path.Combine(
            tessPath,
            "eng.traineddata"
        );

        return Ok(new
        {
            CurrentDirectory = Directory.GetCurrentDirectory(),
            TessPath = tessPath,
            FileExists = System.IO.File.Exists(trainedData)
        });
    }

    [HttpPost("verify-tesseract-service")]
    public IActionResult VerifyTesseractService([FromForm] IFormFile file)
    {
        Console.WriteLine("\n\n\n\n\n Hello World, This is uzaif testing Tesseract Endpoint\n\n");
        //string imagePath = @"C:\Users\user\Documents\OCR_Folder\image.png";

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var result = _ocrService.ExtractText(file);

        return Ok(result);

    }



}
    