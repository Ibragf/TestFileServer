using FileService.Controllers.Files.Dto;
using FileService.Controllers.Files.Responses;
using FileService.Entities;

namespace FileService.Controllers.Files;

public static class FilesMapping
{
    public static CloudFileDto MapToCloudFileDto(this CloudFile file)
    {
        return new CloudFileDto
        {
            Id = file.Id,
            FileName = file.VirtualName
        };
    }
}