using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ExpensAI.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        // POST api/upload
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Save file to a temporary location (could be blob storage)
            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            // Forward file to Python service for processing
            var httpClient = new HttpClient();
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(System.IO.File.OpenRead(filePath));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await httpClient.PostAsync("http://ai_service:8000/process", content);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Processing failed.");

            var resultJson = await response.Content.ReadAsStringAsync();
            return Ok(resultJson);
        }
    }
}
