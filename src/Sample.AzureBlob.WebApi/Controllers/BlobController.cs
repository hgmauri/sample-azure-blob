using Microsoft.AspNetCore.Mvc;
using Sample.AzureBlob.Infrastructure.Blob;

namespace Sample.AzureBlob.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly ILogger<BlobController> _logger;
        private readonly IAzureBlobStorage _azureBlobStorage;

        public BlobController(ILogger<BlobController> logger, IAzureBlobStorage azureBlobStorage)
        {
            _logger = logger;
            _azureBlobStorage = azureBlobStorage;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(ICollection<IFormFile> files)
        {
            var container = await _azureBlobStorage.CreateContainerAsync("images");
            string? result = null;
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var extension = Path.GetExtension(formFile.FileName);
                    await using var stream = formFile.OpenReadStream();
                    result = await _azureBlobStorage.UploadAsync(extension, stream, container);
                }
            }
            return Ok(result);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadAsync([FromQuery] string fileName)
        {
            var container = await _azureBlobStorage.CreateContainerAsync("images");
            var result = await _azureBlobStorage.DownloadAsync(fileName, container);

            return Ok(result);
        }
    }
}