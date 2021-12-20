using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace Sample.AzureBlob.Infrastructure.Blob;

public interface IAzureBlobStorage
{
    Task<IEnumerable<string>> AllBlobs(string containerName);
    Task<string> GetBlobAsync(string name, string containerName);
    Task<BlobDownloadInfo> GetBlobBytesAsync(string name, string containerName);
    string GetBlobUrlAsync(string containerName, string fileName);
    Task<bool> UploadBlobAsync(string fileExtension, IFormFile file, string containerName);
    Task<bool> UploadBlobAsync(string fileExtension, Stream uploadFileStream, string containerName, bool overwrite);
    Task<List<BlobItem>> ListAllItemsInContainer();
    Task<bool> DeleteBlob(string name, string containerName);
}