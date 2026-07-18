namespace OmniBlox.Api.Controllers.Requests;

public record UpdateProfileRequest
{
    public string? Name { get; init; }
    public string? CompanyName { get; init; }
    public string? Industry { get; init; }
    public string? OtherIndustry { get; init; }
    public string? Country { get; init; }
}
