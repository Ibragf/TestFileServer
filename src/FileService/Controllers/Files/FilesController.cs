using System.Net;
using FileService.Controllers.Files.Responses;
using FileService.Controllers.Files.Services;
using FileService.Entities;
using FileService.Exceptions;
using FileService.Extensions;
using FileService.Extensions.Attributes;
using FileService.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace FileService.Controllers.Files;

[ApiController]
[Route("api/files")]
public sealed class FilesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly MultipartRequestService _multipartRequestService;
    private readonly IFileService _fileService;
    private readonly IHubContext<FileHub> _hubContext;
    private readonly AccessLinkSettings _accessLinkSettings;
    
    public FilesController(
        ApplicationDbContext dbContext,
        MultipartRequestService multipartRequestService,
        IFileService fileService,
        IHubContext<FileHub> hubContext,
        IOptions<AccessLinkSettings> accessLinkOptions)
    {
        _dbContext = dbContext;
        _multipartRequestService = multipartRequestService;
        _fileService = fileService;
        _hubContext = hubContext;
        _accessLinkSettings = accessLinkOptions.Value;
    }
    
    [HttpGet]
    public async Task<ActionResult<GetFilesResponse>> GetFilesAsync(int limit)
    {
        var files = await _dbContext.Files.Take(limit).ToListAsync();

        var response = new GetFilesResponse
        {
            Files = files.Select(f => f.MapToCloudFileDto())
        };

        return response;
    }
    
    [HttpGet("{id:guid}/access-link")]
    public async Task<ActionResult<string>> GetAccessLinkAsync(Guid id)
    {
        var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);
        if (file is null)
        {
            return NotFound();
        }

        var accessToken = new AccessToken
        {
            Token = RandomHelper.GenerateRandomString(20),
            CloudFile = file,
            CloudFileId = file.Id
        };

        _dbContext.AccessTokens.Add(accessToken);
        await _dbContext.SaveChangesAsync();

        return new Uri(_accessLinkSettings.BaseUrl, $"api/files/{accessToken.Token}").ToString();
    }
    
    [HttpGet("{token}")]
    public async Task<ActionResult> DownloadFileByTokenAsync(string token)
    {
        var accessToken = await _dbContext.AccessTokens
            .Where(f => f.Token == token)
            .Include(a => a.CloudFile)
            .FirstOrDefaultAsync();
        
        if (accessToken is null)
        {
            return NotFound();
        }

        var fileStream = new FileStream(accessToken.CloudFile.Path, FileMode.Open, FileAccess.Read);
        var contentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = accessToken.CloudFile.VirtualName
        };

        _dbContext.AccessTokens.Remove(accessToken);
        await _dbContext.SaveChangesAsync();
        
        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
        
        return new FileStreamResult(fileStream, "application/octet-stream");
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> DownloadFileByIdAsync(Guid id)
    {
        var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);
        if (file is null)
        {
            return NotFound();
        }

        var fileStream = new FileStream(file.Path, FileMode.Open, FileAccess.Read);
        var contentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = file.VirtualName
        };
        
        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
        
        return new FileStreamResult(fileStream, "application/octet-stream");
    }
    
    [HttpPost("upload")]
    [DisableFormValueModelBinding]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 1024L * 10L)]
    [RequestSizeLimit(1024L * 1024L * 1024L * 10L)]
    public async Task<ActionResult> UploadFilesAsync(string? hubConnectionId)
    {
        if (!_multipartRequestService.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File", $"The request couldn't be processed.");
            return BadRequest(ModelState);
        }

        CloudFile file = null!;
        try
        {
            var boundary = _multipartRequestService.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), 70);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section is not null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader)
                {
                    if (!_multipartRequestService.HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File", $"The request couldn't be processed.");
                        return BadRequest(ModelState);
                    }

                    if (string.IsNullOrWhiteSpace(contentDisposition!.FileName.Value) || contentDisposition.FileName.Value.Length > 75)
                    {
                        return BadRequest();
                    }

                    file = new CloudFile();
                    _dbContext.Files.Add(file);
                
                    file.VirtualName = contentDisposition.FileName.Value;
                    file.Path = _fileService.CreateDirectoryAndGetFilePath(file.Id.ToString());

                    await _fileService.ProcessRecordingFileAsync(file.Path, section.Body);
                    await _dbContext.SaveChangesAsync();
                    if (hubConnectionId is not null)
                    {
                        await _hubContext.Clients.Client(hubConnectionId).SendAsync("FileUploaded", file.VirtualName);
                    }
                }
            
                section = await reader.ReadNextSectionAsync();
            }
        }
        catch (Exception exception) when (
            exception is InvalidDataException
            or IOException
            or DbUpdateException
            or DbUpdateConcurrencyException)
        {
            var fileUploadProcessException = new FileUploadProcessException(exception.Message, exception, file);
            throw fileUploadProcessException;
        }
        
        return Ok();
    }
}