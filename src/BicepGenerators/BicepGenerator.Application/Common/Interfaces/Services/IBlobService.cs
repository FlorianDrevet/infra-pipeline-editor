using Microsoft.AspNetCore.Http;

namespace BicepGenerator.Application.Common.Interfaces.Services;

public interface IBlobService
{
    Task<Uri> UploadFileAsync(IFormFile formFile);
    Task<Uri> UploadContentAsync(string fileName, string content, string contentType = "text/plain");
    Task<string> DeleteFileAsync(string fileName);
}
