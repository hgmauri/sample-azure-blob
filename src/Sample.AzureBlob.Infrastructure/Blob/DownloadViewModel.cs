using Microsoft.AspNetCore.Http;

namespace Sample.AzureBlob.Infrastructure.Blob;

public class DownloadViewModel
{
    public string Name { get; set; }
    public string File { get; set; }
}