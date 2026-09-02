using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for <see cref="DocumentIntelligence"/> aggregate persistence.</summary>
public interface IDocumentIntelligenceRepository : IRepository<DocumentIntelligence>
{
    /// <summary>Returns all Document Intelligence resources belonging to the specified resource group.</summary>
    Task<List<DocumentIntelligence>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
