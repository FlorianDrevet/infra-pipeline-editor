using Microsoft.AspNetCore.Http;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

public interface IBlobService
{
    Task<Uri> UploadFileAsync(IFormFile formFile);
    Task<Uri> UploadContentAsync(string fileName, string content, string contentType = "text/plain");
    Task<string> DeleteFileAsync(string fileName);
    Task<string?> DownloadContentAsync(string blobName);
    Task<IReadOnlyCollection<string>> ListBlobsAsync(string prefix);
}