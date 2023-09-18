namespace FileService.Controllers.Files.Dto;

public sealed class CloudFileDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = default!;
}