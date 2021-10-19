using Microsoft.AspNetCore.Http;

namespace Sample.AzureBlob.Infrastructure.Blob;

public class DownloadViewModel
{
    public MemoryStream Stream { get; set; }
    public IFormFile File { get; set; }
}