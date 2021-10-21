using System.Text;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Sample.AzureBlob.Infrastructure.Blob;

public class AzureBlobStorage : IAzureBlobStorage
{
    private CloudStorageAccount StorageAccount { get; }

    public AzureBlobStorage(AzureBlobSettings settings)
    {
        if (CloudStorageAccount.TryParse(settings.StorageConnectionString, out var storageAccount))
            StorageAccount = storageAccount;
        else
            throw new Exception("unable to parse connection string");
    }
    public async Task<CloudBlobContainer> CreateContainerAsync(string containerName)
    {
        containerName = containerName.ToLower();
        var blobClient = StorageAccount.CreateCloudBlobClient();

        var blobContainer = blobClient.GetContainerReference(containerName);
        await blobContainer.CreateIfNotExistsAsync();

        var permissions = await blobContainer.GetPermissionsAsync();
        permissions.PublicAccess = BlobContainerPublicAccessType.Container;

        await blobContainer.SetPermissionsAsync(permissions);

        return blobContainer;
    }

    public async Task<List<CloudBlobContainer>> ListContainersAsync()
    {
        var client = StorageAccount.CreateCloudBlobClient();
        BlobContinuationToken continuationToken = null;
        var containers = new List<CloudBlobContainer>();

        do
        {
            var response = await client.ListContainersSegmentedAsync(continuationToken);
            continuationToken = response.ContinuationToken;
            containers.AddRange(response.Results);

        } while (continuationToken != null);

        return containers;
    }

    public async Task UploadAsync(string fileExtension, string filePath, CloudBlobContainer blobContainer)
    {
        var fileName = $"{Guid.NewGuid()}{fileExtension}";

        var blockBlob = GetBlockBlobAsync(fileName, blobContainer);

        await using var fileStream = File.Open(filePath, FileMode.Open);
        fileStream.Position = 0;
        await blockBlob.UploadFromStreamAsync(fileStream);
    }

    public async Task<string> UploadAsync(string fileExtension, Stream stream, CloudBlobContainer blobContainer, string contentType)
    {
        var fileName = $"{Guid.NewGuid()}{fileExtension}";

        var blockBlob = GetBlockBlobAsync(fileName, blobContainer);
        blockBlob.Properties.ContentType = contentType;

        stream.Position = 0;
        await blockBlob.UploadFromStreamAsync(stream);

        return blockBlob.Uri.AbsoluteUri;
    }

    public async Task<DownloadViewModel> DownloadAsync(string blobName, CloudBlobContainer blobContainer)
    {
        var model = new DownloadViewModel();
        var blockBlob = GetBlockBlobAsync(blobName, blobContainer);

        await using var memoryStream = new MemoryStream();
        await blockBlob.DownloadToStreamAsync(memoryStream);
        var str = Encoding.ASCII.GetString(memoryStream.ToArray());

        model.File = str;
        model.Name = blobName;

        return model;
    }

    public async Task DownloadAsync(string blobName, string path, CloudBlobContainer blobContainer)
    {
        var blockBlob = GetBlockBlobAsync(blobName, blobContainer);

        await blockBlob.DownloadToFileAsync(path, FileMode.Create);
    }

    public async Task DeleteAsync(string blobName, CloudBlobContainer blobContainer)
    {
        var blockBlob = GetBlockBlobAsync(blobName, blobContainer);

        await blockBlob.DeleteAsync();
    }

    public async Task<bool> ExistsAsync(string blobName, CloudBlobContainer blobContainer)
    {
        var blockBlob = GetBlockBlobAsync(blobName, blobContainer);

        return await blockBlob.ExistsAsync();
    }

    public async Task<List<AzureBlobItem>> ListAsync(CloudBlobContainer blobContainer)
    {
        return await GetBlobListAsync(blobContainer);
    }

    public async Task<List<AzureBlobItem>> ListAsync(string rootFolder, CloudBlobContainer blobContainer)
    {
        switch (rootFolder)
        {
            case "*":
                return await ListAsync(blobContainer);
            case "/":
                rootFolder = "";
                break;
        }

        var list = await GetBlobListAsync(blobContainer);
        return list.Where(i => i.Folder == rootFolder).ToList();
    }

    public async Task<List<string>> ListFoldersAsync(CloudBlobContainer blobContainer)
    {
        var list = await GetBlobListAsync(blobContainer);
        return list.Where(i => !string.IsNullOrEmpty(i.Folder))
            .Select(i => i.Folder)
            .Distinct()
            .OrderBy(i => i)
            .ToList();
    }

    public async Task<List<string>> ListFoldersAsync(string rootFolder, CloudBlobContainer blobContainer)
    {
        if (rootFolder == "*" || rootFolder == "") return await ListFoldersAsync(blobContainer); //All Folders

        var list = await GetBlobListAsync(blobContainer);
        return list.Where(i => i.Folder.StartsWith(rootFolder))
            .Select(i => i.Folder)
            .Distinct()
            .OrderBy(i => i)
            .ToList();
    }

    public CloudBlobContainer GetContainerAsync(string containerName)
    {
        var blobClient = StorageAccount.CreateCloudBlobClient();

        var blobContainer = blobClient.GetContainerReference(containerName);

        return blobContainer;
    }

    private CloudBlockBlob GetBlockBlobAsync(string blobName, CloudBlobContainer blobContainer)
    {
        var blockBlob = blobContainer.GetBlockBlobReference(blobName);

        return blockBlob;
    }

    public async Task<List<AzureBlobItem>> GetBlobListAsync(CloudBlobContainer blobContainer, bool useFlatListing = true)
    {
        var list = new List<AzureBlobItem>();
        BlobContinuationToken token = null;
        do
        {
            var resultSegment =
                await blobContainer.ListBlobsSegmentedAsync("", useFlatListing, new BlobListingDetails(), null, token, null, null);
            token = resultSegment.ContinuationToken;

            list.AddRange(resultSegment.Results.Select(item => new AzureBlobItem(item)));
        } while (token != null);

        return list.OrderBy(i => i.Folder).ThenBy(i => i.Name).ToList();
    }
    public async Task DeleteFile(CloudBlobContainer container, string uniqueFileIdentifier)
    {

        var blob = container.GetBlockBlobReference(uniqueFileIdentifier);
        await blob.DeleteIfExistsAsync();
    }

    public string GetContainerSasUri(CloudBlobContainer container, string storedPolicyName = null)
    {
        string sasContainerToken;

        if (storedPolicyName == null)
        {
            var adHocPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List
            };

            sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
        }
        else
        {
            sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
        }

        return container.Uri + sasContainerToken;
    }
    public string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null)
    {
        string sasBlobToken;

        var blob = container.GetBlockBlobReference(blobName);

        if (policyName == null)
        {
            var adHocSas = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
            };

            sasBlobToken = blob.GetSharedAccessSignature(adHocSas);
        }
        else
        {

            sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
        }

        return blob.Uri + sasBlobToken;
    }

    public async void CreateSharedAccessPolicy(CloudBlobContainer container, string policyName)
    {
        var sharedPolicy = new SharedAccessBlobPolicy()
        {
            SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
            Permissions = SharedAccessBlobPermissions.Read
        };

        var permissions = await container.GetPermissionsAsync();

        permissions.SharedAccessPolicies.Add(policyName, sharedPolicy);
        await container.SetPermissionsAsync(permissions);
    }
}