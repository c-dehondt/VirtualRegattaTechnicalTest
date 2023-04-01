using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Shared.Models.Todo;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(builder =>
    {
        builder.UseLocalhostClustering();
        builder.AddMemoryGrainStorageAsDefault();
    })
    .ConfigureLogging(builder => builder.AddConsole())
    .RunConsoleAsync();