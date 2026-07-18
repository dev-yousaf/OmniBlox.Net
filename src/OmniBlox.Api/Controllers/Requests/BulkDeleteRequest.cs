namespace OmniBlox.Api.Controllers.Requests;

public record BulkDeleteRequest
{
    public List<Guid> Ids { get; init; } = [];
}
