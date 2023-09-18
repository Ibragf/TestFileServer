using FileService.Controllers.Files.Dto;

namespace FileService.Controllers.Files.Responses;

public sealed class GetFilesResponse
{
    public IEnumerable<CloudFileDto> Files { get; set; } = default!;
}