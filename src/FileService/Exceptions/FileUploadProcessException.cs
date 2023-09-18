using FileService.Entities;

namespace FileService.Exceptions;

public sealed class FileUploadProcessException : Exception
{
    public CloudFile File { get; }

    public FileUploadProcessException(CloudFile file)
    {
        File = file;
    }

    public FileUploadProcessException(string message, CloudFile file) : base(message)
    {
        File = file;
    }

    public FileUploadProcessException(string message, Exception innerException, CloudFile file) :
        base(message, innerException)
    {
        File = file;
    }
}