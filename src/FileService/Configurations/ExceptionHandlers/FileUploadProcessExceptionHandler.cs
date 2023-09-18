using System.Net;
using FileService.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FileService.Configurations.ExceptionHandlers;

public sealed class FileUploadProcessExceptionHandler
{
    private readonly RequestDelegate _next;

    public FileUploadProcessExceptionHandler(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        try
        {
            await _next(context);
        }
        catch (FileUploadProcessException exception)
        {
            if (exception.InnerException is InvalidDataException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                
                var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == exception.File.Id);
                if (file is not null)
                {
                    file.ToDelete = true;
                }
                else
                {
                    exception.File.ToDelete = true;
                    await dbContext.AddAsync(exception.File);
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}