using FileService;
using FileService.Configurations.ExceptionHandlers;
using FileService.Controllers.Files.Services;
using FileService.Settings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");
var contentRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
if (!Directory.Exists(contentRootPath))
{
    Directory.CreateDirectory(contentRootPath);
}

builder.Environment.ContentRootPath = contentRootPath;

builder.Services.Configure<AccessLinkSettings>(builder.Configuration.GetSection(AccessLinkSettings.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHostedService<FileShredderHostedService>();
builder.Services.AddSignalR();

builder.Services.AddSingleton<MultipartRequestService>();
builder.Services.AddSingleton<IFileService, FileService.Controllers.Files.Services.FileService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<FileUploadProcessExceptionHandler>();

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<FileHub>("api/hub/file");

app.Run();
