using Microsoft.Azure.Storage.Blob;

namespace Sample.AzureBlob.Infrastructure.Blob;

public interface IAzureBlobStorage
{
    Task<List<AzureBlobItem>> GetBlobListAsync(CloudBlobContainer blobContainer, bool useFlatListing);
    Task UploadAsync(string fileExtensiom, string filePath, CloudBlobContainer blobContainer);
    Task<string> UploadAsync(string fileExtensiom, Stream stream, CloudBlobContainer blobContainer, string contentType = "application/octet-stream");
    Task<DownloadViewModel> DownloadAsync(string blobName, CloudBlobContainer blobContainer);
    Task DownloadAsync(string blobName, string path, CloudBlobContainer blobContainer);
    Task DeleteAsync(string blobName, CloudBlobContainer blobContainer);
    Task<bool> ExistsAsync(string blobName, CloudBlobContainer blobContainer);
    Task<List<AzureBlobItem>> ListAsync(CloudBlobContainer blobContainer);
    Task<List<AzureBlobItem>> ListAsync(string rootFolder, CloudBlobContainer blobContainer);
    Task<List<string>> ListFoldersAsync(CloudBlobContainer blobContainer);
    Task<List<string>> ListFoldersAsync(string rootFolder, CloudBlobContainer blobContainer);
    Task<CloudBlobContainer> CreateContainerAsync(string containerName);
    Task<List<CloudBlobContainer>> ListContainersAsync();
    CloudBlobContainer GetContainerAsync(string containerName);
    string GetContainerSasUri(CloudBlobContainer container, string storedPolicyName = null);
    string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null);
    Task DeleteFile(CloudBlobContainer container, string uniqueFileIdentifier);
}