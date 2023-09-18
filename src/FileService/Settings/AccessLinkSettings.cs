namespace FileService.Settings;

public sealed class AccessLinkSettings
{
    public const string SectionName = "AccessLink";

    public Uri BaseUrl { get; set; } = default!;
}