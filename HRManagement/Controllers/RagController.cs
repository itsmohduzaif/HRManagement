using HRManagement.DTOs.RagDTOs;
using HRManagement.Services.DocumentParse;
using HRManagement.Services.Rag;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HRManagement.Helpers;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RagController : ControllerBase
    {
        private readonly RagService _ragService;
        private readonly DocumentParser _parser;

        public RagController(RagService ragService, DocumentParser parser)
        {
            _ragService = ragService;
            _parser = parser;
        }

        [HttpPost("upload-documents-with-Files")]   
        public async Task<IActionResult> UploadDocuments([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            var documents = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 10 * 1024 * 1024) // 10 MB
                    return BadRequest($"File too large: {file.FileName}");

                try
                {
                    var content = await _parser.ExtractTextAsync(file);

                    content = CleanText.CleanTextFunction(content);


                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        documents.Add(content);
                    }
                }
                catch (Exception ex)
                {
                    // log error
                    Console.WriteLine($"Error processing {file.FileName}: {ex.Message}");
                }
            }

            await _ragService.AddDocumentsAsync(documents);

            return Ok(new
            {
                message = "Documents processed successfully",
                count = documents.Count
            });
        }

        [HttpPost("ask-question")]
        public async Task<IActionResult> AskQuestion([FromBody] AskRequestDto request)
        {
            var answer = await _ragService.AskQuestion(request.Question);

            return Ok(new { answer });
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var answer = await _ragService.Test();

            return Ok(new { answer });
        }

        [HttpGet("Test-Qdrant")]
        public async Task<IActionResult> TestQdrant()
        {
            //var answer = await _ragService.TestQdrant();

            //return Ok(new { answer });
            
            await _ragService.TestQdrant();
            return Ok("NICE");
        }

        //[HttpPost("upload-documents")]
        //public async Task<IActionResult> UploadDocuments([FromBody] UploadRequestDto request)
        //{
        //    await _ragService.AddDocumentsAsync(request.Documents);

        //    return Ok(new { message = "Documents stored", count = request.Documents.Count });
        //}


    }
}
