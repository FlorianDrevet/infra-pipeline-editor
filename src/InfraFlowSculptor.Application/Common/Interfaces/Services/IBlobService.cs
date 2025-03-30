using Microsoft.AspNetCore.Http;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

public interface IBlobService
{
    public Task<Uri> UploadFileAsync(IFormFile formFile);
    public Task<string> DeleteFileAsync(string fileName);
}