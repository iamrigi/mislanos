using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public class TimerFunction
{
    private readonly ArchivalService _archivalService;

    public TimerFunction(ArchivalService archivalService)
    {
        _archivalService = archivalService;
    }

    [FunctionName("ArchiveRecords")]
    public async Task Run([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation("Archival function triggered.");
        await _archivalService.ArchiveOldRecordsAsync("CosmosDatabaseName", "CosmosContainerName", "BlobContainerName");
        log.LogInformation("Archival process completed.");
    }
}