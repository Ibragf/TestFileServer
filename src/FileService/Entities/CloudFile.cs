namespace FileService.Entities;

public sealed class CloudFile
{
    public Guid Id { get; set; }

    public string VirtualName { get; set; } = default!;

    public string Path { get; set; } = default!;
    
    public bool ToDelete { get; set; }

    public ICollection<AccessToken> AccessTokens { get; set; } = default!;
}