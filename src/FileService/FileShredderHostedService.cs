using Microsoft.EntityFrameworkCore;

namespace FileService;

public sealed class FileShredderHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    public FileShredderHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var files = await dbContext.Files
                .Where(f => f.ToDelete == true)
                .ToListAsync(stoppingToken);

            foreach (var file in files)
            {
                if (File.Exists(file.Path))
                {
                    File.Delete(file.Path);
                }
            }

            dbContext.RemoveRange(files);
            await dbContext.SaveChangesAsync(stoppingToken);
        }

        await Task.Delay(1000 * 60 * 60, stoppingToken);
    }
}