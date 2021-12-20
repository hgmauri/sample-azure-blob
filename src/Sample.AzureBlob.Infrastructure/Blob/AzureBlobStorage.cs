using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;

namespace Sample.AzureBlob.Infrastructure.Blob;

public class AzureBlobStorage : IAzureBlobStorage
{
    private readonly BlobServiceClient _blobClient;

    public AzureBlobStorage(BlobServiceClient blobClient)
    {
        _blobClient = blobClient;
    }

    public async Task<IEnumerable<string>> AllBlobs(string containerName)
    {
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        var files = new List<string>();
        var blobs = containerClient.GetBlobsAsync();
        await foreach (var item in blobs)
        {
            files.Add(item.Name);
        }
        return files;
    }

    public async Task<string> GetBlobAsync(string name, string containerName)
    {
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(name);

        return await Task.Run(() => blobClient.Uri.AbsoluteUri);
    }

    public async Task<BlobDownloadInfo> GetBlobBytesAsync(string name, string containerName)
    {
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(name);
        var blobDownloadInfo = await blobClient.DownloadAsync();

        return blobDownloadInfo.Value;
    }

    public string GetBlobUrlAsync(string containerName, string fileName)
    {
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        var expiration = DateTimeOffset.Now.AddHours(2);

        var uri = blobClient.GenerateSasUri(BlobSasPermissions.Read, expiration);

        return uri.ToString();
    }

    public async Task<bool> UploadBlobAsync(string fileExtension, IFormFile file, string containerName)
    {
        var fileName = $"{Guid.NewGuid()}_{fileExtension}";
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var res = await blobClient.UploadAsync(file.OpenReadStream(), new BlobHttpHeaders { ContentType = file.ContentType });
        return res != null;
    }

    public async Task<bool> UploadBlobAsync(string fileExtension, Stream uploadFileStream, string containerName, bool overwrite)
    {
        var fileName = $"{Guid.NewGuid()}_{fileExtension}";
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var res = await blobClient.UploadAsync(uploadFileStream, overwrite);
        return res != null;
    }

    public async Task<List<BlobItem>> ListAllItemsInContainer()
    {
        var listBlobs = new List<BlobItem>();
        await foreach (var blobContaineritem in _blobClient.GetBlobContainersAsync())
        {
            var blobContainerClient = _blobClient.GetBlobContainerClient(blobContaineritem.Name);

            await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
            {
                listBlobs.Add(blobItem);
            }
        }
        return listBlobs;
    }

    public async Task<bool> DeleteBlob(string name, string containerName)
    {
        var containerClient = _blobClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(name);
        return await blobClient.DeleteIfExistsAsync();
    }
}