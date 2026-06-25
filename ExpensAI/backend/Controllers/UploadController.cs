using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ExpensAI.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Recomenda-se habilitar após configurar o provedor JWT no Program.cs
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        
        // Allowlist de extensões e MimeTypes
        private readonly string[] _allowedExtensions = { ".csv", ".pdf", ".ofx" };
        private readonly string[] _allowedMimeTypes = { "text/csv", "application/pdf", "application/x-ofx", "application/vnd.intu.qbo" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public UploadController(ILogger<UploadController> logger)
        {
            _logger = logger;
        }

        // POST api/upload
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Tentativa de upload sem arquivo.");
                    return BadRequest("Arquivo inválido ou não fornecido.");
                }

                if (file.Length > MaxFileSize)
                {
                    _logger.LogWarning("Tentativa de upload de arquivo muito grande. Tamanho: {Size}", file.Length);
                    return BadRequest("O arquivo excede o tamanho máximo permitido (10MB).");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension) || !_allowedMimeTypes.Contains(file.ContentType))
                {
                    _logger.LogWarning("Tentativa de upload com formato não suportado. Extensão: {Ext}, Mime: {Mime}", extension, file.ContentType);
                    return BadRequest("Formato de arquivo não suportado. Apenas PDF, CSV e OFX são aceitos.");
                }

            // Save file to a temporary location (could be blob storage)
            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            // Forward file to Python service for processing
            using var httpClient = new HttpClient();
            using var content = new MultipartFormDataContent();
            
            // Usar file.OpenReadStream() em vez de salvar no disco fisicamente reduz risco de LFI e preenchimento de disco
            using var fileStream = file.OpenReadStream();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            
            // Sanitização do nome do arquivo (Path Traversal protection)
            var safeFileName = Path.GetFileName(file.FileName);
            content.Add(fileContent, "file", safeFileName);

            var response = await httpClient.PostAsync("http://ai_service:8000/process", content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro no serviço Python. Status Code: {StatusCode}", response.StatusCode);
                return StatusCode(500, "Ocorreu um erro interno ao processar o arquivo. Tente novamente mais tarde.");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            return Ok(resultJson);
            }
            catch (Exception ex)
            {
                // Tratamento de Erros: Loga a exceção real, mas retorna mensagem genérica
                _logger.LogError(ex, "Erro crítico durante o upload e processamento de arquivo.");
                return StatusCode(500, "Ocorreu um erro interno no servidor. Tente novamente mais tarde.");
            }
        }
    }
}
