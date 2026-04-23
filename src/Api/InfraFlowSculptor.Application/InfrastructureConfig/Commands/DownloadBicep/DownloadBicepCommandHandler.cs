using InfraFlowSculptor.Application.Common.Interfaces;
using System.IO.Compression;
using System.Text;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBicep;

public sealed class DownloadBicepCommandHandler(
    IBlobService blobService,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DownloadBicepCommand, DownloadBicepResult>
{
    public async Task<ErrorOr<DownloadBicepResult>> Handle(
        DownloadBicepCommand command,
        CancellationToken cancellationToken)
    {
        // Audit APP-002 (2026-04-23): enforce read access before exposing artifacts.
        var accessResult = await accessService.VerifyReadAccessAsync(
            new InfrastructureConfigId(command.InfrastructureConfigId), cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var prefix = $"bicep/{command.InfrastructureConfigId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.InfrastructureConfig.BicepFilesNotFoundError(command.InfrastructureConfigId);

        // Find the latest timestamp folder (format: bicep/{configId}/{yyyyMMddHHmmss}/...)
        var latestPrefix = allBlobs
            .Select(b => string.Join('/', b.Split('/').Take(3)))
            .Distinct()
            .OrderDescending()
            .First();

        var latestBlobs = allBlobs
            .Where(b => b.StartsWith(latestPrefix, StringComparison.Ordinal))
            .ToList();

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var blobName in latestBlobs)
            {
                var content = await blobService.DownloadContentAsync(blobName);
                if (content is null) continue;

                // Strip the prefix to get relative path: main.bicep, main.bicepparam, modules/xyz.bicep
                var relativePath = blobName[(latestPrefix.Length + 1)..];
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(Encoding.UTF8.GetBytes(content), cancellationToken);
            }
        }

        var fileName = $"bicep-{command.InfrastructureConfigId:N}.zip";
        return new DownloadBicepResult(memoryStream.ToArray(), fileName);
    }
}
