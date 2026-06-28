var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Todo storage. Singleton so in-memory data persists for the app's lifetime.
// Swap this single line for a database-backed repository in Milestone 2.
builder.Services.AddSingleton<TodoApi.Repositories.ITodoRepository, TodoApi.Repositories.InMemoryTodoRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
