using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Infrastructure.Services.BlobService;

public sealed class BlobService : IBlobService
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobSettings> blobStorageSettings)
    {
        ArgumentNullException.ThrowIfNull(blobServiceClient);
        ArgumentNullException.ThrowIfNull(blobStorageSettings);

        var containerName = BlobSettings.ResolveContainerName(blobStorageSettings.Value.ContainerName);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<Uri> UploadFileAsync(IFormFile formFile)
    {
        await _blobContainerClient.CreateIfNotExistsAsync();
        var fileName = Path.GetFileName(formFile.FileName);
        await using var stream = formFile.OpenReadStream();

        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, overwrite: true);
        return blobClient.Uri;
    }

    public async Task<Uri> UploadContentAsync(
        string fileName,
        string content,
        string contentType = "text/plain")
    {
        await _blobContainerClient.CreateIfNotExistsAsync();
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);

        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });
        return blobClient.Uri;
    }

    public Task<string> DeleteFileAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> DownloadContentAsync(string blobName)
    {
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync())
            return null;

        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToString();
    }

    public async Task<IReadOnlyList<string>> ListBlobsAsync(string prefix)
    {
        var blobs = new List<string>();
        await foreach (var blob in _blobContainerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, CancellationToken.None))
        {
            blobs.Add(blob.Name);
        }
        return blobs;
    }
}