using Microsoft.AspNetCore.Mvc;
using Sample.AzureBlob.Infrastructure.Blob;

namespace Sample.AzureBlob.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IAzureBlobStorage _azureBlobStorage;

        public BlobController(IAzureBlobStorage azureBlobStorage)
        {
            _azureBlobStorage = azureBlobStorage;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(ICollection<IFormFile> files)
        {
            var container = "images";
            bool result = false;
            foreach (var formFile in files)
            {
                if (formFile.Length <= 0)
                    continue;

                var extension = Path.GetExtension(formFile.FileName);
                await using var stream = formFile.OpenReadStream();
                result = await _azureBlobStorage.UploadBlobAsync(extension, formFile, container);
            }
            return Ok(result);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadAsync([FromQuery] string fileName)
        {
            var result = await _azureBlobStorage.GetBlobBytesAsync(fileName, "images");

            return Ok(result);
        }
    }
}