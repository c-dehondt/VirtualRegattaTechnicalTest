

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient(builder => { builder.UseLocalhostClustering(); });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();