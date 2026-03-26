using System.IO.Compression;
using System.Text;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;

/// <summary>Handles the <see cref="DownloadProjectBicepCommand"/>.</summary>
public sealed class DownloadProjectBicepCommandHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : IRequestHandler<DownloadProjectBicepCommand, ErrorOr<DownloadProjectBicepResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<DownloadProjectBicepResult>> Handle(
        DownloadProjectBicepCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(command.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var prefix = $"bicep/project/{command.ProjectId.Value}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Error.NotFound(
                "DownloadProjectBicep.NotFound",
                $"No generated Bicep files found for project '{command.ProjectId.Value}'.");

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var latestBlobs = allBlobs
            .Where(blobName => blobName.StartsWith(latestPrefix, StringComparison.Ordinal))
            .ToList();

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var blobName in latestBlobs)
            {
                var content = await blobService.DownloadContentAsync(blobName);
                if (content is null)
                    continue;

                var relativePath = blobName[(latestPrefix.Length + 1)..];
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(Encoding.UTF8.GetBytes(content), cancellationToken);
            }
        }

        var fileName = $"project-bicep-{command.ProjectId.Value:N}.zip";
        return new DownloadProjectBicepResult(memoryStream.ToArray(), fileName);
    }
}