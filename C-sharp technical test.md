# C-sharp technical test

Build a distributed system with an API and Orleans.

First, you must have `.Net 7.0` or higher installed and ready to use.

The goal of this technical test is to manage "todos" through an API inside a silo using one or multiple grain(s). We should be able to `create`, `edit`, `remove`, `clear` and `list` todos by `OwnerKey` which represents an user (you don't have to manage users so, any owner key is authorized).

_Be sure to follow __Installation__ and __Files setup__ bellow to get started !_

Some issues are present in the snippets, we expect you to describe why :

- The given `TodoItem item` parameter of `TodoController.Create` isn't good practice.
- There's no response from `TodoController.Create` which isn't good practice too.
- Feel free to point out any issues you found or things to optimize/improve.

Exemple of request :

```sh
curl -X POST http://localhost:5035/todo/create -H 'Content-Type: application/json' -d '{"Title": "Do the technical test","IsDone":false,"OwnerKey":"f8e3b1cb-45f6-442a-ab12-f66bd61a9df8"}'
```

The idea is to spend at most 3~4 hours on this test. Feel free to add anything which sounds interesting for you. We don't expect a finished test (should run at least), but we will look at how you handle a profesionnal project from scratch (what are your priorities and what you do and don't do).

_Note: the lack of specs or more detailed informations is intended._

## Useful links

- [ASP.Net Docs](https://dotnet.microsoft.com/en-us/apps/aspnet)
- [Orleans Docs](https://learn.microsoft.com/en-us/dotnet/orleans/)
- [Orleans Code Samples](https://github.com/dotnet/samples/tree/main/orleans)

## Installation

```sh
dotnet new sln --name=VirtualRegattaTechnicalTest

dotnet new webapp -o GameServicesAPI --no-https -f net7.0 && dotnet sln add GameServicesAPI
dotnet new console -o GameSilos -f net7.0 && dotnet sln add GameSilos
dotnet new classlib -o Shared -f net7.0 && dotnet sln add Shared

dotnet add GameServicesAPI reference Shared
dotnet add GameSilos reference Shared

dotnet add GameServicesAPI package Microsoft.Orleans.Client
dotnet add GameSilos package Microsoft.Orleans.Server && dotnet add GameSilos package Microsoft.Extensions.Hosting && dotnet add GameSilos package Microsoft.Extensions.Logging
dotnet add Shared package Microsoft.Orleans.Sdk
```

## Files setup

Delete file `Shared/Class1.cs`.

Add file `Shared/Models/Todo.cs` :

```csharp
namespace Shared.Models;

[Immutable]
[GenerateSerializer]
public record class TodoItem(
    Guid Key,
    string Title,
    bool IsDone,
    Guid OwnerKey,
    DateTime? Timestamp = null
);
```

Add file `Shared/Grains/ITodoGrain.cs` :

```csharp
using Shared.Models;

namespace Shared.Grains;

public interface ITodoGrain : IGrainWithGuidKey
{
    Task SetAsync(TodoItem item);
    Task<TodoItem?> GetAsync();
}
```

Change file `GameSilos/Program.cs` :

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder()
    .UseOrleans(builder =>
    {
        builder.UseLocalhostClustering();
        builder.AddMemoryGrainStorageAsDefault();
    })
    .ConfigureLogging(builder => builder.AddConsole())
    .RunConsoleAsync();
```

Add file `GameSilos/Grains/TodoGrain.cs` :

```csharp
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Shared.Grains;
using Shared.Models;

namespace Silo.Grains;

public class TodoGrain : Grain, ITodoGrain
{
    private readonly ILogger<TodoGrain> _logger;
    private readonly IPersistentState<State> _state;

    private static string GrainType => nameof(TodoGrain);
    private Guid GrainKey => this.GetPrimaryKey();

    public TodoGrain(ILogger<TodoGrain> logger, [PersistentState("State")] IPersistentState<State> state)
    {
        _logger = logger;
        _state = state;
    }

    public Task<TodoItem?> GetAsync() => Task.FromResult(_state.State.Item);

    public async Task SetAsync(TodoItem item)
    {
        _state.State.Item = item;
        await _state.WriteStateAsync();

        _logger.LogInformation(
            "{@GrainType} {@GrainKey} now contains {@Todo}",
            GrainType, GrainKey, item);
    }

    [GenerateSerializer]
    public class State
    {
        [Id(0)] public TodoItem? Item { get; set; }
    }
}
```

Change file `GameServicesAPI/Program.cs` :

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient(builder => { builder.UseLocalhostClustering(); });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Remove folders `GameServicesAPI/Pages` and `GameServicesAPI/wwwroot`.

Add file `GameServicesAPI/Controllers/TodoController.cs` :

```csharp
using Microsoft.AspNetCore.Mvc;
using Shared.Grains;
using Shared.Models;

namespace GameServicesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoController : Controller
{
    private readonly ILogger<TodoController> _logger;
    private readonly IClusterClient _client;
    
    public TodoController(ILogger<TodoController> logger, IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(TodoItem item)
    {
        _logger.LogInformation("Adding {@item}.", item);
        await _client.GetGrain<ITodoGrain>(item.Key).SetAsync(item);

        return Ok();
    }
}
```
