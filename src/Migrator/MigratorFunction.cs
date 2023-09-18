using FluentMigrator.Runner;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Migrator
{
    public static class MigratorFunction
    {
        [FunctionName("Migrator")]
        public static  IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var serviceProvider = MigratorStartup.Setup();
            var migrationRunner = serviceProvider.GetRequiredService<IMigrationRunner>();

            migrationRunner.MigrateUp();

            return new OkObjectResult("Successful migration");
        }
    }
}
