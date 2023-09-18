namespace FileService.Entities;

public sealed class AccessToken
{
    public int Id { get; set; }

    public string Token { get; set; } = default!;

    public Guid CloudFileId { get; set; } = default!;
    
    public CloudFile CloudFile { get; set; } = default!;
}