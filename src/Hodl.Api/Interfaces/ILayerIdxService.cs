namespace Hodl.Api.Interfaces;

public interface ILayerIdxService
{
    Task<byte> IdxAssignmentStrategy(Holding holding, CancellationToken cancellationToken = default);
}