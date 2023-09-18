using System.Security.Cryptography;
using System.Text;

namespace FileService.Controllers.Files.Services;

public interface IFileService
{
    string CreateDirectoryAndGetFilePath(string fileId);
    Task ProcessRecordingFileAsync(string filePath, Stream sourceStream);
}

public sealed class FileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public FileService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task ProcessRecordingFileAsync(string filePath, Stream sourceStream)
    {
        const int chunkSize = 1024 * 1024; // 1МБ
        var buffer = new byte[chunkSize];
        int bytesRead = 0;

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            do
            {
                bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length);

                await fileStream.WriteAsync(buffer, 0, bytesRead);

            } while (bytesRead > 0);
        }
    }

    public string CreateDirectoryAndGetFilePath(string fileId)
    {
        var hash = GetHashFromId(fileId);
        
        string firstPart = Path.Combine(_webHostEnvironment.ContentRootPath, hash.Substring(0, 3));
        string secondPart = Path.Combine(firstPart, hash.Substring(3, 3));

        Directory.CreateDirectory(secondPart);

        return Path.Combine(secondPart, hash.Substring(6, hash.Length - 6));
    }

    private string GetHashFromId(string fileId)
    {
        var stringBuilder = new StringBuilder();
        var buffer = Encoding.UTF8.GetBytes(fileId + DateTime.Now);
        
        using(var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(buffer);
            foreach (var item in hashBytes)
            {
                stringBuilder.Append(item.ToString("x2"));
            }
        }
        
        return stringBuilder.ToString();
    }
}